#if DISABLED
using System;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.Util;
using Entropy.Assets.Scripts.Atmospherics;
using Entropy.Assets.Scripts.Utilities;
using HarmonyLib;
using Objects.Pipes;
using UnityEngine;
using static Assets.Scripts.Atmospherics.Chemistry;
using System.Runtime.CompilerServices;
using Entropy.Assets.Scripts.Assets.Scripts.Atmospherics;
using Assets.Scripts;

namespace Entropy.Scripts.Patches
{
    [HarmonyPatch(typeof(DeviceAtmospherics), nameof(DeviceAtmospherics.OnAtmosphericTick))]
    public class DeviceAtmosphericsOnAtmosphericTickStub
    {
        [HarmonyReversePatch]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void OnAtmosphericTick(DeviceAtmospherics instance)
        {
            throw new NotImplementedException("This is a stub method that should never be executed");
        }
    }

    /// <summary>
    /// A patch to prevent a mixer from creating negative pressure differential and to reduce it's power consumption.
    /// </summary>
    [HarmonyPatch(typeof(Mixer), nameof(Mixer.OnAtmosphericTick))]
    [HarmonyPatchCategory(PatchCategory.AtmosphericPatches)]
    public static class MixerOnAtmosphericTickPatch
    {
		/// <summary>
		/// The number of steps to integrate the gas transfer over.
		/// </summary>
		public static readonly int IntegrationSteps = 10;
		/// <summary>
		/// The area of the orifice in m^2
		/// </summary>
		public static readonly double OrificeArea = 0.0001;
		public static bool Prefix(Mixer __instance)
        {
            var @this = __instance;
            var @tthis = Traverse.Create(@this);
			DeviceAtmosphericsOnAtmosphericTickStub.OnAtmosphericTick(@this);
            GasMixture gasMixture = GasMixtureHelper.Create();
			@this.OnGasMixChanged?.Invoke();
            if (!@this.OnOff || !@this.Powered || @this.Error == 1) return false;
            var transferMoles1 = new MoleQuantity(@this.PressurePerTick * (@this.Ratio1 / 100.0), PipeVolume, @this.InputNetwork.Atmosphere.Temperature);
            var transferMoles2 = new MoleQuantity(@this.PressurePerTick * (@this.Ratio2 / 100.0), PipeVolume, @this.InputNetwork2.Atmosphere.Temperature);
            var pressureDifferential = PhysicsMath.Min(@this.InputNetwork.Atmosphere.PressureGasses, @this.InputNetwork2.Atmosphere.PressureGasses) - @this.OutputNetwork.Atmosphere.PressureGasses;
            @this.UsedPower = 5;

            if (pressureDifferential > PressurekPa.Zero)
            {
                var maxMolesPerTick = @tthis.Method("MaxMolesPerTick", new[] { typeof(Atmosphere), typeof(PressurekPa) });
                var num1 = maxMolesPerTick.GetValue<MoleQuantity>(@this.InputNetwork.Atmosphere, pressureDifferential);
                var num2 = maxMolesPerTick.GetValue<MoleQuantity>(@this.InputNetwork2.Atmosphere, pressureDifferential);
                var a = num1 / transferMoles1;
                var num3 = transferMoles2;
                var b = num2 / num3;
                var num4 = PhysicsMath.Max(MoleQuantity.One, PhysicsMath.Min(a, b));
                transferMoles1 *= num4;
                transferMoles2 *= num4;
            }
            else
            {
                return false;
            }
            var gassesAndLiquids1 = @this.InputNetwork.Atmosphere.GasMixture.GetTotalMolesGassesAndLiquids;
            var gassesAndLiquids2 = @this.InputNetwork2.Atmosphere.GasMixture.GetTotalMolesGassesAndLiquids;
            if (gassesAndLiquids1 < transferMoles1 || gassesAndLiquids2 < transferMoles2)
            {
                var num = MoleQuantity.Zero;
                if (transferMoles1 > MoleQuantity.Zero && transferMoles2 > MoleQuantity.Zero)
                    num = PhysicsMath.Min(gassesAndLiquids1 / transferMoles1, gassesAndLiquids2 / transferMoles2);
                else if (transferMoles2 < MoleQuantity.One / 1000.0 && transferMoles1 > MoleQuantity.Zero)
                    num = gassesAndLiquids1 / transferMoles1;
                else if (transferMoles1 < MoleQuantity.One / 1000.0 && transferMoles2 > MoleQuantity.Zero)
                    num = gassesAndLiquids2 / transferMoles2;
                transferMoles1 *= num;
                transferMoles2 *= num;
            }
            if (transferMoles1 > MoleQuantity.Zero)
                gasMixture.Add(@this.InputNetwork.Atmosphere.Remove(transferMoles1, AtmosphereHelper.MatterState.All));
            if (transferMoles2 > MoleQuantity.Zero)
                gasMixture.Add(@this.InputNetwork2.Atmosphere.Remove(transferMoles2, AtmosphereHelper.MatterState.All));
            @this.OutputNetwork.Atmosphere.Add(gasMixture);
            return false;
        }

        public static bool Prefix1(Mixer __instance)
		{
            var @this = __instance;
            var tthis = Traverse.Create(@this);
			// Invoke base method
			DeviceAtmosphericsOnAtmosphericTickStub.OnAtmosphericTick(@this);
            @this.OnGasMixChanged?.Invoke();
			if (!@this.OnOff || !@this.Powered || @this.Error == 1 || !tthis.Property<bool>("IsOperable").Value)
				return false;
			// Since the game tick is relatively long real time, we want to integrate the gas transfer over multiple steps to get more accurate result, at cost of performance.
			for (int i = 0; i < IntegrationSteps; i++)
            {
                var outputPressure = @this.OutputNetwork.Atmosphere.PressureGasses;
				// Get the difference between the lowest input pressure and the output pressure
				var pressureDifferential = PhysicsMath.Min(@this.InputNetwork.Atmosphere.PressureGasses, @this.InputNetwork2.Atmosphere.PressureGasses) - outputPressure;
                // If it's positive, there's nothing to transfer.
                if (pressureDifferential <= PressurekPa.Zero)
                    return false;
				// Determine the maximum amount of moles that can be transferred through both inputs when orifices are fully open.
				var matterFlow1 = PhysicsHelper.GetMatterFlowRateThroughOrifice(ref @this.InputNetwork.Atmosphere.GasMixture, ref @this.OutputNetwork.Atmosphere.GasMixture, OrificeArea, @this.InputNetwork.Atmosphere.Volume, @this.OutputNetwork.Atmosphere.Volume);
                var matterFlow2 = PhysicsHelper.GetMatterFlowRateThroughOrifice(ref @this.InputNetwork2.Atmosphere.GasMixture, ref @this.OutputNetwork.Atmosphere.GasMixture, OrificeArea, @this.InputNetwork2.Atmosphere.Volume, @this.OutputNetwork.Atmosphere.Volume);
                matterFlow1 *= GameManager.GameTickSpeedSeconds * 1.0 / IntegrationSteps; // scale the flow rate according to time and step time.
				matterFlow2 *= GameManager.GameTickSpeedSeconds * 1.0 / IntegrationSteps;

				// Try to determine the ideal ratio of moles to transfer through each input.
				// Since the mixer is pressure driven and tries to proportionally mix the gases according to their pressure,
				// we want to transfer as much gas as matter flow through fully open orifices allows.
				// But that depends on configured mixing ratio and the pressure change that each input causes.
				// So in this section we try to find the scaling factor, that would allow maximum flow rate overall while ensuring
				// the contribution of each input to output's pressure is proportional to the configured mixing ratio.

				// make copies of mixtures for estimations
				var inputMixture1 = @this.InputNetwork.Atmosphere.GasMixture;
                var inputMixture2 = @this.InputNetwork2.Atmosphere.GasMixture;
				var outputMixture1 = @this.OutputNetwork.Atmosphere.GasMixture;
                var outputMixture2 = @this.OutputNetwork.Atmosphere.GasMixture;
                // Get resulting output mixture for both inputs separately.
				outputMixture1.Add(inputMixture1.Remove(matterFlow1, AtmosphereHelper.MatterState.All));
				outputMixture2.Add(inputMixture2.Remove(matterFlow2, AtmosphereHelper.MatterState.All));
				// Get the pressure change that each input causes.
				var outputPressureDelta1 = outputPressure - outputMixture1.GetGasPressure(@this.OutputNetwork.Atmosphere.Volume, outputMixture1.Temperature);
                var outputPressureDelta2 = outputPressure - outputMixture2.GetGasPressure(@this.OutputNetwork.Atmosphere.Volume, outputMixture2.Temperature);
                // This is not the actual pressure increase when both inputs are opened to maximum, but just used as a sum for finding the correct scaling factor.
				var pressureDeltaSum = outputPressureDelta1 + outputPressureDelta2;
				// Given the sum, calculate the ideal pressure change that each input should cause if flow rate is not limited.
				var idealDelta1 = pressureDeltaSum * @this.Ratio1 * 0.01;
				var idealDelta2 = pressureDeltaSum * @this.Ratio2 * 0.01;
                var factor = 1.0;

				// Compare ideal pressure change to the actual pressure change.
				// If the ideal pressure change is greater than actual maximum flow rate, set the scaling factor accordingly.
				if (idealDelta1 > outputPressureDelta1)
                    factor = (outputPressureDelta1 / idealDelta1).ToDouble();
				// Same for the second input, except also ensure that lowest of both scaling factors is chosen.
				if (idealDelta2 > outputPressureDelta2)
                    factor = Math.Min(factor, (outputPressureDelta2 / idealDelta2).ToDouble());

				// Now we have the global scaling factor and ratio scaling factors, calculate the actual flow rates.
				matterFlow1 *= factor * @this.Ratio1 * 0.01;
				matterFlow2 *= factor * @this.Ratio2 * 0.01;
                @this.OutputNetwork.Atmosphere.Add(@this.InputNetwork.Atmosphere.Remove(matterFlow1, AtmosphereHelper.MatterState.All));
				@this.OutputNetwork.Atmosphere.Add(@this.InputNetwork2.Atmosphere.Remove(matterFlow2, AtmosphereHelper.MatterState.All));
			}
            return false;
		}
    }

    /// <summary>
    /// A patch for pumps to change it's power consumption, and to allow faster fluid movement with positive pressure differential.
    /// </summary>
    [HarmonyPatch(typeof(VolumePump), "MoveAtmosphere")]
    [HarmonyPatchCategory(PatchCategory.AtmosphericPatches)]
    public static class VolumePumpMoveAtmospherePatch
    {
        public static bool Prefix(VolumePump __instance, Atmosphere inputAtmosphere, Atmosphere outputAtmosphere)
        {

            if (inputAtmosphere.PressureGassesAndLiquids <= new PressurekPa(0.0000001) || outputAtmosphere.Volume <= new VolumeLitres(0.0000001))
            {
                __instance.UsedPower = 5;
                return false;
            }
            float setting = (float)(__instance.Setting / __instance.MaxSetting);
            float pumpingVolume;
            if (__instance is TurboVolumePump)
                pumpingVolume = 50;
            else
                pumpingVolume = 10;

            if (pumpingVolume <= 0)
                return false;
            var nominalPower = (__instance is TurboVolumePump ? Plugin.Config.LargePumpPower.Value : Plugin.Config.SmallPumpPower.Value);
            var regulator = __instance.GetExtension<DeviceAtmospherics, DeviceAtmosphericsRegulator>();
            pumpingVolume = regulator != null && regulator.VolumeLimit > 0 && regulator.VolumeLimit < pumpingVolume ? regulator.VolumeLimit : pumpingVolume;
            pumpingVolume *= setting;
            var powerLimit = regulator?.PowerLimit ?? 0;
            nominalPower = powerLimit > 5 && powerLimit < nominalPower ? powerLimit : nominalPower;
            __instance.UsedPower = 5 + AtmosphericHelper.PumpVolume(
                inputAtmosphere,
                outputAtmosphere,
                pumpingVolume,
                0,
                float.MaxValue,
                nominalPower - 5,
                AtmosphereHelper.MatterState.All);
            return false;
        }
    }

    [HarmonyPatch(typeof(ActiveVent), "PumpGasToWorld")]
    [HarmonyPatchCategory(PatchCategory.AtmosphericPatches)]
    public static class ActiveVentPumpGasToWorldPatch
    {
        public static bool Prefix(ActiveVent __instance,
            Atmosphere worldAtmosphere,
            Atmosphere pipeAtmosphere,
            float totalTemperature, ref float __result)
        {
            if (!__instance.OnOff || !__instance.Powered || __instance.Error == 1)
                return false;
            AtmosphericHelper.Debug = __instance.CustomName == "__DEBUG__";
            float pressureGasses = worldAtmosphere.PressureGasses;
            var nominalPower = Plugin.Config.SmallPumpPower.Value;
            var regulator = __instance.GetExtension<DeviceAtmospherics, DeviceAtmosphericsRegulator>();
            var pumpingVolume = 50f;
            pumpingVolume = regulator != null && regulator.VolumeLimit > 0 && regulator.VolumeLimit < pumpingVolume ? regulator.VolumeLimit : pumpingVolume;
            var powerLimit = regulator?.PowerLimit ?? 0;
            nominalPower = powerLimit > 5 && powerLimit < nominalPower ? powerLimit : nominalPower;
            __instance.UsedPower = 5 + AtmosphericHelper.PumpVolume(
                pipeAtmosphere,
                worldAtmosphere,
                pumpingVolume,
                0,
                float.MaxValue,
                nominalPower - 5f,
                AtmosphereHelper.MatterState.All);
            __result = worldAtmosphere.PressureGasses - pressureGasses;
            AtmosphericHelper.Debug = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(ActiveVent), "PumpGasToPipe")]
    [HarmonyPatchCategory(PatchCategory.AtmosphericPatches)]
    public static class ActiveVentPumpGasToPipePatch
    {
        public static bool Prefix(ActiveVent __instance,
            Atmosphere pipeAtmosphere,
            Atmosphere worldAtmosphere,
            float totalTemperature, ref float __result)
        {
            if (!__instance.OnOff || !__instance.Powered || __instance.Error == 1)
                return false;
            AtmosphericHelper.Debug = __instance.CustomName == "__DEBUG__";
            float pressureGasses1 = worldAtmosphere.PressureGasses;
            var nominalPower = Plugin.Config.SmallPumpPower.Value;
            var regulator = __instance.GetExtension<DeviceAtmospherics, DeviceAtmosphericsRegulator>();
            var powerLimit = regulator?.PowerLimit ?? 0;
            var pumpingVolume = 50f;
            pumpingVolume = regulator != null && regulator.VolumeLimit > 0 && regulator.VolumeLimit < pumpingVolume ? regulator.VolumeLimit : pumpingVolume;
            nominalPower = powerLimit > 5 && powerLimit < nominalPower ? powerLimit : nominalPower;
            __instance.UsedPower = 5 + AtmosphericHelper.PumpVolume(
                worldAtmosphere,
                pipeAtmosphere,
                pumpingVolume,
                0,
                float.MaxValue,
                nominalPower - 5,
                AtmosphereHelper.MatterState.Gas);
            float pressureGasses2 = worldAtmosphere.PressureGasses;
            __result = pressureGasses1 - pressureGasses2;
            AtmosphericHelper.Debug = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(PoweredVent), "PumpGasToWorld")]
    [HarmonyPatchCategory(PatchCategory.AtmosphericPatches)]
    public static class PoweredVentPumpGasToWorldPatch
    {
        public static bool Prefix(PoweredVent __instance,
            Atmosphere worldAtmosphere,
            Atmosphere targetAtmosphere,
            float totalTemperature,
            float pressureToMove,
            bool force,
            ref float __result)
        {
            AtmosphericHelper.Debug = __instance.CustomName == "__DEBUG__";
            float pressureGasses = worldAtmosphere.PressureGasses;
            var nominalPower = Plugin.Config.LargePumpPower.Value;
            var regulator = __instance.GetExtension<DeviceAtmospherics, DeviceAtmosphericsRegulator>();
            var powerLimit = regulator?.PowerLimit ?? 0;
            var pumpingVolume = 150f;
            pumpingVolume = regulator != null && regulator.VolumeLimit > 0 && regulator.VolumeLimit < pumpingVolume ? regulator.VolumeLimit : pumpingVolume;
            nominalPower = powerLimit > 5 && powerLimit < nominalPower ? powerLimit : nominalPower;
            __instance.UsedPower = 5 + AtmosphericHelper.PumpVolume(
                targetAtmosphere,
                worldAtmosphere,
                pumpingVolume,
                0,
                float.MaxValue,
                nominalPower - 5f,
                AtmosphereHelper.MatterState.All);
            __result = worldAtmosphere.PressureGasses - pressureGasses;
            AtmosphericHelper.Debug = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(PoweredVent), "PumpGasToPipe")]
    [HarmonyPatchCategory(PatchCategory.AtmosphericPatches)]
    public static class PoweredVentPumpGasToPipePatch
    {
        public static bool Prefix(PoweredVent __instance,
            Atmosphere worldAtmosphere,
            Atmosphere targetAtmosphere,
            float pressureToMove,
            bool force,
            ref float __result)
        {
            AtmosphericHelper.Debug = __instance.CustomName == "__DEBUG__";
            float pressureGasses1 = worldAtmosphere.PressureGasses;
            var nominalPower = Plugin.Config.LargePumpPower.Value;
            var regulator = __instance.GetExtension<DeviceAtmospherics, DeviceAtmosphericsRegulator>();
            var pumpingVolume = 150f;
            pumpingVolume = regulator != null && regulator.VolumeLimit > 0 && regulator.VolumeLimit < pumpingVolume ? regulator.VolumeLimit : pumpingVolume;
            var powerLimit = regulator?.PowerLimit ?? 0;
            nominalPower = powerLimit > 5 && powerLimit < nominalPower ? powerLimit : nominalPower;
            __instance.UsedPower = 5 + AtmosphericHelper.PumpVolume(
                worldAtmosphere,
                targetAtmosphere,
                pumpingVolume,
                0,
                float.MaxValue,
                nominalPower - 5f,
                AtmosphereHelper.MatterState.Gas);
            float pressureGasses2 = worldAtmosphere.PressureGasses;
            __result = pressureGasses1 - pressureGasses2;
            AtmosphericHelper.Debug = false;
            return false;
        }
    }

    /// <summary>
    /// A patch for pressure regulator to change it's power consumption, and to allow faster fluid movement with positive pressure differential.
    /// </summary>
    [HarmonyPatch(typeof(PressureRegulator), nameof(PressureRegulator.OnAtmosphericTick))]
    [HarmonyPatchCategory(PatchCategory.AtmosphericPatches)]
    public static class RegulatorOnAtmosphericTickPatch
    {
        public static readonly float RegulatorVolume = 10;
        public static bool Prefix(PressureRegulator __instance)
        {
            DeviceAtmosphericsOnAtmosphericTickStub.OnAtmosphericTick(__instance);

            __instance.UsedPower = 5;
            if (__instance.OnPressureRegulatorSet != null)
                UnityMainThreadDispatcher.Instance().Enqueue(() => __instance.OnPressureRegulatorSet());
            if (!__instance.OnOff || !__instance.Powered || __instance.Error == 1 || __instance.InputNetwork == null || __instance.OutputNetwork == null)
                return false;

            var setting = (float)__instance.Setting;
            var inputAtmosphere = __instance.InputNetwork.Atmosphere;
            var outputAtmosphere = __instance.OutputNetwork.Atmosphere;
            AtmosphericHelper.Debug = __instance.CustomName == "__DEBUG__";

            var inputPressureLimit = __instance.RegulatorType == RegulatorType.Upstream ? 0 : setting;
            var outputPressureLimit = __instance.RegulatorType == RegulatorType.Downstream ? float.MaxValue : setting;
            var nominalPower = Plugin.Config.SmallPumpPower.Value;
            var regulator = __instance.GetExtension<DeviceAtmospherics, DeviceAtmosphericsRegulator>();
            var pumpingVolume = 10f;
            pumpingVolume = regulator != null && regulator.VolumeLimit > 0 && regulator.VolumeLimit < pumpingVolume ? regulator.VolumeLimit : pumpingVolume;
            var powerLimit = regulator?.PowerLimit ?? 0;
            nominalPower = powerLimit > 5 && powerLimit < nominalPower ? powerLimit : nominalPower;
            __instance.UsedPower = 5 + AtmosphericHelper.PumpVolume(
                inputAtmosphere,
                outputAtmosphere,
                pumpingVolume,
                inputPressureLimit,
                outputPressureLimit,
                nominalPower - 5f,
                __instance.MovedContent);
            AtmosphericHelper.Debug = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(AirConditioner), nameof(AirConditioner.OnAtmosphericTick))]
    [HarmonyPatchCategory(PatchCategory.AtmosphericPatches)]
    public static class AirConditionerOnAtmosphericTickPatch
    {
        public static bool Prefix(AirConditioner __instance)
        {
            var traverse = Traverse.Create(__instance);
            traverse.Field<float>("_powerUsedDuringTick").Value = 0.0f;
            var inputAtmosphere = __instance.InputNetwork?.Atmosphere;
            var outputAtmosphere = __instance.OutputNetwork?.Atmosphere;
            var wasteAtmosphere = __instance.OutputNetwork2?.Atmosphere;
            var internalAtmosphere = __instance.InternalAtmosphere;
            internalAtmosphere.Volume = 50;
            if (inputAtmosphere == null || outputAtmosphere == null || wasteAtmosphere == null || !__instance.OnOff || !__instance.Powered ||
                __instance.Mode != 1 || !__instance.IsFullyConnected || Mathf.Abs(__instance.GoalTemperature - inputAtmosphere.Temperature) < 1.0)
                return false;
            float optimalPressureScalar = Mathf.Clamp01(Mathf.Min(
                (float)(inputAtmosphere.PressureGasses / 101.324996948242 - 0.100000001490116),
                (float)(wasteAtmosphere.PressureGasses / 101.324996948242 - 0.100000001490116)));
            var nominalPower = Plugin.Config.AirConditionerPower.Value;
            var powerLimit = __instance.GetExtension<DeviceAtmospherics, DeviceAtmosphericsRegulator>()?.PowerLimit ?? 0;
            nominalPower = powerLimit > 5 && powerLimit < nominalPower ? powerLimit : nominalPower;

            // Transfer atmosphere into internal.
            var pressureDifference = inputAtmosphere.PressureGassesAndLiquidsInPa - internalAtmosphere.PressureGassesAndLiquidsInPa * 0.95f;
            float transferMoles;
            if (internalAtmosphere.TotalMoles > 0)
                transferMoles = pressureDifference * internalAtmosphere.Volume / __instance.GoalTemperature * 0.1202732f;
            else
                transferMoles = pressureDifference * internalAtmosphere.Volume / inputAtmosphere.Temperature * 0.1202732f;
            //EntropyFix.Log(
            //	$"Input->Internal: inputAtm: {inputAtmosphere.PressureGassesAndLiquids}, " +
            //	$"internalAtm: {internalAtmosphere.PressureGassesAndLiquids}, " +
            //	$"pressureDifference: {pressureDifference}, " +
            //	$"transferMoles: {transferMoles}");
            if (transferMoles > 0)
                internalAtmosphere.Add(inputAtmosphere.Remove(transferMoles, AtmosphereHelper.MatterState.All));

            // Do heat exchange
            var temperatureDelta = __instance.GoalTemperature > (double)internalAtmosphere.GasMixture.Temperature
                ? wasteAtmosphere.Temperature - internalAtmosphere.GasMixture.Temperature
                : internalAtmosphere.GasMixture.Temperature - wasteAtmosphere.Temperature;
            // The heat energy we need to transfer to raise/lower temperature to setting value.
            var heatTransfer = Math.Abs(__instance.GoalTemperature - internalAtmosphere.GasMixture.Temperature)
                               * internalAtmosphere.GasMixture.HeatCapacity;
            // Maximum heat energy we can transfer in ideal conditions (all efficiencies at 1)
            var maxHeatTransfer = nominalPower * Plugin.Config.AirConditionerEfficiency.Value;

            float temperatureDeltaEfficiency = __instance.TemperatureDeltaEfficiency.Evaluate(temperatureDelta);
            float inputAndWasteEfficiency = Mathf.Min(
                __instance.InputAndWasteEfficiency.Evaluate(internalAtmosphere.GasMixture.Temperature),
                __instance.InputAndWasteEfficiency.Evaluate(wasteAtmosphere.GasMixture.Temperature));
            // Heat energy used for power calculation scaled by effectiveness.
            var effectiveHeatTransfer = heatTransfer * temperatureDeltaEfficiency * inputAndWasteEfficiency * optimalPressureScalar;
            if (effectiveHeatTransfer > maxHeatTransfer)
                // Scale down heat transfer due to inefficiency and/or too high temperature delta to maximum nominal power.
                heatTransfer *= maxHeatTransfer / effectiveHeatTransfer;
            if (__instance.GoalTemperature > (double)internalAtmosphere.GasMixture.Temperature)
                internalAtmosphere.GasMixture.AddEnergy(wasteAtmosphere.GasMixture.RemoveEnergy(heatTransfer));
            else
                wasteAtmosphere.GasMixture.AddEnergy(internalAtmosphere.GasMixture.RemoveEnergy(heatTransfer));
            traverse.Field<float>("_powerUsedDuringTick").Value = nominalPower * (effectiveHeatTransfer / maxHeatTransfer);
            //EntropyFix.Log(
            //	$"Heat exchange; temperatureDelta: {temperatureDelta}, heatTransfer: {heatTransfer}, effectiveHeatTransfer: {effectiveHeatTransfer}");

            // Transfer atmosphere to output
            if (heatTransfer / maxHeatTransfer < 1)
            {
                pressureDifference = internalAtmosphere.PressureGassesAndLiquidsInPa - outputAtmosphere.PressureGassesAndLiquidsInPa;
                if (outputAtmosphere.TotalMoles > 0)
                    transferMoles = pressureDifference * outputAtmosphere.Volume / outputAtmosphere.Temperature * 0.1202732f;
                else
                    transferMoles = pressureDifference * outputAtmosphere.Volume / internalAtmosphere.Temperature * 0.1202732f;
                //EntropyFix.Log($"Internal->Output: pressureDifference: {pressureDifference}, transferMoles: {transferMoles}");
                if (transferMoles > 0)
                    outputAtmosphere.Add(internalAtmosphere.Remove(transferMoles, AtmosphereHelper.MatterState.All));
            }
            __instance.TemperatureDifferentialEfficiency = temperatureDeltaEfficiency;
            __instance.OperationalTemperatureLimitor = inputAndWasteEfficiency;
            __instance.OptimalPressureScalar = optimalPressureScalar;
            return false;
        }
    }

    /// <summary>
    /// A patch for advanced furnace to replace it's pumps with patched ones and change it's power consumption.
    /// </summary>
    [HarmonyPatch(typeof(AdvancedFurnace), nameof(AdvancedFurnace.HandleGasInput))]
    [HarmonyPatchCategory(PatchCategory.AtmosphericPatches)]
    public static class AdvancedFurnaceHandleGasInputPatch
    {
        public static bool Prefix(AdvancedFurnace __instance)
        {
            if (!__instance.OnOff || !__instance.Powered || __instance.Error > 0)
                return false;
            float power = 10;
            var inputAtmos = __instance.InputNetwork.Atmosphere;
            var outputAtmos = __instance.OutputNetwork.Atmosphere;
            var liquidsAtmos = __instance.OutputNetwork2.Atmosphere;
            var internalAtmos = __instance.InternalAtmosphere;
            var nominalPower = Plugin.Config.SmallPumpPower.Value;
            var regulator = __instance.GetExtension<DeviceAtmospherics, DeviceAtmosphericsRegulator>();
            var powerLimit = regulator?.PowerLimit ?? 0;
            nominalPower = powerLimit > 5 && powerLimit < nominalPower ? powerLimit : nominalPower;
            if (__instance.InputNetwork != null)
                power += AtmosphericHelper.PumpVolume(
                    inputAtmos,
                    internalAtmos,
                    __instance.OutputSetting2,
                    0,
                    float.MaxValue,
                    nominalPower - 5f,
                    AtmosphereHelper.MatterState.All);
            if (__instance.OutputNetwork != null)
                power += AtmosphericHelper.PumpVolume(
                    internalAtmos,
                    outputAtmos,
                    __instance.OutputSetting,
                    0,
                    float.MaxValue,
                    nominalPower - 5f,
                    AtmosphereHelper.MatterState.Gas);
            if (__instance.OutputNetwork2 != null)
                power += AtmosphericHelper.PumpVolume(
                    internalAtmos,
                    liquidsAtmos,
                    __instance.OutputSetting,
                    0,
                    float.MaxValue,
                    nominalPower - 5f,
                    AtmosphereHelper.MatterState.Liquid);
            __instance.UsedPower = power;
            return false;
        }
    }


    //[HarmonyPatch(typeof(MoleHelper), nameof(MoleHelper.LogMessage))]
    //public static class MoleHelperLogMessagePatch
    //{
    //	public static bool Prefix(string errorMessage, ref UniTaskVoid __result)
    //	{
    //		throw new Exception(errorMessage);
    //	}
    //}

    //[HarmonyPatch(typeof(Mole), "StateChangeGas")]
    //public static class MoleStateChangeGasPatch
    //{
    //	public static bool Prefix(Mole __instance, float deficitEnergy, ref float ratio, ref Mole __result)
    //	{
    //		if (__instance.LatentHeatOfVaporization == 0)
    //		{
    //			__result = new Mole(MoleHelper.CondensationType(__instance.Type), 0, 0);
    //			return false;
    //		}

    //		// How much energy in needed to condense entire amount of matter
    //		float condensationEnergyLimit = __instance.Quantity * __instance.LatentHeatOfVaporization;
    //		// Limit energy deficit to condense not more than
    //		deficitEnergy = Mathf.Min(deficitEnergy, condensationEnergyLimit);
    //		// How much matter can be condensed potentially (limited to Quantity)
    //		float potentialStateChangeMatter = deficitEnergy / __instance.LatentHeatOfVaporization;
    //		float condensedMatter = Math.Min(potentialStateChangeMatter, __instance.Quantity * 0.99f);
    //		float convertedRatio = condensedMatter / __instance.Quantity;
    //		if (float.IsNaN(convertedRatio) || float.IsInfinity(convertedRatio) || float.IsNegativeInfinity(convertedRatio))
    //			convertedRatio = 0;
    //		float convertedMatterEnergy = convertedRatio * __instance.Energy;
    //		float energy = __instance.Energy - convertedMatterEnergy + deficitEnergy;
    //		float remainingMatter = Mathf.Max(__instance.Quantity - condensedMatter, 0.0f);
    //		float num5 = 0.0f;
    //		if (energy < 0.0)
    //		{
    //			num5 = energy;
    //			energy = 0.0f;
    //		}
    //		__instance.Set(remainingMatter, energy);
    //		__result = new Mole(MoleHelper.CondensationType(__instance.Type), condensedMatter, convertedMatterEnergy + num5);
    //		EntropyFix.Log($"StateChangeGas, deficitEnergy: {deficitEnergy}, potentialStateChangeMatter: {potentialStateChangeMatter}, " +
    //		               $"condensedMatter: {condensedMatter}, convertedRatio: {convertedRatio}, convertedMatterEnergy: {convertedMatterEnergy}" +
    //		               $"energyRemaining: {energy}, remainingMatter: {remainingMatter}");
    //		return false;
    //	}
    //}

    //[HarmonyPatch(typeof(Mole), "StateChangeLiquid")]
    //public static class MoleStateChangeLiquidPatch
    //{
    //	public static bool Prefix(Mole __instance, float energyForStateChange, ref float ratio, float maxQuantity, ref Mole __result)
    //	{
    //		if (__instance.LatentHeatOfVaporization == 0)
    //		{
    //			__result = new Mole(MoleHelper.EvaporationType(__instance.Type), 0, 0);
    //			return false;
    //		}
    //		maxQuantity = Math.Min(maxQuantity, __instance.Quantity);
    //		float potentialStateChangeMatter = energyForStateChange / __instance.LatentHeatOfVaporization;
    //		float evaporatedMatter = Math.Min(potentialStateChangeMatter, maxQuantity * 0.99f);
    //		float convertedRatio = evaporatedMatter / __instance.Quantity;
    //		if (float.IsNaN(convertedRatio) || float.IsInfinity(convertedRatio) || float.IsNegativeInfinity(convertedRatio))
    //			convertedRatio = 0;
    //		float convertedMatterEnergy = convertedRatio * __instance.Energy;
    //		float energy = __instance.Energy - convertedMatterEnergy - energyForStateChange;
    //		float remainingMatter = Mathf.Max(__instance.Quantity - evaporatedMatter, 0.0f);
    //		float num5 = 0.0f;
    //		if (energy < 0.0)
    //		{
    //			num5 = energy;
    //			energy = 0.0f;
    //		}
    //		__instance.Set(remainingMatter, energy);
    //		__result = new Mole(MoleHelper.EvaporationType(__instance.Type), evaporatedMatter, convertedRatio + num5);
    //		return false;
    //	}
    //}

    //[HarmonyPatch(typeof(Mole), MethodType.Constructor, typeof(Chemistry.GasType), typeof(float), typeof(float))]
    //public static class MoleCtorPatch
    //{
    //	public static bool Prefix(Mole __instance, Chemistry.GasType gasType, float quantity, float energy)
    //	{
    //		if (gasType == Chemistry.GasType.Undefined)
    //		{
    //			return false;
    //		}
    //		return true;
    //	}
    //}
    //[HarmonyPatch(typeof(Mole), nameof(Mole.ChangeState))]
    //public static class MoleChangeStatePatch
    //{
    //	public static bool Prefix(Mole __instance, float pressure, float volume, Mole __result)
    //	{
    //		bool Return(Mole result)
    //		{
    //			__result = result;
    //			return false;
    //		}

    //		if ((double)__instance.Quantity < 9.99999974737875E-06)
    //			return Return(MoleHelper.Invalid);
    //		switch (__instance.MatterState)
    //		{
    //			case AtmosphereHelper.MatterState.Liquid:
    //				if (!MoleHelper.CanEvaporate(__instance.Type))
    //					return Return(MoleHelper.Invalid);
    //				float num1 = __instance.EvaporationTemperatureClamped(pressure);
    //				float num2 = __instance.EvaporationPressureClamped(__instance.Temperature);
    //				if ((double)__instance.Temperature < (double)__instance.FreezingTemperature)
    //					num2 = RocketMath.MapToScale(__instance.FreezingTemperature / 2f, __instance.FreezingTemperature, Chemistry.ArmstrongLimit,
    //						__instance.MinLiquidPressure, __instance.Temperature);
    //				float num3 = num2 - pressure;
    //				if ((double)__instance.Temperature <= (double)num1)
    //				{
    //					if ((double)pressure > (double)num2)
    //						return Return(MoleHelper.Invalid);
    //					num1 = Mathf.Lerp(__instance.Temperature, __instance.Temperature - 10f, num3 / __instance.MinLiquidPressure);
    //				}
    //				float maxQuantity = float.MaxValue;
    //				if ((double)pressure < (double)num2)
    //					maxQuantity = Mathf.Max(0.0f, (float)((double)num3 * (double)volume / (8.31439971923828 * (double)__instance.Temperature)));
    //				float energyForStateChange = (__instance.Temperature - num1) * __instance.SpecificHeat * __instance.Quantity;
    //				if ((double)__instance.Temperature < (double)__instance.FreezingTemperature + 1.0)
    //					energyForStateChange = GameManager.GameTickSpeedSeconds * __instance.SpecificHeat * __instance.Quantity;
    //				var stateChangeLiquid = Traverse.Create(__instance).Method("StateChangeLiquid", new Type[] { typeof(float), typeof(float), typeof(float) });
    //				if ((double)energyForStateChange >= 9.99999974737875E-06 * (double)__instance.LatentHeatOfVaporization)
    //					return Return((Mole)stateChangeLiquid.GetValue(energyForStateChange, 0.1f, maxQuantity));
    //				return Return((double)__instance.Quantity < 0.100000001490116
    //					? (Mole)stateChangeLiquid.GetValue(energyForStateChange, 1f, maxQuantity)
    //					: MoleHelper.Invalid);
    //			case AtmosphereHelper.MatterState.Gas:
    //				if (!MoleHelper.CanCondense(__instance.Type) || (double)pressure < (double)__instance.MinLiquidPressure)
    //					return Return(MoleHelper.Invalid);
    //				float num4 = __instance.EvaporationTemperatureClamped(pressure);
    //				if ((double)__instance.Temperature >= (double)num4)
    //					return Return(MoleHelper.Invalid);
    //				float deficitEnergy = (num4 - __instance.Temperature) * __instance.SpecificHeat * __instance.Quantity;
    //				var stateChangeGas = Traverse.Create(__instance).Method("StateChangeGas", new Type[] { typeof(float), typeof(float) });
    //				if ((double)deficitEnergy >= 9.99999974737875E-06 * (double)__instance.LatentHeatOfVaporization)
    //					return Return((Mole)stateChangeGas.GetValue(deficitEnergy, 0.1f));
    //				return Return((double)__instance.Quantity <= 0.100000001490116 ? (Mole)stateChangeGas.GetValue(deficitEnergy, 1f) : MoleHelper.Invalid);
    //			default:
    //				return Return(MoleHelper.Invalid);
    //		}
    //	}
    //}
}
#endif