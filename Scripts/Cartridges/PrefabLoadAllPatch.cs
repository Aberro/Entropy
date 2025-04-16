using Assets.Scripts.Networks;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using Entropy.Scripts.Utilities;
using HarmonyLib;
using System.Linq;
using ImGuiNET.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ImGuiNET;
using Entropy.Scripts.UI;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Entropy.Scripts.Cartridges
{
    [HarmonyPatch(typeof(Prefab), nameof(Prefab.LoadAll))]
    public class PrefabLoadAllPatch
    {
        public static void Prefix()
        {
			PrepareCartridgeDebug();
			PrepareImGui();
        }

		private static void PrepareImGui()
		{
			var prefab = AssetsManager.LoadAsset<GameObject>("Assets/Prefabs/ImGui.prefab");
			var component = prefab.GetComponent<DearImGui>();

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

			var instance = Object.Instantiate(prefab);
			instance.name = prefab.name;

			Object.DontDestroyOnLoad(instance);
			instance.AddComponent<ModSettings>();
		}

        private static void PrepareCartridgeDebug()
		{
			var prefab = AssetsManager.LoadAsset<GameObject>("Assets/Prefabs/CartridgeDebug.prefab");
			var component = prefab.GetComponent<DebugCartridge>();
			component.PrefabHash = Animator.StringToHash(component.PrefabName);
			Prefab.AllPrefabs.Add(component);
			WorldManager.Instance.SourcePrefabs.Add(component);

			var prototype = WorldManager.Instance.SourcePrefabs.FirstOrDefault(p => p.name == "CartridgeConfiguration");
			//var prefab = WorldManager.Instance.SourcePrefabs.FirstOrDefault(p => p.name == "CartridgeDebug");
			if (prototype == null || prefab == null)
			{
				Plugin.LogError("CartridgeConfiguration or CartridgeDebug prefab not found!");
				return;
			}
			var blueprint = component.Blueprint;
			PrepareBlueprint(blueprint, prototype.gameObject.GetComponent<ConfigCartridge>().Blueprint);
			PreparePrefab(prefab.gameObject, prototype.gameObject);
			var panel = prefab.transform.Find("PanelNormal");
			if (panel is not null)
			{
				var title = panel.Find("ActualTitle");
				if (title is not null && title.TryGetComponent<Text>(out var text))
					text.text = "Registers";
				title = panel.Find("DevicesTitle");
				if (title is not null && title.TryGetComponent<Text>(out text))
					text.text = "Values";
			}
		}

        private static void PreparePrefab(GameObject prefab, GameObject copyFrom)
        {
            PrefabCopier.EnsureGameObjectSimilarity(prefab, copyFrom);
            PrefabCopier.CopyGameObject(prefab, copyFrom);
            var text = prefab.transform.Find("PanelNormal").Find("ScrollPanel").Find("Viewport").Find("Content").GetComponent<TextMeshProUGUI>();
            //text.font = TMP_FontAsset.CreateFontAsset(Font.))
        }
        private static void PrepareBlueprint(GameObject blueprint, GameObject copyFrom)
        {
            PrefabCopier.EnsureGameObjectSimilarity(blueprint, copyFrom);
            PrefabCopier.CopyGameObject(blueprint, copyFrom);
        }
    }

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
}