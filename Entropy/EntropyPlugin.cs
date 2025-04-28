using BepInEx;
using UnityEngine;
using System.Reflection;
using JetBrains.Annotations;
using Entropy.Patches;
using Entropy.Mods;

namespace Entropy;

/// <summary>
/// The entry class for the mod.
/// </summary>
[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInProcess("rocketstation.exe")]
public class EntropyPlugin : BaseUnityPlugin
{
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

	static EntropyPlugin()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="EntropyPlugin"/> class.
	/// </summary>
	public EntropyPlugin()
	{
		var mainMod = new EntropyModData();
		ModsCollector.AddMod(mainMod, Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase));
		Config = mainMod.Config;
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
		// Replace BepInEx's config instance with out instance.
		var backingField = typeof(BaseUnityPlugin).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).First(x => x.Name.Contains(nameof(BaseUnityPlugin.Config)))
			?? throw new Exception("Could not find backing field for Config property");
		backingField.SetValue(this, Config);
	}

	/// <summary>
	/// Called when the plugin is loaded.
	/// </summary>
	public void OnEnable()
	{
		try
		{
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
			PatchesCollector.DisableAll();
		}
		catch(Exception e)
		{
			LogError("Unpatching failed:");
			LogError(e.ToString());
		}
	}

	/// <summary>
	/// Called when the plugin is unloaded.
	/// </summary>
	public void OnDestroy() => Log("Unloading...");
}