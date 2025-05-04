using BepInEx;
using UnityEngine;
using System.Reflection;
using Entropy.Patches;
using Entropy.Mods;
using HarmonyLib;

namespace Entropy;

/// <summary>
/// The entry class for the mod.
/// </summary>
[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInProcess("rocketstation.exe")]
public class EntropyPlugin : BaseUnityPlugin
{
	[HarmonyPatch(typeof(WorkshopMenu), nameof(WorkshopMenu.LoadModConfig))]
	private static class BootstrapPatch
	{
		public static void Postfix() =>
			// Initialize the mods collector
			ModsCollector.Init();
	}
	/// <summary>
	/// The ID of the plugin.
	/// </summary>
	public const string PluginGuid = "net.aberro.stationeers.entropy";
	/// <summary>
	/// The numan readable name of the plugin.
	/// </summary>
	public const string PluginName = "Entropy Framework";
	/// <summary>
	/// The version of the plugin.
	/// </summary>
	public const string PluginVersion = "1.0";

	/// <summary>
	/// The configuration instance for the plugin.
	/// </summary>
	public new static Config Config { get; private set; } = null!;

	/// <summary>
	/// The instance of the plugin.
	/// </summary>
	internal static EntropyPlugin Instance { get; private set; } = null!;

	/// <summary>
	/// Static constructor for the <see cref="EntropyPlugin"/> class, performs preliminary initialization.
	/// </summary>
	/// <remarks>
	/// The plugin uses some of the game's initialization and hence cannot be initialized before the game is partially loaded.
	/// For proper initialization to start, it uses a <see cref="BootstrapPatch"/> to hook into <see cref="WorkshopMenu.LoadModConfig" /> method,
	/// which will initialize <see cref="ModsCollector"/> and apply all remaining Entropy framework patches.
	/// </remarks>
	static EntropyPlugin()
	{
		// We can't fully initialize the plugin before the game is partially loaded, so we'll use this BootstrapPatch to hook to <See
		var harmony = new Harmony("Entropy bootstrapper");
		harmony.CreateClassProcessor(typeof(BootstrapPatch)).Patch();
	}

	[HarmonyPatch(typeof(WorkshopMenu), nameof(WorkshopMenu.LoadModConfig))]
	private static void Postfix() =>
		// Initialize the mods collector
		ModsCollector.Init();

	/// <summary>
	/// Initializes a new instance of the <see cref="EntropyPlugin"/> class.
	/// </summary>
	public EntropyPlugin() => Instance = this;

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

	/// <summary>
	/// Swaps the config instance for this plugin with our instance.
	/// </summary>
	/// <exception cref="Exception"></exception>
	internal void SwapConfig()
	{
		Config = ModsCollector.GetByAssembly(Assembly.GetExecutingAssembly())!.Config;
		// Replace BepInEx's config instance with out instance.
		var backingField = typeof(BaseUnityPlugin).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
			                   .First(x => x.Name.Contains(nameof(BaseUnityPlugin.Config)))
		                   ?? throw new Exception("Could not find backing field for Config property");
		backingField.SetValue(this, Config);
	}

	/// <summary>
	/// Called when the plugin is unloaded.
	/// </summary>
	public void OnDestroy() => Log("Unloading...");
}