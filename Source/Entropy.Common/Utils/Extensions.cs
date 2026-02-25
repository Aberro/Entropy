using Entropy.Common.Attributes;
using Entropy.Common.Configs;
using Entropy.Common.Mods;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Entropy.Common.Utils;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case")]
public static class Extenstions
{
	extension(ArgumentNullException)
	{
		/// <summary>
		/// Throws an <see cref="ArgumentNullException"/> if the provided object is null.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="paramName"></param>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[ContractArgumentValidator]
		public static void ThrowIfNull<T>(T obj, [CallerArgumentExpression(nameof(obj))] string? paramName = null) where T : class
		{
			if (obj == null)
			{
				throw new ArgumentNullException(paramName);
			}
			Contract.EndContractBlock();
		}
	}

	extension(Color)
	{
		/// <summary>
		/// Creates a new Color instance from a 32-bit unsigned integer, interpreting the bytes in reverse order (least
		/// significant byte as red, next as green, then blue, and most significant byte as alpha).
		/// </summary>
		/// <param name="value">The 32-bit unsigned integer whose bytes represent the color components in the order: red (least significant byte),
		/// green, blue, alpha (most significant byte).</param>
		/// <returns>A Color instance corresponding to the specified byte order in the input value.</returns>
		public static Color FromUIntReversed(uint value) =>
			new(
				(value & 0xff) / 255f,
				((value >> 8) & 0xff) / 255f,
				((value >> 16) & 0xff) / 255f,
				((value >> 24) & 0xff) / 255f);
		/// <summary>
		/// Creates a new Color instance from a 32-bit unsigned integer value in RGBA format.
		/// </summary>
		/// <param name="value">The 32-bit unsigned integer representing the color in RGBA format, where the highest 8 bits are red, followed by
		/// green, blue and alpha components.</param>
		/// <returns>A Color instance corresponding to the specified RGBA value.</returns>
		public static Color FromUInt(uint value) =>
			new(
				((value >> 24) & 0xff) / 255f,
				((value >> 16) & 0xff) / 255f,
				((value >> 8) & 0xff) / 255f,
				(value & 0xff) / 255f);
		/// <summary>
		/// Creates a new Color instance from a hexadecimal string representation.
		/// </summary>
		/// <remarks>
		/// The input string can be in one of the following formats:
		/// - RRGGBBAA (8 hex digits, with optional '#' or '0x' prefix): Represents red, green, blue, and alpha components in that order. For example, "#FF00FF80" or "FF00FF80".
		/// - RRGGBB (6 hex digits, with optional '#' or '0x' prefix): Represents red, green, and blue components, with alpha implicitly set to 255 (fully opaque). For example, "#FF00FF" or "FF00FF".
		/// - RGBA (4 hex digits, with optional '#' or '0x' prefix): Represents red, green, blue, and alpha components using 4 bits each. Each component is expanded to 8 bits by replicating the 4-bit value. For example, "#F0F8" or "F0F8" would be interpreted as R=0xFF, G=0x00, B=0xFF, A=0x88.
		/// - RGB (3 hex digits, with optional '#' or '0x' prefix): Represents red, green, and blue components using 4 bits each, with alpha implicitly set to 255 (fully opaque). Each component is expanded to 8 bits by replicating the 4-bit value. For example, "#F0F" or "F0F" would be interpreted as R=0xFF, G=0x00, B=0xFF, A=0xFF.
		/// </remarks>
		/// <param name="value">A string containing a hexadecimal color value, optionally prefixed with '#' or '0x'. For example, "#FF00FF" or "FF00FF".</param>
		/// <returns>A Color instance corresponding to the specified hexadecimal value.</returns>
		public static Color FromString(string value)
		{
			ArgumentNullException.ThrowIfNull(value);
			if (string.IsNullOrEmpty(value))
				throw new ArgumentException("Color value is in incorrect/unknown format!");
			ReadOnlySpan<char> strVal;
			try
			{
				if (_namedColors.TryGetValue(value, out var result))
					return result;
				if (value[0] == '#')
					strVal = value.AsSpan(1);
				else if (value[0] == '0' && value[1] == 'x')
					strVal = value.AsSpan(2);
				else
					// Just assume it's in hex notation already without a prefix
					strVal = value.AsSpan();
				if (strVal.Length == 8)
					return FromUInt(uint.Parse(strVal, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
				if (strVal.Length == 6)
					// RRGGBB format, shift the value and substitute 0xff alpha (lowest byte):
					return FromUInt((uint.Parse(strVal, NumberStyles.HexNumber, CultureInfo.InvariantCulture) << 8) | 0xff);
				if (strVal.Length == 4)
				{
					// RGBA format (4 bits per color), needs to be extended by replicating each quartet
					var val = uint.Parse(strVal, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
					// Extract components:
					var r = (val >> 12) & 0xF;
					var g = (val >> 8) & 0xF;
					var b = (val >> 4) & 0xF;
					var a = val & 0xF;
					return FromUInt((r << 28) | (r << 24) | (g << 20) | (g << 16) | (b << 12) | (b << 8) | (a << 4) | a);
				}
				if (strVal.Length == 3)
				{
					// RGB format (4 bits per color), needs to be extended by replicating each quartet and substituting 0xf alpha:
					var val = uint.Parse(strVal, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
					// Extract components:
					var r = (val >> 8) & 0xF;
					var g = (val >> 4) & 0xF;
					var b = val & 0xF;
					return FromUInt((r << 28) | (r << 24) | (g << 20) | (g << 16) | (b << 12) | (b << 8) | 0xFF);
				}
				throw new ArgumentException("Color value is in incorrect/unknown format!");
			}
			catch
			{
				// Just convert exception to our ArgumentException.
				throw new ArgumentException("Color value is in incorrect/unknown format!");
			}
		}
		public static Color FromVector3(Vector3 value) => new(value.x, value.y, value.z, 1);
		public static Color FromVector4(Vector4 value) => new(value.x, value.y, value.z, value.w);
		public static Color operator+(Color c1, Color c2) => new(c1.r + c2.r, c1.g + c2.g, c1.b + c2.b, c1.a + c2.a);
		public static Color operator-(Color c1, Color c2) => new(c1.r - c2.r, c1.g - c2.g, c1.b - c2.b, c1.a - c2.a);
		public static Color operator*(Color color, float multiplier) => new(color.r * multiplier, color.g * multiplier, color.b * multiplier, color.a * multiplier);
		public static Color operator/(Color color, float divisor) => new(color.r / divisor, color.g / divisor, color.b / divisor, color.a / divisor);

	}
	private static readonly Dictionary<string, Color> _namedColors = new()
	{
		{ "red", new Color(1, 0, 0) },
		{ "green", new Color(0, 1, 0) },
		{ "blue", new Color(0, 0, 1) },
		{ "orange", new Color(1, 0.64453f, 0) },
		{ "white", new Color(1, 1, 1) },
		{ "gray", new Color(0.5f, 0.5f, 0.5f) },
		{ "black", new Color(0, 0, 0) },
	};
	public static uint ToUInt(this Color color)
	{
		var r = (uint)(color.r * 255) & 0xFF;
		var g = (uint)(color.g * 255) & 0xFF;
		var b = (uint)(color.b * 255) & 0xFF;
		var a = (uint)(color.a * 255) & 0xFF;
		return (a << 24) | (r << 16) | (g << 8) | b;
	}
	public static Vector3 ToVector3(this Color color) => new(color.r, color.g, color.b);
	public static Vector4 ToVector4(this Color color) => new(color.r, color.g, color.b, color.a);
	public static Color ToColor(this Vector3 vector, float alpha = 1) => new(vector.x, vector.y, vector.z, alpha);
	public static Color ToColor(this Vector4 vector) => new(vector.x, vector.y, vector.z, vector.w);

	/// <summary>
	/// Performs the specified action on each element of the <see cref="IEnumerable{T}"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="source"></param>
	/// <param name="action"></param>
	public static void Each<T>(this IEnumerable<T> source, Action<T> action)
	{
		ArgumentNullException.ThrowIfNull(action);
		ArgumentNullException.ThrowIfNull(source);
		foreach (var item in source)
			action(item);
	}

	/// <summary>
	/// Performs the specified action on each element of the <see cref="IEnumerable{T}"/> along with the element's index.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="source"></param>
	/// <param name="action"></param>
	public static void Each<T>(this IEnumerable<T> source, Action<T, int> action)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(action);
		var index = 0;
		foreach (var item in source)
			action(item, index++);
	}

	/// <summary>
	/// Performs the specified action on each element of the <see cref="IEnumerable{T
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TArg"></typeparam>
	/// <param name="source"></param>
	/// <param name="action"></param>
	/// <param name="argument"></param>
	public static void Each<T, TArg>(this IEnumerable<T> source, Action<T, TArg> action, TArg argument)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(action);
		foreach (var item in source)
			action(item, argument);
	}

	/// <summary>
	/// Gets a <see cref="ConfigCategory"/> of an enumeration value from <see cref="ConfigCategoryAttribute"/> attribute of the given type.
	/// </summary>
	/// <param name="type">Type of the class to get <see cref="ConfigCategory"/> from.</param>
	/// <returns>A <see cref="ConfigCategory"/> of the give type or a default mod category if <see cref="ConfigCategoryAttribute"/> nor <see cref="ConfigCategoryDefinitionAttribute"/> can be found.</returns>
	public static ConfigCategory GetCategory(this Type type)
	{
		ArgumentNullException.ThrowIfNull(type);
		var mod = EntropyModBase.GetModByAssembly(type.Assembly) ?? throw new InvalidOperationException($"Failed to get mod for {type.Name} type!");
		var attributes = type.GetCustomAttributes<ConfigCategoryAttribute>().ToArray();
		if (attributes is not null && attributes.Length != 0)
		{
			if (attributes.Length == 1)
				return ConfigCategory.Get(mod, attributes.First().Category) ?? throw new ApplicationException($"Tried to use category {attributes.First().Category} that is not defined! Use PatchCategoryDefinitionAttribute to define a category");
			else
				throw new ApplicationException($"Multiple categories defined for a patch {type.FullName}!");
		}
		var definitions = type.GetCustomAttributes<ConfigCategoryDefinitionAttribute>().ToArray();
		if (definitions is not null && definitions.Length != 0)
		{
			if (definitions.Length == 1)
				return ConfigCategory.Define(mod, definitions.First().Name, definitions.First().DisplayName, definitions.First().Description);
		}
		return ConfigCategory.GetDefault(mod);
	}

	/// <summary>
	/// Gets a <see cref="PatchConfigEntry"/> for the given patching type and category from <see cref="PatchConfigAttribute"/> attribute of the given type.
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
	/// <exception cref="ApplicationException"></exception>
	public static PatchConfigEntry? GetPatchConfigEntry(this Type type)
	{
		ArgumentNullException.ThrowIfNull(type);
		var mod = EntropyModBase.GetModByAssembly(type.Assembly) ?? throw new InvalidOperationException($"Failed to get mod for {type.Name} type!");
		var attributes = type.GetCustomAttributes<PatchConfigAttribute>().ToArray();
		if (attributes is not null && attributes.Length != 0)
		{
			if (attributes.Length == 1)
				return PatchConfigEntry.Get(mod, attributes.First().Name, type.GetCategory()) ?? throw new ApplicationException($"Tried to use patch config entry {attributes.First().Name} that is not defined! Use PatchConfigDefinition to define a patch config entry");
			else
				throw new ApplicationException($"Multiple patch config entries defined for a patch {type.FullName}!");
		}
		var definitions = type.GetCustomAttributes<PatchConfigDefinitionAttribute>().ToArray();
		if (definitions is not null && definitions.Length != 0)
		{
			if (definitions.Length == 1)
				return PatchConfigEntry.Get(mod, definitions.First().Name, type.GetCategory()) ?? throw new ApplicationException($"Tried to use patch config entry {definitions.First().Name} that is not defined! Use PatchConfigDefinition to define a patch config entry");
		}
		return null;
	}

	/// <summary>
	/// Gets a <see cref="ConfigCategory"/> of an enumeration value from <see cref="ConfigCategoryAttribute"/> attribute of the given type.
	/// </summary>
	/// <param name="type">Type of the class to get <see cref="ConfigCategory"/> from.</param>
	/// <returns>A <see cref="ConfigCategory"/> of the give type or a default mod category if <see cref="ConfigCategoryAttribute"/> nor <see cref="ConfigCategoryDefinitionAttribute"/> can be found.</returns>
	public static ConfigCategory GetCategory(this MethodInfo method)
	{
		ArgumentNullException.ThrowIfNull(method);
		var mod = EntropyModBase.GetModByAssembly(method.DeclaringType.Assembly) ?? throw new InvalidOperationException($"Failed to get mod for {method.DeclaringType.Name} type!");
		var attributes = method.GetCustomAttributes<ConfigCategoryAttribute>().ToArray();
		if (attributes is not null && attributes.Length != 0)
		{
			if (attributes.Length == 1)
				return ConfigCategory.Get(mod, attributes.First().Category) ?? throw new ApplicationException($"Tried to use category {attributes.First().Category} that is not defined! Use PatchCategoryDefinitionAttribute to define a category");
			else
				throw new ApplicationException($"Multiple categories defined for a patch {method.DeclaringType.FullName}.{method.Name} method!");
		}
		var definitions = method.GetCustomAttributes<ConfigCategoryDefinitionAttribute>().ToArray();
		if (definitions is not null && definitions.Length != 0)
		{
			if (definitions.Length == 1)
				return ConfigCategory.Get(mod, definitions.First().Name) ?? ConfigCategory.GetDefault(mod);
		}
		// Could not find a category by attributes, try looking for a category defined on the declaring type
		var typeAttributes = method.DeclaringType.GetCustomAttributes<ConfigCategoryAttribute>().ToArray();
		if (typeAttributes is not null && typeAttributes.Length != 0)
		{
			if (typeAttributes.Length == 1)
				return ConfigCategory.Get(mod, typeAttributes.First().Category) ?? throw new ApplicationException($"Tried to use category {attributes.First().Category} that is not defined! Use PatchCategoryDefinitionAttribute to define a category");
			else
				throw new ApplicationException($"Multiple categories defined for a patch {method.DeclaringType.FullName} type!");
		}
		var typeDefinitions = method.DeclaringType.GetCustomAttributes<ConfigCategoryDefinitionAttribute>().ToArray();
		if (typeDefinitions is not null && typeDefinitions.Length != 0)
		{
			if (typeDefinitions.Length == 1)
				return ConfigCategory.Get(mod, typeDefinitions.First().Name) ?? ConfigCategory.GetDefault(mod);
		}
		// Still could not find a category, return the default one
		return ConfigCategory.GetDefault(mod);
	}

	public static PatchConfigEntry? GetPatchConfigEntry(this MethodInfo method)
	{
		ArgumentNullException.ThrowIfNull(method);
		var mod = EntropyModBase.GetModByAssembly(method.DeclaringType.Assembly) ?? throw new InvalidOperationException($"Failed to get mod for {method.DeclaringType.Name} type!");
		var attributes = method.GetCustomAttributes<PatchConfigAttribute>().ToArray();
		if (attributes is not null && attributes.Length != 0)
		{
			if (attributes.Length == 1)
				return PatchConfigEntry.Get(mod, attributes.First().Name, method.GetCategory().Name) ?? throw new ApplicationException($"Tried to use patch config entry {attributes.First().Name} that is not defined! Use PatchConfigDefinition to define a patch config entry");
			else
				throw new ApplicationException($"Multiple patch config entries defined for a patch {method.DeclaringType.FullName}.{method.Name} method!");
		}
		var definitions = method.GetCustomAttributes<PatchConfigDefinitionAttribute>().ToArray();
		if (definitions is not null && definitions.Length != 0)
		{
			if (definitions.Length == 1)
				return PatchConfigEntry.Get(mod, definitions.First().Name, method.GetCategory().Name) ?? throw new ApplicationException($"Tried to use patch config entry {definitions.First().Name} that is not defined! Use PatchConfigDefinition to define a patch config entry");
		}
		// Could not find a patch config entry by attributes, try looking for a patch config entry defined on the declaring type
		attributes = method.DeclaringType.GetCustomAttributes<PatchConfigAttribute>().ToArray();
		if (attributes is not null && attributes.Length != 0)
		{
			if (attributes.Length == 1)
				return PatchConfigEntry.Get(mod, attributes.First().Name, method.GetCategory().Name) ?? throw new ApplicationException($"Tried to use patch config entry {attributes.First().Name} that is not defined! Use PatchConfigDefinition to define a patch config entry");
			else
				throw new ApplicationException($"Multiple patch config entries defined for a patch {method.DeclaringType.FullName} type!");
		}
		definitions = method.DeclaringType.GetCustomAttributes<PatchConfigDefinitionAttribute>().ToArray();
		if (definitions is not null && definitions.Length != 0)
		{
			if (definitions.Length == 1)
				return PatchConfigEntry.Get(mod, definitions.First().Name, method.GetCategory().Name) ?? throw new ApplicationException($"Tried to use patch config entry {definitions.First().Name} that is not defined! Use PatchConfigDefinition to define a patch config entry");
		}
		// Still could not find a patch config entry, return null
		return null;
	}
	public static bool IsNumeric(this Type type) =>
		Type.GetTypeCode(type) switch
		{
			TypeCode.Byte
			or TypeCode.SByte
			or TypeCode.UInt16
			or TypeCode.Int16
			or TypeCode.UInt32
			or TypeCode.Int32
			or TypeCode.UInt64
			or TypeCode.Int64
			or TypeCode.Single
			or TypeCode.Double
			or TypeCode.Decimal => true,
			_ => false,
		};
}