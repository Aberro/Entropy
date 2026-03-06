using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Util;
using Entropy.Common.Utils;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.UIElements;
using static Assets.Scripts.Atmospherics.AtmosphereHelper;

namespace Entropy.Adiabatics;

public static class PhysicsHelper
{
	/// <summary>
	/// A helper enumerator class that avoids memory allocations and orders constituents of a given gas mixture by matter state - gases first, 
	/// then liquids.
	/// </summary>
	public struct MolesEnumerator : IEnumerator<Mole>
	{
		private static Chemistry.GasType[] _gases =
		{
			Chemistry.GasType.Oxygen,
			Chemistry.GasType.Nitrogen,
			Chemistry.GasType.CarbonDioxide,
			Chemistry.GasType.Volatiles,
			Chemistry.GasType.Pollutant,
			Chemistry.GasType.NitrousOxide,
			Chemistry.GasType.Steam,
			Chemistry.GasType.Hydrogen,
		};
		private static Chemistry.GasType[] _liquids =
		{
			Chemistry.GasType.Water,
			Chemistry.GasType.LiquidNitrogen,
			Chemistry.GasType.LiquidOxygen,
			Chemistry.GasType.LiquidVolatiles,
			Chemistry.GasType.LiquidCarbonDioxide,
			Chemistry.GasType.LiquidPollutant,
			Chemistry.GasType.LiquidNitrousOxide,
			Chemistry.GasType.LiquidHydrogen,
			Chemistry.GasType.PollutedWater,
		};


		private GasMixture _gasMixture;
		private int _current;
		private MatterState _matterState;
		public Mole Current { get; private set; }

		readonly object? IEnumerator.Current => this.Current;

		public MolesEnumerator(ref GasMixture gasMixture, MatterState matterState = MatterState.All)
		{
			_gasMixture = gasMixture;
		}
		public readonly void Dispose() { }
		public bool MoveNext()
		{
			if (_matterState is MatterState.None)
				return false;
			if (_current == -1) // enumerator is uninitialized
				// either start with gases or liquids
				_current = _matterState is MatterState.Gas or MatterState.All ? 0 : _gases.Length;
			_current++;
			if (_current < _gases.Length)
			{
				Current = _gasMixture.GetMoleValue(_gases[_current]);
				return true;
			}
			if (_matterState is MatterState.Liquid or MatterState.All && _current < (_gases.Length + _liquids.Length))
			{
				Current = _gasMixture.GetMoleValue(_liquids[_current - _gases.Length]);
				return true;
			}
			else
				return false;
		}
		public void Reset()
		{
			_current = -1;
			Current = default;
		}
	}

	private static readonly double DoublePrecision = Math.Pow(2.0, -53.0);
	private static readonly double PositiveDoublePrecision = 2.0 * DoublePrecision;
	private static readonly double DefaultDoubleAccuracy = DoublePrecision * 10.0;
	/// <summary>
	/// Gas constant in J/(mol*K)
	/// </summary>
	public const double R = 8.314462618f;

	/// <summary>
	/// Calculates the sum of values projected from each mole in the specified gas mixture, optionally filtered by matter
	/// state.
	/// </summary>
	/// <remarks>Does not cause memory allocations.</remarks>
	/// <param name="mixture">The gas mixture to enumerate. The mixture is passed by reference and is not modified by this method.</param>
	/// <param name="selector">A function that projects a value from each mole in the mixture. Cannot be null.</param>
	/// <param name="matterState">The matter state to filter moles by. Only moles matching this state are included in the sum. The default is
	/// MatterState.All.</param>
	/// <returns>The sum of the values returned by the selector function for each mole in the specified matter state. Returns 0 if
	/// no moles match the filter.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the selector function is null.</exception>
	[SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
	public static double Sum(this ref GasMixture mixture, Func<Mole, double> selector, MatterState matterState = MatterState.All)
	{
		ArgumentNullException.ThrowIfNull(selector);
		var enumerator = new MolesEnumerator(ref mixture, matterState);
		double result = 0;
		while (enumerator.MoveNext())
			result += selector(enumerator.Current);
		return result;
	}

	public static void AddEnergy(this ref GasMixture mixture, MoleEnergy energy) =>
		mixture.AddEnergy(energy);
	public static MoleEnergy RemoveEnergy(ref this GasMixture mixture, MoleEnergy energy)
	{
		var energyDebug = mixture.TotalEnergy;
		var result = mixture.RemoveEnergy(energy);
		Debug.Assert(mixture.TotalEnergy < energyDebug);
		return result;
	}
	public static void Add(ref GasMixture mixture, ref GasMixture addMixture) =>
		mixture.Add(addMixture);
	public static void Remove(ref GasMixture mixture, ref GasMixture removeMixture) =>
		mixture.Remove(removeMixture);
	public static GasMixture Remove(ref GasMixture mixture, MoleQuantity amount, MatterState matterState) =>
		mixture.Remove(amount, matterState);
	/// <summary>
	/// Calculate the total joules of energy of all gases in a mixture.
	/// </summary>
	/// <param name="mixture">The mixture for which the gas energy is calculated.</param>
	/// <returns></returns>
	public static MoleEnergy GetGasEnergy(this ref GasMixture mixture) =>
		new(mixture.Sum(x => x.Energy.ToDouble(), MatterState.Gas));

	/// <summary>
	/// Calculate the total heat capacity of all gases in a mixture.
	/// </summary>
	/// <param name="mixture">The mixture for which the gas heat capacity is calculated.</param>
	/// <returns></returns>
	public static HeatCapacity GetGasHeatCapacity(this ref GasMixture mixture) =>
		new(mixture.Sum(x => x.HeatCapacity.ToDouble(), MatterState.Gas));

	public static TemperatureKelvin GetGasTemperature(this ref GasMixture mixture)
	{
		var gasEnergy = mixture.GetGasEnergy();
		return gasEnergy > MoleEnergy.Zero ? new TemperatureKelvin(gasEnergy, mixture.GetGasHeatCapacity()) : TemperatureKelvin.Zero;
	}

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
			: (Chemistry.DiatomicDegreesOfFreedom * (mixture.Oxygen.Quantity / gasses).ToDouble())
			+ (Chemistry.DiatomicDegreesOfFreedom * (mixture.Nitrogen.Quantity / gasses).ToDouble())
			+ (Chemistry.TriatomicDegreesOfFreedom * (mixture.CarbonDioxide.Quantity / gasses).ToDouble())
			+ (Chemistry.TriatomicDegreesOfFreedom * (mixture.Volatiles.Quantity / gasses).ToDouble())
			+ (Chemistry.PolyatomicDegreesOfFreedom * (mixture.Pollutant.Quantity / gasses).ToDouble())
			+ (Chemistry.TriatomicDegreesOfFreedom * (mixture.NitrousOxide.Quantity / gasses).ToDouble())
			+ (Chemistry.TriatomicDegreesOfFreedom * (mixture.Steam.Quantity / gasses).ToDouble())
			+ (Chemistry.DiatomicDegreesOfFreedom * (mixture.Hydrogen.Quantity / gasses).ToDouble());
	}

	/// <summary>
	/// Calculate the pressure of all gases in a mixture at given volume and temperature.
	/// </summary>
	/// <remarks>Volume must be greater than the volume of liquids in the gas mixture.</remarks>
	/// <param name="mixture">The mixture for which the pressure is calculated.</param>
	/// <param name="volume">The volume for which the pressure is calculated.</param>
	/// <param name="temperature">The temperature for which the pressure is calculated.</param>
	/// <returns>The pressure of given gas at given volume and temperature.</returns>
	public static PressurekPa GetGasPressure(this ref GasMixture mixture, VolumeLitres volume)
	{
		if (volume <= mixture.VolumeLiquids)
			throw new ApplicationException("Volume must be greater than volume of liquids in the gas mixture.");
		// P = nRT/V
		return new PressurekPa(mixture.GetTotalMolesGasses, mixture.GetGasTemperature(), volume - mixture.VolumeLiquids);
	}

	/// <summary>
	/// Get the volume that gases in a mixture would occupy at given pressure and temperature during adiabatic expansion or compression.
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
		var initialPressure = GetGasPressure(ref mixture, initialVolume);
		// V_final = V_initial * (P_initial / P_final)^(1/γ)
		var result = (initialVolume * (double) Math.Pow((initialPressure / targetPressure).ToDouble(), 1.0 / heatCapacityRatio)) + mixture.VolumeLiquids; // add back the subtracted volume of liquids.
		Debug.Assert(result.ToDouble() > 0);
		return result;
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
		var initialPressure = GetGasPressure(ref mixture, initialVolume);
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
		finalVolume -= mixture.VolumeLiquids;
		Debug.Assert(initialVolume > VolumeLitres.Zero, "Initial volume must be greater than or equal to volume of liquids in the gas mixture.");
		Debug.Assert(finalVolume >= VolumeLitres.Zero, "Final volume must be greater than or equal to volume of liquids in the gas mixture.");
		var gasHeatCapacity = mixture.GetGasHeatCapacity();
		if (gasHeatCapacity.IsDenormalToNegative())
			return MoleEnergy.Zero;
		var P1 = mixture.GetGasPressure(initialVolume).ToDouble();
		var V1 = initialVolume.ToDouble();
		var V2 = finalVolume.ToDouble();
		var γ = mixture.GetGasHeatCapacityRatio();
		var W = (P1 * V1 / (1 - γ)) * (Math.Pow(V1 / V2, γ - 1) - 1);
		// inversed for our purposes
		return new MoleEnergy(-W);
	}

	public static VolumeLitres GetAdiabaticVolumeForWorkDone(this ref GasMixture mixture, MoleEnergy workDone, VolumeLitres initialVolume)
	{
		var W = workDone.ToDouble();
		var γ = mixture.GetGasHeatCapacityRatio();
		var V1 = initialVolume.ToDouble();
		var P1 = mixture.GetGasPressure(initialVolume).ToDouble();
		var ΔV = V1 * ((1 / Math.Pow((W * (1 - γ) / (P1 * V1)) + 1, 1 / (γ - 1))) - 1);
		return new VolumeLitres(ΔV);
	}

	public static GasMixture Split(this ref GasMixture gas, double fraction, MatterState matterState = MatterState.All)
	{
		if (fraction < 0.0f || fraction > 1.0f)
			throw new ArgumentOutOfRangeException(nameof(fraction), "Fraction must be in range [0, 1].");
		var result = gas;
		gas.Scale(1 - fraction, matterState);
		result.Scale(fraction, matterState);
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
		return mixture.Sum(x => x.GetMass()) / totalMoles.ToDouble();
	}

	public static double GetAverageMolarMass(this ref GasMixture mixture)
	{
		var totalMoles = mixture.GetTotalMolesGassesAndLiquids;
		if (totalMoles < Chemistry.MINIMUM_QUANTITY_MOLES)
			return 0.0;
		return mixture.Sum(x => x.GetMass(), MatterState.Gas) / totalMoles.ToDouble();
	}
	public static MoleEnergy SimulateAdiabaticPumping(GasMixture intake, GasMixture exhaust, MatterState matterState, out GasMixture pumpedGas, VolumeLitres inputVolume, VolumeLitres outputVolume, VolumeLitres pumpVolume, MoleEnergy powerLimit) =>
		DoAdiabaticPumping(ref intake, ref exhaust, matterState, out pumpedGas, inputVolume, outputVolume, pumpVolume, powerLimit);

	/// <summary>
	/// Does the same as <see cref="DoAdiabaticPumping"/>, but without changing gas mixtures, only returning the work done.
	/// </summary>
	/// <param name="intake">The intake mixture.</param>
	/// <param name="exhaust">The exhaust mixture.</param>
	/// <param name="inputVolume">The volume of the intake atmosphere.</param>
	/// <param name="outputVolume">The volume of the exhaust atmosphere.</param>
	/// <param name="pumpVolume">The internal volume of the pump. The resulting work done by pump depends on this argument.</param>
	/// <returns>The work done by pumping (can be negative).</returns>
	public static MoleEnergy SimulateAdiabaticPumping(
		GasMixture intake,
		GasMixture exhaust,
		MatterState matterState,
		out GasMixture pumpedGas,
		VolumeLitres inputVolume,
		VolumeLitres outputVolume,
		VolumeLitres pumpVolume,
		MoleEnergy powerLimit,
		bool geared,
		PressurekPa inputMinPressure,
		PressurekPa outputMaxPressure) =>
		DoAdiabaticPumping(ref intake, ref exhaust, matterState, out pumpedGas, inputVolume, outputVolume, pumpVolume, powerLimit, geared, inputMinPressure, outputMaxPressure);

	/// <summary>
	/// Does adiabatic pumping of a mixture from intake to exhaust. Moves fraction of gases and liquids from intake to exhaust, removes energy from intake mixture and adds energy to exhaust mixture.
	/// </summary>
	/// <param name="intake">The intake mixture.</param>
	/// <param name="exhaust">The exhaust mixture.</param>
	/// <param name="matterState">The matter state to pump. Only supports All and Gas.</param>
	/// <param name="pumpedGas">The gas pumped from intake to exhaust output.</param>
	/// <param name="inputVolume">The volume of the intake atmosphere.</param>
	/// <param name="outputVolume">The volume of the exhaust atmosphere.</param>
	/// <param name="pumpVolume">The internal volume of the pump. The resulting work done by pump depends on this argument.</param>
	/// <returns>The work done by pumping (can be negative).</returns>
	public static MoleEnergy DoAdiabaticPumping(
		ref GasMixture intake,
		ref GasMixture exhaust,
		MatterState matterState,
		out GasMixture pumpedGas,
		VolumeLitres inputVolume,
		VolumeLitres outputVolume,
		VolumeLitres pumpVolume,
		MoleEnergy powerLimit)
	{
		if (matterState is MatterState.Liquid)
			throw new NotSupportedException("Liquid adiabatic pumping is not supported! Use AtmosphericsHelper.MoveVolumeLiquids instead!");
		var totalEnergy = intake.TotalEnergy + exhaust.TotalEnergy;
		var totalQuantity = intake.GetQuantity + exhaust.GetQuantity;
		// First phase - adiabatic expansion.
		// At initial state the intake is open, the piston is at the top dead center.
		// At end of this phase the piston is at the bottom dead center, pushed by the gas from intake.

		// The amount of work done by the gas during adiabatic compression, where the gas is expanding and pushing the piston.
		// This work will be reused at second stroke when gas will be pushed out of the cylinder by the piston.
		var firstPhaseWork = intake.GetAdiabaticProcessWorkDone(inputVolume, inputVolume + pumpVolume);
		// firstPhase (expansion) is always negative, as gas pushes the piston, so we add it to powerLimit,
		// as the work done by gas is going to be recuperated.
		var powerLimitRecuperative = powerLimit - firstPhaseWork;
		Debug.Assert(firstPhaseWork <= MoleEnergy.Zero, "Work done by the gas during adiabatic expansion must be zero or negative.");
		Debug.Assert(intake.GetGasEnergy() + firstPhaseWork > MoleEnergy.Zero, "Adiabatic process removed all energy");
		intake.AddEnergy(firstPhaseWork);
		var currentTotalEnergyDifference = totalEnergy.ToDouble() - (intake.TotalEnergy + exhaust.TotalEnergy - firstPhaseWork).ToDouble();
		Debug.Assert(Math.Abs(currentTotalEnergyDifference) < (totalEnergy.ToDouble() / 100000));
		// Transition from phase 1 to phase 2: close the intake, equalize pressure in the cylinder and exhaust and open the exhaust.
		pumpedGas = intake; // The gas in the cylinder. Copy from intake gas.

		pumpedGas.Scale((pumpVolume / (inputVolume + pumpVolume)).ToDouble(), matterState); // ... and scale down to ratio of volumes.
		intake.Remove(pumpedGas); // Remove the gas in the cylinder from the intake atmosphere.
		AddEnergy(ref pumpedGas, RemoveEnergy(ref intake, FlowWork(pumpedGas.GetTotalMolesGasses, intake.Temperature)));

		var transitionVolume = pumpVolume;
		// Check if there's any gas in the exhaust.
		if (exhaust.GetTotalMolesGasses > Chemistry.MINIMUM_QUANTITY_MOLES)
		{
			// If there is - equalize the pressure in the cylinder and exhaust by choosing the volume at which the pressure is the same.
			transitionVolume = pumpedGas.GetAdiabaticVolumeForTargetPressure(pumpVolume, exhaust.GetGasPressure(outputVolume));
		}
		var compressionWork = MoleEnergy.Zero;
		// If the volume of the gas in the cylinder is less than the volume of the pump, the gas will be compressed.
		if (transitionVolume < pumpVolume)
		{
			compressionWork = pumpedGas.GetAdiabaticProcessWorkDone(pumpVolume, transitionVolume);
		}
		// Otherwise the gas will be released into the exhaust directly.

		// Check if we can even reach the exhaust pressure
		if (compressionWork > powerLimitRecuperative)
		{
			// The exhaust pressure can't be reached, the pump efficiency is zero, just return the pumped gas back into intake
			intake.AddEnergy(-firstPhaseWork);
			Add(ref intake, ref pumpedGas);
			//intake.AddEnergy(firstPhaseWork); // return work done by intake gas back
			// and add waste power to both sides
			//intake.AddEnergy(powerLimit * 0.5);
			exhaust.AddEnergy(powerLimit/* * 0.5*/);
			Debug.Assert((intake.TotalEnergy - exhaust.TotalEnergy - totalEnergy) <= powerLimit);
			currentTotalEnergyDifference = ((totalEnergy + powerLimit) - (intake.TotalEnergy + exhaust.TotalEnergy)).ToDouble();
			Debug.Assert(Math.Abs(currentTotalEnergyDifference) < ((totalEnergy + powerLimit).ToDouble() / 100000));
			Debug.Assert(Math.Abs(totalQuantity - intake.GetQuantity - exhaust.GetQuantity) < totalQuantity / 1000000);
			return powerLimit;
		}
		if (pumpedGas.GetTotalMolesGassesAndLiquids > Chemistry.MINIMUM_VALID_TOTAL_MOLES)
			pumpedGas.AddEnergy(compressionWork); // Add adiabatic compression work done on pumped gas
		else
			exhaust.AddEnergy(compressionWork);
		currentTotalEnergyDifference = totalEnergy.ToDouble() - (intake.TotalEnergy + exhaust.TotalEnergy - firstPhaseWork + pumpedGas.TotalEnergy - compressionWork).ToDouble();
		Debug.Assert(Math.Abs(currentTotalEnergyDifference) < (totalEnergy.ToDouble() / 100000));
		powerLimitRecuperative -= compressionWork;

		// Second phase - adiabatic compression (pushing pumped gas out of the pump)
		// At initial state the exhaust is open, the piston is in between the top and bottom dead centers.
		// At end of this phase the piston is at the top dead center, pushed by motor, reusing the work done by the gas during the first phase.

		// First do a simulation to get the power of this step
		var exhaustCopy = exhaust;
		Add(ref exhaustCopy, ref pumpedGas); // Open the exhaust and mix the gas from the cylinder with the gas in the exhaust.
		var secondPhaseWork = exhaustCopy.GetAdiabaticProcessWorkDone(outputVolume + PhysicsMath.Min(pumpVolume, transitionVolume), outputVolume);
		if (secondPhaseWork > powerLimitRecuperative)
		{
			// not enough power to push all gas from the pump, slipping occurs, part of the gas returns back to intake.
			var workRatio = powerLimitRecuperative / secondPhaseWork;
			var gasRatio = workRatio.ToDouble(); // work is proportional to square of gas quantity, so we use square root of work ratio to get gas ratio.
			pumpedGas.AddEnergy(-firstPhaseWork);
			pumpedGas.AddEnergy(-compressionWork);
			//pumpedGas.AddEnergy(powerLimit);
			var slippedGas = pumpedGas.Split(1 - gasRatio);
			Add(ref exhaust, ref pumpedGas); // Add the gas we can push to the exhaust.
			exhaust.AddEnergy(powerLimit);
			Add(ref intake, ref slippedGas);
			Debug.Assert((intake.TotalEnergy - exhaust.TotalEnergy - totalEnergy) <= powerLimit);
			currentTotalEnergyDifference = ((totalEnergy + powerLimit) - (intake.TotalEnergy + exhaust.TotalEnergy)).ToDouble();
			Debug.Assert(Math.Abs(currentTotalEnergyDifference) < ((totalEnergy + powerLimit).ToDouble() / 100000));
			Debug.Assert(Math.Abs(totalQuantity - intake.GetQuantity - exhaust.GetQuantity) < totalQuantity / 1000000);
			return powerLimit;
			// Now add energy
		}
		// Now repeat on actual gas arguments
		Add(ref exhaust, ref pumpedGas);
		//Debug.Assert(secondPhaseWork >= 0.0f, "Work done by the gas during adiabatic compression must be zero or positive.");
		exhaust.AddEnergy(secondPhaseWork);
		if (firstPhaseWork + compressionWork + secondPhaseWork < MoleEnergy.Zero)
		{
			// Pump work by pressure difference alone and does not need power input.
			// But this extracts untapped energy from the gas, and the pump should not work as a power generator,
			// So we return that extra energy back to gases on both sides.
			var extraEnergy = -firstPhaseWork - compressionWork - secondPhaseWork;
			// Spread the energy proportionally to amount of gas
			var ratio = (intake.GetTotalMolesGasses / (intake.GetTotalMolesGasses + exhaust.GetTotalMolesGasses)).ToDouble();
			intake.AddEnergy(extraEnergy * new MoleEnergy(ratio));
			exhaust.AddEnergy(extraEnergy * new MoleEnergy(1 - ratio));
		}
		Debug.Assert((intake.TotalEnergy - exhaust.TotalEnergy - totalEnergy) <= powerLimit);
		currentTotalEnergyDifference = ((totalEnergy + firstPhaseWork + compressionWork + secondPhaseWork) - (intake.TotalEnergy + exhaust.TotalEnergy)).ToDouble();
		Debug.Assert(Math.Abs(currentTotalEnergyDifference) < ((totalEnergy + powerLimit).ToDouble() / 100000));
		Debug.Assert(Math.Abs(totalQuantity - intake.GetQuantity - exhaust.GetQuantity) < totalQuantity / 1000000);
		Debug.Assert(intake.VolumeLiquids < inputVolume);
		Debug.Assert(exhaust.VolumeLiquids < outputVolume);
		return firstPhaseWork + compressionWork + secondPhaseWork;
	}

	public static MoleEnergy DoAdiabaticPumping(
		ref GasMixture intake,
		ref GasMixture exhaust,
		MatterState matterState,
		out GasMixture pumpedGas,
		VolumeLitres inputVolume,
		VolumeLitres outputVolume,
		VolumeLitres pumpVolume,
		MoleEnergy powerLimit,
		bool geared)
	{
		var intakeCopy = intake;
		var exhaustCopy = exhaust;
		// Simulate pumping at unrestricted power
		// We don't use Simulate method here because we could reuse results if they fit constraints
		var power = DoAdiabaticPumping(ref intakeCopy, ref exhaustCopy, matterState, out var pumpedGasSimulated, inputVolume, outputVolume, pumpVolume, MoleEnergy.MaxValue);
		if (power < powerLimit)
		{
			// Power consumption is below limit, so accept simulation results.
			intake = intakeCopy;
			exhaust = exhaustCopy;
			pumpedGas = pumpedGasSimulated;
			return power;
		}
		if (geared)
		{
			// Restrict the volume proportionally
			//pumpVolume = new VolumeLitres(100);
			//var workDone = intake.GetAdiabaticProcessWorkDone(inputVolume, inputVolume + pumpVolume);
			//var displacementVolume = intake.GetAdiabaticVolumeForWorkDone(workDone, inputVolume);
			//var sanityCheck = intake.GetAdiabaticVolumeForWorkDone(-workDone, inputVolume);
			//Debug.Assert(Math.Abs((displacementVolume - pumpVolume).ToDouble()) < 0.01);
			intakeCopy = intake;
			exhaustCopy = exhaust;
			// Sadly, I couldn't figure out an equation for pump volume for given power limit, so I resort to doing it by optimization function.
			// Usually it takes 3 iterations, worst case observed - 5.
			if (TryFindRoot(
				(x) =>
				{
					return (SimulateAdiabaticPumping(intakeCopy, exhaustCopy, matterState, out var _, inputVolume, outputVolume, new VolumeLitres(x), MoleEnergy.MaxValue) - powerLimit).ToDouble();
				}, 0, pumpVolume.ToDouble(), 0.001, 100, out var v, out var iterations))
			{
				return DoAdiabaticPumping(ref intake, ref exhaust, matterState, out pumpedGas, inputVolume, outputVolume, new VolumeLitres(v), MoleEnergy.MaxValue);
			}
			// If that fails - which it usually doesn't do - get an estimation by GetAdiabaticVolumeForWorkDone
			// It's very imprecise, but at the very least it's better than reducing pump volume by powerRatio -
			// worst case observed is like 20% below or above power limit, whereas for powerRatio
			// it could be off by over 300%.
			var intakeWork = intake.GetAdiabaticProcessWorkDone(inputVolume, inputVolume + pumpVolume);
			var powerRatio = powerLimit / power;
			var reducedVolume = intake.GetAdiabaticVolumeForWorkDone(-intakeWork * powerRatio, inputVolume);
			return DoAdiabaticPumping(ref intake, ref exhaust, matterState, out pumpedGas, inputVolume, outputVolume, reducedVolume, MoleEnergy.MaxValue);
		}
		else
			return DoAdiabaticPumping(ref intake, ref exhaust, matterState, out pumpedGas, inputVolume, outputVolume, pumpVolume, powerLimit);
	}
	public static MoleEnergy DoAdiabaticPumping(
		ref GasMixture intake,
		ref GasMixture exhaust,
		MatterState matterState,
		out GasMixture pumpedGas,
		VolumeLitres inputVolume,
		VolumeLitres outputVolume,
		VolumeLitres pumpVolume,
		MoleEnergy powerLimit,
		bool geared,
		PressurekPa inputMinPressure,
		PressurekPa outputMaxPressure)
	{
		var intakeCopy = intake;
		var exhaustCopy = exhaust;
		var power = DoAdiabaticPumping(ref intakeCopy, ref exhaustCopy, matterState, out var pumpedGasSimulated, inputVolume, outputVolume, pumpVolume, powerLimit, geared);
		var pressureScaling = 1.0;
		var intakePressure = intake.GetGasPressure(inputVolume);
		var exhaustPressure = exhaust.GetGasPressure(outputVolume);
		var resultingIntakePressure = intakeCopy.GetGasPressure(inputVolume);
		var resultingExhaustPressure = exhaustCopy.GetGasPressure(outputVolume);
		if (resultingIntakePressure < inputMinPressure)
		{
			var targetPressureDifference = intakePressure - inputMinPressure;
			if (targetPressureDifference < PressurekPa.Zero)
			{
				// Already at minimum intake pressure, pump shuts down
				pumpedGas = default;
				return MoleEnergy.Zero;
			}
			var pressureDifference = intakePressure - resultingIntakePressure;
			pressureScaling = targetPressureDifference.ToDouble() / pressureDifference.ToDouble();
		}
		if (resultingExhaustPressure > outputMaxPressure)
		{
			var targetPressureDifference = exhaustPressure - outputMaxPressure;
			if (targetPressureDifference > PressurekPa.Zero)
			{
				// Already at maximum exhaust pressure, pump shuts down
				pumpedGas = default;
				return MoleEnergy.Zero;
			}
			var pressureDifference = exhaustPressure - resultingExhaustPressure;
			pressureScaling = targetPressureDifference.ToDouble() / pressureDifference.ToDouble();
		}
		Debug.Assert(pressureScaling >= 0 && pressureScaling <= 1);
		if (pressureScaling == 1)
		{
			// Pressures are within limits, so accept simulation results.
			intake = intakeCopy;
			exhaust = exhaustCopy;
			pumpedGas = pumpedGasSimulated;
			return power;
		}
		var reducedVolume = pumpVolume * pressureScaling * 0.50;
		var reducedPower = powerLimit * pressureScaling * 0.50;
		return DoAdiabaticPumping(ref intake, ref exhaust, matterState, out pumpedGas, inputVolume, outputVolume, reducedVolume, reducedPower, geared);
	}
	public static MoleEnergy DoAdiabaticPumpingIterative(
		ref GasMixture intake,
		ref GasMixture exhaust,
		MatterState matterState,
		out GasMixture pumpedGas,
		VolumeLitres inputVolume,
		VolumeLitres outputVolume,
		VolumeLitres pumpVolume,
		MoleEnergy powerLimit,
		bool geared,
		int iterations = 10)
	{
		var powerSum = MoleEnergy.Zero;
		pumpedGas = default;
		for (var i = 0; i < iterations; i++)
		{
			powerSum += DoAdiabaticPumping(
				ref intake,
				ref exhaust,
				matterState,
				out var pumpedGasIteration,
				inputVolume,
				outputVolume,
				pumpVolume,
				powerLimit,
				geared);
			pumpedGas.Add(pumpedGasIteration);
		}
		return powerSum / iterations;
	}
	public static MoleEnergy DoAdiabaticPumpingIterative(
		ref GasMixture intake,
		ref GasMixture exhaust,
		MatterState matterState,
		out GasMixture pumpedGas,
		VolumeLitres inputVolume,
		VolumeLitres outputVolume,
		VolumeLitres pumpVolume,
		MoleEnergy powerLimit,
		bool geared,
		PressurekPa inputMinPressure,
		PressurekPa outputMaxPressure,
		int iterations = 10)
	{
		var powerSum = MoleEnergy.Zero;
		pumpedGas = default;
		for (var i = 0; i < iterations; i++)
		{
			powerSum += DoAdiabaticPumping(
				ref intake,
				ref exhaust,
				matterState,
				out var pumpedGasIteration,
				inputVolume,
				outputVolume,
				pumpVolume,
				powerLimit,
				geared,
				inputMinPressure,
				outputMaxPressure);
			Add(ref pumpedGas, ref pumpedGasIteration);
		}
		return powerSum / iterations;
	}
	public static MoleEnergy DoAdiabaticPumpingIterative(
		Atmosphere intake,
		Atmosphere exhaust,
		MatterState matterState,
		VolumeLitres pumpVolume,
		MoleEnergy powerLimit,
		bool geared,
		int iterations = 10)
	{
		if (intake is null || exhaust is null)
			return MoleEnergy.Zero;
		return DoAdiabaticPumpingIterative(
			ref intake.GasMixture,
			ref exhaust.GasMixture,
			matterState,
			out var _,
			intake.Volume,
			exhaust.Volume,
			pumpVolume,
			powerLimit,
			geared,
			iterations);
	}
	public static MoleEnergy DoAdiabaticPumpingIterative(
		Atmosphere intake,
		Atmosphere exhaust,
		MatterState matterState,
		out GasMixture pumpedGas,
		VolumeLitres pumpVolume,
		MoleEnergy powerLimit,
		bool geared,
		int iterations = 10)
	{
		pumpedGas = GasMixtureHelper.Invalid;
		if (intake is null || exhaust is null)
			return MoleEnergy.Zero;
		return DoAdiabaticPumpingIterative(ref intake.GasMixture, ref exhaust.GasMixture, matterState, out pumpedGas, intake.Volume, exhaust.Volume, pumpVolume, powerLimit, geared, iterations);
	}
	public static MoleEnergy DoAdiabaticPumpingIterative(
		Atmosphere intake,
		Atmosphere exhaust,
		MatterState matterState,
		VolumeLitres pumpVolume,
		MoleEnergy powerLimit,
		bool geared,
		PressurekPa inputMinPressure,
		PressurekPa outputMaxPressure,
		int iterations = 10)
	{
		if (intake is null || exhaust is null)
			return MoleEnergy.Zero;
		return DoAdiabaticPumpingIterative(
			ref intake.GasMixture,
			ref exhaust.GasMixture,
			matterState,
			out var _,
			intake.Volume,
			exhaust.Volume,
			pumpVolume,
			powerLimit,
			geared,
			inputMinPressure,
			outputMaxPressure,
			iterations);
	}
	public static MoleEnergy DoAdiabaticPumpingIterative(
		Atmosphere intake,
		Atmosphere exhaust,
		MatterState matterState,
		out GasMixture pumpedGas,
		VolumeLitres pumpVolume,
		MoleEnergy powerLimit,
		bool geared,
		PressurekPa inputMinPressure,
		PressurekPa outputMaxPressure,
		int iterations = 10)
	{
		pumpedGas = GasMixtureHelper.Invalid;
		if (intake is null || exhaust is null)
			return MoleEnergy.Zero;
		return DoAdiabaticPumpingIterative(
			ref intake.GasMixture,
			ref exhaust.GasMixture,
			matterState,
			out pumpedGas,
			intake.Volume,
			exhaust.Volume,
			pumpVolume,
			powerLimit,
			geared,
			inputMinPressure,
			outputMaxPressure,
			iterations);
	}

	public static readonly double DischargeCoefficient = 0.61f;
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
		// we use high pressure heat capacity ratio because it will be the source of moving gas,
		// so we consider that diffusion of gas from low pressure side would be negligible.
		// That is true at least for when critical pressure ratio is important.
		var heatCapacityRatio = highPressureSide.GetGasHeatCapacityRatio();
		var criticalRatio = Math.Pow(2 / (heatCapacityRatio + 1), heatCapacityRatio / (heatCapacityRatio - 1));
		var lowPressure = lowPressureSide.GetGasPressure(lowPressureVolume).ToDouble() * 1000; // convert to Pa
		var highPressure = highPressureSide.GetGasPressure(highPressureVolume).ToDouble() * 1000;
		var pressureRatio = lowPressure / highPressure;
		Debug.Assert(highPressure > lowPressure, $"{nameof(highPressureSide)} should have higher pressure that {nameof(lowPressureSide)}!");
		double massFlowRate;
		if (pressureRatio > criticalRatio)
		{
			// Subsonic flow
			massFlowRate = DischargeCoefficient * orificeArea * (highPressure - lowPressure)
				* Math.Sqrt((2 * heatCapacityRatio) / (Chemistry.IdealGasConstant * highPressureSide.Temperature.ToDouble() * (heatCapacityRatio - 1))
				* (Math.Pow(pressureRatio, 2 / heatCapacityRatio) - Math.Pow(pressureRatio, (heatCapacityRatio + 1) / heatCapacityRatio)));
		}
		else
		{
			// Choked flow
			massFlowRate = DischargeCoefficient * orificeArea * (highPressure - lowPressure)
				* Math.Sqrt(heatCapacityRatio / (Chemistry.IdealGasConstant * highPressureSide.Temperature.ToDouble()))
				* Math.Pow(2 / (heatCapacityRatio + 1), (heatCapacityRatio + 1) / (2 * (heatCapacityRatio - 1)));
		}
		return new MoleQuantity(massFlowRate / highPressureSide.GetAverageGasMolarMass());
	}
	public static void Mix(
		Atmosphere left,
		Atmosphere right,
		VolumeLitres mixingVolume,
		MatterState leftMatterState,
		MatterState rightMatterState)
	{
		if (left is null || right is null)
			return;
		if (left.Volume <= VolumeLitres.Zero || right.Volume <= VolumeLitres.Zero)
			return;
		GasMixture mix;
		if(left.TotalVolumeLiquids >= left.Volume || right.TotalVolumeLiquids >= right.Volume)
		{
			var totalVolumeLiquids = left.TotalVolumeLiquids + right.TotalVolumeLiquids;
			var leftRatio = leftMatterState is MatterState.Gas ? 0d : rightMatterState is MatterState.Gas ? 1 : (left.TotalVolumeLiquids / totalVolumeLiquids).ToDouble();
			var rightRatio = 1 - leftRatio;
			
			mix = Split(ref left.GasMixture, leftRatio, MatterState.Liquid);
			mix.Add(Split(ref right.GasMixture, rightRatio, MatterState.Liquid));
			left.Add(mix.Split(0.5));
			right.Add(mix);
		}

		var source = left;
		var receiver = right;
		var matterState = leftMatterState;

		if (left.PressureGassesAndLiquids < right.PressureGassesAndLiquids)
		{
			source = right;
			receiver = left;
			matterState = rightMatterState;
		}

		// First mix atmospheres
		var mixVolumeLimited = RocketMath.Min(source.GetVolume(matterState), RocketMath.Min(receiver.GetVolume(matterState), mixingVolume));
		var leftAmount = IdealGas.Quantity(left.PressureGassesAndLiquids, mixVolumeLimited / 2, left.Temperature);
		var rightAmount = IdealGas.Quantity(right.PressureGassesAndLiquids, mixVolumeLimited / 2, right.Temperature);
		mix = left.Remove(leftAmount, leftMatterState);
		mix.Add(right.Remove(rightAmount, rightMatterState));
		left.Add(Remove(ref mix, leftAmount, MatterState.All));
		right.Add(mix);

		// Then equalize them.
		//var pressureDifference = (source.PressureGassesAndLiquids - receiver.PressureGassesAndLiquids) * 0.8; // reduce equalization, to make it more smooth
		//if (pressureDifference <= PressurekPa.Zero)
		//	return;
		//mixVolumeLimited = RocketMath.Min(source.GetVolume(matterState), RocketMath.Min(receiver.GetVolume(matterState), mixingVolume));
		//var moveAmount = IdealGas.Quantity(pressureDifference, mixVolumeLimited, source.Temperature);
		//var movedGas = source.Remove(moveAmount, matterState);
		//receiver.Add(movedGas);
		//var equilibriumPressure = source.PressureGassesAndLiquids;
		//AddEnergy(ref receiver.GasMixture, RemoveEnergy(ref source.GasMixture, FlowWork(moveAmount, source.Temperature)));
		var pressureDifference = (source.PressureGassesAndLiquids - receiver.PressureGassesAndLiquids) * 0.8; // reduce equalization, to make it more smooth
		if (pressureDifference <= PressurekPa.Zero)
			return;
		var γ = source.GasMixture.GetGasHeatCapacityRatio();
		var Vs = source.GetVolume(matterState);
		var Vr = receiver.GetVolume(matterState);
		// Harmonic combination of volumes: Vs·Vr/(Vs+Vr), limited by physical mixing volume.
		var effectiveVolume = RocketMath.Min(Vs * (Vr / (Vs + Vr)).ToDouble(), mixingVolume);
		// δn = ΔP·V_eff / (γ·R·Ts) — dividing by γ accounts for the flow work heating/cooling.
		var moveAmount = IdealGas.Quantity(pressureDifference * (1.0 / γ), effectiveVolume, source.Temperature);
		var movedGas = source.Remove(moveAmount, matterState);
		receiver.Add(movedGas);
		AddEnergy(ref receiver.GasMixture, RemoveEnergy(ref source.GasMixture, FlowWork(moveAmount, source.Temperature)));

		Debug.Assert(left.TotalVolumeLiquids < left.Volume);
		Debug.Assert(right.TotalVolumeLiquids < right.Volume);
	}
	public static void Mix(Atmosphere? left, Atmosphere? right, VolumeLitres mixingVolume, MatterState matterState = MatterState.All) =>
		Mix(left!, right!, mixingVolume, matterState, matterState);
	public static MoleEnergy FlowWork(this MoleQuantity flowQuantity, TemperatureKelvin temperature) =>
		new(flowQuantity.ToDouble() * R * temperature.ToDouble());

	private static void Swap<T>(ref T left, ref T right)
	{
		(left, right) = (right, left);
	}
	static bool TryFindRoot(Func<double, double> f, double lowerBound, double upperBound, double accuracy, int maxIterations, out double root, out int iterations)
	{
		ArgumentNullException.ThrowIfNull(f);
		if (accuracy <= 0)
			throw new ArgumentOutOfRangeException(nameof(accuracy), "Must be greater than zero.");

		var fmin = f(lowerBound);
		var fmax = f(upperBound);
		var froot = fmax;
		var d = 0.0d;
		var e = 0.0d;

		root = upperBound;
		var xMid = double.NaN;

		// Root must be bracketed.
		if (Math.Sign(fmin) == Math.Sign(fmax))
		{
			iterations = 0;
			return false;
		}

		for (var i = 0; i <= maxIterations; i++)
		{
			// adjust bounds
			if (Math.Sign(froot) == Math.Sign(fmax))
			{
				upperBound = lowerBound;
				fmax = fmin;
				e = d = root - lowerBound;
			}

			if (Math.Abs(fmax) < Math.Abs(froot))
			{
				lowerBound = root;
				root = upperBound;
				upperBound = lowerBound;
				fmin = froot;
				froot = fmax;
				fmax = fmin;
			}

			// convergence check
			var xAcc1 = (PositiveDoublePrecision * Math.Abs(root)) + (0.5 * accuracy);
			var xMidOld = xMid;
			xMid = (upperBound - root) / 2.0;

			if (Math.Abs(xMid) <= xAcc1 || AlmostEqualNormRelative(froot, 0, froot, accuracy))
			{
				iterations = i;
				return true;
			}

			if (xMid == xMidOld)
			{
				iterations = i;
				// accuracy not sufficient, but cannot be improved further
				return false;
			}

			if (Math.Abs(e) >= xAcc1 && Math.Abs(fmin) > Math.Abs(froot))
			{
				// Attempt inverse quadratic interpolation
				var s = froot / fmin;
				double p;
				double q;
				if (AlmostEqualRelative(lowerBound, upperBound))
				{
					p = 2.0 * xMid * s;
					q = 1.0 - s;
				}
				else
				{
					q = fmin / fmax;
					var r = froot / fmax;
					p = s * ((2.0 * xMid * q * (q - r)) - ((root - lowerBound) * (r - 1.0)));
					q = (q - 1.0) * (r - 1.0) * (s - 1.0);
				}

				if (p > 0.0)
					// Check whether in bounds
					q = -q;

				p = Math.Abs(p);
				if (2.0 * p < Math.Min((3.0 * xMid * q) - Math.Abs(xAcc1 * q), Math.Abs(e * q)))
				{
					// Accept interpolation
					e = d;
					d = p / q;
				}
				else
				{
					// Interpolation failed, use bisection
					d = xMid;
					e = d;
				}
			}
			else
			{
				// Bounds decreasing too slowly, use bisection
				d = xMid;
				e = d;
			}

			lowerBound = root;
			fmin = froot;
			if (Math.Abs(d) > xAcc1)
				root += d;
			else
				root += Math.Sign(xMid) * xAcc1;
			froot = f(root);
		}
		iterations = maxIterations;
		return false;

		static bool AlmostEqualNormRelative(double a, double b, double diff, double maximumError)
		{
			if (double.IsInfinity(a) || double.IsInfinity(b))
				return a == b;

			if (double.IsNaN(a) || double.IsNaN(b))
				return false;

			if (Math.Abs(a) < DoublePrecision || Math.Abs(b) < DoublePrecision)
				return Math.Abs(diff) < maximumError;

			if ((a == 0.0 && Math.Abs(b) < maximumError) || (b == 0.0 && Math.Abs(a) < maximumError))
				return true;

			return Math.Abs(diff) < maximumError * Math.Max(Math.Abs(a), Math.Abs(b));
		}
		static bool AlmostEqualRelative(double a, double b) =>
			AlmostEqualNormRelative(a, b, a - b, DefaultDoubleAccuracy);
	}
}
