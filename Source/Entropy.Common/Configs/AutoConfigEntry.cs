using Entropy.Common.Attributes;
using Entropy.Common.Mods;
using Entropy.Common.Utils;
using HarmonyLib;
using JetBrains.Annotations;
using System.Reflection;

namespace Entropy.Common.Configs;

/// <summary>
/// A configuration entry that is automatically applied to a property of a class using Harmony patches.
/// </summary>
/// <typeparam name="T"></typeparam>
public class AutoConfigEntry<T> : ConfigEntry<T>
{
	static Dictionary<MethodInfo, AutoConfigEntry<T>> _map = [];
	private AutoConfigEntry(EntropyModBase mod, string name, string description, string? category, Optional<T> defaultValue, Optional<T> minValue, Optional<T> maxValue)
		: base(mod, name, description, category, defaultValue, minValue, maxValue)
	{
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider")]
	public static AutoConfigEntry<T> Define(EntropyModBase mod, PropertyInfo property, string? category, AutoConfigDefinitionAttribute attribute)
	{
		ArgumentNullException.ThrowIfNull(mod);
		ArgumentNullException.ThrowIfNull(property);
		ArgumentNullException.ThrowIfNull(attribute);
		var getter = property.GetGetMethod(true);
		var setter = property.GetSetMethod(true);
		var name = attribute.Name == "$MemberName" ? property.Name : attribute.Name;
		var defaultvalue = attribute.DefaultValue is null ? default : (Optional<T>) (T) Convert.ChangeType(attribute.DefaultValue, typeof(T));
		Optional<T> minValue = default, maxValue = default;
		if (typeof(T).IsNumeric())
		{
			minValue = attribute.MinValue is null ? default : (Optional<T>) (T) Convert.ChangeType(attribute.MinValue, typeof(T));
			maxValue = attribute.MaxValue is null ? default : (Optional<T>) (T) Convert.ChangeType(attribute.MaxValue, typeof(T));
		}
		var result = new AutoConfigEntry<T>(mod, name, attribute.Description, category, defaultvalue, minValue, maxValue);
		mod.Config.BindConfigEntry(result, result.Default, result.MinValue, result.MaxValue);
		result.Order = attribute.Order;
		result.DisplayName = attribute.DisplayName!;
		result.Disabled = attribute.Enabled;
		result.Visible = attribute.Visible;
		result.RequireRestart = attribute.RequireRestart;
		result.Format = attribute.Format;
		var type = attribute.CustomDrawerType;
		if (type != null)
		{
			var methodInfo = type.GetMethod(attribute.CustomDrawer, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (methodInfo == null)
			{
				throw new ArgumentException($"Config '{category}.{name}' definition declares custom drawer type '{type.Name}', but no '{attribute.CustomDrawer}' static method with expected signature was found.");
			}
			result.CustomDrawer = (Func<ConfigEntryBase, bool>) methodInfo.CreateDelegate(typeof(Func<ConfigEntryBase, bool>));
		}

		var getterPatch = typeof(AutoConfigEntry<T>).GetMethod("GetterPatch", BindingFlags.Static | BindingFlags.NonPublic);
		var setterPatch = typeof(AutoConfigEntry<T>).GetMethod("SetterPatch", BindingFlags.Static | BindingFlags.NonPublic);
		result.Category.Harmony.Patch(getter, prefix: new HarmonyMethod(getterPatch));
		result.Category.Harmony.Patch(setter, prefix: new HarmonyMethod(setterPatch));
		_map.Add(getter, result);
		_map.Add(setter, result);
		return result;
	}

	[UsedImplicitly]
	private static bool GetterPatch(MethodInfo __originalMethod, ref T __result)
	{
		__result = _map[__originalMethod].Value!;
		return false;
	}

	[UsedImplicitly]
	private static bool SetterPatch(MethodInfo __originalMethod, T value)
	{
		_map[__originalMethod].Value = value;
		return false;
	}
}