using Entropy.Common.Mods;
using System.Reflection;

[assembly: AssemblyTitle("Entropy.Adiabatics")]
[assembly: AssemblyCompany("Aberro")]
[assembly: AssemblyVersion("1.0.0.490")]
[assembly: AssemblyDescription(@"
[h1][color=#ffa500]Entropy.Adiabatics[/color][/h1][br]
[h2][color=#ff0000]Experimental![/color][/h2][br]
This mod is currently in development. It works, at least it should, but untested, unbalanced, unpolished and many patches are still missing.
 Use it at your own risk! Any feedback is welcomed and appreciated.[br]
[br]
A brutally realistic atmospheric physics overhaul that accounts for adiabatic processes:
 compressing gasses heat them up, expanding cool them down, based on physical equations, mostly.
 The physics is brutal, expect ruptured pipes, freezing, exploding fuel mixture lines, etc.[br]
Pumps heat up gasses in total, i.e. the input is still going colder, but when the input and output is the same atmosphere it will heat up quickly,
 from work done by the pump on the gas.[br]
Also, active vents and powered vents consume A LOT more power now, while working much worse - because vanilla vents are unrealistic.
 To resolve that you need to keep their output pressure low by additional pumps or a turbo pump.
 Portable Scrubber is also much less efficient. But pumps now only consume power when actually doing a work, proportional to the amount of work they do.
 So, a pump pumping gas from high pressure to low pressure do not consume power.[br]
The current implementation still breaks physics and energy conservation in favor of gameplay by doing multiple iterations per tick
 while consuming power only once per tick. Otherwise grass grows faster than pressure dropping to zero in your powered vent airlock.[br]
All patches with pumps involved include configuration to change pump max power consumption, power efficiency, pump volume
 (lower volume at the same power allows for higher pressure but lower flow), gearing (geared pumps reach indefinitely high pressure, but has lower flow),
 and number of iterations.[br]
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
[assembly: AssemblyMetadata(AssemblyMetadata.WorkshopHandle, "3679285978")]
[assembly: AssemblyMetadata(AssemblyMetadata.Tag, Tags.Mod)]
[assembly: AssemblyMetadata(AssemblyMetadata.Tag, Tags.StationeersLaunchPad)]
[assembly: AssemblyMetadata(AssemblyMetadata.DependsOn, "ModID=\"Entropy.Common\"")]
[assembly: AssemblyMetadata(AssemblyMetadata.OrderAfter, "ModID=\"Entropy.Common\"")]
// Last processed commit: a29441aee2a5f3ce15d7fd3c24d8a51d4fd38258
// Last processed version: 1.0.0.489
[assembly: AssemblyMetadata(AssemblyMetadata.ChangeLog, @"
	[h1]Update v1.0.0.488 to v1.0.0.489[/h1]
	[list]
		[*] 0caeb74: Added multiline git log comments support to Increment-Version.ps1
		[*] 614c73a: Entropy.Common:
 HarmonyPatchInfo.cs:
 * Fixed formatting in logging for CRC values.
 Patches.cs:
 * Updated SLP patching to exclude Entropy.Common from the list of ""offenders"".
Entropy.Adiabatics:
 Patches.cs:
 * Updated patches CRCs, added decompiled sources to facilitate faster check for updates.
 * Removed LiquidRocketEnginePatches - liquid rocket engine should not be applicable for adiabatic process.
	[/list]
")]