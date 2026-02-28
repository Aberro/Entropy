using Entropy.Common.Mods;
using System.Reflection;

[assembly: AssemblyTitle("Entropy.Adiabatics")]
[assembly: AssemblyCompany("Aberro")]
[assembly: AssemblyVersion("1.0.0.381")]
[assembly: AssemblyDescription(@"
[h1][color=#ffa500]Entropy.Adiabatics[/color][/h1][br]
A realistic atmospheric overhaul that accounts for adiabatic processes.
 Compressed gasses heat up, expanded gases cool down, based on physical equations.[br]
Worth noting, though, that not every expanding gas cools down nor should cool down - only a gas that performs some work does.
 I.e. gas that pushes something. Gas that escapes through a ruptured pipe or a vent into lower pressure does not cool down as it does not
 perform any work.[br]
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
[assembly: AssemblyMetadata(AssemblyMetadata.GameType, GameType.Both)]
[assembly: AssemblyMetadata(AssemblyMetadata.WorkshopHandle, "3673139243")]
[assembly: AssemblyMetadata(AssemblyMetadata.Tag, Tags.Mod)]
[assembly: AssemblyMetadata(AssemblyMetadata.Tag, Tags.StationeersLaunchPad)]
[assembly: AssemblyMetadata(AssemblyMetadata.DependsOn, "ModID=\"Entropy.Common\"")]
[assembly: AssemblyMetadata(AssemblyMetadata.OrderAfter, "ModID=\"Entropy.Common\"")]
// Last processed commit: c0edee4aea26a065630eb0965049bd7bd5ad80ac
// Last processed version: 1.0.0.361
[assembly: AssemblyMetadata(AssemblyMetadata.ChangeLog, @"
	[h1]Update v1.0.0.357 to v1.0.0.361[/h1]
	[list]
		[*] Minor corrections in AssemblyInfo.cs.
	[/list]
")]