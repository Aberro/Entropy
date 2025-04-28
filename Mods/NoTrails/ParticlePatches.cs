using HarmonyLib;
using Assets.Scripts;
using System.Reflection;

[assembly: AssemblyTitle("No Particle Trails")]
[assembly: AssemblyDescription("Disables gas particle trails.")]

namespace Entropy.Mods.NoTrails;

/// <summary>
/// A patch to disable gas particle trails in the AtmosphericsController.
/// </summary>
[HarmonyPatch(typeof(AtmosphericsController), MethodType.Constructor, typeof(GridController))]
public static class NoTrailsPatch
{
	private static readonly List<WeakReference<AtmosphericsController>> _patchedInstances = [];
	/// <summary>
	/// Disables particle trails if AtmosphericsController is already initialized.
	/// </summary>
	public static void Prepare()
	{
		if(AtmosphericsController.World != null)
		{
			var trails = AtmosphericsController.World.GasVisualizerParticleSystem.trails;
			trails.enabled = false;
		}
	}
	/// <summary>
	/// Patches AtmosphericsController constructor to disable particle trails.
	/// </summary>
	/// <param name="__instance"></param>
	public static void Postfix(AtmosphericsController __instance)
	{
		var trails = __instance.GasVisualizerParticleSystem.trails;
		trails.enabled = false;
		_patchedInstances.Add(new(__instance));
	}

	/// <summary>
	/// Unpatches AtmosphericsController constructor to re-enable particle trails.
	/// </summary>
	public static void Unpatch()
	{
		if(AtmosphericsController.World != null)
		{
			var trails = AtmosphericsController.World.GasVisualizerParticleSystem.trails;
			trails.enabled = true;
		}
	}
}