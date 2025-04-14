using System;
using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Pipes;
using HarmonyLib;
using Networks;

namespace Entropy.Assets.Scripts.Patches
{
    /// <summary>
    /// A patch to see all internal atmospheres in devices that have more than one.
    /// </summary>
    [HarmonyPatch(typeof(AtmosAnalyser), nameof(AtmosAnalyser.ScannedAtmosphere), MethodType.Getter)]
    [HarmonyPatchCategory(PatchCategory.AtmosAnalyzerInternalAtmospheres)]
    public class AtmosAnalyzerScannedAtmospherePatch
    {
        public static bool Prefix(AtmosAnalyser __instance, ref Atmosphere __result)
        {
            try
            {
                Thing cursorThing = CursorManager.CursorThing;
                if ((bool)__instance.RootParent && __instance.RootParent.HasAuthority && (bool)cursorThing)
                {
                    var traverse = Traverse.Create(cursorThing);
                    var internalAtmosphere1 = traverse.Field("InternalAtmosphere2")?.GetValue<Atmosphere>();
                    var internalAtmosphere2 = traverse.Field("InternalAtmosphere2")?.GetValue<Atmosphere>();
                    var internalAtmosphere3 = traverse.Field("InternalAtmosphere3")?.GetValue<Atmosphere>();

                    if (internalAtmosphere1 != null || internalAtmosphere2 != null || internalAtmosphere3 != null)
                    {
                        __result = new Atmosphere();
                        if (cursorThing.InternalAtmosphere != null && cursorThing.InternalAtmosphere.TotalMoles > MoleQuantity.Zero)
                        {
                            __result.Add(cursorThing.InternalAtmosphere.GasMixture);
                            __result.Volume += cursorThing.InternalAtmosphere.Volume;
                        }
                        if (internalAtmosphere1 != null && internalAtmosphere1.TotalMoles > MoleQuantity.Zero)
                        {
                            __result.Add(internalAtmosphere1.GasMixture);
                            __result.Volume += internalAtmosphere1.Volume;
                        }
                        if (internalAtmosphere2 != null && internalAtmosphere2.TotalMoles > MoleQuantity.Zero)
                        {
                            __result.Add(internalAtmosphere2.GasMixture);
                            __result.Volume += internalAtmosphere2.Volume;
                        }
                        if (internalAtmosphere3 != null && internalAtmosphere3.TotalMoles > MoleQuantity.Zero)
                        {
                            __result.Add(internalAtmosphere3.GasMixture);
                            __result.Volume += internalAtmosphere3.Volume;
                        }
                        __result.Thing = cursorThing;
                        Traverse.Create(__instance).Field("_selectedText").SetValue(cursorThing.DisplayName.ToUpper());
                        return false;
                    }
                    if (cursorThing.InternalAtmosphere != null)
                    {
                        __result = cursorThing.InternalAtmosphere;
                        return false;
                    }
                    if ((cursorThing is INetworkedAtmospherics networkedAtmospherics ? networkedAtmospherics.StructureNetwork : null) is
                        AtmosphericsNetwork atmosphericsNetwork)
                    {
                        __result = atmosphericsNetwork.Atmosphere;
                        return false;
                    }
                    GasTankStorage gasTankStorage = cursorThing as GasTankStorage;
                    if ((bool)gasTankStorage && (bool)gasTankStorage.Slots[0].Get())
                    {
                        __result = (gasTankStorage.Slots[0].Get() as GasCanister).InternalAtmosphere;
                        return false;
                    }
                    Human human = cursorThing as Human;
                    if ((bool)human && human.Suit is not null && human.Suit.InternalAtmosphere is not null)
                    {
                        __result = human.Suit.InternalAtmosphere;
                        return false;
                    }
                    VendingMachineRefrigerated machineRefrigerated = cursorThing as VendingMachineRefrigerated;
                    if ((bool)machineRefrigerated && machineRefrigerated.InternalAtmosphere != null)
                    {
                        __result = machineRefrigerated.InternalAtmosphere;
                        return false;
                    }
                }
                __result = __instance.WorldAtmosphere;
                return false;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
            return true;
        }
    }
}
