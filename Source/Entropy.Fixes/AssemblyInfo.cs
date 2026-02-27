using Entropy.Common.Mods;
using System.Reflection;

[assembly: AssemblyTitle("Entropy.Fixes")]
[assembly: AssemblyCompany("Aberro")]
[assembly: AssemblyVersion("1.0.0.356")]
[assembly: AssemblyDescription(@"
[h1][color=#ffa500]Entropy.Fixes[/color][/h1][br]
Contains various minor fixes or tweaks for Stationeers not covered by other mods.[br]
[list][br]
[*] [b]Clear Occupancy Sensor stack memory[/b] - the sensor only fills the stack with memory, but never clears it, resulting in
 it holding stale records of inventory items or entities that are not in the room anymore. This patch clears the stack
 on each tick before the sensor fills it again to fix that.[br]
[*] [b]Disable Advanced Tablet IC when off[/b] - the Advanced Tablet's Integrated Circuit is still active and draining power even 
 when the tablet is turned off. This patch disables the IC when the tablet is turned off to fix that.[br]
[*] [b]Vending Machine Setting[/b] - this patch allows writing/reading the Setting value of Vending Machine corresponding to it's
 currently selected slot, which is not possible in vanilla and makes it automation very limited.[br]
[*] [b]Smaller Particles[/b] - this patch changes the size of gas particles to specified value, which makes them look better and less obtrusive.[br]
[*] [b]No Trails[/b] - this patch removes trails from gas particles, which makes them look better and less obtrusive.[br]
[/list][br]
[h2]Uninstall[h2][br]
To remove this mod safely, follow this instruction:[br]
[olist]
[*] Remove only this mod (do not remove [b]Entropy.Common[/b])[br]
[*] Load and save the game[br]
[*] If you don't use other Entropy mods - you may remove [b]Entropy.Common[/b] mod.[br]
[/olist]
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
// Last processed commit: 41947de2f4730eb6f48a3fe85b00db12e0182f19
// Last processed version: 1.0.0.343
[assembly: AssemblyMetadata(AssemblyMetadata.ChangeLog, @"
	[h1]Update v1.0.0.458 to v1.0.0.343[/h1]
	[list]
		[*] Fixes to Increment-Version.ps1. Made InGameDescription assembly attribute optional, added automatic conversion from [BBCode] to Unity's TMP's markup. Added support for steam unsupported [BBCode] attributes (those are used in generated in-game description and removed from description) Added [br] tag instead of line breaks for better formatting in AssemblyInfo.cs files. Extended Entropy.Common description.
	[/list]
")]