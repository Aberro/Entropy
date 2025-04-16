using Assets.Scripts.Atmospherics;
using HarmonyLib;
using UnityEngine;

namespace Entropy.Scripts.Patches
{

#if DISABLE
    /// <summary>
    /// Fix for clamping.
    /// </summary>
    [HarmonyPatch(typeof(AtmosphereHelper), nameof(AtmosphereHelper.MoveVolume))]
    public class AtmosphereHelper_MoveVolume_Patch
    {
        public static bool Prefix(Atmosphere inputAtmos, Atmosphere outputAtmos, float volume, AtmosphereHelper.MatterState matterStateToMove)
        {
            float num = Mathf.Clamp01(volume / inputAtmos.GetVolume(matterStateToMove));
            if (num <= 0.0)
                return false;
            GasMixture gasMixture = inputAtmos.Remove(num * inputAtmos.GasMixture.GetTotalMoles(matterStateToMove), matterStateToMove);
            outputAtmos.Add(gasMixture);
            return false;
        }
    }
#endif
}