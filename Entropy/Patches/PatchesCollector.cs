using BepInEx.Configuration;
using Entropy.Mods;
using HarmonyLib;

namespace Entropy.Patches;

/// <summary>
/// A class to collect and manage patches.
/// </summary>
internal static class PatchesCollector
{
	/// <summary>
	/// List of existing patches.
	/// </summary>
	private static readonly Dictionary<EntropyModData, Dictionary<PatchCategory, List<HarmonyPatchInfo>>> Patches = [];

	static PatchesCollector()
	{
	}

	internal static void DisableAll()
	{
		foreach(var mod in Patches.Keys)
		{
			ConfigureMod(mod, false);
		}
	}

	internal static void RegisterMod(EntropyModData mod) => mod.Config.SettingChanged += (_, e) => OnConfigurationChanged(mod, e);

	internal static void AddPatch(EntropyModData mod, HarmonyPatchInfo patch)
	{
		if(!Patches.TryGetValue(mod, out var modPatches))
		{
			modPatches = [];
			Patches.Add(mod, modPatches);
		}
		if(!modPatches.TryGetValue(patch.Category, out var categoryPatches))
		{
			categoryPatches = [];
			modPatches.Add(patch.Category, categoryPatches);
		}
		if(!categoryPatches.Contains(patch))
			categoryPatches.Add(patch);
	}

	private static void OnConfigurationChanged(EntropyModData mod, SettingChangedEventArgs e)
	{
		var category = mod.Config.GetCategory(e.ChangedSetting);
		if(category != null)
			ConfigureCategory(mod, category, (bool)e.ChangedSetting.BoxedValue);
	}

	private static void ConfigureCategory(EntropyModData mod, PatchCategory category, bool enabled, bool silent = false)
	{
		if(!Patches.TryGetValue(mod, out var categories))
			return;
		if(!categories.TryGetValue(category, out var patches))
			return;
		var applied = false;
		foreach(var patch in patches.Where(x => x.IsPatched != enabled))
		{
			var harmony = new Harmony($"{mod.Name}.{category}");
			applied |= enabled ? patch.Patch(harmony) : patch.Unpatch(harmony);
		}
		if(!silent && applied)
			if(enabled)
			{
				if(category.IsDefault)
					EntropyPlugin.Log($"`{mod.Name}' feature enabled.");
				else
					EntropyPlugin.Log($"`{mod.Name}.{category.DisplayName}' feature enabled.");
			}
			else
			{
				if(category.IsDefault)
					EntropyPlugin.Log($"`{mod.Name}' feature disabled.");
				else
					EntropyPlugin.Log($"`{mod.Name}.{category.DisplayName}' feature disabled.");
			}
	}

	private static void ConfigureMod(EntropyModData mod, bool enabled)
	{
		if(!Patches.TryGetValue(mod, out var categories))
			return;
		foreach(var category in categories.Keys)
		{
			ConfigureCategory(mod, category, enabled, true);
		}
		if(enabled)
			EntropyPlugin.Log($"`{mod.Name}' mod enabled.");
		else
			EntropyPlugin.Log($"`{mod.Name}' mod disabled.");
	}
}