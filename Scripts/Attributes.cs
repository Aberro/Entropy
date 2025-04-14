using System;
using HarmonyLib;

namespace Entropy.Assets.Scripts
{
    /// <summary>Annotation to define a category for use with PatchCategory</summary>
    ///
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class HarmonyPatchCategory : HarmonyAttribute
    {
        public PatchCategory Category { get; }
        /// <summary>Annotation specifying the category</summary>
        /// <param name="category">Name of patch category</param>
        ///
        public HarmonyPatchCategory(PatchCategory category)
        {
            Category = category;
        }
    }
}
