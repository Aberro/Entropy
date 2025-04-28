using Entropy.Patches;
using HarmonyLib;

namespace Entropy.Attributes;

/// <summary>
/// Attribute that specified <see cref="PatchCategory"/> of a patch.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class HarmonyPatchCategoryAttribute : HarmonyAttribute
{
	/// <summary>
	/// The category of a patch.
	/// </summary>
	public string Category { get; }

	/// <summary>
	/// Creates a new instance of the <see cref="HarmonyPatchCategoryAttribute"/> attribute.
	/// </summary>
	/// <param name="category">Name of the category of a patch.</param>
	public HarmonyPatchCategoryAttribute(string category)
    {
        Category = category;
    }
}