using Entropy.Common.Mods;
using System.Reflection;

[assembly: AssemblyTitle("Entropy.Common")]
[assembly: AssemblyCompany("Aberro")]
[assembly: AssemblyVersion("1.0.0.662")]
[assembly: AssemblyDescription(@"[center][h1][color=#ffa500]Entropy.Common[/color][/h1][/center][br]
A Stationeers modding framework and utility library.
 It contains common code and utilities for Entropy mods, automates configuration handling, provides easy API for drawing ImGUI controls,
 has various helper types, and more, allowing for faster and easier development of mods.[br]
[h2]List of features:[/h2][br]
[list]
[*] Automatic About.xml generation based on assembly metadata, see Template in source code for example[br]
[*] Shared Steam and in-game description based on custom [noparse][BBCode][/noparse] flavor,
see Template and AboutGenerator.Program.cs in source code[br]
[*] Patch management - patch/unpatch on the fly[br]
[*] Configuration entries management[br]
[*] Objects extensions - additional data of any kind that can be assigned to existing reference types[br]
[*] Serializable extensions - any serializable extension data attached to an instance of a type derived from Thing[br]
[*] ImGui extra features, convenience methods, and more native types mapped into C#[br]
[*] Extra easy mod entry point declaration - just a class derived from EntryMod, nothing else is required[br]
[*] Optional[noparse]<>[/noparse] type - same as Nullable[noparse]<>[/noparse], but works both for reference and value types[br]
[*] Color converters extension methods[br]
[*] ImGui transparency fix - multiple transparent drawcalls now blend more naturally,
 instead of the topmost overriding transparency of underlaying graphics[br]
[*] Patching validation - ensures that patch is applied only to specific method version by PatchValidateCrc attribute.
[/list][br]
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
[b][color=#ff0000]WARNING:[/color][/b] 
This is a StationeersLaunchPad Plugin Mod. It requires Bepinex to be installed with the StationeersLaunchPad plugin.[br]
See: [url=https://github.com/StationeersLaunchPad/StationeersLaunchPad]StationeersLaunchPad Github page[/url]")]
[assembly: AssemblyMetadata(AssemblyMetadata.GameType, "Both")]
[assembly: AssemblyMetadata(AssemblyMetadata.WorkshopHandle, "3673138658")]
[assembly: AssemblyMetadata(AssemblyMetadata.Tag, Tags.Mod)]
[assembly: AssemblyMetadata(AssemblyMetadata.Tag, Tags.StationeersLaunchPad)]
[assembly: AssemblyMetadata(AssemblyMetadata.Tag, Tags.Library)]

////////////////////////////////////
// Code-generated: do not modify. //
////////////////////////////////////
// Last processed commit: a29441aee2a5f3ce15d7fd3c24d8a51d4fd38258
// Last processed version: 1.0.0.661
[assembly: AssemblyMetadata(AssemblyMetadata.ChangeLog, @"
	[h1]Update v1.0.0.660 to v1.0.0.661[/h1]
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