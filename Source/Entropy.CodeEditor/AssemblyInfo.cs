using Entropy.Common.Mods;
using System.Reflection;

[assembly: AssemblyTitle("Entropy.CodeEditor")]
[assembly: AssemblyCompany("Aberro")]
[assembly: AssemblyVersion("1.0.0.481")]
[assembly: AssemblyDescription(@"
[h1][color=#ffa500]Entropy.CodeEditor[/color][/h1][br]
Advanced source code editor for programmable chips in Stationeers.[br]
Supports syntax highlighting, built-in color themes, clipboard, color theme customization,
 importing/exporting code from the device or network, auto-pausing the game, and it's much more performant than default code editor.[br]
[h2]Uninstall[/h2][br]
To remove this mod safely, follow this instruction:[br]
[olist]
[*] Remove only this mod (do not remove [b]Entropy.Common[/b])[br]
[*] Load and save the game[br]
[*] If you don't use other Entropy mods - you may remove [b]Entropy.Common[/b] mod.[br]
[/olist][br]
[br]
Discussion: [url=https://discord.com/channels/1370137389837717545/1476117082906296451]Discord[/url][br]
Source code: [url=https://github.com/Aberro/Entropy]Github[/url][br]
[b][color=#ff0000]WARNING:[/color][/b] This is a StationeersLaunchPad Plugin Mod. It requires Bepinex to be installed with the StationeersLaunchPad plugin.[br]
See: [url=https://github.com/StationeersLaunchPad/StationeersLaunchPad]StationeersLaunchPad Github page[/url]")]
[assembly: AssemblyMetadata(AssemblyMetadata.GameType, GameType.Client)]
[assembly: AssemblyMetadata(AssemblyMetadata.WorkshopHandle, "3673139847")]
[assembly: AssemblyMetadata(AssemblyMetadata.Tag, Tags.Mod)]
[assembly: AssemblyMetadata(AssemblyMetadata.Tag, Tags.StationeersLaunchPad)]
[assembly: AssemblyMetadata(AssemblyMetadata.Tag, Tags.UI)]
[assembly: AssemblyMetadata(AssemblyMetadata.Tag, Tags.Scripting)]
[assembly: AssemblyMetadata(AssemblyMetadata.Tag, Tags.QualityOfLife)]
[assembly: AssemblyMetadata(AssemblyMetadata.DependsOn, "ModID=\"Entropy.Common\"")]
[assembly: AssemblyMetadata(AssemblyMetadata.OrderAfter, "ModID=\"Entropy.Common\"")]

////////////////////////////////////
// Code-generated: do not modify. //
////////////////////////////////////
// Last processed commit: c1b2fee7f4fa2ca5d281897a634498e130300c40
// Last processed version: 1.0.0.479
[assembly: AssemblyMetadata(AssemblyMetadata.ChangeLog, @"
	[h1]Update v1.0.0.438 to v1.0.0.479[/h1]
	[list]
		[*] Attempt to fix passive vents/passive equalization.
	[/list]
")]