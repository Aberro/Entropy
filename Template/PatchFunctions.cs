using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects.Electrical;
using Entropy.Common.Attributes;
using Entropy.Common.Configs;
using HarmonyLib;
using UnityEngine;

namespace Template;

// This is not necessary, just for convenience of having config entries declared in a single scope.
public static class ConfigEntries
{
	// PatchConfigDeclaration declares a boolean config entry that automatically patches or unpaches a patch method marked with PatchConfig attribute.
	[PatchConfigDefinition(OccupancySensorConfigEntry, "Clear occupancy sensor's stack memory to avoid it holding stale data", DisplayName = "Clear Occupancy Sensor stack", Order = 1)]
	public const string OccupancySensorConfigEntry = "OccupancySensor";
	[PatchConfigDefinition(AdvancedTabletDisableIC, "Disables Integrated Circuit in Advanced Tablet when it's turned off to stop it from draining power.", DisplayName = "Disable Advanced Tablet IC when off", Order = 2)]
	public const string AdvancedTabletDisableIC = "AdvancedTabletDisableIC";
	[PatchConfigDefinition(VendingMachineSetting, "Allow writing/reading Setting value of Vending Machine corresponding to it's currently selected slot", DisplayName = "Vending Machine Setting", Order = 3)]
	public const string VendingMachineSetting = "VendingMachineSetting";
	[PatchConfigDefinition(SmallerParticles, "Changes the size of gas particles", DisplayName = "Smaller Particles", DefaultValue = false, Order = 4)]
	public const string SmallerParticles = "SmallerParticles";
	[PatchConfigDefinition(NoTrails, "Removes trails from gas particles", DisplayName = "No Trails", DefaultValue = false, Order = 6)]
	public const string NoTrails = "NoTrails";
}

[HarmonyPatch]
public static class PatchFunctions
{
	// Instead of declaring PatchConfigDeclaration separately and using config entry by PatchConfig attribute,
	// it's possible to declare it here instead of PatchConfig - when a patch method has one PatchConfigDeclaration,
	// it's interpreted as the config entry for this patch. Other patch methods could also use PatchConfig with the same name,
	// but there should be only one PatchConfigDeclaration.
	[PatchConfig(ConfigEntries.OccupancySensorConfigEntry)]
	// The regular HarmonyPatch, HarmonyPrefix, HarmonyPostfix etc attributes are working as expected.
	[HarmonyPatch(typeof(OccupancySensor), nameof(OccupancySensor.OnLogicTick))]
	[HarmonyPrefix]
	public static void OccupancySensorStackClearPrefix(OccupancySensor __instance)
	{
		if (__instance == null)
			return;
		var stack = Traverse.Create(__instance).Field("_stack").GetValue<LogicStack>();
		stack.Clear();
	}

	// AutoConfigDefinition used to patch the property it's applied to so that it reads and writes from/to config entry.
	// Only primitive types and enumerations are supported.
	// In other words - a property with this attribute would result in creating a config entry, and whenever you read from that property
	// you read from that config entry, and when you write to the property - you write to the config entry.
	// No need to define backing field or getter/setter - they would be overwritten by the framework.
	// Also, AutoConfigDefinition supports config tags provided by StationeersLaunchPad.
	[AutoConfigDefinition("Particle size", DefaultValue = 0.1f, Enabled = false, MinValue = 0.001, MaxValue = 1, Order = 5)]
	public static float ParticleSize { get; set; }
	// This attribute marks a method that will be invoked when config entry is modified - either by property setter or from configuration window.
	// The attribute argument should be the name of an AutoConfig property or the name of a PatchConfig.
	[ConfigPropertyChanged(nameof(ParticleSize))]
	public static void ParticleSizeChanged(ConfigEntry<float> entry)
	{
		AtmosphericsManager.emitParams.startSize = ParticleSize;
		AtmosphericsManager.emitParams.startSize3D = new Vector3(ParticleSize, ParticleSize, ParticleSize);
	}

	[PatchConfig(ConfigEntries.SmallerParticles)]
	[HarmonyPatch(typeof(AtmosphericsManager), nameof(AtmosphericsManager.ManagerAwake))]
	[HarmonyPostfix]
	public static void AtmosphericsManagerManagerAwakePostfix()
	{
		AtmosphericsManager.emitParams = new ParticleSystem.EmitParams()
		{
			// An example of using AutoConfig property - the value would be provided from config.
			applyShapeToPosition = true,
			startSize = ParticleSize,
			startSize3D = new Vector3(ParticleSize, ParticleSize, ParticleSize),
		};
	}

	// An example of using extensions
	[PatchConfig(ConfigEntries.VendingMachineSetting)]
	[HarmonyPatch(typeof(VendingMachine), nameof(VendingMachine.CurrentIndex), MethodType.Setter)]
	[HarmonyPostfix]
	public static void VendingMachineSetCurrentIndexPostfix(VendingMachine __instance, int value)
	{
		if (__instance is null)
			return;
		// See Extensions.cs for definition - the Setting property isn't part of VendingMachine class, it's an extension property.
		// Entropy.Common binds extensions to objects and provides automated serialization whenever the bound extension is serializable,
		// so this Setting value would be saved and loaded with the game.
		// During deserialization unknown extensions from removed mods are ignored, so disabling/removing mods should not break a saved game.
		__instance.Setting = value;
	}
}