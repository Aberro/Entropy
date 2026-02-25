using Assets.Scripts.Objects.Motherboards;
using Entropy.Common.Utils;
using HarmonyLib;

namespace Entropy.CodeEditor;

[HarmonyPatch]
public static class Patches
{
	[HarmonyPatch(typeof(ProgrammableChipMotherboard), nameof(ProgrammableChipMotherboard.OnEdit))]
	[HarmonyPrefix]
	public static bool ProgrammableChipMotherboardOnEditPrefix(ProgrammableChipMotherboard __instance)
	{
		ArgumentNullException.ThrowIfNull(__instance);
		SourceCodeEditor.Open(__instance);
		return false;
	}
}
