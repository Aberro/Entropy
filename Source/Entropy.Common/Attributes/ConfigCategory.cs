using HarmonyLib;
using Entropy.Common.Configs;

namespace Entropy.Common.Attributes;

/// <summary>
/// Attribute that specified <see cref="ConfigCategory"/> of a patch.
/// </summary>
/// <remarks>
/// The category name specified in this attribute should be defined by a <see cref="ConfigCategoryDefinitionAttribute"/> attribute somewhere (anywhere would work).
/// If patch has a single <see cref="ConfigCategoryDefinitionAttribute"/>, <see cref="ConfigCategoryAttribute"/> is not required, and the patch will use the category specified in the definition.
/// </remarks>
/// <param name="category">Name of the category of a patch.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class ConfigCategoryAttribute(string category) : HarmonyAttribute
{
	/// <summary>
	/// The category of a patch.
	/// </summary>
	public string Category { get; } = category;
}