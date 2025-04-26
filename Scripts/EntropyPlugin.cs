using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using Entropy.Scripts.Utilities;
using JetBrains.Annotations;
using BepInEx.Configuration;
using Entropy.Scripts.UI;

namespace Entropy.Scripts;

/// <summary>
/// The entry class for the mod.
/// </summary>
[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInProcess("rocketstation.exe")]
public class EntropyPlugin : BaseUnityPlugin
{
	public const string PluginGuid = "net.aberro.stationeers.entropy";
	public const string PluginName = "Entropy Modpack";
	public const string PluginVersion = "1.0";

	/// <summary>
	/// List of existing patches in the mod.
	/// </summary>
	private static readonly Dictionary<PatchCategory, HarmonyPatchInfo[]> Patches;

	public new static PluginConfigFile Config { get; private set; } = null!;

	static EntropyPlugin()
	{
		// Find all patches in the current assembly.
		Patches = CategorizePatches(FindHarmonyPatches(AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly())));

		IEnumerable<HarmonyPatchInfo> FindHarmonyPatches(Type[] types)
		{
			foreach(var type in types)
			{
				var patch = HarmonyPatchInfo.Create(type);
				if(patch != null)
					yield return patch;
			}
		}
		Dictionary<PatchCategory, HarmonyPatchInfo[]> CategorizePatches(IEnumerable<HarmonyPatchInfo> patches) =>
			patches.GroupBy(patch => patch.Category)
				.ToDictionary(group => group.Key, group => group.ToArray());
	}

	/// <summary>
	/// Log a message to the Unity debug log.
	/// </summary>
	/// <param name="line"></param>
	public static void Log(string line) =>
		Debug.Log("[" + PluginName + "]: " + line);
	/// <summary>
	/// Log a warning to the Unity debug log.
	/// </summary>
	/// <param name="line"></param>
	public static void LogWarning(string line) =>
		Debug.LogWarning("[" + PluginName + "]: " + line);
	/// <summary>
	/// Log an error to the Unity debug log.
	/// </summary>
	/// <param name="line"></param>
	public static void LogError(string line) =>
		Debug.LogError("[" + PluginName + "]: " + line);

	[UsedImplicitly]
	void Awake()
	{
		Log("Initializing...");
		Config = new PluginConfigFile(base.Config.ConfigFilePath, true);
		// Replace BepInEx's config instance with out instance.
		var backingField = typeof(BaseUnityPlugin).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).First(x => x.Name.Contains(nameof(BaseUnityPlugin.Config)));
		if (backingField == null)
			throw new Exception("Could not find backing field for Config property");
		backingField.SetValue(this, Config);
	}

	private void OnEnable()
	{
		try
		{
			Configure();
			Config.SettingChanged += OnConfigurationChanged;
			Log("Patching done.");
		}
		catch(Exception e)
		{
			LogError("Patching failed:");
			LogError(e.ToString());
		}
	}

	private void OnDisable()
	{
		try
		{
			Configure(true);
		}
		catch(Exception e)
		{
			LogError("Unpatching failed:");
			LogError(e.ToString());
		}
	}

	public void OnDestroy()
	{
		Log("Unloading...");
	}

	private void OnConfigurationChanged(object sender, SettingChangedEventArgs e) => Configure();

	private static void Configure(bool forceDisable = false)
	{
		foreach (PatchCategory category in Enum.GetValues(typeof(PatchCategory)))
		{
			Harmony? harmony = null;

			var enabled = category == PatchCategory.None || Config.Features[category].Value;
			if (!forceDisable && (category == PatchCategory.None || enabled))
			{
				if (Patches.TryGetValue(category, out var patches))
				{
					harmony ??= new Harmony($"{PluginGuid}.{category}");
					bool patched = false;
					foreach (var patch in patches.Where(x => !x.IsPatched))
						patched |= patch.Patch(harmony);
					if (category != PatchCategory.None && patched)
						Log($"`{category.GetDisplayName()}' feature enabled.");
				}
			}
			else
			{
				if (Patches.TryGetValue(category, out var patches))
				{
					harmony ??= new Harmony($"{PluginGuid}.{category}");
					bool unpatched = false;
					foreach (var patch in patches.Where(x => x.IsPatched))
						unpatched |= patch.Unpatch(harmony);
					harmony.UnpatchSelf(); // Just to ensure
					if (unpatched)
						Log($"`{category.GetDisplayName()}' feature disabled.");
				}
			}
		}
	}
}