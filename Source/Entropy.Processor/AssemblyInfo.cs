using Entropy.Common.Mods;
using System.Reflection;

[assembly: AssemblyTitle("Entropy.Processor")]
[assembly: AssemblyVersion("1.0.0.343")]
[assembly: AssemblyDescription(@"
[h1][color=#ffa500]Entropy.Processor[/color][/h1][br]
Integrated Circuit processor replacement for Stationeers. Has better performance and supports easy extensibility.[br]
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

// Last processed commit: 389837a7025f3e4fd5d147cca7e7428353f8fc59
// Last processed version: 1.0.0.458
[assembly: AssemblyMetadata(AssemblyMetadata.ChangeLog, @"
	[h1]Update v1.0.0.456 to v1.0.0.458[/h1]
	[list]
		[*] Fixed auto-version incrementing and changelog generation.
	[/list]
")]