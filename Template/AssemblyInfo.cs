using Entropy.Common.Mods;
using System.Reflection;

// This file contains mod description that will be used to generate About.xml file.
// ModID is the assembly name itself.
// Mod display name.
[assembly: AssemblyTitle("Template [StationeersLaunchPad]")]
// Mod version. The minor version is going to be automatically incremented on each build.
[assembly: AssemblyVersion("1.0.0.0")]
// The mod description, formatting is BBCode.
// You could use [color] tags here, but it will be removed due to steam not supporting it.
// But it's still useful when InGameDescription metadata is not provided.
[assembly: AssemblyDescription(@"
[h1][color=#ff0000]Template[/color][/h1][br]
[b][color=#ff0000]WARNING:[/color][/b] This is a StationeersLaunchPad Plugin Mod. It requires Bepinex to be installed with the StationeersLaunchPad plugin.[br]
See: https://github.com/StationeersLaunchPad/StationeersLaunchPad[br]
This is a template mod for demonstration.")]

// This attribute is optional - in it's absense, the AboutGenerator utility would generate InGameDescription and translate from BBCode to TMP formatting.
// With provided AssemblyDescription, the resulting InGameDescription should be same as in this commented metadata attribute:
/*[assembly: AssemblyMetadata(AssemblyMetadata.InGameDescription, @"
<![CDATA[<size=""30""><color=#ffa500>Template mod</color></size>
<b><color=#ff0000>WARNING:</color></b> This is a StationeersLaunchPad Plugin Mod. It requires Bepinex to be installed with the StationeersLaunchPad plugin.
See: https://github.com/StationeersLaunchPad/StationeersLaunchPad

This is a template mod for demonstration.]]>")]*/
// All supported AssemblyMetadata key values are listed in AssemblyMetadata class, use them instead of string literals.
// GameType constants are in GameType class
[assembly: AssemblyMetadata(AssemblyMetadata.GameType, GameType.Both)]
[assembly: AssemblyMetadata(AssemblyMetadata.WorkshopHandle, "0")]
// Some most common tags are included into Tags static class:
[assembly: AssemblyMetadata(AssemblyMetadata.Tag, Tags.Mod)]
[assembly: AssemblyMetadata(AssemblyMetadata.Tag, Tags.Scripting)]
[assembly: AssemblyMetadata(AssemblyMetadata.Tag, Tags.StationeersLaunchPad)]
// This is a requirement for mods based on this framework
[assembly: AssemblyMetadata(AssemblyMetadata.DependsOn, "ModID=\"Entropy.Common\"")]
[assembly: AssemblyMetadata(AssemblyMetadata.OrderAfter, "ModID=\"Entropy.Common\"")]
// Not yet supported...
[assembly: AssemblyMetadata(AssemblyMetadata.IncompatibleWith, "ModID=\"123456789\" Version=\"1.2.3\"")]
// This code will be automatically generated on each build, so better not to touch it:

////////////////////////////////////
// Code-generated: do not modify. //
////////////////////////////////////
// Last processed commit: a38333a4f8520a2cce1e7010b6bcb1bc699151ff
// Last processed version: 1.0.0.0
[assembly: AssemblyMetadata(AssemblyMetadata.ChangeLog, "")]