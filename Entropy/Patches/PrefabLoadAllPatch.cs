using Assets.Scripts.Objects;
using HarmonyLib;
using Entropy.UI;

namespace Entropy.Patches;

/// <summary>
/// Patch to attach to the Prefab.LoadAll method and initialize the ImGuiHost.
/// </summary>
[HarmonyPatch(typeof(Prefab), nameof(Prefab.LoadAll))]
public class PrefabLoadAllPatch
{
	private static void Postfix()
	{
		ImGuiHost.Init();
		ImGuiHost.Instance.GetOrAddComponent<ModSettings>();
	}
}