using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using Entropy.Scripts.SEGI;
using Entropy.Scripts.Utilities;
using JetBrains.Annotations;

namespace Entropy.Scripts;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInProcess("rocketstation.exe")]
public class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "net.aberro.stationeers.entropy";
    public const string PluginName = "Entropy Modpack";
    public const string PluginVersion = "1.0";

    private static readonly Dictionary<PatchCategory, HarmonyPatchInfo[]> Patches;

    public new static PluginConfigFile Config { get; private set; } = null!;

    static Plugin()
    {
        var patches = AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly())
            .Select(HarmonyPatchInfo.Create).Where(x => x != null).Select(x => x!);
        Patches = patches.Where(p => p != null)
            .GroupBy(patch => patch.Category)
            .ToDictionary(group => group.Key, group => group.ToArray());
    }
    public static void Log(string line)
    {
        Debug.Log("[" + PluginName + "]: " + line);
    }
    public static void LogWarning(string line)
    {
        Debug.LogWarning("[" + PluginName + "]: " + line);
    }
    public static void LogError(string line)
    {
        Debug.LogError("[" + PluginName + "]: " + line);
    }
    [UsedImplicitly]
    void Awake()
    {
        Log("Initializing...");
        Config = new PluginConfigFile(base.Config.ConfigFilePath, true);
        // Force updating backing field
        var backingField = typeof(BaseUnityPlugin).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).First(x => x.Name.Contains(nameof(BaseUnityPlugin.Config)));
        if (backingField == null)
            throw new Exception("Could not find backing field for Config property");
        backingField.SetValue(this, Config);
        try
        {
            AssetsManager.Init();
            Configure();
            Config.SettingChanged += (_, _) => Configure();
            Log("Patching done.");
        }
        catch (Exception e)
        {
            Log("Patching failed:");
            Log(e.ToString());
        }
    }

    /// <summary>
    ///  This is a stub method, maybe it would be implemented later on to disable the plugin.
    /// </summary>
    [UsedImplicitly]
    public void Disable()
    {
        Log("Unloading...");
        AssetsManager.Unload();
    }

    private static void Configure()
    {
        foreach (PatchCategory category in Enum.GetValues(typeof(PatchCategory)))
        {
            Harmony? harmony = null;

            var enabled = category == PatchCategory.None || Config.Features[category].Value;
            if (category == PatchCategory.None || enabled)
            {
                if (Patches.TryGetValue(category, out var patches))
                {
                    harmony ??= new Harmony($"{PluginGuid}.{category}");
                    bool patched = false;
                    foreach (var patch in patches) patched |= patch.Patch(harmony);
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
                    foreach (var patch in patches) unpatched |= patch.Unpatch(harmony);
                    harmony.UnpatchSelf(); // Just to ensure
                    if (unpatched)
                        Log($"`{category.GetDisplayName()}' feature disabled.");
                }
            }
        }
        if (Config.Features[PatchCategory.SEGI].Value)
            SEGIManager.Enable();
        else
            SEGIManager.Disable();
    }
}