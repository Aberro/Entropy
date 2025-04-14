using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Atmospherics;
using UnityEngine;
using static Assets.Scripts.Atmospherics.AtmosphereHelper;

namespace Entropy.Assets.Scripts.Atmospherics
{
	public static class PhysicsHelper
	{
		/// <summary>
		/// Gas constant in J/(mol*K)
		/// </summary>
		public const float R = 8.314462618f;

		public static IEnumerable<Mole> Moles(this Atmosphere atmosphere, MatterState matterState = MatterState.All)
		{
			if (matterState is MatterState.Gas or MatterState.All)
			{
				yield return atmosphere.GasMixture.Oxygen;
				yield return atmosphere.GasMixture.Nitrogen;
				yield return atmosphere.GasMixture.CarbonDioxide;
				yield return atmosphere.GasMixture.Volatiles;
				yield return atmosphere.GasMixture.Pollutant;
				yield return atmosphere.GasMixture.Steam;
				yield return atmosphere.GasMixture.NitrousOxide;
				yield return atmosphere.GasMixture.Hydrogen;
			}
			if (matterState is MatterState.Liquid or MatterState.All)
			{
				yield return atmosphere.GasMixture.Water;
				yield return atmosphere.GasMixture.PollutedWater;
				yield return atmosphere.GasMixture.LiquidNitrogen;
				yield return atmosphere.GasMixture.LiquidOxygen;
				yield return atmosphere.GasMixture.LiquidVolatiles;
				yield return atmosphere.GasMixture.LiquidCarbonDioxide;
				yield return atmosphere.GasMixture.LiquidPollutant;
				yield return atmosphere.GasMixture.LiquidNitrousOxide;
				yield return atmosphere.GasMixture.LiquidHydrogen;
				yield return atmosphere.GasMixture.PollutedWater;
			}
		}
		public static IEnumerable<Mole> Moles(this GasMixture gasMixture, MatterState matterState = MatterState.All)
		{
			if (matterState is MatterState.Gas or MatterState.All)
			{
				yield return gasMixture.Oxygen;
				yield return gasMixture.Nitrogen;
				yield return gasMixture.CarbonDioxide;
				yield return gasMixture.Volatiles;
				yield return gasMixture.Pollutant;
				yield return gasMixture.Steam;
				yield return gasMixture.NitrousOxide;
				yield return gasMixture.Hydrogen;
			}
			if (matterState is MatterState.Liquid or MatterState.All)
			{
				yield return gasMixture.Water;
				yield return gasMixture.LiquidNitrogen;
				yield return gasMixture.LiquidOxygen;
				yield return gasMixture.LiquidVolatiles;
				yield return gasMixture.LiquidCarbonDioxide;
				yield return gasMixture.LiquidPollutant;
				yield return gasMixture.LiquidNitrousOxide;
				yield return gasMixture.LiquidHydrogen;
				yield return gasMixture.PollutedWater;
			}
		}
		/// <summary>
		/// Calculate the total joules of energy of all gases in a mixture.
		/// </summary>
		/// <param name="mixture">The mixture for which the gas energy is calculated.</param>
		/// <returns></returns>
		public static MoleEnergy GetGasEnergy(this ref GasMixture mixture) =>
			new(mixture.Moles(AtmosphereHelper.MatterState.Gas).Sum(x => x.Energy.ToDouble()));

		/// <summary>
		/// Calculate the total heat capacity of all gases in a mixture.
		/// </summary>
		/// <param name="mixture">The mixture for which the gas heat capacity is calculated.</param>
		/// <returns></returns>
		public static HeatCapacity GetGasHeatCapacity(this ref GasMixture mixture) =>
			new(mixture.Moles(AtmosphereHelper.MatterState.Gas).Sum(x => x.HeatCapacity.ToDouble()));

		public static TemperatureKelvin GetGasTemperature(this ref GasMixture mixture) =>
			new(mixture.GetGasEnergy(), mixture.GetGasHeatCapacity());

		/// <summary>
		/// Calculate the total gases heat capacity ratio of all gases in a mixture.
		/// </summary>
		/// <param name="mixture">The mixture for which the gases heat capacity ratio is calculated.</param>
		/// <returns></returns>
		public static double GetGasHeatCapacityRatio(this ref GasMixture mixture)
		{
			var gasses = mixture.GetTotalMolesGasses;
			return gasses < Chemistry.MINIMUM_QUANTITY_MOLES
				? Chemistry.TriatomicDegreesOfFreedom
				: Chemistry.DiatomicDegreesOfFreedom * (mixture.Oxygen.Quantity / gasses).ToDouble()
				+ Chemistry.DiatomicDegreesOfFreedom * (mixture.Nitrogen.Quantity / gasses).ToDouble()
				+ Chemistry.TriatomicDegreesOfFreedom * (mixture.CarbonDioxide.Quantity / gasses).ToDouble()
				+ Chemistry.TriatomicDegreesOfFreedom * (mixture.Volatiles.Quantity / gasses).ToDouble()
				+ Chemistry.PolyatomicDegreesOfFreedom * (mixture.Pollutant.Quantity / gasses).ToDouble()
				+ Chemistry.TriatomicDegreesOfFreedom * (mixture.NitrousOxide.Quantity / gasses).ToDouble()
				+ Chemistry.TriatomicDegreesOfFreedom * (mixture.Steam.Quantity / gasses).ToDouble()
				+ Chemistry.DiatomicDegreesOfFreedom * (mixture.Hydrogen.Quantity / gasses).ToDouble();
		}

		/// <summary>
		/// Calculate the pressure of all gases in a mixture at given volume and temperature.
		/// </summary>
		/// <remarks>Volume must be greater than the volume of liquids in the gas mixture.</remarks>
		/// <param name="mixture">The mixture for which the pressure is calculated.</param>
		/// <param name="volume">The volume for which the pressure is calculated.</param>
		/// <param name="temperature">The temperature for which the pressure is calculated.</param>
		/// <returns>The pressure of given gas at given volume and temperature.</returns>
		public static PressurekPa GetGasPressure(this ref GasMixture mixture, VolumeLitres volume, TemperatureKelvin temperature)
		{
			Debug.Assert(volume > mixture.VolumeLiquids, "Volume must be greater than volume of liquids in the gas mixture.");
			// P = nRT/V
			return new PressurekPa(mixture.GetTotalMolesGasses, temperature, volume - mixture.VolumeLiquids);
		}

		/// <summary>
		/// Get the volume that gases in a mixture would occupy at target pressure and temperature during adiabatic expansion or compression.
		/// </summary>
		/// <param name="mixture">The mixture for which the adiabatic volume is calculated.</param>
		/// <param name="initialVolume">The initial volume of the gas mixture.</param>
		/// <param name="targetPressure">The target pressure for which the volume is calculated.</param>
		/// <returns></returns>
		public static VolumeLitres GetAdiabaticVolumeForTargetPressure(this ref GasMixture mixture, VolumeLitres initialVolume, PressurekPa targetPressure)
		{
			var gasHeatCapacity = mixture.GetGasHeatCapacity();
			if (gasHeatCapacity.IsDenormalToNegative())
				return VolumeLitres.Zero;
			var heatCapacityRatio = mixture.GetGasHeatCapacityRatio(); // γ
			// Liquids are incompressible, so we need to subtract their volume from the initial volume.
			initialVolume -= mixture.VolumeLiquids;
			var initialPressure = GetGasPressure(ref mixture, initialVolume, mixture.GetGasTemperature());
			// V_final = V_initial * (P_initial / P_final)^(1/γ)
			return initialVolume * (float)Math.Pow((initialPressure / targetPressure).ToDouble(), 1.0 / heatCapacityRatio) + mixture.VolumeLiquids; // add back the subtracted volume of liquids.
		}

		/// <summary>
		/// Calculate the final pressure of a mixture after adiabatic compression or expansion.
		/// </summary>
		/// <remarks>The initial and final volume must be greater than the volume of liquids in the mixture.</remarks>
		/// <param name="mixture">The mixture for which the adiabatic pressure is calculated.</param>
		/// <param name="initialVolume"></param>
		/// <param name="finalVolume"></param>
		/// <returns></returns>
		public static PressurekPa GetAdiabaticPressure(this ref GasMixture mixture, VolumeLitres initialVolume, VolumeLitres finalVolume)
		{
			initialVolume -= mixture.VolumeLiquids;
			Debug.Assert(initialVolume > VolumeLitres.Zero, "Initial volume must be greater than or equal to volume of liquids in the gas mixture.");
			finalVolume -= mixture.VolumeLiquids;
			Debug.Assert(finalVolume > VolumeLitres.Zero, "Final volume must be greater than or equal to volume of liquids in the gas mixture.");
			var gasHeatCapacity = mixture.GetGasHeatCapacity();
			if (gasHeatCapacity.IsDenormalToNegative())
				return PressurekPa.Zero;
			var heatCapacityRatio = mixture.GetGasHeatCapacityRatio(); // γ
			var initialPressure = GetGasPressure(ref mixture, initialVolume, mixture.GetGasTemperature());
			// P_final = P_initial * (V_initial / V_final)^γ
			return initialPressure * Math.Pow((initialVolume / finalVolume).ToDouble(), heatCapacityRatio);
		}

		/// <summary>
		/// Calculate the work done on (positive value) or by (negative value) the gas during adiabatic compression or expansion of a mixture.
		/// </summary>
		/// <remarks>The initial and final volume must be greater than the volume of liquids in the gas mixture.</remarks>
		/// <param name="mixture">The mixture for which the amount of work is calculated.</param>
		/// <param name="initialVolume">The initial volume before the adiabatic process.</param>
		/// <param name="finalVolume">The target volume after the adiabatic process.</param>
		/// <returns></returns>
		public static MoleEnergy GetAdiabaticProcessWorkDone(this ref GasMixture mixture, VolumeLitres initialVolume, VolumeLitres finalVolume)
		{
			initialVolume -= mixture.VolumeLiquids;
			Debug.Assert(initialVolume > VolumeLitres.Zero, "Initial volume must be greater than or equal to volume of liquids in the gas mixture.");
			finalVolume -= mixture.VolumeLiquids;
			Debug.Assert(finalVolume > VolumeLitres.Zero, "Final volume must be greater than or equal to volume of liquids in the gas mixture.");
			var gasHeatCapacity = mixture.GetGasHeatCapacity();
			if (gasHeatCapacity.IsDenormalToNegative())
				return MoleEnergy.Zero;
			// W = (1 / (1 - γ)) * (P_final * V_final - P_initial * V_initial)
			// inversed for our purposes
			return new MoleEnergy(-(1.0f / (1.0f - mixture.GetGasHeatCapacityRatio()))
				* (mixture.GetAdiabaticPressure(initialVolume, finalVolume).ToDouble() * finalVolume.ToDouble()
					- mixture.GetGasPressure(initialVolume, mixture.GetGasTemperature()).ToDouble() * initialVolume.ToDouble()));
		}

		public static GasMixture Split(this ref GasMixture gas, float fraction)
		{
			Debug.Assert(fraction >= 0.0f && fraction <= 1.0f, "Fraction must be in range [0, 1].");
			var result = gas;
			gas.Scale(1-fraction);
			result.Scale(fraction);
			return result;
		}

		/// <summary>
		/// Calculate the average molar mass of all gases in a mixture.
		/// </summary>
		/// <param name="mixture"></param>
		/// <returns>The average molar mass of all gases in a mixture.</returns>
		public static double GetAverageGasMolarMass(this ref GasMixture mixture)
		{
			var totalMoles = mixture.GetTotalMolesGasses;
			if (totalMoles < Chemistry.MINIMUM_QUANTITY_MOLES)
				return 0.0;
			return mixture.Moles(AtmosphereHelper.MatterState.Gas).Sum(x => x.GetMass()) / totalMoles.ToDouble();
		}

		public static double GetAverageMolarMass(this ref GasMixture mixture)
		{
			var totalMoles = mixture.GetTotalMolesGassesAndLiquids;
			if (totalMoles < Chemistry.MINIMUM_QUANTITY_MOLES)
				return 0.0;
			return mixture.Moles().Sum(x => x.GetMass()) / totalMoles.ToDouble();
		}

		/// <summary>
		/// Does adiabatic pumping of a mixture from intake to exhaust. Moves fraction of gases and liquids from intake to exhaust, removes energy from intake mixture and adds energy to exhaust mixture.
		/// </summary>
		/// <param name="intake">The intake mixture.</param>
		/// <param name="exhaust">The exhaust mixture.</param>
		/// <param name="inputVolume">The volume of the intake atmosphere.</param>
		/// <param name="outputVolume">The volume of the exhaust atmosphere.</param>
		/// <param name="pumpVolume">The internal volume of the pump. The resulting work done by pump depends on this argument.</param>
		/// <returns>The work done by pumping (can be negative).</returns>
		public static MoleEnergy DoAdiabaticPumping(ref GasMixture intake, ref GasMixture exhaust, VolumeLitres inputVolume, VolumeLitres outputVolume, VolumeLitres pumpVolume)
		{
			// First phase - adiabatic expansion.
			// At initial state the intake is open, the piston is at the top dead center.
			// At end of this phase the piston is at the bottom dead center, pushed by the gas from intake.

			// The amount of work done by the gas during adiabatic compression, where the gas is expanding and pushing the piston.
			// This work will be reused at second stroke when gas will be pushed out of the cylinder by the piston.
			var firstPhaseWork = intake.GetAdiabaticProcessWorkDone(inputVolume, inputVolume + pumpVolume);
			Debug.Assert(firstPhaseWork <= MoleEnergy.Zero, "Work done by the gas during adiabatic expansion must be zero or negative.");
			intake.AddEnergy(firstPhaseWork);

			// Transition from phase 1 to phase 2: close the intake, equalize pressure in the cylinder and exhaust and open the exhaust.
			var movedGas = intake; // The gas in the cylinder. Copy from intake gas.
			movedGas.Scale((pumpVolume / (inputVolume + pumpVolume)).ToDouble()); // ... and scale down to ratio of volumes.
			intake.Remove(movedGas); // Remove the gas in the cylinder from the intake atmosphere.
			VolumeLitres transitionVolume = pumpVolume;
			// Check if there's any gas in the exhaust.
			if (exhaust.GetTotalMolesGasses > Chemistry.MINIMUM_QUANTITY_MOLES)
			{
				// If there is - equalize the pressure in the cylinder and exhaust by choosing the volume at which the pressure is the same.
				transitionVolume = movedGas.GetAdiabaticVolumeForTargetPressure(pumpVolume, exhaust.GetGasPressure(outputVolume, exhaust.GetGasTemperature()));
			}
			var compressionWork = MoleEnergy.Zero;
			// If the volume of the gas in the cylinder is less than the volume of the pump, the gas will be compressed.
			if (transitionVolume < pumpVolume)
			{
				compressionWork = movedGas.GetAdiabaticProcessWorkDone(pumpVolume, transitionVolume);
				movedGas.AddEnergy(compressionWork); // And therefore the gas will have energy added by the pump.
			} // Otherwise the gas will be released into the exhaust directly.
			exhaust.Add(movedGas); // Open the exhaust and mix the gas from the cylinder with the gas in the exhaust.

			// Second phase - adiabatic compression.
			// At initial state the exhaust is open, the piston is in between the top and bottom dead centers.
			// At end of this phase the piston is at the top dead center, pushed by motor, reusing the work done by the gas during the first phase.
			var secondPhaseWork = exhaust.GetAdiabaticProcessWorkDone(outputVolume + PhysicsMath.Min(pumpVolume, transitionVolume), outputVolume);
			//Debug.Assert(secondPhaseWork >= 0.0f, "Work done by the gas during adiabatic compression must be zero or positive.");
			exhaust.AddEnergy(secondPhaseWork);
			if (firstPhaseWork + compressionWork + secondPhaseWork < MoleEnergy.Zero)
			{
				// Pump work by pressure difference alone and does not need power input.
				var extraEnergy = -firstPhaseWork - compressionWork - secondPhaseWork;
				// Return extra energy to exhaust first, to stop temperature from rising.
				//exhaust.AddEnergy(-Math.Min(extraEnergy, secondPhaseWork + compressionWork));
				//extraEnergy += Math.Min(extraEnergy, secondPhaseWork + compressionWork);
				var ratio = intake.GetTotalMolesGasses / (intake.GetTotalMolesGasses + exhaust.GetTotalMolesGasses);
				intake.AddEnergy(extraEnergy * new MoleEnergy(ratio.ToDouble()));
				exhaust.AddEnergy(extraEnergy * new MoleEnergy(1 - ratio.ToDouble()));
			}

			return firstPhaseWork + compressionWork + secondPhaseWork;
		}

		public static readonly float DischargeCoefficient = 0.61f;
		/// <summary>
		/// Calculate the flow rate of gas through an orifice between two sides.
		/// </summary>
		/// <param name="lowPressureSide">A low pressure side.</param>
		/// <param name="highPressureSide">A high pressure side.</param>
		/// <param name="orificeArea">An area of orifice.</param>
		/// <param name="lowPressureVolume">Volume of the low pressure side.</param>
		/// <param name="highPressureVolume">Volume of the high pressure side.</param>
		/// <returns></returns>
		public static MoleQuantity GetMatterFlowRateThroughOrifice(ref GasMixture lowPressureSide, ref GasMixture highPressureSide, double orificeArea, VolumeLitres lowPressureVolume, VolumeLitres highPressureVolume)
		{
			var lowPTemperature = lowPressureSide.GetGasTemperature();
			var highPTemperature = highPressureSide.GetGasTemperature();
			// we use high pressure heat capacity ratio because it will be the source of moving gas,
			// so we consider that diffusion of gas from low pressure side would be negligible.
			// That is true at least for when critical pressure ratio is important.
			var heatCapacityRatio = highPressureSide.GetGasHeatCapacityRatio();
			var criticalRatio = Math.Pow(2 / (heatCapacityRatio + 1), heatCapacityRatio / (heatCapacityRatio - 1));
			var lowPressure = lowPressureSide.GetGasPressure(lowPressureVolume, lowPTemperature).ToDouble() * 1000; // convert to Pa
			var highPressure = highPressureSide.GetGasPressure(highPressureVolume, highPTemperature).ToDouble() * 1000;
			var pressureRatio = lowPressure / highPressure;
			Debug.Assert(highPressure > lowPressure, $"{nameof(highPressureSide)} should have higher pressure that {nameof(lowPressureSide)}!");
			double massFlowRate;
			if (pressureRatio < criticalRatio)
			{
				// Subsonic flow
				massFlowRate = DischargeCoefficient * orificeArea * highPressure
					* Math.Sqrt((2 * heatCapacityRatio) / (Chemistry.IdealGasConstant * highPressureSide.Temperature.ToDouble() * (heatCapacityRatio - 1))
					* (Math.Pow(pressureRatio, 2 / heatCapacityRatio) - Math.Pow(pressureRatio, (heatCapacityRatio + 1) / heatCapacityRatio)));
			}
			else
			{
				// Choked flow
				massFlowRate = DischargeCoefficient * orificeArea * highPressure
					* Math.Sqrt(heatCapacityRatio / (Chemistry.IdealGasConstant * highPressureSide.Temperature.ToDouble()))
					* Math.Pow(2 / (heatCapacityRatio + 1), (heatCapacityRatio + 1) / (2 * (heatCapacityRatio - 1)));
			}
			return new MoleQuantity(massFlowRate / highPressureSide.GetAverageGasMolarMass());
		}

		private static void Swap<T>(ref T left, ref T right)
		{
			(left, right) = (right, left);
		}
	}
}
