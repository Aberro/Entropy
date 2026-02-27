using Entropy.Common.Mods;
using System.Reflection;

[assembly: AssemblyTitle("Entropy.Processor")]
[assembly: AssemblyVersion("1.0.0.357")]
[assembly: AssemblyDescription(@"
[h1][color=#ffa500]Entropy.Processor[/color][/h1][br]
Integrated Circuit processor replacement for Stationeers. Has better performance and supports easy extensibility.[br]
[h2]Uninstall[h2][br]
To remove all Entropy mods safely, follow this instruction:[br]
[olist]
[*] Remove all Entropy mods except [b]Entropy.Common[/b][br]
[*] Load and save the game, exit[br]
[*] Remove [b]Entropy.Common[/b] mod.[br]
[/olist]
[br]
Discussion: [url=https://discord.com/channels/1370137389837717545/1476117082906296451]Discord[/url][br]
Source code: [url=https://github.com/Aberro/Entropy]Github[/url][br]
[b][color=#ff0000]WARNING:[/color][/b] This is a StationeersLaunchPad Plugin Mod. It requires Bepinex to be installed with the StationeersLaunchPad plugin.[br]
See: [url=https://github.com/StationeersLaunchPad/StationeersLaunchPad]StationeersLaunchPad Github page[/url]")]
[assembly: AssemblyMetadata(AssemblyMetadata.GameType, GameType.Both)]
[assembly: AssemblyMetadata(AssemblyMetadata.WorkshopHandle, "0")]
[assembly: AssemblyMetadata(AssemblyMetadata.Tag, Tags.Mod)]
[assembly: AssemblyMetadata(AssemblyMetadata.Tag, Tags.StationeersLaunchPad)]
[assembly: AssemblyMetadata(AssemblyMetadata.Tag, Tags.Scripting)]
[assembly: AssemblyMetadata(AssemblyMetadata.DependsOn, "ModID=\"Entropy.Common\"")]
[assembly: AssemblyMetadata(AssemblyMetadata.OrderAfter, "ModID=\"Entropy.Common\"")]

// Last processed commit: 41947de2f4730eb6f48a3fe85b00db12e0182f19
// Last processed version: 1.0.0.344
[assembly: AssemblyMetadata(AssemblyMetadata.ChangeLog, @"
	[h1]Update v1.0.0.458 to v1.0.0.344[/h1]
	[list]
		[*] Fixes to Increment-Version.ps1. Made InGameDescription assembly attribute optional, added automatic conversion from [BBCode] to Unity's TMP's markup. Added support for steam unsupported [BBCode] attributes (those are used in generated in-game description and removed from description) Added [br] tag instead of line breaks for better formatting in AssemblyInfo.cs files. Extended Entropy.Common description.
	[/list]
")]