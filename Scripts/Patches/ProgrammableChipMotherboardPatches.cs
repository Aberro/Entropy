using Assets.Scripts.Objects.Motherboards;
using HarmonyLib;
using ImGuiNET;
using ImGuiNET.Unity;

namespace Entropy.Assets.Scripts.Assets.Scripts.Patches
{
	[HarmonyPatch(typeof(ProgrammableChipMotherboard), nameof(ProgrammableChipMotherboard.OnEdit))]
	[HarmonyPatchCategory(PatchCategory.ProgrammableChipPatches)]
	public class ProgrammableChipMotherboard_OnEdit_Patch
	{
		public static bool Prefix(ProgrammableChipMotherboard __instance)
		{
			var codeEditor = __instance.gameObject.GetComponent<SourceCodeEditor>();
			if(!codeEditor)
				codeEditor = __instance.gameObject.AddComponent<SourceCodeEditor>();
			codeEditor.ShowTextEditor();
			return false;
		}
	}
}
