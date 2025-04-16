using Assets.Scripts.Util;
using HarmonyLib;
using UnityEngine;
using Entropy.Scripts.SEGI;
using Object = UnityEngine.Object;

namespace Entropy.Scripts.Patches
{
    [HarmonyPatch(typeof(ManagerBase), "ManagerAwake")]
    public class ManagerBase_ManagerAwake
    {
        public static bool Initialized { get; private set; }
        public static void Postfix()
        {
            if (Initialized)
                return;
            Plugin.Log("EntryPoint initialization...");
            if (Plugin.Config.Features[PatchCategory.SEGI].Value) SEGIManager.Enable();
            var man = Object.FindObjectOfType<ConfigurationManager.ConfigurationManager>();
            if (man == null)
            {
                var go = new GameObject("@@ConfigurationManager");
                Object.DontDestroyOnLoad(go);
                go.AddComponent<ConfigurationManager.ConfigurationManager>();
                Plugin.Log("Configuration Manager not found, creating a new one");
            }
            Initialized = true;
        }
    }
}
