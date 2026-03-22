using Entropy.Common.Mods;
using System.Reflection;

[assembly: AssemblyTitle("Entropy.Processor")]
[assembly: AssemblyVersion("1.0.0.491")]
[assembly: AssemblyDescription(@"
[h1][color=#ffa500]Entropy.Processor[/color][/h1][br]
Integrated Circuit processor replacement for Stationeers. Has better performance and supports easy extensibility.[br]
[h2]Uninstall[/h2][br]
To remove all Entropy mods safely, follow this instruction:[br]
[olist]
[*] Remove all Entropy mods except [b]Entropy.Common[/b][br]
[*] Load and save the game, exit[br]
[*] Remove [b]Entropy.Common[/b] mod.[br]
[/olist][br]
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

// Last processed commit: a29441aee2a5f3ce15d7fd3c24d8a51d4fd38258
// Last processed version: 1.0.0.490
[assembly: AssemblyMetadata(AssemblyMetadata.ChangeLog, @"
	[h1]Update v1.0.0.489 to v1.0.0.490[/h1]
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