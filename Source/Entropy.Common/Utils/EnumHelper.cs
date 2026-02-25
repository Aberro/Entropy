using Entropy.Common.Attributes;
using Entropy.Common.Patching;
using Entropy.Common.Mods;
using System.ComponentModel;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace Entropy.Common.Utils;

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
		ArgumentNullException.ThrowIfNull(value);
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
		ArgumentNullException.ThrowIfNull(value);
		var fieldInfo = value.GetType().GetField(value.ToString());
#pragma warning disable CS0436 // Type conflicts with imported type
		var attribute = fieldInfo.GetCustomAttribute<DisplayAttribute>();
#pragma warning restore CS0436 // Type conflicts with imported type
		return attribute == null ? value.ToString() : attribute.Name;
	}
}