using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Motherboards;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.Util;
using Entropy.Scripts.Utilities;
using HarmonyLib;
using JetBrains.Annotations;
using Objects.Pipes;

namespace Entropy.Scripts.Patches;

/// <summary>
/// An extension class that stores power and volume limit values.
/// </summary>
public class DeviceAtmosphericsRegulator
{
    public float PowerLimit { get; set; }
    public float VolumeLimit { get; set; }
}

/// <summary>
/// A patch that allows many atmospheric devices to set power and volume limits via logic network.
/// </summary>
[HarmonyPatchCategory(PatchCategory.AtmosphericRegulatorPatches)]
[HarmonyPatch(typeof(DeviceAtmospherics), nameof(DeviceAtmospherics.CanLogicWrite))]
public static class DeviceAtmosphericsPowerRegulatorPatch
{
    [UsedImplicitly]
    public static bool Prefix(DeviceAtmospherics __instance, LogicType logicType, ref bool __result)
    {
        if (logicType is not LogicType.Power and not LogicType.Volume
            || __instance is not VolumePump and not PressureRegulator and not ActiveVent and not PoweredVent and not AdvancedFurnace and not AirConditioner)
            return true;
        __result = true;
        return false;

	}
}

/// <summary>
/// A patch that sets power or volume limits for atmospheric devices from logic network.
/// </summary>
[HarmonyPatch(typeof(DeviceAtmospherics), nameof(DeviceAtmospherics.SetLogicValue))]
[HarmonyPatchCategory(PatchCategory.AtmosphericRegulatorPatches)]
public static class DeviceAtmosphericsSetLogicValuePatch
{
    [UsedImplicitly]
    public static bool Prefix(DeviceAtmospherics __instance, LogicType logicType, double value)
    {
        if (logicType is not LogicType.Power and not LogicType.Volume
            || __instance is not VolumePump and not PressureRegulator and not ActiveVent and not PoweredVent and not AdvancedFurnace and not AirConditioner)
            return true;
		if(logicType == LogicType.Power)
		{
			__instance.GetOrCreateExtension(_ => new DeviceAtmosphericsRegulator()).PowerLimit = (float)value;
		}
		else if(logicType == LogicType.Volume)
		{
			__instance.GetOrCreateExtension(_ => new DeviceAtmosphericsRegulator()).VolumeLimit = (float)value;
		}
        return false;
    }
}