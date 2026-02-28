using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects;
using Entropy.Common.Attributes;
using HarmonyLib;
using static Assets.Scripts.Atmospherics.AtmosphereHelper;

namespace Entropy.Adiabatics;

[HarmonyPatch]
public static class Patches
{
	/// <summary>
	/// The Adiabatic pumping itself. Heats up/cools down gases by calculating the work done by pumps.
	/// </summary>
	/// <param name="inputAtmos"></param>
	/// <param name="outputAtmos"></param>
	/// <param name="volume"></param>
	/// <param name="matterStateToMove"></param>
	/// <returns></returns>
	[HarmonyPatch(typeof(AtmosphereHelper), nameof(AtmosphereHelper.MoveVolume))]
	[HarmonyPrefix]
	public static bool AtmosphereHelperMoveVolumePrefix(Atmosphere inputAtmos, Atmosphere outputAtmos, VolumeLitres volume, MatterState matterStateToMove, MoleQuantity minMolesToMove)
	{
		PhysicsHelper.DoAdiabaticPumping(ref inputAtmos.GasMixture, ref outputAtmos.GasMixture, inputAtmos.Volume, outputAtmos.Volume, volume);
		return false;
	}
	[HarmonyPatch(typeof(DynamicGenerator))]
	public static class DynamicGeneratorPatches
	{
		//[PatchValidateCrc(13821876323455797224)]
		[PatchValidateCrc(0)]
		[HarmonyPatch(nameof(DynamicGenerator.OnAtmosphericTick))]
		[HarmonyPrefix]
		public static bool OnAtmosphericTickPrefix(DynamicGenerator __instance)
		{
			return true;
		}
	}
}