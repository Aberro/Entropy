using Entropy.Common.Mods;
using System.Reflection;

[assembly: AssemblyTitle("Entropy.Common")]
[assembly: AssemblyCompany("Aberro")]
[assembly: AssemblyVersion("1.0.0.510")]
[assembly: AssemblyDescription(@"[center][h1][color=#ffa500]Entropy.Common[/color][/h1][/center][br]
A Stationeers modding framework and utility library.
 It contains common code and utilities for Entropy mods, automates configuration handling, provides easy API for drawing ImGUI controls,
 has various helper types, and more, allowing for faster and easier development of mods.[br]
[h2]List of features:[h2][br]
[list]
[*] Automatic About.xml generation based on assembly metadata, see Template in source code for example.[br]
[*] Shared Steam and in-game description based on custom [noparse][BBCode][/noparse] flavor,
see Template and AboutGenerator.Program.cs in source code.[br]
[*] patch management - patch/unpatch on the fly[br]
[*] configuration entries management[br]
[*] objects extensions - additional data of any kind that can be assigned to existing reference types[br]
[*] serializable extensions - any serializable extension data attached to an instance of a type derived from Thing[br]
[*] ImGui extra features, convenience methods, and more native types mapped into C#[br]
[*] Extra easy mod entry point declaration - just a class derived from EntryMod, nothing else is required.[br]
[*] Optional[noparse]<>[/noparse] type - same as Nullable[noparse]<>[/noparse], but works both for reference and value types.[br]
[*] Color converters extension methods[br]
[*] ImGui transparency fix - multiple transparent drawcalls now blend more naturally,
 instead of the topmost overriding transparency of underlaying graphics.[br]
[/list]
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
// Last processed commit: 41947de2f4730eb6f48a3fe85b00db12e0182f19
// Last processed version: 1.0.0.489
[assembly: AssemblyMetadata(AssemblyMetadata.ChangeLog, @"
	[h1]Update v1.0.0.458 to v1.0.0.489[/h1]
	[list]
		[*] Fixes to Increment-Version.ps1. Made InGameDescription assembly attribute optional, added automatic conversion from [BBCode] to Unity's TMP's markup. Added support for steam unsupported [BBCode] attributes (those are used in generated in-game description and removed from description) Added [br] tag instead of line breaks for better formatting in AssemblyInfo.cs files. Extended Entropy.Common description.
	[/list]
")]