﻿/*
 * Copyright (c) 2019 Dummiesman
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
*/

using Dummiesman;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityExtension;

public class MTLLoader
{
    // public List<string> SearchPaths = new List<string>() { "%FileName%_Textures", string.Empty};
    public List<string> SearchPaths = new List<string>() { "%FileName%", string.Empty };

    private FileInfo _objFileInfo = null;

    /// <summary>
    /// The texture loading function. Overridable for stream loading purposes.
    /// </summary>
    /// <param name="path">The path supplied by the OBJ file, converted to OS path seperation</param>
    /// <param name="isNormalMap">Whether the loader is requesting we convert this into a normal map</param>
    /// <returns>Texture2D if found, or NULL if missing</returns>
    public virtual Texture2D TextureLoadFunction(string path, bool isNormalMap)
    {
        //return if eists
        if (LordAshes.FileAccessPlugin.File.Exists(path))
        {
            var tex = ImageLoader.LoadTexture(LordAshes.FileAccessPlugin.File.Find(path)[0]);

            if (isNormalMap) { tex = ImageUtils.ConvertToNormalMap(tex); }

            return tex;
        }
        //not found
        Debug.Log("Texture '" + path + "' file does not exist");
        return null;
    }

    private Texture2D TryLoadTexture(string texturePath, bool normalMap = false)
    {
        //swap directory seperator char
        texturePath = texturePath.Replace('\\', Path.DirectorySeparatorChar);
        texturePath = texturePath.Replace('/', Path.DirectorySeparatorChar);

        return TextureLoadFunction(texturePath, normalMap);
    }
    
    private int GetArgValueCount(string arg)
    {
        switch (arg)
        {
            case "-bm":
            case "-clamp":
            case "-blendu":
            case "-blendv":
            case "-imfchan":
            case "-texres":
                return 1;
            case "-mm":
                return 2;
            case "-o":
            case "-s":
            case "-t":
                return 3;
        }
        return -1;
    }

    private int GetTexNameIndex(string[] components)
    {
        for(int i=1; i < components.Length; i++)
        {
            var cmpSkip = GetArgValueCount(components[i]);
            if(cmpSkip < 0)
            {
                return i;
            }
            i += cmpSkip;
        }
        return -1;
    }

    private float GetArgValue(string[] components, string arg, float fallback = 1f)
    {
        string argLower = arg.ToLower();
        for(int i=1; i < components.Length - 1; i++)
        {
            var cmp = components[i].ToLower();
            if(argLower == cmp)
            {
                return OBJLoaderHelper.FastFloatParse(components[i+1]);
            }
        }
        return fallback;
    }

    private string GetTexPathFromMapStatement(string processedLine, string[] splitLine)
    {
        int texNameCmpIdx = GetTexNameIndex(splitLine);
        if(texNameCmpIdx < 0)
        {
            Debug.LogError($"texNameCmpIdx < 0 on line {processedLine}. Texture not loaded.");
            return null;
        }

        int texNameIdx = processedLine.IndexOf(splitLine[texNameCmpIdx]);
        string texturePath = processedLine.Substring(texNameIdx);

        return texturePath;
    }

    /// <summary>
    /// Loads a *.mtl file
    /// </summary>
    /// <param name="input">The input stream from the MTL file</param>
    /// <returns>Dictionary containing loaded materials</returns>
    public Dictionary<string, Material> Load(Stream input)
    {
        Debug.Log("Processing MTL file");

        var inputReader = new StreamReader(input);
        var reader = new StringReader(inputReader.ReadToEnd());

        Dictionary<string, Material> mtlDict = new Dictionary<string, Material>();
        Material currentMaterial = null;

        for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string processedLine = line.Clean();
            // UnityEngine.Debug.Log("MTLLoader(): "+processedLine);
            string[] splitLine = processedLine.Split(' ');

            //blank or comment
            if (splitLine.Length < 2 || processedLine[0] == '#')
                continue;

            //newmtl
            if (splitLine[0] == "newmtl")
            {
                string materialName = processedLine.Substring(7);

                // Shader shader = Shader.Find("Standard (Specular setup)");
                Shader shader = ShaderDetector.Find();

                var newMtl = new Material(shader) { name = materialName };
                mtlDict[materialName] = newMtl;
                currentMaterial = newMtl;

                if (ShaderDetector.PropName("_MetallicGlossMap")!="")
                {
                    var mgMap = TryLoadTexture("..\\MetallicGlossMap.bmp");
                    if(mgMap!=null)
                    {
                        currentMaterial.SetTexture(ShaderDetector.PropName("_MetallicGlassMap"), mgMap);
                    }
                    else
                    {
                        UnityEngine.Debug.Log("Problem loading the common 'MetallicGlossMap.bmp' file");
                    }
                }

                continue;
            }

            //anything past here requires a material instance
            if (currentMaterial == null)
                continue;

            //diffuse color
            if ((splitLine[0] == "Kd" || splitLine[0] == "kd") && (ShaderDetector.PropName("_Color") != ""))
            {
                var currentColor = currentMaterial.GetColor(ShaderDetector.PropName("_Color"));
                var kdColor = OBJLoaderHelper.ColorFromStrArray(splitLine);
                currentMaterial.SetColor(ShaderDetector.PropName("_Color"), new Color(kdColor.r, kdColor.g, kdColor.b, currentColor.a));
                continue;
            }

            //diffuse map
            if ((splitLine[0] == "map_Kd" || splitLine[0] == "map_kd") && (ShaderDetector.PropName("_MainTex")!=""))
            {
                string texturePath = GetTexPathFromMapStatement(processedLine, splitLine);
                if(texturePath == null)
                {
                    continue; //invalid args or sth
                }

                UnityEngine.Debug.Log("Loading Texture '" + texturePath + "'");

                var KdTexture = TryLoadTexture(texturePath);
                currentMaterial.SetTexture(ShaderDetector.PropName("_MainTex"), KdTexture);

                //set transparent mode if the texture has transparency
                if(KdTexture != null && (KdTexture.format == TextureFormat.DXT5 || KdTexture.format == TextureFormat.ARGB32))
                {
                    OBJLoaderHelper.EnableMaterialTransparency(currentMaterial);
                }

                //flip texture if this is a dds
                if(Path.GetExtension(texturePath).ToLower() == ".dds")
                {
                    currentMaterial.mainTextureScale = new Vector2(1f, -1f);
                }

                continue;
            }

            //bump map
            if ((splitLine[0] == "map_Bump" || splitLine[0] == "map_bump") && (ShaderDetector.PropName("_BumpMap")!=""))
            {
                string texturePath = GetTexPathFromMapStatement(processedLine, splitLine);
                if(texturePath == null)
                {
                    continue; //invalid args or sth
                }

                UnityEngine.Debug.Log("Loading Bump '" + texturePath + "'");

                var bumpTexture = TryLoadTexture(texturePath, true);
                float bumpScale = GetArgValue(splitLine, "-bm", 1.0f);

                if (bumpTexture != null) {
                    currentMaterial.SetTexture(ShaderDetector.PropName("_BumpMap"), bumpTexture);
                    if (ShaderDetector.PropName("_BumpMap") != "") { currentMaterial.SetFloat(ShaderDetector.PropName("_BumpScale"), bumpScale); }
                    if (ShaderDetector.PropName("_NORMALMAP") != "") { currentMaterial.EnableKeyword(ShaderDetector.PropName("_NORMALMAP")); }
                }

                continue;
            }

            //specular color
            if ((splitLine[0] == "Ks" || splitLine[0] == "ks") && (ShaderDetector.PropName("_SpecColor")!=""))
            {
                currentMaterial.SetColor(ShaderDetector.PropName("_SpecColor"), OBJLoaderHelper.ColorFromStrArray(splitLine));
                continue;
            }

            //emission color
            if ((splitLine[0] == "Ka" || splitLine[0] == "ka") && (ShaderDetector.PropName("_EmissionColor")!=""))
            {
                currentMaterial.SetColor(ShaderDetector.PropName("_EmissionColor"), OBJLoaderHelper.ColorFromStrArray(splitLine, 0.05f));
                currentMaterial.EnableKeyword(ShaderDetector.PropName("_EMISSION"));
                continue;
            }

            //emission map
            if ((splitLine[0] == "map_Ka" || splitLine[0] == "map_ka") && (ShaderDetector.PropName("_EmissionMap")!=""))
            {
                string texturePath = GetTexPathFromMapStatement(processedLine, splitLine);
                if(texturePath == null)
                {
                    continue; //invalid args or sth
                }

                currentMaterial.SetTexture(ShaderDetector.PropName("_EmissionMap"), TryLoadTexture(texturePath));
                continue;
            }

            //alpha
            if ((splitLine[0] == "d" || splitLine[0] == "Tr") && (ShaderDetector.PropName("_Color")!=""))
            {
                float visibility = OBJLoaderHelper.FastFloatParse(splitLine[1]);
                
                //tr statement is just d inverted
                if(splitLine[0] == "Tr")
                    visibility = 1f - visibility;  

                if(visibility < (1f - Mathf.Epsilon))
                {
                    var currentColor = currentMaterial.GetColor(ShaderDetector.PropName("_Color"));

                    currentColor.a = visibility;
                    currentMaterial.SetColor(ShaderDetector.PropName("_Color"), currentColor);

                    OBJLoaderHelper.EnableMaterialTransparency(currentMaterial);
                }
                continue;
            }

            //glossiness
            if ((splitLine[0] == "Ns" || splitLine[0] == "ns") && (ShaderDetector.PropName("_Glossiness")!=""))
            {
                float Ns = OBJLoaderHelper.FastFloatParse(splitLine[1]);
                Ns = (Ns / 1000f);
                currentMaterial.SetFloat(ShaderDetector.PropName("_Glossiness"), Ns);
            }
        }

        UnityEngine.Debug.Log("Stream Processed");

        //return our dict
        return mtlDict;
    }

    /// <summary>
    /// Loads a *.mtl file
    /// </summary>
    /// <param name="path">The path to the MTL file</param>
    /// <returns>Dictionary containing loaded materials</returns>
	public Dictionary<string, Material> Load(string path)
    {
        byte[] content = LordAshes.FileAccessPlugin.File.ReadAllBytes(path);
        using (MemoryStream ms = new MemoryStream(content))
        {
            return Load(ms); //actually load
        }        
    }
}
