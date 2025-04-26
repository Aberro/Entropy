using UnityEngine;
using Assets.Scripts.Networks;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using ImGuiNET.Unity;
using HarmonyLib;
using Entropy.Scripts.Cartridges;
using Entropy.Scripts.UI;
using Entropy.Scripts.Utilities;
using Object = UnityEngine.Object;
using UnityEngine.UI;

namespace Entropy.Scripts.Patches;

[HarmonyPatch(typeof(Prefab), nameof(Prefab.LoadAll))]
public class PrefabLoadAllPatch
{
	public static void Postfix()
	{
		ImGuiHost.Init();
		ImGuiHost.Instance.GetOrAddComponent<ModSettings>();
		if(EntropyPlugin.Config.Features[PatchCategory.ConfigCartridgeDebug].Value)
			PrepareDebugCartridge();
	}

	private static void PrepareDebugCartridge()
	{
		// First, find the cartridge prefab from which we're going to make a copy and make a copy of it.
		var prototype = WorldManager.Instance.SourcePrefabs.FirstOrDefault(p => p.name == "CartridgeConfiguration");
		var copy = PrefabCopier.CopyGameObject(prototype.GameObject);
		// Then, find the blueprint prefab, copy it as well.
		var blueprint = prototype.gameObject.GetComponent<ConfigCartridge>().Blueprint;
		var copyBlueprint = PrefabCopier.CopyGameObject(blueprint.gameObject);
		// Update the blueprint in the copy.
		copy.GetComponent<ConfigCartridge>().Blueprint = copyBlueprint;
		// Replace the cartridge with our component.
		copy.AddComponent<DebugCartridge>();
		Object.DestroyImmediate(copy.GetComponent<ConfigCartridge>());

		// Finally, add our copies to game prefabs.
		Prefab.AllPrefabs.Add(copy.GetComponent<ConfigCartridge>());
		Cartridge.AllCartridgePrefabs.Add(copy.GetComponent<ConfigCartridge>());
		WorldManager.Instance.SourcePrefabs.Add(copy.GetComponent<ConfigCartridge>());
	}
}

[HarmonyPatchCategory(PatchCategory.ConfigCartridgeDebug)]
[HarmonyPatch(typeof(ElectricityManager), nameof(ElectricityManager.ElectricityTick))]
public static class ElectricityManager_ElectricityTick_Patch
{
	public static void Postfix()
	{
		foreach (var cartridge in DebugCartridge.AllDebugCartridges)
			if (cartridge.Tablet && cartridge.Tablet.OnOff && cartridge.Tablet.Powered)
				cartridge.ReadLogicText();
	}
}