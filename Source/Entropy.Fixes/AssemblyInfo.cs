using Entropy.Common.Mods;
using System.Reflection;

[assembly: AssemblyTitle("Entropy.Fixes")]
[assembly: AssemblyCompany("Aberro")]
[assembly: AssemblyVersion("1.0.0.342")]
[assembly: AssemblyDescription(@"
[h1][color=#ffa500]Entropy.Fixes[/color][/h1][br]
Contains various minor fixes or tweaks for Stationeers not covered by other mods.[br]
[list][br]
[*] Clear Occupancy Sensor stack memory - the sensor only fills the stack with memory, but never clears it, resulting in
 it holding stale records of inventory items or entities that are not in the room anymore. This patch clears the stack
 on each tick before the sensor fills it again to fix that.[br]
[*] Disable Advanced Tablet IC when off - the Advanced Tablet's Integrated Circuit is still active and draining power even 
 when the tablet is turned off. This patch disables the IC when the tablet is turned off to fix that.[br]
[*] Vending Machine Setting - this patch allows writing/reading the Setting value of Vending Machine corresponding to it's
 currently selected slot, which is not possible in vanilla and makes it automation very limited.[br]
[*] Smaller Particles - this patch changes the size of gas particles to specified value, which makes them look better and less obtrusive.[br]
[*] No Trails - this patch removes trails from gas particles, which makes them look better and less obtrusive.[br]
[/list][br]
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
// Last processed commit: 389837a7025f3e4fd5d147cca7e7428353f8fc59
// Last processed version: 1.0.0.458
[assembly: AssemblyMetadata(AssemblyMetadata.ChangeLog, @"
	[h1]Update v1.0.0.456 to v1.0.0.458[/h1]
	[list]
		[*] Fixed auto-version incrementing and changelog generation.
	[/list]
")]