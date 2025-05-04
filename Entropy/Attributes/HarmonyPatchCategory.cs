using Entropy.Patches;
using HarmonyLib;

namespace Entropy.Attributes;

/// <summary>
/// Attribute that specified <see cref="PatchCategory"/> of a patch.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="HarmonyPatchCategoryAttribute"/> attribute.
/// </remarks>
/// <param name="category">Name of the category of a patch.</param>
[AttributeUsage(AttributeTargets.Class)]
public class HarmonyPatchCategoryAttribute(string category) : HarmonyAttribute
{
	/// <summary>
	/// The category of a patch.
	/// </summary>
	public string Category { get; } = category;
}