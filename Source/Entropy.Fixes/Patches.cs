using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Motherboards;
using Assets.Scripts.Util;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using Entropy.Common.Attributes;
using Entropy.Common.Patching;
using Entropy.Common.Utils;
using HarmonyLib;
using System.Collections;
using UnityEngine;
using ConfigEntryBase = Entropy.Common.Configs.ConfigEntryBase;


namespace Entropy.Fixes;



public static class ConfigEntries
{
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
public static class Patches
{
	/// <summary>
	/// A patch to clear the OccupancySensor stack memory on each tick, as it otherwise is holding stale records.
	/// </summary>
	/// <param name="__instance"></param>
	[PatchConfig(ConfigEntries.OccupancySensorConfigEntry)]
	[HarmonyPatch(typeof(OccupancySensor), nameof(OccupancySensor.OnLogicTick))]
	[HarmonyPrefix]
	public static void OccupancySensorStackClearPrefix(OccupancySensor __instance)
	{
		if (__instance == null)
			return;
		var stack = Traverse.Create(__instance).Field("_stack").GetValue<LogicStack>();
		stack.Clear();
	}

	/// <summary>
	/// A patch to stop IC execution when the Tablet is turned off.
	/// </summary>
	[PatchConfig(ConfigEntries.AdvancedTabletDisableIC)]
	[HarmonyPatch(typeof(AdvancedTablet), nameof(AdvancedTablet.Execute))]
	[HarmonyPrefix]
	public static bool AdvancedTabletExecutePrefix(AdvancedTablet __instance)
	{
		// Disable IC execution when off
		if (__instance != null && !__instance.OnOff)
			return false;
		return true;
	}

	[PatchConfig(ConfigEntries.AdvancedTabletDisableIC)]
	[HarmonyPatch(typeof(Tablet), nameof(AdvancedTablet.OnInteractableUpdated))]
	[HarmonyPostfix]
	public static void AdvancedTabletOnInteractableUpdatedPostfix(AdvancedTablet __instance, Interactable interactable)
	{
		if (interactable is null)
			return;
		if (__instance is AdvancedTablet instance && interactable.Action == InteractableType.OnOff && instance.Slots.Count > 3)
		{
			// Reset IC when turned off and force execution when turned on
			if (instance.OnOff)
				instance.Execute();
			else
				(instance.ChipSlot.Get() as ProgrammableChip)?.Reset();
		}
	}

	[PatchConfig(ConfigEntries.VendingMachineSetting)]
	[HarmonyPatch(typeof(VendingMachine), nameof(VendingMachine.CanLogicRead), typeof(LogicType))]
	[HarmonyPostfix]
	public static void VendingMachineCanLogicReadPostfix(VendingMachine __instance, LogicType logicType, ref bool __result)
	{
		if (logicType == LogicType.Setting)
			__result = true;
	}

	[PatchConfig(ConfigEntries.VendingMachineSetting)]
	[HarmonyPatch(typeof(VendingMachine), nameof(VendingMachine.CanLogicWrite), typeof(LogicType))]
	[HarmonyPostfix]
	public static void VendingMachineCanLogicWritePostfix(VendingMachine __instance, LogicType logicType, ref bool __result)
	{
		if (logicType == LogicType.Setting)
			__result = true;
	}

	[PatchConfig(ConfigEntries.VendingMachineSetting)]
	[HarmonyPatch(typeof(VendingMachine), nameof(VendingMachine.GetLogicValue))]
	[HarmonyPostfix]
	public static void VendingMachineGetLogicValuePostfix(VendingMachine __instance, LogicType logicType, ref double __result)
	{
		if (__instance is null)
			return;
		if (logicType == LogicType.Setting)
			__result = __instance.Setting ?? 0;
	}

	private static readonly WaitForSeconds _waitForDelay = new(0.5f);
	private static readonly WaitForEndOfFrame _waitForFrame = new();
	[PatchConfig(ConfigEntries.VendingMachineSetting)]
	[HarmonyPatch(typeof(VendingMachine), nameof(VendingMachine.SetLogicValue))]
	[HarmonyPostfix]
	public static void VendingMachineSetLogicValuePostfix(VendingMachine __instance, LogicType logicType, double value)
	{
		if (__instance is null)
			return;
		if (logicType == LogicType.Setting)
		{
			__instance.Setting = value;
			if (value >= 2 && value < __instance.Slots.Count)
			{
				if (GameManager.IsThread)
					UnityMainThreadDispatcher.Instance().Enqueue(WaitSetRequestFromSlot(__instance, (int) value));
				else
					__instance.StartCoroutine(WaitSetRequestFromSlot(__instance, (int) value));
			}
		}

		static IEnumerator WaitSetRequestFromSlot(VendingMachine __instance, int slot)
		{
			if (slot >= 2 && slot < __instance.Slots.Count && __instance.Slots[slot].Get())
			{
				while (!__instance.Powered || __instance.IsLocked)
					yield return _waitForFrame;
				OnServer.Interact(__instance.InteractLock, 1);
				__instance.CurrentIndex = slot;
				yield return _waitForDelay;
				OnServer.MoveToSlot(__instance.CurrentSlot.Get(), __instance.ExportSlot);
				OnServer.Interact(__instance.InteractExport, 1);
				__instance.RequestedHash = 0;
				__instance.Extension = null!;
				OnServer.Interact(__instance.InteractLock, 0);
			}
		}
	}

	[PatchConfig(ConfigEntries.VendingMachineSetting)]
	[HarmonyPatch(typeof(VendingMachine), nameof(VendingMachine.CurrentIndex), MethodType.Setter)]
	[HarmonyPostfix]
	public static void VendingMachineSetCurrentIndexPostfix(VendingMachine __instance, int value)
	{
		if (__instance is null)
			return;
		__instance.Setting = value;
	}

	[AutoConfigDefinition("Particle size", DefaultValue = 0.1f, Enabled = false, MinValue = 0.001, MaxValue = 1, Order = 5)]
	public static float ParticleSize { get; set; }
	[ConfigPropertyChanging(ConfigEntries.SmallerParticles)]
	public static bool SmallerParticlesPropertyChanging(Common.Configs.ConfigEntry<bool> entry, bool value)
	{
		var particleSizeEntry = ConfigEntryBase.Get<float>(FixesMod.Instance, "ParticleSize");
		if(particleSizeEntry is not null)
			particleSizeEntry.Disabled = !value;
		return true;
	}
	[ConfigPropertyChanged(nameof(ParticleSize))]
	public static void ParticleSizeChanged(Common.Configs.ConfigEntry<float> entry)
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
			applyShapeToPosition = true,
			startSize = ParticleSize,
			startSize3D = new Vector3(ParticleSize, ParticleSize, ParticleSize),
		};
	}

	[PatchConfig(ConfigEntries.NoTrails)]
	[HarmonyPatch(typeof(AtmosphericsController), MethodType.Constructor, typeof(GridController))]
	[HarmonyPostfix]
	public static void AtmosphericsControllerCtorPostfix(AtmosphericsController __instance)
	{
		if (__instance is null)
			return;
		var trails = __instance.GasVisualizerParticleSystem.trails;
		trails.enabled = false;
	}

	[HarmonyPatch(typeof(InventoryManager), "HandlePrimaryUse")]
	[HarmonyPrefix]
	public static bool InventoryManagerHandlePrimaryUsePrefix(InventoryManager __instance)
	{

		return true;
	}
}