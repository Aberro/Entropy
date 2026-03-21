using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.Util;
using Cysharp.Threading.Tasks;
using Entropy.Common.Attributes;
using Entropy.Common.Utils;
using HarmonyLib;
using Objects.Electrical;
using Objects.LandingPads;
using Objects.Pipes;
using Objects.RoboticArm;
using Objects.Rockets;
using Reagents;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Util;
using static Assets.Scripts.Atmospherics.AtmosphereHelper;
using static Assets.Scripts.Atmospherics.Chemistry;
using static Entropy.Adiabatics.PhysicsHelper;
using MatterState = Assets.Scripts.Atmospherics.AtmosphereHelper.MatterState;

namespace Entropy.Adiabatics;

[HarmonyPatch]
public static class Patches
{
	[HarmonyPatch(typeof(DynamicGenerator))]
	public static class DynamicGeneratorPatches
	{
		// This patch only validates that OnAtmosphericTick hasn't been modified.
		[PatchValidateCrc(0xDC386ED5)]
		[HarmonyPatch(nameof(DynamicGenerator.OnAtmosphericTick))]
		[HarmonyPrefix]
		public static void OnAtmosphericTickPrefix() { }

		// PowerGeneration is executed in OnAtmospohericTick before resetting the internal atmosphere. So, we could use it to introduce adiabatic pumping,
		// instead of patching the OnAtmosphericTick.
		[PatchValidateCrc(0x4C01FE63)]
		[HarmonyPatch("PowerGeneration")]
		[HarmonyPrefix]
		public static void PowerGenerationPrefix(DynamicGenerator __instance, ref float ____powerGenerated, ref MoleEnergy ____previousCombustionEnergy)
		{
			var @this = __instance;
			if (@this is null)
				return;

			// Calculate the amount of energy the pumping gas out takes.
			var gasMixture = @this.WorldAtmosphere.GasMixture;
			PhysicsHelper.Add(ref gasMixture, ref __instance.InternalAtmosphere.GasMixture);
			var workDone = PhysicsHelper.GetAdiabaticProcessWorkDone(
				ref gasMixture,
				@this.WorldAtmosphere.Volume + @this.InternalAtmosphere.Volume,
				@this.WorldAtmosphere.Volume);
			// subtract it from the exhaust gas

			PhysicsHelper.RemoveEnergy(ref @this.InternalAtmosphere.GasMixture, workDone);
			// and reduce combustion energy
			____previousCombustionEnergy -= workDone;
			var loss = ____previousCombustionEnergy * (1f - DynamicGenerator.Efficiency);
			if (@this.WorldAtmosphere == null)
				@this.WorldAtmosphere = @this.GridController.AtmosphericsController.CloneGlobalAtmosphere(@this.WorldGrid, 0L);
			PhysicsHelper.AddEnergy(ref @this.WorldAtmosphere.GasMixture, loss);
		}
	}
	[ConfigCategoryDefinition("Portable Scrubber", "Portable Scrubber", "Portable Scrubber pump settings")]
	[HarmonyPatch(typeof(DynamicScrubber))]
	public static class DynamicScrubberPatches
	{
		[AutoConfigDefinition("Portable Scrubber pumping volume", "Portable Scrubber", DisplayName = "Pumping volume", DefaultValue = 10d)]
		private static double PumpVolume{ get; set; }
		[AutoConfigDefinition("Portable scrubber pumping power", "Portable Scrubber", DisplayName = "Pumping power", DefaultValue = 50d)]
		private static double PumpPower{ get; set; }
		[AutoConfigDefinition("Portable scrubber simulation iterations", "Portable Scrubber", DisplayName = "Iterations", DefaultValue = 10)]
		private static int Iterations { get; set; }
		[AutoConfigDefinition("Portable scrubber pump gearing", "Portable Scrubber", DisplayName = "Geared", DefaultValue = true)]
		private static bool Geared { get; set; }
		[AutoConfigDefinition("Portable scrubber pump efficiency", "Portable Scrubber", DisplayName = "Efficiency", DefaultValue = 0.75d)]
		private static float Efficiency { get; set; }
		/*
		public override void OnAtmosphericTick()
		{
			base.OnAtmosphericTick();
			if (!this.OnOff || !(bool) (UnityEngine.Object) this.BatteryCell || this.BatteryCell.IsEmpty)
			{
				if (!this.Powered)
					return;
				OnServer.Interact(this.InteractPowered, 0);
			}
			else
			{
				if (!this.Powered)
					OnServer.Interact(this.InteractPowered, 1);
				if (!this.IsOperable())
					return;
				this.BatteryCell.PowerStored -= this.UsedPower;
				if (this.IsOpen)
					this.ReleaseToAtmos();
				else
					this.GetFromAtmos();
			}
		}

		private void ReleaseToAtmos()
		{
			if (this.WorldAtmosphere == null)
				this.WorldAtmosphere = this.GridController.AtmosphericsController.CloneGlobalAtmosphere(this.WorldGrid);
			if (this.InternalAtmosphere.PressureGassesAndLiquids < new PressurekPa(100.0))
			{
				this.WorldAtmosphere.Add(this.InternalAtmosphere.GasMixture);
				this.InternalAtmosphere.GasMixture.Reset();
			}
			else
				AtmosphereHelper.MoveVolume(this.InternalAtmosphere, this.WorldAtmosphere, this.InternalAtmosphere.Volume * 0.05000000074505806, AtmosphereHelper.MatterState.All, MoleQuantity.Zero);
		}

		private void GetFromAtmos()
		{
			if (this.WorldAtmosphere == null)
				return;
			this.WorldAtmosphere = this.GridController.AtmosphericsController.CloneGlobalAtmosphere(this.WorldGrid);
			if (this.WorldAtmosphere.IsAboveArmstrong())
				this.WorldAtmosphere.GasMixture.AddEnergy(new MoleEnergy((double) this.UsedPower * (double) this.PowerEfficiency));
			this.GetFromCell((Grid3) this.WorldGrid);
			foreach (Grid3 openNeighbor in this.WorldAtmosphere.OpenNeighbors)
				this.GetFromCell(openNeighbor);
		}
		 */
		[PatchValidateCrc(0xF413D5AB)]
		[HarmonyPatch(nameof(DynamicScrubber.OnAtmosphericTick))]
		[HarmonyPrefix]
		public static bool OnAtmosphericTickPrefix(DynamicScrubber __instance)
		{
			var @this = __instance;
			if (@this is null)
				return true;

			ThingOnAtmosphericTick(__instance);
			@this.UsedPower = 5;
			@this.PowerEfficiency = Efficiency;
			if (!@this.OnOff || !@this.BatteryCell || @this.BatteryCell.IsEmpty)
			{
				if (!@this.Powered)
					return false;
				OnServer.Interact(@this.InteractPowered, 0);
			}
			else
			{
				if (!@this.Powered)
					OnServer.Interact(@this.InteractPowered, 1);
				if (!@this.IsOperable())
					return false;
				if (@this.IsOpen)
				{
					// ReleaseToAtmos()
					if (@this.WorldAtmosphere == null)
						@this.WorldAtmosphere = @this.GridController.AtmosphericsController.CloneGlobalAtmosphere(@this.WorldGrid);
					@this.UsedPower += Math.Max(0, DoAdiabaticPumpingIterative(
						@this.InternalAtmosphere,
						@this.WorldAtmosphere,
						MatterState.Gas,
						new VolumeLitres(PumpVolume),
						new MoleEnergy(PumpPower * @this.PowerEfficiency),
						Geared,
						Iterations).ToFloat()) / @this.PowerEfficiency;
				}
				else
				{
					// GetFromAtmos()
					if (@this.WorldAtmosphere == null)
						return false;
					@this.WorldAtmosphere = @this.GridController.AtmosphericsController.CloneGlobalAtmosphere(@this.WorldGrid);
					var pumpedGas = @this.WorldAtmosphere.GasMixture;
					if (@this.WorldAtmosphere.IsAboveArmstrong() && @this.HasFilters)
					{
						// Not sure... Is pumping from and to the same atmosphere fine?
						@this.UsedPower += Math.Max(0, DoAdiabaticPumpingIterative(
							@this.WorldAtmosphere,
							@this.WorldAtmosphere,
							MatterState.Gas,
							out pumpedGas,
							new VolumeLitres(PumpVolume),
							new MoleEnergy(PumpPower * @this.PowerEfficiency),
							Geared,
							Iterations).ToFloat()) / @this.PowerEfficiency;
					}
					foreach (var filter in @this.GasFilters)
					{
						var pumpedGasCopy = pumpedGas;
						filter.FilterGas(
							ref pumpedGas,
							ref @this.InternalAtmosphere.GasMixture,
							@this.WorldAtmosphere,
							DynamicScrubber.MinimumRatioToFilterAll);
						PhysicsHelper.Remove(ref pumpedGasCopy, ref pumpedGas); // this should remove all gases except filtered.
						PhysicsHelper.Remove(ref @this.WorldAtmosphere.GasMixture, ref pumpedGasCopy); // Remove filtered gas from atmosphere
					}
				}
				@this.BatteryCell.PowerStored -= @this.UsedPower;
				if (@this.WorldAtmosphere != null && @this.WorldAtmosphere.IsAboveArmstrong())
					@this.WorldAtmosphere.GasMixture.AddEnergy(new MoleEnergy(@this.UsedPower));

			}
			return false;
		}
	}
	[ConfigCategoryDefinition("Advanced Furnace", "Advanced Furnace", "Advanced Furnace pump settings")]
	[HarmonyPatch(typeof(AdvancedFurnace))]
	public static class AdvancedFurnacePatches
	{
		[AutoConfigDefinition("Advanced Furnace pumping volume", "Advanced Furnace", DisplayName = "Pumping volume", DefaultValue = 10d)]
		private static double PumpVolume { get; set; }
		[AutoConfigDefinition("Advanced Furnace pumping power", "Advanced Furnace", DisplayName = "Pumping power", DefaultValue = 500d)]
		private static double PumpPower { get; set; }
		[AutoConfigDefinition("Advanced Furnace simulation iterations", "Advanced Furnace", DisplayName = "Iterations", DefaultValue = 10)]
		private static int Iterations { get; set; }
		[AutoConfigDefinition("Advanced Furnace pump gearing", "Advanced Furnace", DisplayName = "Geared", DefaultValue = true)]
		private static bool Geared { get; set; }
		[AutoConfigDefinition("Advanced Furnace pump efficiency", "Advanced Furnace", DisplayName = "Efficiency", DefaultValue = 0.75d)]
		private static float Efficiency { get; set; }
		/*
		public override void HandleGasInput()
		{
			if (!this.OnOff || !this.Powered || this.Error > 0)
				return;
			if (this.InputNetwork != null)
				AtmosphereHelper.MoveVolume(this.InputNetwork.Atmosphere, this.InternalAtmosphere, new VolumeLitres((double) this.OutputSetting2), AtmosphereHelper.MatterState.All, MoleQuantity.Zero);
			if (this.OutputNetwork != null)
				AtmosphereHelper.MoveVolume(this.InternalAtmosphere, this.OutputNetwork.Atmosphere, new VolumeLitres((double) this.OutputSetting), AtmosphereHelper.MatterState.Gas, MoleQuantity.Zero);
			if (this.OutputNetwork2 == null)
				return;
			AtmosphereHelper.MoveLiquidVolume(this.InternalAtmosphere, this.OutputNetwork2.Atmosphere, new VolumeLitres((double) this.OutputSetting));
		}
		 */
		[PatchValidateCrc(0xCC496EEA)]
		[HarmonyPatch(nameof(AdvancedFurnace.HandleGasInput))]
		[HarmonyPrefix]
		public static bool HandleGasInputPrefix(AdvancedFurnace __instance)
		{
			var @this = __instance;
			if(@this is null)
				return false;
			if(!@this.OnOff || !@this.Powered || @this.Error > 0)
				return false;
			@this.UsedPower = 5;
			if(@this.InputNetwork != null)
				@this.UsedPower += Math.Max(0, DoAdiabaticPumpingIterative(
					@this.InputNetwork.Atmosphere,
					@this.InternalAtmosphere,
					MatterState.All,
					new VolumeLitres(PumpVolume),
					new MoleEnergy(PumpPower * (@this.OutputSetting2 / @this.MaxSetting2)) * Efficiency,
					Geared,
					Iterations).ToFloat()) / Efficiency;
			if(@this.OutputNetwork != null)
				@this.UsedPower += Math.Max(0, DoAdiabaticPumpingIterative(
					@this.InternalAtmosphere,
					@this.OutputNetwork.Atmosphere,
					MatterState.Gas,
					new VolumeLitres(PumpVolume),
					new MoleEnergy(PumpPower * (@this.OutputSetting / @this.MaxSetting)) * Efficiency,
					Geared,
					Iterations).ToFloat()) / Efficiency;
			if(@this.OutputNetwork2 != null)
				MoveLiquidVolume(
					@this.InternalAtmosphere,
					@this.OutputNetwork2.Atmosphere,
					new VolumeLitres(@this.OutputSetting));
			return false;
		}
	}

	[ConfigCategoryDefinition("Industrial Burner", "Industrial Burner", "Industrial Burner pump settings")]
	[HarmonyPatch(typeof(IndustrialBurner))]
	public static class IndustrialBurnerPatches
	{
		[AutoConfigDefinition("Industrial Burner pumping volume", "Industrial Burner", DisplayName = "Pumping volume", DefaultValue = 10d)]
		private static double PumpVolume { get; set; }
		[AutoConfigDefinition("Industrial Burner pumping power", "Industrial Burner", DisplayName = "Pumping power", DefaultValue = 100d)]
		private static double PumpPower { get; set; }
		[AutoConfigDefinition("Industrial Burner simulation iterations", "Industrial Burner", DisplayName = "Iterations", DefaultValue = 10)]
		private static int Iterations { get; set; }
		[AutoConfigDefinition("Industrial Burner pump gearing", "Industrial Burner", DisplayName = "Geared", DefaultValue = true)]
		private static bool Geared { get; set; }
		[AutoConfigDefinition("Industrial Burner pump efficiency", "Industrial Burner", DisplayName = "Efficiency", DefaultValue = 0.75d)]
		private static float Efficiency { get; set; }
		/*
		public override void HandleGasInput()
		{
			if (!this.IsStructureCompleted || !this.FullyExtended)
				return;
			AtmosphereHelper.MoveToEqualize(this.InternalAtmosphere, this.AtmosphericsController.CloneGlobalAtmosphere(this.ChimneyExtension.VentGrid), new PressurekPa((double) RocketMath.MapToScaleClamp(PressurekPa.Zero.ToFloat(), Chemistry.Limits.MAXPressureGasPipe.ToFloat(), Chemistry.OneAtmosphere.ToFloat(), Chemistry.Limits.MAXPressureGasPipe.ToFloat() / 10f, this.InternalAtmosphere.PressureGasses.ToFloat())), AtmosphereHelper.MatterState.Gas);
			if (!this.OnOff || !this.Powered || !this.IsOperable)
				return;
			if (this.InputNetwork != null)
				AtmosphereHelper.MoveVolume(this.InputNetwork.Atmosphere, this.InternalAtmosphere, new VolumeLitres((double) this.OutputSetting), AtmosphereHelper.MatterState.All, MoleQuantity.Zero);
			if (!this.CanBurn())
				return;
			double carbonUsed = 0.0;
			double hydrocarbonUsed = 0.0;
			MoleQuantity quantity = this.InternalAtmosphere.GasMixture.Oxygen.Quantity;
			GasMixture gasMixture = GasMixtureHelper.Invalid;
			if ((double) (Reagent) this.ReagentMixture.Carbon > 0.0)
			{
				carbonUsed = Math.Min(quantity.ToDouble() / 100.0, this.ReagentMixture.Carbon.Quantity);
				gasMixture = this.InternalAtmosphere.Remove(new MoleQuantity(carbonUsed * 100.0), Chemistry.GasType.Oxygen);
			}
			else if ((double) (Reagent) this.ReagentMixture.Hydrocarbon > 0.0)
			{
				hydrocarbonUsed = Math.Min(quantity.ToDouble() / 100.0, this.ReagentMixture.Hydrocarbon.Quantity);
				gasMixture = this.InternalAtmosphere.Remove(new MoleQuantity(hydrocarbonUsed * 100.0), Chemistry.GasType.Oxygen);
			}
			if (gasMixture.IsValid)
			{
				float pollutantRatio = this.CarbonDioxideToPollutantRatio();
				this.CarbonToPollutantRatio = pollutantRatio;
				this.InternalAtmosphere.Add(new Mole(Chemistry.GasType.CarbonDioxide, gasMixture.Oxygen.Quantity * (double) pollutantRatio, gasMixture.Oxygen.Energy * (double) pollutantRatio));
				this.InternalAtmosphere.Add(new Mole(Chemistry.GasType.Pollutant, gasMixture.Oxygen.Quantity * (1.0 - (double) pollutantRatio), gasMixture.Oxygen.Energy * (1.0 - (double) pollutantRatio)));
				this.InternalAtmosphere.GasMixture.AddEnergy(new MoleEnergy((carbonUsed + hydrocarbonUsed) * 2300000.0));
			}
			this.BurnCancellation.CancelAndInitialize();
			this.BurnCarbonNextFrame(this.BurnCancellation.Token, carbonUsed, hydrocarbonUsed).Forget();
		}
		 */
		[PatchValidateCrc(0x8B72F7C1)]
		[HarmonyPatch(nameof(IndustrialBurner.HandleGasInput))]
		[HarmonyPrefix]
		public static bool HandleGasInputPrefix(IndustrialBurner __instance, CancellationTokenWrapper ___BurnCancellation)
		{
			var @this = __instance;
			var traverse = Traverse.Create(@this);
			if(@this is null)
				return false;
			if(!@this.IsStructureCompleted || !@this.FullyExtended)
				return false;
			MoveToEqualize(
				@this.InternalAtmosphere,
				traverse.Property<AtmosphericsController>("AtmosphericsController").Value.CloneGlobalAtmosphere(@this.ChimneyExtension.VentGrid),
				new PressurekPa(
					(double)RocketMath.MapToScaleClamp(
						PressurekPa.Zero.ToFloat(),
						Limits.MAXPressureGasPipe.ToFloat(),
						OneAtmosphere.ToFloat(),
						Limits.MAXPressureGasPipe.ToFloat() / 10f,
						@this.InternalAtmosphere.PressureGasses.ToFloat())),
				MatterState.Gas);
			if(!@this.OnOff || !@this.Powered || !traverse.Property<bool>("IsOperable").Value)
				return false;
			@this.UsedPower = 5;
			if(@this.InputNetwork != null)
			{
				@this.UsedPower = 5 + Math.Max(0, DoAdiabaticPumpingIterative(
					@this.InputNetwork.Atmosphere,
					@this.InternalAtmosphere,
					MatterState.All,
					new VolumeLitres(PumpVolume),
					new MoleEnergy(PumpPower * (@this.OutputSetting / @this.MaxSetting)) * Efficiency,
					Geared,
					Iterations).ToFloat() / Efficiency);
			}
			if(!@this.CanBurn())
				return false;
			var carbonUsed = 0.0;
			var hydrocarbonUsed = 0.0;
			var quantity = @this.InternalAtmosphere.GasMixture.Oxygen.Quantity;
			var gasMixture = GasMixtureHelper.Invalid;
			if(@this.ReagentMixture.Carbon > 0.0)
			{
				carbonUsed = Math.Min(quantity.ToDouble() / 100.0, @this.ReagentMixture.Carbon.Quantity);
				gasMixture = @this.InternalAtmosphere.Remove(new MoleQuantity(carbonUsed * 100.0), Chemistry.GasType.Oxygen);
			}
			else if(@this.ReagentMixture.Hydrocarbon > 0.0)
			{
				hydrocarbonUsed = Math.Min(quantity.ToDouble() / 100.0, @this.ReagentMixture.Hydrocarbon.Quantity);
				gasMixture = @this.InternalAtmosphere.Remove(new MoleQuantity(hydrocarbonUsed * 100.0), Chemistry.GasType.Oxygen);
			}
			if(gasMixture.IsValid)
			{
				var pollutantRatio = @this.CarbonDioxideToPollutantRatio();
				@this.CarbonToPollutantRatio = pollutantRatio;
				@this.InternalAtmosphere.Add(new Mole(Chemistry.GasType.CarbonDioxide, gasMixture.Oxygen.Quantity * (double)pollutantRatio, gasMixture.Oxygen.Energy * (double)pollutantRatio));
				@this.InternalAtmosphere.Add(new Mole(Chemistry.GasType.Pollutant, gasMixture.Oxygen.Quantity * (1.0 - (double)pollutantRatio), gasMixture.Oxygen.Energy * (1.0 - (double)pollutantRatio)));
				@this.InternalAtmosphere.GasMixture.AddEnergy(new MoleEnergy((carbonUsed + hydrocarbonUsed) * 2300000.0));
			}
			___BurnCancellation?.CancelAndInitialize();
			traverse.Method("BurnCarbonNextFrame").GetValue<UniTaskVoid>(___BurnCancellation?.Token, carbonUsed, hydrocarbonUsed).Forget();
			return false;
		}
	}
	[ConfigCategoryDefinition("Landing Pad Connection", "Landing Pad Connection", "Landing Pad Connection pump settings")]
	[HarmonyPatch(typeof(LandingPadGas))]
	public static class LandingPadGasPatches
	{
		[AutoConfigDefinition("Landing Pad Connection pumping volume", "Landing Pad Connection", DisplayName = "Pumping volume", DefaultValue = 50d)]
		private static double PumpVolume { get; set; }
		[AutoConfigDefinition("Landing Pad Connection pumping power", "Landing Pad Connection", DisplayName = "Pumping power", DefaultValue = 500d)]
		private static double PumpPower { get; set; }
		[AutoConfigDefinition("Landing Pad Connection simulation iterations", "Landing Pad Connection", DisplayName = "Iterations", DefaultValue = 10)]
		private static int Iterations { get; set; }
		[AutoConfigDefinition("Landing Pad Connection pump gearing", "Landing Pad Connection", DisplayName = "Geared", DefaultValue = true)]
		private static bool Geared { get; set; }
		[AutoConfigDefinition("Landing Pad Connection pump efficiency", "Landing Pad Connection", DisplayName = "Efficiency", DefaultValue = 0.75d)]
		private static float Efficiency { get; set; }

		/*
		public override void OnAtmosphericTick()
		{
			base.OnAtmosphericTick();
			if (!this.OnOff || !this.Powered || !this.IsOperable)
				return;
			Atmosphere inputAtmos = this.input ? this.InputNetwork.Atmosphere : this.LandingPadNetwork.Atmosphere;
			Atmosphere outputAtmos = this.input ? this.LandingPadNetwork.Atmosphere : this.OutputNetwork.Atmosphere;
			switch (this.pumpType)
			{
				case AtmosphereHelper.MatterState.Liquid:
					AtmosphereHelper.MoveLiquidVolume(inputAtmos, outputAtmos, new VolumeLitres((double) this.OutputSetting));
					break;
				case AtmosphereHelper.MatterState.Gas:
					AtmosphereHelper.MoveVolume(inputAtmos, outputAtmos, new VolumeLitres((double) this.OutputSetting), AtmosphereHelper.MatterState.Gas, new MoleQuantity((double) this.OutputSetting));
					break;
				default:
					ConsoleWindow.PrintError(this.DisplayName + " unsupported matter state");
					break;
			}
		}
		 */
		[PatchValidateCrc(0xB87646C5)]
		[HarmonyPatch(nameof(LandingPadGas.OnAtmosphericTick))]
		[HarmonyPrefix]
		public static bool OnAtmosphericTickPrefix(LandingPadGas __instance, bool ___input, MatterState ___pumpType)
		{
			var @this = __instance;
			var traverse = Traverse.Create(@this);
			if(@this is null)
				return false;
			DeviceAtmosphericsOnAtmosphericTick(@this);
			if(!@this.OnOff || !@this.Powered || !traverse.Property<bool>("IsOperable").Value)
				return false;
			var inputAtmos = ___input ? @this.InputNetwork.Atmosphere : @this.LandingPadNetwork.Atmosphere;
			var outputAtmos = ___input ? @this.LandingPadNetwork.Atmosphere : @this.OutputNetwork.Atmosphere;
			switch(___pumpType)
			{
			case MatterState.Liquid:
				MoveLiquidVolume(inputAtmos, outputAtmos, new VolumeLitres(@this.OutputSetting));
				break;
			case MatterState.Gas:
				@this.UsedPower = 5 + Math.Max(0, DoAdiabaticPumpingIterative(
					inputAtmos,
					outputAtmos,
					MatterState.Gas,
					new VolumeLitres(PumpVolume),
					new MoleEnergy(PumpPower * (@this.OutputSetting / @this.MaxSetting)) * Efficiency,
					Geared,
					Iterations).ToFloat() / Efficiency);
				break;
			default:
				ConsoleWindow.PrintError(@this.DisplayName + " unsupported matter state");
				break;
			}
			return false;
		}
	}
	[ConfigCategoryDefinition("Landing Pad Tank", "Landing Pad Tank", "Landing Pad Tank pump settings")]
	[HarmonyPatch(typeof(LandingPadTankConnector))]
	public static class LandingPadTankConnectorPatches
	{
		[AutoConfigDefinition("Landing Pad Tank pumping volume", "Landing Pad Tank", DisplayName = "Pumping volume", DefaultValue = 50d)]
		private static double PumpVolume { get; set; }
		[AutoConfigDefinition("Landing Pad Tank pumping power", "Landing Pad Tank", DisplayName = "Pumping power", DefaultValue = 500d)]
		private static double PumpPower { get; set; }
		[AutoConfigDefinition("Landing Pad Tank simulation iterations", "Landing Pad Tank", DisplayName = "Iterations", DefaultValue = 10)]
		private static int Iterations { get; set; }
		[AutoConfigDefinition("Landing Pad Tank pump gearing", "Landing Pad Tank", DisplayName = "Geared", DefaultValue = true)]
		private static bool Geared { get; set; }
		[AutoConfigDefinition("Landing Pad Tank pump efficiency", "Landing Pad Tank", DisplayName = "Efficiency", DefaultValue = 0.75d)]
		private static float Efficiency { get; set; }

		/*
		public override void OnAtmosphericTick()
		{
			base.OnAtmosphericTick();
			if (!this.OnOff || !this.Powered || !this.IsOperable)
				return;
			PortableAtmospherics portableAtmospherics = this.TankSlot.Get<PortableAtmospherics>();
			if (portableAtmospherics == null)
			{
				this.SetPumpOnOff(0);
			}
			else
			{
				Atmosphere inputAtmos = this.Mode == 1 ? portableAtmospherics.InternalAtmosphere : this.LandingPadNetwork.Atmosphere;
				Atmosphere outputAtmos = this.Mode == 1 ? this.LandingPadNetwork.Atmosphere : portableAtmospherics.InternalAtmosphere;
				switch (this.pumpType)
				{
					case AtmosphereHelper.MatterState.Liquid:
						AtmosphereHelper.MoveLiquidVolume(inputAtmos, outputAtmos, new VolumeLitres((double) this.volumeMoved));
						break;
					case AtmosphereHelper.MatterState.Gas:
						AtmosphereHelper.MoveVolume(inputAtmos, outputAtmos, new VolumeLitres((double) this.volumeMoved), AtmosphereHelper.MatterState.Gas, new MoleQuantity((double) this.volumeMoved));
						break;
					default:
						ConsoleWindow.PrintError(this.DisplayName + " unsupported matter state");
						break;
				}
			}
		}
		 */
		[PatchValidateCrc(0x7C9C7533)]
		[HarmonyPatch(nameof(LandingPadTankConnector.OnAtmosphericTick))]
		[HarmonyPrefix]
		public static bool OnAtmosphericTickPrefix(LandingPadTankConnector __instance, MatterState ___pumpType, float ___volumeMoved)
		{
			var @this = __instance;
			var traverse = Traverse.Create(@this);
			if(@this is null)
				return false;
			DeviceOnAtmosphericTick(@this);
			if(!@this.OnOff || !@this.Powered || !traverse.Property<bool>("IsOperable").Value)
				return false;
			var portableAtmospherics = @this.TankSlot.Get<PortableAtmospherics>();
			if(portableAtmospherics == null)
				traverse.Method("SetPumpOnOff").GetValue(0);
			else
			{
				var inputAtmos = @this.Mode == 1 ? portableAtmospherics.InternalAtmosphere : @this.LandingPadNetwork.Atmosphere;
				var outputAtmos = @this.Mode == 1 ? @this.LandingPadNetwork.Atmosphere : portableAtmospherics.InternalAtmosphere;
				switch(___pumpType)
				{
				case MatterState.Liquid:
					MoveLiquidVolume(inputAtmos, outputAtmos, new VolumeLitres(___volumeMoved));
					break;
				case MatterState.Gas:
					@this.UsedPower = 5 + Math.Max(0, DoAdiabaticPumpingIterative(
						inputAtmos,
						outputAtmos,
						MatterState.Gas,
						new VolumeLitres(PumpVolume),
						new MoleEnergy(PumpPower * Efficiency),
						Geared,
						Iterations).ToFloat() / Efficiency);
					break;
				default:
					ConsoleWindow.PrintError(@this.DisplayName + " unsupported matter state");
					break;
				}
			}
			return false;
		}
	}
	[ConfigCategoryDefinition("Volume Pump", "Volume Pump", "Volume Pump settings")]
	[ConfigCategoryDefinition("Turbo Pump", "Turbo Pump", "Turbo Pump settings")]
	[HarmonyPatch(typeof(VolumePump))]
	public static class VolumePumpPatches
	{
		[AutoConfigDefinition("Volume pump pumping volume", "Volume Pump", DisplayName = "Pumping volume", DefaultValue = 10d)]
		private static double VolumePumpVolume { get; set; }
		[AutoConfigDefinition("Volume Pump pumping power", "Volume Pump", DisplayName = "Pumping power", DefaultValue = 500d)]
		private static double VolumePumpPower { get; set; }
		[AutoConfigDefinition("Volume Pump simulation iterations", "Volume Pump", DisplayName = "Iterations", DefaultValue = 10)]
		private static int VolumePumpIterations { get; set; }
		[AutoConfigDefinition("Volume Pump gearing", "Volume Pump", DisplayName = "Geared", DefaultValue = true)]
		private static bool VolumePumpGeared { get; set; }
		[AutoConfigDefinition("Volume Pump efficiency", "Volume Pump", DisplayName = "Efficiency", DefaultValue = 0.75d)]
		private static float VolumePumpEfficiency { get; set; }
		[AutoConfigDefinition("Turbo pump pumping volume", "Turbo Pump", DisplayName = "Pumping volume", DefaultValue = 50d)]
		private static double TurboPumpVolume { get; set; }
		[AutoConfigDefinition("Turbo Pump pumping power", "Turbo Pump", DisplayName = "Pumping power", DefaultValue = 5000d)]
		private static double TurboPumpPower { get; set; }
		[AutoConfigDefinition("Turbo Pump simulation iterations", "Turbo Pump", DisplayName = "Iterations", DefaultValue = 10)]
		private static int TurboPumpIterations { get; set; }
		[AutoConfigDefinition("Turbo Pump gearing", "Turbo Pump", DisplayName = "Geared", DefaultValue = true)]
		private static bool TurboPumpGeared { get; set; }
		[AutoConfigDefinition("Turbo Pump efficiency", "Turbo Pump", DisplayName = "Efficiency", DefaultValue = 0.75d)]
		private static float TurboPumpEfficiency { get; set; }

		/*
			switch (outputAtmosphere.AllowedMatterState)
			{
				case AtmosphereHelper.MatterState.Liquid:
					AtmosphereHelper.MoveLiquidVolume(inputAtmosphere, outputAtmosphere, new VolumeLitres((double) this.OutputSetting));
					AtmosphereHelper.MoveToEqualize(inputAtmosphere, outputAtmosphere, PressurekPa.MaxValue, AtmosphereHelper.MatterState.Gas);
					break;
				case AtmosphereHelper.MatterState.Gas:
				case AtmosphereHelper.MatterState.All:
					AtmosphereHelper.MoveVolume(inputAtmosphere, outputAtmosphere, new VolumeLitres((double) this.OutputSetting), AtmosphereHelper.MatterState.All, MoleQuantity.Zero);
					break;
			}
		 */

		// For gases the patch completely replaces the code with our adiabatic pumping. For liquids - it falls back to original method.
		[PatchValidateCrc(0x74A7B0FE)]
		[HarmonyPatch("MoveAtmosphere")]
		[HarmonyPrefix]
		public static bool MoveAtmospherePrefix(VolumePump __instance, Atmosphere inputAtmosphere, Atmosphere outputAtmosphere)
		{
			var @this = __instance;
			if(@this is null || inputAtmosphere is null || outputAtmosphere is null)
				return false;
			switch(outputAtmosphere.AllowedMatterState)
			{
			case MatterState.Liquid:
				// Fallback to original method.
				return true;
			case MatterState.Gas:
			case MatterState.All:
				// Switch between kinds of pumps.
				if(@this is TurboVolumePump)
				{
					@this.UsedPower = 5 + Math.Max(0, DoAdiabaticPumpingIterative(
					inputAtmosphere,
					outputAtmosphere,
					MatterState.All,
					new VolumeLitres(TurboPumpVolume),
					new MoleEnergy(TurboPumpPower * TurboPumpEfficiency),
					TurboPumpGeared,
					TurboPumpIterations).ToFloat() / TurboPumpEfficiency);
				}
				else
				{
					@this.UsedPower = 5 + Math.Max(0, DoAdiabaticPumpingIterative(
					inputAtmosphere,
					outputAtmosphere,
					MatterState.All,
					new VolumeLitres(VolumePumpVolume),
					new MoleEnergy(VolumePumpPower * VolumePumpEfficiency),
					VolumePumpGeared,
					VolumePumpIterations).ToFloat() / VolumePumpEfficiency);
				}
				break;
			}
			return false;
		}
	}

	[ConfigCategoryDefinition("Active Vent", "Active Vent", "Active Vent settings")]
	[HarmonyPatch(typeof(ActiveVent))]
	public static class ActiveVentPatches
	{
		[AutoConfigDefinition("Active Vent pumping volume", "Active Vent", DisplayName = "Pumping volume", DefaultValue = 60d)]
		private static double PumpVolume { get; set; }
		[AutoConfigDefinition("Active Vent pumping power", "Active Vent", DisplayName = "Pumping power", DefaultValue = 4000d)]
		private static double PumpPower { get; set; }
		[AutoConfigDefinition("Active Vent simulation iterations", "Active Vent", DisplayName = "Iterations", DefaultValue = 10)]
		private static int Iterations { get; set; }
		[AutoConfigDefinition("Active Vent gearing", "Active Vent", DisplayName = "Geared", DefaultValue = false)]
		private static bool Geared { get; set; }
		[AutoConfigDefinition("Active Vent efficiency", "Active Vent", DisplayName = "Efficiency", DefaultValue = 0.75d)]
		private static float Efficiency { get; set; }

		[PatchValidateCrc(0x71808F6E)]
		[HarmonyPatch(nameof(ActiveVent.OnAtmosphericTick))]
		[HarmonyPrefix]
		public static bool OnAtmosphericTickPrefix(ActiveVent __instance)
		{
			var @this = __instance;
			var traverse = Traverse.Create(@this);
			if(@this is null)
				return false;
			DeviceAtmosphericsOnAtmosphericTick(@this);
			if(@this.OnOff && @this.Powered && traverse.Property<bool>("IsOperable").Value)
			{
				var worldAtmosphere = traverse.Property<AtmosphericsController>("AtmosphericsController").Value.CloneGlobalAtmosphere(@this.WorldGrid);
				var atmosphere = @this.ConnectedPipeNetwork.Atmosphere;
				//var pressureExternal = worldAtmosphere.PressureGassesAndLiquids;
				//var temperatureExternal = worldAtmosphere.Temperature;
				//var pressureInternal = atmosphere.PressureGassesAndLiquids;
				//var temperatureInternal = atmosphere.Temperature;
				var pumpedGas = GasMixtureHelper.Invalid;
				var intake = @this.VentDirection is VentDirection.Outward ? atmosphere : (@this.VentDirection is VentDirection.Inward ? worldAtmosphere : null);
				var exhaust = @this.VentDirection is VentDirection.Outward ? worldAtmosphere : (@this.VentDirection is VentDirection.Inward ? atmosphere : null);

				var minPressure = PressurekPa.Zero;
				var maxPressure = PressurekPa.MaxValue;
				if (@this.VentDirection is VentDirection.Outward)
				{
					minPressure = @this.InternalPressure;
					maxPressure = @this.ExternalPressure;
				}
				else if (@this.VentDirection is VentDirection.Inward)
				{
					minPressure = @this.ExternalPressure;
					maxPressure = @this.InternalPressure;
				}

				if (intake is not null && exhaust is not null)
					@this.UsedPower = 5 + Math.Max(0, DoAdiabaticPumpingIterative(
						intake,
						exhaust,
						MatterState.All,
						out pumpedGas,
						new VolumeLitres(PumpVolume),
						new MoleEnergy(PumpPower * Efficiency),
						Geared,
						minPressure,
						maxPressure,
						Iterations).ToFloat() / Efficiency);
				else
					@this.UsedPower = 5;
				var pressure = PhysicsHelper.GetGasPressure(ref pumpedGas, new VolumeLitres(PumpVolume));
				@this.FlowIndicatorStatus = pressure.ToDouble() > 10
					? FlowIndicatorState.Max
					: (pressure.ToDouble() > 1
						? (@this.VentDirection is VentDirection.Inward ? FlowIndicatorState.InwardsLimited : FlowIndicatorState.OutwardsLimited)
						: (@this.VentDirection is VentDirection.Inward ? FlowIndicatorState.InwardsVeryLimited : FlowIndicatorState.OutwardsVeryLimited));
				//AdiabaticsMod.Instance.Logger.LogDebug($"ActiveVent{GameManager.GameTickCount}.\n" +
				//	$"Before: External {pressureExternal.ToDouble()}kPa, {temperatureExternal.ToDouble()}K, Internal {pressureInternal.ToDouble()}kPa, {temperatureInternal.ToDouble()}K\n" +
				//	$"After: External {worldAtmosphere.PressureGassesAndLiquids.ToDouble()}kPa, {worldAtmosphere.Temperature.ToDouble()}K, Internal {atmosphere.PressureGassesAndLiquids.ToDouble()}kPa, {atmosphere.Temperature.ToDouble()}K");
			}
			else
				@this.FlowIndicatorStatus = FlowIndicatorState.None;
			return false;
		}
	}

	[ConfigCategoryDefinition("Powered Vent", "Powered Vent", "Powered Vent settings")]
	[ConfigCategoryDefinition("Powered Vent Large", "Powered Vent (Large)", "Powered Vent (Large) settings")]
	[HarmonyPatch(typeof(PoweredVent))]
	public static class PoweredVentPatches
	{
		[AutoConfigDefinition("Powered Vent pumping volume", "Powered Vent", DisplayName = "Pumping volume", DefaultValue = 60d)]
		private static double PumpVolume { get; set; }
		[AutoConfigDefinition("Powered Vent pumping power", "Powered Vent", DisplayName = "Pumping power", DefaultValue = 4000d)]
		private static double PumpPower { get; set; }
		[AutoConfigDefinition("Powered Vent simulation iterations", "Powered Vent", DisplayName = "Iterations", DefaultValue = 10)]
		private static int Iterations { get; set; }
		[AutoConfigDefinition("Powered Vent gearing", "Powered Vent", DisplayName = "Geared", DefaultValue = false)]
		private static bool Geared { get; set; }
		[AutoConfigDefinition("Powered Vent efficiency", "Powered Vent", DisplayName = "Efficiency", DefaultValue = 0.75d)]
		private static float Efficiency { get; set; }

		[AutoConfigDefinition("Powered Vent Large pumping volume", "Powered Vent Large", DisplayName = "Pumping volume", DefaultValue = 60d)]
		private static double LargePumpVolume { get; set; }
		[AutoConfigDefinition("Powered Vent Large pumping power", "Powered Vent Large", DisplayName = "Pumping power", DefaultValue = 4000d)]
		private static double LargePumpPower { get; set; }
		[AutoConfigDefinition("Powered Vent Large simulation iterations", "Powered Vent Large", DisplayName = "Iterations", DefaultValue = 10)]
		private static int LargeIterations { get; set; }
		[AutoConfigDefinition("Powered Vent Large gearing", "Powered Vent Large", DisplayName = "Geared", DefaultValue = false)]
		private static bool LargeGeared { get; set; }
		[AutoConfigDefinition("Powered Vent Large efficiency", "Powered Vent Large", DisplayName = "Efficiency", DefaultValue = 0.75d)]
		private static float LargeEfficiency { get; set; }

		/*
		public override void OnAtmosphericTick()
		{
			base.OnAtmosphericTick();
			if (!this.IsOperable || !this.IsStructureCompleted)
				this.FlowIndicatorStatus = FlowIndicatorState.None;
			else if (!this.OnOff || !this.Powered)
				this.FlowIndicatorStatus = FlowIndicatorState.None;
			else
				this.ExchangeWithWorld();
		}
		 */
		// This patch replaces OnAtmosphericTick and intentionally skips ExchangeWithTheWorld - we do it's implementation using adiabatic pumping.
		[PatchValidateCrc(0x106E06C2)]
		[HarmonyPatch(nameof(PoweredVent.OnAtmosphericTick))]
		[HarmonyPrefix]
		public static bool OnAtmosphericTick(PoweredVent __instance)
		{
			var @this = __instance;
			var traverse = Traverse.Create(@this);
			if (@this is null)
				return false;

			DeviceAtmosphericsOnAtmosphericTick(@this);
			if (!traverse.Property<bool>("IsOperable").Value || !@this.IsStructureCompleted)
				@this.FlowIndicatorStatus = FlowIndicatorState.None;
			else if (!@this.OnOff || !@this.Powered)
				@this.FlowIndicatorStatus = FlowIndicatorState.None;
			else
			{
				var worldAtmosphere = traverse.Property<AtmosphericsController>("AtmosphericsController").Value.CloneGlobalAtmosphere(@this.WorldGrid);
				var atmosphere = @this.ConnectedPipeNetwork.Atmosphere;
				var pumpedGas = GasMixtureHelper.Invalid;
				var intake = @this.VentDirection is VentDirection.Outward ? atmosphere : (@this.VentDirection is VentDirection.Inward ? worldAtmosphere : null);
				var exhaust = @this.VentDirection is VentDirection.Outward ? worldAtmosphere : (@this.VentDirection is VentDirection.Inward ? atmosphere : null);
				var minPressure = @this.VentDirection is VentDirection.Outward ? @this.InternalPressure : (@this.VentDirection is VentDirection.Inward ? @this.ExternalPressure : PressurekPa.Zero);
				var maxPressure = @this.VentDirection is VentDirection.Outward ? @this.ExternalPressure : (@this.VentDirection is VentDirection.Inward ? @this.InternalPressure : PressurekPa.MaxValue);
				if (intake is not null && exhaust is not null)
					@this.UsedPower = 5 + Math.Max(0, DoAdiabaticPumpingIterative(
						atmosphere,
						worldAtmosphere,
						MatterState.All,
						out pumpedGas,
						new VolumeLitres(@this is PoweredVentSingleGrid ? PumpVolume : LargePumpVolume),
						new MoleEnergy(@this is PoweredVentSingleGrid ? PumpPower * Efficiency : LargePumpPower * LargeEfficiency),
						Geared,
						minPressure,
						maxPressure,
						@this is PoweredVentSingleGrid ? Iterations : LargeIterations).ToFloat() / (@this is PoweredVentSingleGrid ? Efficiency : LargeEfficiency));
				else
					@this.UsedPower = 5;
				var pressure = PhysicsHelper.GetGasPressure(ref pumpedGas, new VolumeLitres(PumpVolume));
				@this.FlowIndicatorStatus = pressure.ToDouble() > 10
					? FlowIndicatorState.Max
					: (pressure.ToDouble() > 1
						? (@this.VentDirection is VentDirection.Inward ? FlowIndicatorState.InwardsLimited : FlowIndicatorState.OutwardsLimited)
						: (@this.VentDirection is VentDirection.Inward ? FlowIndicatorState.InwardsVeryLimited : FlowIndicatorState.OutwardsVeryLimited));
			}
			return false;
		}
	}
	[ConfigCategoryDefinition("Air Conditioner", "Air Conditioner", "Air Conditioner settings")]
	[HarmonyPatch(typeof(AirConditioner))]
	public static class AirConditionerPatches
	{
		[AutoConfigDefinition("Air Conditioner pumping volume", "Air Conditioner", DisplayName = "Pumping volume", DefaultValue = 10d)]
		private static double PumpVolume { get; set; }
		[AutoConfigDefinition("Air Conditioner pumping power", "Air Conditioner", DisplayName = "Pumping power", DefaultValue = 4000d)]
		private static double PumpPower { get; set; }
		[AutoConfigDefinition("Air Conditioner pump simulation iterations", "Air Conditioner", DisplayName = "Iterations", DefaultValue = 10)]
		private static int Iterations { get; set; }
		[AutoConfigDefinition("Air Conditioner pump gearing", "Air Conditioner", DisplayName = "Geared", DefaultValue = true)]
		private static bool Geared { get; set; }
		[AutoConfigDefinition("Air Conditioner pump efficiency", "Air Conditioner", DisplayName = "Efficiency", DefaultValue = 0.75d)]
		private static float Efficiency { get; set; }
		/*
		public override void OnAtmosphericTick()
		{
			this._powerUsedDuringTick = 0.0f;
			if (this.OnOff && this.Powered && this.Mode == 1 && this.IsFullyConnected && this.IsOperable)
			{
				if (RocketMath.Abs(this.GoalTemperature - this.InputNetwork.Atmosphere.Temperature) >= TemperatureKelvin.One)
				{
					PressurekPa pressurekPa = new PressurekPa(0.1);
					float num1 = Mathf.Clamp01(RocketMath.Min(this.InputNetwork.Atmosphere.PressureGasses / Chemistry.OneAtmosphere - pressurekPa, this.OutputNetwork2.Atmosphere.PressureGasses / Chemistry.OneAtmosphere - pressurekPa).ToFloat());
					MoleQuantity transferMoles = IdealGas.Quantity(this.PressurePerTick, new VolumeLitres(100.0), this.InputNetwork.Atmosphere.Temperature);
					this.ProcessedMoles = transferMoles;
					if (!(transferMoles > MoleQuantity.Zero))
						return;
					this.InternalAtmosphere.Add(this.InputNetwork.Atmosphere.Remove(transferMoles, AtmosphereHelper.MatterState.All));
					float num2 = 14000f;
					float num3 = this.TemperatureDeltaEfficiency.Evaluate((this.GoalTemperature > this.InternalAtmosphere.GasMixture.Temperature ? this.OutputNetwork2.Atmosphere.Temperature - this.InternalAtmosphere.GasMixture.Temperature : this.InternalAtmosphere.GasMixture.Temperature - this.OutputNetwork2.Atmosphere.Temperature).ToFloat());
					float num4 = Math.Min(this.InputAndWasteEfficiency.Evaluate(this.InternalAtmosphere.GasMixture.Temperature.ToFloat()), this.InputAndWasteEfficiency.Evaluate(this.OutputNetwork2.Atmosphere.GasMixture.Temperature.ToFloat()));
					double num5 = 1.0;
					MoleEnergy energy = new MoleEnergy((double) num2 * (double) num3 * (double) num4 * (double) num1 * num5);
					if (this.GoalTemperature > this.InternalAtmosphere.GasMixture.Temperature)
						this.InternalAtmosphere.GasMixture.AddEnergy(this.OutputNetwork2.Atmosphere.GasMixture.RemoveEnergy(energy));
					else
						this.OutputNetwork2.Atmosphere.GasMixture.AddEnergy(this.InternalAtmosphere.GasMixture.RemoveEnergy(energy));
					this.EnergyMoved = energy;
					this._powerUsedDuringTick = this.HeatPumpIdlePower;
					this.OutputNetwork.Atmosphere.Add(this.InternalAtmosphere.GasMixture);
					this.InternalAtmosphere.GasMixture.Reset();
					this.TemperatureDifferentialEfficiency = num3;
					this.OperationalTemperatureLimitor = num4;
					this.OptimalPressureScalar = num1;
				}
				else
					this.ProcessedMoles = MoleQuantity.Zero;
			}
			else
				this.ProcessedMoles = MoleQuantity.Zero;
		}
		 */
		[PatchValidateCrc(0x2EE5E2E2)]
		[HarmonyPatch(nameof(AirConditioner.OnAtmosphericTick))]
		[HarmonyPrefix]
		public static bool OnAtmosphericTickPrefix(AirConditioner __instance, ref float ____powerUsedDuringTick, float ___HeatPumpIdlePower)
		{
			var @this = __instance;
			var traverse = Traverse.Create(@this);
			if (@this is null)
				return false;

			____powerUsedDuringTick = 0.0f;
			if (!@this.OnOff || !@this.Powered || @this.Mode != 1 || !@this.IsFullyConnected || !traverse.Property<bool>("IsOperable").Value)
			{
				@this.ProcessedMoles = MoleQuantity.Zero;
				return false;
			}
			if (RocketMath.Abs(@this.GoalTemperature - @this.InputNetwork.Atmosphere.Temperature) < TemperatureKelvin.One)
			{
				@this.ProcessedMoles = MoleQuantity.Zero;
				return false;
			}

			//var transferMoles = IdealGas.Quantity(@this.PressurePerTick, new VolumeLitres(100.0), @this.InputNetwork.Atmosphere.Temperature);
			//@this.ProcessedMoles = transferMoles;
			var pumpingPower = DoAdiabaticPumpingIterative(
				@this.InputNetwork.Atmosphere,
				@this.OutputNetwork.Atmosphere,
				MatterState.All,
				out var pumpedGas,
				new VolumeLitres(PumpVolume),
				new MoleEnergy(PumpPower),
				Geared,
				Iterations);
			var pumpPressure = pumpedGas.GetGasPressure(new VolumeLitres(PumpVolume));
			var pressureEfficiency = Math.Clamp(Math.Tanh(pumpPressure.ToDouble() / 100), 0, 1);
			if (!(pumpedGas.GetTotalMolesGassesAndLiquids <= MoleQuantity.Zero))
			{
				@this.ProcessedMoles = MoleQuantity.Zero;
				return false;
			}
			var maxHeatTransfer = 14000d;
			var heatTransferEfficiency = (double)@this.TemperatureDeltaEfficiency.Evaluate(
				(@this.GoalTemperature > pumpedGas.Temperature
					? @this.OutputNetwork2.Atmosphere.Temperature - pumpedGas.Temperature
					: pumpedGas.Temperature - @this.OutputNetwork2.Atmosphere.Temperature).ToFloat());
			var temperatureEfficiency = (double)Math.Min(
				@this.InputAndWasteEfficiency.Evaluate(pumpedGas.Temperature.ToFloat()),
				@this.InputAndWasteEfficiency.Evaluate(@this.OutputNetwork2.Atmosphere.GasMixture.Temperature.ToFloat()));
			var energy = new MoleEnergy(maxHeatTransfer * heatTransferEfficiency * temperatureEfficiency * pressureEfficiency * 1.0);
			if (@this.GoalTemperature > pumpedGas.Temperature)
				PhysicsHelper.AddEnergy(ref @this.OutputNetwork.Atmosphere.GasMixture, PhysicsHelper.RemoveEnergy(ref @this.OutputNetwork2.Atmosphere.GasMixture, energy));
			else
				PhysicsHelper.AddEnergy(ref @this.OutputNetwork2.Atmosphere.GasMixture, PhysicsHelper.RemoveEnergy(ref @this.OutputNetwork.Atmosphere.GasMixture, energy));
			@this.EnergyMoved = energy;
			____powerUsedDuringTick = ___HeatPumpIdlePower + pumpingPower.ToFloat();
			//@this.OutputNetwork.Atmosphere.Add(@this.InternalAtmosphere.GasMixture);
			//@this.InternalAtmosphere.GasMixture.Reset();
			@this.TemperatureDifferentialEfficiency = (float)heatTransferEfficiency;
			@this.OperationalTemperatureLimitor = (float)temperatureEfficiency;
			@this.OptimalPressureScalar = (float)pressureEfficiency;
			return false;
		}
	}
	[HarmonyPatch(typeof(Mixer))]
	public static class MixerPatches
	{
		[ConfigCategoryDefinition("Mixer", "Mixer", "Mixer configuration")]
		[AutoConfigDefinition("Mixer volume", "Mixer", DisplayName = "Mixer volume", DefaultValue = 10)]
		public static double VolumeMixer { get; set; }
		/*public override void OnAtmosphericTick()
		{
			base.OnAtmosphericTick();
			GasMixture gasMixture = GasMixtureHelper.Create();
			Thing.Event onGasMixChanged = this.OnGasMixChanged;
			if (onGasMixChanged != null)
				onGasMixChanged();
			if (!this.OnOff || !this.Powered || this.Error == 1 || !this.IsOperable)
				return;
			MoleQuantity transferMoles1 = IdealGas.Quantity(this.PressurePerTick * ((double) this.Ratio1 / 100.0), Chemistry.PipeVolume, this.InputNetwork.Atmosphere.Temperature);
			MoleQuantity transferMoles2 = IdealGas.Quantity(this.PressurePerTick * ((double) this.Ratio2 / 100.0), Chemistry.PipeVolume, this.InputNetwork2.Atmosphere.Temperature);
			PressurekPa pressureDifferential = RocketMath.WeightedAverage(this.InputNetwork.Atmosphere.PressureGasses, this.InputNetwork2.Atmosphere.PressureGasses, this.Ratio1 / 100f) - this.OutputNetwork.Atmosphere.PressureGasses;
			if (pressureDifferential > PressurekPa.Zero)
			{
				MoleQuantity moleQuantity1 = this.MaxMolesPerTick(this.InputNetwork.Atmosphere, pressureDifferential);
				MoleQuantity moleQuantity2 = this.MaxMolesPerTick(this.InputNetwork2.Atmosphere, pressureDifferential);
				float val1 = (moleQuantity1 / transferMoles1).ToFloat();
				MoleQuantity moleQuantity3 = transferMoles2;
				float val2 = (moleQuantity2 / moleQuantity3).ToFloat();
				double num = Math.Max(1.0, !RocketMath.Approximately((double) this.Ratio1, 100.0) ? (!RocketMath.Approximately((double) this.Ratio2, 100.0) ? (double) Math.Min(val1, val2) : (double) val2) : (double) val1);
				transferMoles1 *= num;
				transferMoles2 *= num;
			}
			MoleQuantity gassesAndLiquids1 = this.InputNetwork.Atmosphere.GasMixture.GetTotalMolesGassesAndLiquids;
			MoleQuantity gassesAndLiquids2 = this.InputNetwork2.Atmosphere.GasMixture.GetTotalMolesGassesAndLiquids;
			if (gassesAndLiquids1 < transferMoles1 || gassesAndLiquids2 < transferMoles2)
			{
				MoleQuantity moleQuantity = MoleQuantity.Zero;
				if (transferMoles1 > MoleQuantity.Zero && transferMoles2 > MoleQuantity.Zero)
					moleQuantity = RocketMath.Min(gassesAndLiquids1 / transferMoles1, gassesAndLiquids2 / transferMoles2);
				else if (transferMoles2 < Chemistry.MINIMUM_VALID_TOTAL_MOLES && transferMoles1 > MoleQuantity.Zero)
					moleQuantity = gassesAndLiquids1 / transferMoles1;
				else if (transferMoles1 < Chemistry.MINIMUM_VALID_TOTAL_MOLES && transferMoles2 > MoleQuantity.Zero)
					moleQuantity = gassesAndLiquids2 / transferMoles2;
				transferMoles1 *= moleQuantity;
				transferMoles2 *= moleQuantity;
			}
			if (transferMoles1 > MoleQuantity.Zero)
				gasMixture.Add(this.InputNetwork.Atmosphere.Remove(transferMoles1, AtmosphereHelper.MatterState.All));
			if (transferMoles2 > MoleQuantity.Zero)
				gasMixture.Add(this.InputNetwork2.Atmosphere.Remove(transferMoles2, AtmosphereHelper.MatterState.All));
			this.OutputNetwork.Atmosphere.Add(gasMixture);
		}
		 */
		// This patch completely replaces OnAtmosphericTick to use our own adiabatic implementation of passive mixing of gases, by PhysicsHelper.Equalize.
		[PatchValidateCrc(0x82CF0C7A)]
		[HarmonyPatch(nameof(Mixer.OnAtmosphericTick))]
		[HarmonyPrefix]
		public static bool OnAtmosphericTickPrefix(Mixer __instance)
		{
			var @this = __instance;
			var traverse = Traverse.Create(@this);
			if (@this is null)
				return false;
			DeviceAtmosphericsOnAtmosphericTick(@this);
			var gasMixture = GasMixtureHelper.Create();
			var onGasMixChanged = @this.OnGasMixChanged;
			if (onGasMixChanged != null)
				onGasMixChanged();
			if (!@this.OnOff || @this.Error == 1 || !traverse.Property<bool>("IsOperable").Value)
				return false;
			@this.UsedPower = 5;
			Mixer(
				ref @this.InputNetwork.Atmosphere.GasMixture,
				ref @this.InputNetwork2.Atmosphere.GasMixture,
				ref @this.OutputNetwork.Atmosphere.GasMixture,
				@this.InputNetwork.Atmosphere.Volume,
				@this.InputNetwork2.Atmosphere.Volume,
				@this.OutputNetwork.Atmosphere.Volume,
				new VolumeLitres(VolumeMixer),
				@this.Ratio1 / 100d);
			return false;

			static void Mixer(
				ref GasMixture leftMixture,
				ref GasMixture rightMixture,
				ref GasMixture outputMixture,
				VolumeLitres leftVolume,
				VolumeLitres rightVolume,
				VolumeLitres outputVolume,
				VolumeLitres mixerVolume,
				double ratio)
			{
				var totalEnergy = (leftMixture.TotalEnergy + rightMixture.TotalEnergy + outputMixture.TotalEnergy).ToDouble();

				var outputPressure = outputMixture.GetGasPressure(outputVolume);
				var leftPressure = leftMixture.GetGasPressure(leftVolume);
				var rightPressure = rightMixture.GetGasPressure(rightVolume);
				// Get the difference between the lowest input pressure and the output pressure
				var pressureDifferential = PhysicsMath.Min(leftPressure, rightPressure) - outputPressure;
				// If it's positive, there's nothing to transfer.
				if (pressureDifferential <= PressurekPa.Zero)
					return;

				// Try to determine the ideal ratio of moles to transfer through each input.
				// Since the mixer is pressure driven and tries to proportionally mix the gases according to their pressure,
				// we want to transfer as much gas as matter flow through fully open orifices allows.
				// But that depends on configured mixing ratio and the pressure change that each input causes.
				// So in this section we try to find the scaling factor, that would allow maximum flow rate overall while ensuring
				// the contribution of each input to output's pressure is proportional to the configured mixing ratio.

				// make copies of mixtures for estimations
				var inputMixture1 = leftMixture;
				var inputMixture2 = rightMixture;
				var outputMixture1 = outputMixture;
				var outputMixture2 = outputMixture;
				// Get resulting output mixture for both inputs separately.
				var matterFlow1 = inputMixture1.GetTotalMolesGassesAndLiquids;
				var matterFlow2 = inputMixture2.GetTotalMolesGassesAndLiquids;
				PhysicsHelper.Equalize(ref inputMixture1, ref outputMixture1, leftVolume, outputVolume, mixerVolume, MatterState.All);
				matterFlow1 -= inputMixture1.GetTotalMolesGassesAndLiquids;
				PhysicsHelper.Equalize(ref inputMixture2, ref outputMixture2, rightVolume, outputVolume, mixerVolume, MatterState.All);
				matterFlow2 -= inputMixture2.GetTotalMolesGassesAndLiquids;
				// Get the pressure change that each input causes.
				var outputPressureDelta1 = outputMixture1.GetGasPressure(outputVolume) - outputPressure;
				var outputPressureDelta2 = outputMixture2.GetGasPressure(outputVolume) - outputPressure;
				// This is not the actual pressure increase when both inputs are opened to maximum, but just used as a sum for finding the correct scaling factor.
				var pressureDeltaSum = outputPressureDelta1 + outputPressureDelta2;
				// Given the sum, calculate the ideal pressure change that each input should cause if flow rate is not limited.
				var idealDelta1 = pressureDeltaSum * ratio;
				var idealDelta2 = pressureDeltaSum * (1 - ratio);
				var constrictionFactor = 1.0;

				// Compare ideal pressure change to the actual pressure change.
				// If the ideal pressure change is greater than actual maximum flow rate, set the scaling factor accordingly.
				if (idealDelta1 > outputPressureDelta1)
					constrictionFactor = (outputPressureDelta1 / idealDelta1).ToDouble();
				// Same for the second input, except also ensure that lowest of both scaling factors is chosen.
				if (idealDelta2 > outputPressureDelta2)
					constrictionFactor = Math.Min(constrictionFactor, (outputPressureDelta2 / idealDelta2).ToDouble());

				// Now we have the global scaling factor and ratio scaling factors, calculate the actual flow rates.
				matterFlow1 *= constrictionFactor * ratio;
				matterFlow2 *= constrictionFactor * (1.0 - ratio);

				outputMixture.Add(leftMixture.Remove(matterFlow1, MatterState.All));
				outputMixture.Add(rightMixture.Remove(matterFlow2, MatterState.All));

				// Flow work: the input gas does PV work pushing mass through the orifice.
				// For ideal gas: W_flow = Δn * R * T  (per input)
				var flowWork1 = matterFlow1.FlowWork(leftMixture.Temperature);
				var flowWork2 = matterFlow2.FlowWork(rightMixture.Temperature);

				// Inputs lose energy (did work pushing gas out)
				var flowWorkTotal = leftMixture.RemoveEnergy(flowWork1);
				flowWorkTotal += rightMixture.RemoveEnergy(flowWork2);

				// Output gains that energy (kinetic energy of flow dissipates as heat)
				outputMixture.AddEnergy(flowWorkTotal);

				var newTotalEnergy = leftMixture.TotalEnergy + rightMixture.TotalEnergy + outputMixture.TotalEnergy;
				Debug.Assert(Math.Abs(totalEnergy - newTotalEnergy.ToDouble()) < totalEnergy / 10000);
			}
		}
	}
	[HarmonyPatch(typeof(Pipe))]
	public static class PipePatches
	{
		/*
		public override void OnAtmosphericTick()
		{
			base.OnAtmosphericTick();
			RocketState? rocketState = this.RocketNetwork?.Rocket?.RocketState;
			bool flag;
			if (rocketState.HasValue)
			{
				switch (rocketState.GetValueOrDefault())
				{
					case RocketState.Launching:
					case RocketState.Landing:
						flag = true;
						goto label_4;
				}
			}
			flag = false;
label_4:
			if (flag)
				this.RegisterCurrentGrids();
			if (!this._bursting)
				return;
			this.PipeNetwork.SetNetworkFault(true);
			if (!this.CanMixInWorld())
				return;
			this.SpawnIces().Forget();
			foreach (WorldGrid currentGrid in this.CurrentGrids)
			{
				if (this.GridController.CanContainAtmos(currentGrid))
				{
					PressurekPa gassesAndLiquids = this.PipeNetwork.Atmosphere.PressureGassesAndLiquids;
					Assets.Scripts.Atmospherics.Atmosphere atmosphere = this.AtmosphericsController.CloneGlobalAtmosphere(currentGrid);
					PressurekPa pressurekPa = RocketMath.Abs(atmosphere.PressureGassesAndLiquids - gassesAndLiquids);
					PressurekPa amountPressureToMove = RocketMath.Max(pressurekPa - this.MaxPressure, pressurekPa / 10.0 / (double) this.CurrentGrids.Count);
					double num1 = (this.Volume / this.PipeNetwork.Atmosphere.Volume).ToDouble() * this.PipeNetwork.Atmosphere.TotalMoles.ToDouble();
					double num2 = pressurekPa > this.MaxPressure ? 3.4028234663852886E+38 : num1;
					VolumeLitres maxVolumeToMove = this.PipeNetwork.Atmosphere.TotalVolumeLiquids > this.PipeNetwork.Atmosphere.Volume ? this.PipeNetwork.Atmosphere.TotalVolumeLiquids - this.PipeNetwork.Atmosphere.Volume * 0.99 : this.Volume;
					AtmosphereHelper.DrainLiquids(this.PipeNetwork.Atmosphere, atmosphere, maxVolumeToMove);
					if (pressurekPa > Chemistry.OneAtmosphere)
					{
						double num3 = num2 / (double) this.CurrentGrids.Count;
						AtmosphereHelper.MoveToEqualizeBidirectional(this.PipeNetwork.Atmosphere, atmosphere, amountPressureToMove, AtmosphereHelper.MatterState.Gas, new MoleQuantity(num3));
					}
					else
						AtmosphereHelper.Mix(this.PipeNetwork.Atmosphere, atmosphere, AtmosphereHelper.MatterState.Gas);
				}
			}
		}
		 */
		[PatchValidateCrc(0x84D28CA5)]
		[HarmonyPatch(nameof(Pipe.OnAtmosphericTick))]
		[HarmonyPrefix]
		public static bool OnAtmosphericTickPrefix(Pipe __instance, bool ____bursting)
		{
			var @this = __instance;
			var traverse = Traverse.Create(@this);
			if (@this is null)
				return false;
			ThingOnAtmosphericTick(@this);
			var rocketState = @this.RocketNetwork?.Rocket?.RocketState;
			var movingRocket = rocketState is RocketState.Launching or RocketState.Landing;
			if (movingRocket)
				traverse.Method("RegisterCurrentGrids").GetValue();
			if (!____bursting)
				return false;

			@this.PipeNetwork.SetNetworkFault(true);
			if (!traverse.Method("CanMixInWorld").GetValue<bool>())
				return false;
			traverse.Method("SpawnIces").GetValue<UniTaskVoid>().Forget();
			foreach (var currentGrid in @this.CurrentGrids)
			{
				if (!@this.GridController.CanContainAtmos(currentGrid))
					continue;
				var gassesAndLiquids = @this.PipeNetwork.Atmosphere.PressureGassesAndLiquids;
				var atmosphere = traverse.Property<AtmosphericsController>("AtmosphericsController").Value.CloneGlobalAtmosphere(currentGrid);
				var pressurekPa = RocketMath.Abs(atmosphere.PressureGassesAndLiquids - gassesAndLiquids);
				var amountPressureToMove = RocketMath.Max(pressurekPa - @this.MaxPressure, pressurekPa / 10.0 / @this.CurrentGrids.Count);
				var num1 = (@this.Volume / @this.PipeNetwork.Atmosphere.Volume).ToDouble() * @this.PipeNetwork.Atmosphere.TotalMoles.ToDouble();
				var num2 = pressurekPa > @this.MaxPressure ? float.MaxValue : num1;
				var maxVolumeToMove = @this.PipeNetwork.Atmosphere.TotalVolumeLiquids > @this.PipeNetwork.Atmosphere.Volume
					? @this.PipeNetwork.Atmosphere.TotalVolumeLiquids - (@this.PipeNetwork.Atmosphere.Volume * 0.99) 
					: @this.Volume;
				//DrainLiquids(@this.PipeNetwork.Atmosphere, atmosphere, maxVolumeToMove);
				var quantity = @this.PipeNetwork.Atmosphere.GasMixture.Sum(x => x.Quantity.ToDouble(), MatterState.Gas);
				//if (pressurekPa > OneAtmosphere)
				//{
				//	var num3 = num2 / (double) @this.CurrentGrids.Count;
				//	MoveToEqualizeBidirectional(@this.PipeNetwork.Atmosphere, atmosphere, amountPressureToMove, MatterState.Gas, new MoleQuantity(num3));
				//}
				//else
				//{
				//	Mix(@this.PipeNetwork.Atmosphere, atmosphere, MatterState.Gas);
				//}
				if (IsSubmerged(@this.Position, atmosphere))
					Mix(@this.PipeNetwork.Atmosphere, atmosphere, PipeVolume, MatterState.All);
				else
					Mix(@this.PipeNetwork.Atmosphere, atmosphere, PipeVolume, MatterState.All, MatterState.Gas);
			}
			return false;
		}
	}
	[HarmonyPatch(typeof(PassiveVent))]
	public static class PassiveVentPatches
	{
		/*
		public override void OnAtmosphericTick()
		{
			if (!this.HasOpenGrid || (Object) this.DockedAtmosArm != (Object) null && this.DockedAtmosArm.ArmState == ArmState.Down)
				return;
			PassiveVent._environment = this.AtmosphericsController.CloneGlobalAtmosphere(this.WorldGrid);
			if (AtmosphereHelper.IsSubmerged(this.Position, PassiveVent._environment))
				AtmosphereHelper.Mix(this.PipeNetwork?.Atmosphere, PassiveVent._environment, AtmosphereHelper.MatterState.All);
			else
				AtmosphereHelper.Mix(this.PipeNetwork?.Atmosphere, PassiveVent._environment, AtmosphereHelper.MatterState.Gas);
		}
		 */
		[PatchValidateCrc(0x539D6D00)]
		[HarmonyPatch(nameof(PassiveVent.OnAtmosphericTick))]
		[HarmonyPrefix]
		public static bool OnAtmosphericTickPrefix(PassiveVent __instance)
		{
			var @this = __instance;
			var traverse = Traverse.Create(@this);
			if (@this is null)
				return false;

			if (!@this.HasOpenGrid || (@this.DockedAtmosArm != null && @this.DockedAtmosArm.ArmState == ArmState.Down) || @this.PipeNetwork is null)
				return false;
			var atmosphere = traverse.Property<AtmosphericsController>("AtmosphericsController").Value.CloneGlobalAtmosphere(@this.WorldGrid);
			if (IsSubmerged(@this.Position, atmosphere))
				Mix(@this.PipeNetwork.Atmosphere, atmosphere, @this.Volume, MatterState.All);
			else
				Mix(@this.PipeNetwork.Atmosphere, atmosphere, @this.Volume, MatterState.All, MatterState.Gas);
			return false;
		}
	}
	/// <summary>
	/// The Adiabatic pumping itself. Heats up/cools down gases by calculating the work done by pumps.
	/// </summary>
	/// <remarks>This is a fallback and should not be invoked.</remarks>
	[HarmonyPatch(typeof(AtmosphereHelper), nameof(MoveVolume))]
	[HarmonyPrefix]
	[SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
	public static bool AtmosphereHelperMoveVolumePrefix(Atmosphere inputAtmos, Atmosphere outputAtmos, VolumeLitres volume, MatterState matterStateToMove, MoleQuantity minMolesToMove)
	{
		ArgumentNullException.ThrowIfNull(inputAtmos);
		ArgumentNullException.ThrowIfNull(outputAtmos);
		DoAdiabaticPumping(
			ref inputAtmos.GasMixture,
			ref outputAtmos.GasMixture,
			matterStateToMove,
			out var _,
			inputAtmos.Volume,
			outputAtmos.Volume,
			volume,
			MoleEnergy.MaxValue,
			true);
		return false;
	}
	[HarmonyPatch(typeof(DeviceAtmospherics), nameof(DeviceAtmospherics.OnAtmosphericTick))]
	[HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
	public static void DeviceAtmosphericsOnAtmosphericTick(DeviceAtmospherics __instance) { }
	[HarmonyPatch(typeof(Device), nameof(Device.OnAtmosphericTick))]
	[HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
	public static void DeviceOnAtmosphericTick(Device __instance) { }
	[HarmonyPatch(typeof(Thing), nameof(Thing.OnAtmosphericTick))]
	[HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
	public static void ThingOnAtmosphericTick(Thing __instance) { }
}