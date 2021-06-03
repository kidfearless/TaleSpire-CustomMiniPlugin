# Custom Mini Plugin

This unofficial TaleSpire plugin is for adding an unlimited number of custom minis.
Re-applies transformation automatically on re-load and requires no blank base.
Now supports assetBundles and assetBundle animations.

Somewhat Outdated Demo Video: https://youtu.be/sRYln7Gc6Dg

Adding OBJ/MTL Content Video: https://youtu.be/JJ0xJQUM01U

(Adding AssetBundle content is very similar to adding OBJ/MTL content)

## Change Log

3.0.0: Added assetBundle support
3.0.0: Added animation support
2.0.0: Blank base is no longer needed
2.0.0: Transformation are automatically restored on loaded
2.0.0: Trasformation triggered using CTRL+M (can be changed in R2ModMan)
2.0.0: Moved from Chat distribution to Name distribution
1.6.1: Exposed Plugin Guid to allow it to be marked as a dependency
1.6.0: OBJ, MTL and texture files expected in a minis folder and then a sub-folder named after the asset (e.g. TaleSpire_CustomData\Minis\Wizard\Wizard.obj)
1.6.0: Added fake JPG and PNG support. Asset can use JPG/PNG textures which the plugin automatically converts to BMPs.
1.5.0: Fixed shader bug. Content uses the Standard shader (not the TaleSpire\Creature shader).
1.5.0: OBJ, MTL and texture files expected in a sub folder named after the asset (e.g. TaleSpire_CustomData\Wizard\Wizard.obj)
1.5.0: Includes the TaleSpire_CustomData folder in ZIP along with a Test content

## Install

Install using R2ModMan or similar and place custom contents (OBJ/MTL and texture files or AssetBundles) in TaleSpire_CustomData\Minis\{ContentName}\

## Usage

Add a mini to the board and select it. To transform the mini, press the Transform shotcut (CTRL+M by default but can be changed in
R2ModMan configuration for the plugin). Enter the name of the content to which the mini should be transformed. Ensure that the entered
content name corresponds to an OBJ and MTL file or a assetBundle file in the location TaleSpire_CustomData\Minis\{ContentName}\

For example:

TaleSpire_CustomData\Minis\Wizard01\Wizard01.OBJ
TaleSpire_CustomData\Minis\Wizard01\Wizard01.MTL

or for an assetBundle:

TaleSpire_CustomData\Minis\Wizard01\Wizard01

Transformations are automatically loaded when the board is loaded as long as the client has the corresponding content files.

## Adding Custom Content

### OBJ/MTL Content

Each piece of custom content needs to consist of a OBJ file and MTL file. It can, optionally also contain texture files, which
should be in BMP format. PNG and JPG can now be used but will be automatically converted to BMP by the plugin. References to files
should be relative and in the same directory (i.e. don't use full paths for texture file names). The name of OBJ file and MTL file
should be the same except for the extension. The texture files can have any name (e.g. if used by multiple models). The content
file name is the name that is used to access it in TaleSpire. For example, the content Warlock.OBJ and Warlock.MTL would be
accessed by Warlock (without the extension).

OBJ, MTL and texture files are to be placed in \Steam\steamapps\common\TaleSpire\TaleSpire_CustomData\Minis\{ContentName}\

Since the custom content files are only read when they are needed, the content can be added, removed or modified while TaleSpire
is running. This allows easy testing of custom models.

### AssetBundle Content

Place the assetBundle file (with no extension) into a folder with the same name as the content, as in:

\Steam\steamapps\common\TaleSpire\TaleSpire_CustomData\Minis\{ContentName}\

The folder, assetBundle file and the content within it should all have the same name. For example:

\Steam\steamapps\common\TaleSpire\TaleSpire_CustomData\Minis\Wizard01\Wizard01 should contain a Wizard01 prefab.

## Animations & Poses

AssetBundles can contain animations. Custom Mini Plugin allows triggering of up to 5 different animations if the asset contains
them. It should be noted that while the names of the animations are configurable (in the R2ModMon config for the plugin) they
are common to all assets. For example, if the animation names are configured for Idle, Ready, Attack, Dance and Die then these
animation names are use for all assets. They shortcut keys for triggering the animations are configurable but default to
(LEFT)CTRL+4 to (LEFT)CTRL+8. The triggering keys work like a toggle. If the animation is not playing, it will start. If an
animation is already playing it will be stopped. If an asset does not have the corresponding animation, no animation is played.

Poses can be implemented by short (typically one frame) animations. It is highly recommended to add a default or Idle pose
because when animations are stopped the asset is not returned back to an idle or non-animated state. The asset remains in the
position that the asset was in when the animation was stopped. By having an idle pose, the asset can be returned to this pose
(manually).

## Limitations

The fly and some of the emotions don't work with the Custom Mini Plugin. Fly will show the fly stand but the mini will disapper while
fly mode is on. Some of the Emotions do not animate.
