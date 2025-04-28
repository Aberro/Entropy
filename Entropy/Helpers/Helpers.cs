using System.ComponentModel;
using System.Reflection;
using Entropy.Attributes;
using Entropy.Mods;
using Entropy.Patches;

namespace Entropy.Helpers;

/// <summary>
/// Helper class for working with enumerations.
/// </summary>
public static class EnumHelper
{
	/// <summary>
	/// Gets a <see cref="DescriptionAttribute"/> of an enumeration value
	/// </summary>
	/// <param name="value">The enumeration value.</param>
	/// <returns>A <see cref="DescriptionAttribute"/> of the enumeration value or value's name if not found.</returns>
	public static string GetDescription(this Enum value)
	{
		var fieldInfo = value.GetType().GetField(value.ToString());
		var attribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
		return attribute == null ? value.ToString() : attribute.Description;
	}

	/// <summary>
	/// Gets a <see cref="DisplayNameAttribute"/> of an enumeration value
	/// </summary>
	/// <param name="value">The enumeration value.</param>
	/// <returns>A <see cref="DisplayNameAttribute"/> of the enumeration value or the value's name if not found.</returns>
	public static string GetDisplayName(this Enum value)
	{
		var fieldInfo = value.GetType().GetField(value.ToString());
#pragma warning disable CS0436 // Type conflicts with imported type
		var attribute = fieldInfo.GetCustomAttribute<DisplayNameAttribute>();
#pragma warning restore CS0436 // Type conflicts with imported type
		return attribute == null ? value.ToString() : attribute.DisplayName;
	}

	/// <summary>
	/// Gets a <see cref="PatchCategory"/> of an enumeration value from <see cref="HarmonyPatchCategoryAttribute"/> attribute of the given type.
	/// </summary>
	/// <param name="type">Type of the class to get <see cref="PatchCategory"/> from.</param>
	/// <returns>A <see cref="PatchCategory"/> of the give type or a default mod category if <see cref="HarmonyPatchCategoryAttribute"/> nor <see cref="PatchCategoryDefinitionAttribute"/> can be found.</returns>
	public static PatchCategory GetCategory(this Type type)
	{
		var mod = ModsCollector.GetByAssembly(type.Assembly) ?? throw new ApplicationException($"Failed to get mod for {type.Name} type!");
		var attribute = type.GetCustomAttribute<HarmonyPatchCategoryAttribute>();
		if(attribute is not null)
			return PatchCategory.Get(mod, attribute.Category);
		var definition = type.GetCustomAttribute<PatchCategoryDefinitionAttribute>();
		if(definition is not null)
			return PatchCategory.Get(mod, definition.Name);
		return PatchCategory.GetDefault(mod);
	}

	/// <summary>
	/// Executes the specified action on each element of the enumerable.
	/// </summary>
	/// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
	/// <param name="source">The enumerable source.</param>
	/// <param name="action">The action to execute on each element.</param>
	public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
	{
		foreach(var item in source)
			action(item);
	}
}
