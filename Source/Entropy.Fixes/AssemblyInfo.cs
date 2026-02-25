using Entropy.Common.Mods;
using System.Reflection;

[assembly: AssemblyTitle("Entropy.Fixes")]
[assembly: AssemblyCompany("Aberro")]
[assembly: AssemblyVersion("1.0.0.302")]
[assembly: AssemblyDescription(@"
		[h1]Entropy.Fixes[/h1]

		Contains various minor fixes or tweaks for Stationeers not covered by other mods.
		*	Clear Occupancy Sensor stack memory - the sensor only fills the stack with memory, but never clears it, resulting in
			it holding stale records of inventory items or entities that are not in the room anymore. This patch clears the stack
			on each tick before the sensor fills it again to fix that.
		*	Disable Advanced Tablet IC when off - the Advanced Tablet's Integrated Circuit is still active and draining power even 
			when the tablet is turned off. This patch disables the IC when the tablet is turned off to fix that.
		*	Vending Machine Setting - this patch allows writing/reading the Setting value of Vending Machine corresponding to it's
			currently selected slot, which is not possible in vanilla and makes it automation very limited.
		*	Smaller Particles - this patch changes the size of gas particles to specified value, which makes them look better and less obtrusive.
		*	No Trails - this patch removes trails from gas particles, which makes them look better and less obtrusive.

		[b]WARNING:[/b] This is a StationeersLaunchPad Plugin Mod. It requires Bepinex to be installed with the StationeersLaunchPad plugin.
		See: https://github.com/StationeersLaunchPad/StationeersLaunchPad")]
[assembly: AssemblyMetadata("InGameDescription", @"<![CDATA[<align=""center""><size=""30""><b><color=#ffa500>Entropy.Fixes</color></b></size></align>
Contains various minor fixes or tweaks for Stationeers not covered by other mods.
*<indent=5%><b>Clear Occupancy Sensor stack memory</b> - the sensor only fills the stack with memory, but never clears it, resulting in it holding stale records of inventory items or entities that are not in the room anymore. This patch clears the stack on each tick before the sensor fills it again to fix that.</indent>
*<indent=5%><b>Disable Advanced Tablet IC when off</b> - the Advanced Tablet's Integrated Circuit is still active and draining power even when the tablet is turned off. This patch disables the IC when the tablet is turned off to fix that.</indent>
*<indent=5%><b>Vending Machine Setting</b> - this patch allows writing/reading the Setting value of Vending Machine corresponding to it's currently selected slot, which is not possible in vanilla and makes it automation very limited.</indent>
*<indent=5%><b>Smaller Particles</b> - this patch changes the size of gas particles to specified value, which makes them look better and less obtrusive.</indent>
*<indent=5%><b>No Trails</b> - this patch removes trails from gas particles, which makes them look better and less obtrusive.</indent>

<color=#ff0000><b>WARNING:</b></color> This is a <b>StationeersLaunchPad</b> Plugin Mod. It requires <b>Bepinex</b> to be installed with the <b>StationeersLaunchPad</b> plugin.
See: <link=""https://github.com/StationeersLaunchPad/StationeersLaunchPad"">https://github.com/StationeersLaunchPad/StationeersLaunchPad</link>
]]>")]
[assembly: AssemblyMetadata("GameType", GameType.Both)]
[assembly: AssemblyMetadata("WorkshopHandle", "0")]
[assembly: AssemblyMetadata("Tags", Tags.Mod)]
[assembly: AssemblyMetadata("Tags", Tags.StationeersLaunchPad)]
[assembly: AssemblyMetadata("DependsOn", "ModID=\"Entropy.Common\"")]
[assembly: AssemblyMetadata("OrderAfter", "ModID=\"Entropy.Common\"")]
// Last processed commit: 
// Last processed version: 1.0.0.0
[assembly: AssemblyMetadata("ChangeLog", "")]