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
	private static readonly Dictionary<EntropyMod, Dictionary<PatchCategory, List<HarmonyPatchInfo>>> Patches = [];

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

	/// <summary>
	/// Registers a mod to the patches collector. This method is called by the <see cref="EntropyMod"/> constructor.
	/// </summary>
	/// <param name="mod">The mod to register.</param>
	internal static void RegisterMod(EntropyMod mod) => mod.Config.SettingChanged += (_, e) => OnConfigurationChanged(mod, e);

	/// <summary>
	/// Configures a mod by enabling or disabling its patches depending on loaded configuration file.
	/// </summary>
	/// <remarks>
	/// When <paramref name="enabled"/> is <see langword="true"/>, only patches in categories that are enabled in config are applied.
	/// When it's <see langword="false" />, all patches are unpatched.
	/// </remarks>
	/// <param name="mod">The mod to configure.</param>
	/// <param name="enabled">Flag indicating if the mod needs to be enabled or disabled.</param>
	internal static void ConfigureMod(EntropyMod mod, bool enabled)
	{
		if(!Patches.TryGetValue(mod, out var categories))
			return;
		foreach(var category in categories.Keys)
			ConfigureCategory(mod, category, category.Enabled, true);

		EntropyPlugin.Log(enabled ? $"`{mod.Name}' mod enabled." : $"`{mod.Name}' mod disabled.");
	}

	internal static void AddPatch(EntropyMod mod, HarmonyPatchInfo patch)
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

	private static void OnConfigurationChanged(EntropyMod mod, SettingChangedEventArgs e)
	{
		var category = mod.Config.GetCategory(e.ChangedSetting);
		if(category != null)
			ConfigureCategory(mod, category, (bool)e.ChangedSetting.BoxedValue);
	}

	private static void ConfigureCategory(EntropyMod mod, PatchCategory category, bool enabled, bool silent = false)
	{
		if(!Patches.TryGetValue(mod, out var categories))
			return;
		if(!categories.TryGetValue(category, out var patches))
			return;
		var applied = false;
		var harmony = new Harmony($"{mod.Name}.{category.Name}");

		// Filter out patches that are already in the desired state
		foreach(var patch in patches.Where(x => x.IsPatched != enabled))
			applied |= enabled ? patch.Patch(harmony) : patch.Unpatch(harmony);

		// Nothing to log
		if(silent || !applied)
			return;

		if(enabled)
			EntropyPlugin.Log(category.IsDefault
				? $"`{mod.Name}' feature enabled."
				: $"`{mod.Name}.{category.DisplayName}' feature enabled.");
		else
			EntropyPlugin.Log(category.IsDefault
				? $"`{mod.Name}' feature disabled."
				: $"`{mod.Name}.{category.DisplayName}' feature disabled.");
	}
}