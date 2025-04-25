using Assets.Scripts.Networks;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using Entropy.Scripts.Utilities;
using HarmonyLib;
using ImGuiNET.Unity;
using UnityEngine;
using Entropy.Scripts.UI;
using Object = UnityEngine.Object;
using Entropy.Scripts.Cartridges;

namespace Entropy.Scripts.Patches;

[HarmonyPatch(typeof(Prefab), nameof(Prefab.LoadAll))]
public class PrefabLoadAllPatch
{
	public static void Postfix()
	{
		if(Plugin.Config.Features[PatchCategory.ConfigCartridgeDebug].Value)
			PrepareDebugCartridge();
		PrepareImGui();
	}

	private static void PrepareImGui()
	{
		var prefab = AssetsManager.LoadAsset<GameObject>("Assets/Prefabs/ImGui.prefab");
		var component = prefab?.GetComponent<DearImGui>();
		if(!prefab.IsValid() || !component.IsValid())
			throw new ApplicationException("Could not find ImGui.prefab asset!");

		var fontAtlasConfigAsset = Resources.FindObjectsOfTypeAll<FontAtlasConfigAsset>().FirstOrDefault();
		var cursorShapesAsset = Resources.FindObjectsOfTypeAll<CursorShapesAsset>().FirstOrDefault();
		var shaderResourcesAsset = Resources.FindObjectsOfTypeAll<ShaderResourcesAsset>().FirstOrDefault();

		var imGuiTraverse = new Traverse(component);
		// Modify the preset before instantiating to reuse default resources from the dll.
		if(fontAtlasConfigAsset != null)
			imGuiTraverse.Field("_fontAtlasConfiguration")?.SetValue(fontAtlasConfigAsset);
		if(cursorShapesAsset != null)
			imGuiTraverse.Field("_cursorShapes")?.SetValue(cursorShapesAsset);
		if(shaderResourcesAsset != null)
			imGuiTraverse.Field("_shaders")?.SetValue(shaderResourcesAsset);

		var instance = Object.Instantiate(prefab)!;
		instance.name = prefab.name;

		Object.DontDestroyOnLoad(instance);
		instance.AddComponent<ModSettings>();
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

		var prefab = AssetsManager.LoadAsset<GameObject>("Assets/Prefabs/CartridgeDebug.prefab");
		var component = prefab.GetComponent<DebugCartridge>();
		component.PrefabHash = Animator.StringToHash(component.PrefabName);
		Prefab.AllPrefabs.Add(component);
		WorldManager.Instance.SourcePrefabs.Add(component);
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