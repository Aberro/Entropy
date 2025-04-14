﻿using Assets.Scripts.Atmospherics;
using HarmonyLib;
using Assets.Scripts;
using UnityEngine;

namespace Entropy.Assets.Scripts.Patches
{
    [HarmonyPatch(typeof(AtmosphericsManager), nameof(AtmosphericsManager.ManagerAwake))]
    [HarmonyPatchCategory(PatchCategory.SmallerParticles)]
    public static class AtmosphericsManagerManagerAwakePatch
    {
        public static void Postfix()
        {
            AtmosphericsManager.emitParams = new ParticleSystem.EmitParams()
            {
                applyShapeToPosition = true,
                startSize = 0.01f,
                startSize3D = new Vector3(0.01f, 0.01f, 0.01f),
            };
        }
    }

    [HarmonyPatch(typeof(AtmosphericsController), MethodType.Constructor, typeof(GridController))]
    [HarmonyPatchCategory(PatchCategory.NoTrails)]
    public static class AtmosphericsControllerCtorPatch
    {
        public static void Postfix(AtmosphericsController __instance)
        {
            var trails = __instance.GasVisualizerParticleSystem.trails;
            trails.enabled = false;
        }
    }
}
