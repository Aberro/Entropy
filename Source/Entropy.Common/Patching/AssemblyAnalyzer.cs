using BepInEx.Configuration;
using Entropy.Common.Attributes;
using Entropy.Common.Configs;
using Entropy.Common.Mods;
using Entropy.Common.UI;
using HarmonyLib;
using LaunchPadBooster;
using MoonSharp.Interpreter.Interop.StandardDescriptors.HardwiredDescriptors;
using Steamworks;
using System.Diagnostics.CodeAnalysis;
using System.EnterpriseServices;
using System.Reflection;
using ConfigEntryBase = Entropy.Common.Configs.ConfigEntryBase;
using SettingChangedEventArgs = Entropy.Common.Configs.SettingChangedEventArgs;

namespace Entropy.Common.Patching;

/// <summary>
/// A class to collect and manage patches and automated configuration entries.
/// </summary>
internal static class AssemblyAnalyzer
{
	/// <summary>
	/// List of existing patches.
	/// </summary>
	private static readonly Dictionary<EntropyModBase, Dictionary<ConfigCategory, List<HarmonyPatchInfo>>> Patches = [];

	static AssemblyAnalyzer()
	{
	}

	internal static void DisableAll()
	{
		foreach(var mod in Patches.Keys)
		{
			ConfigureMod(mod, false);
		}
	}

	/// <summary>
	/// Registers a mod to the patches collector. This method is called by the <see cref="EntropyMod"/> constructor.
	/// </summary>
	/// <param name="mod">The mod to register.</param>
	internal static void RegisterMod(EntropyModBase mod) => mod.Config.SettingChanged += (_, e) => OnConfigurationChanged(mod, e);

	[SuppressMessage("Performance", "CA1851:Possible multiple enumerations of 'IEnumerable' collection")]
	internal static void AnalyzeAssembly(EntropyModBase mod)
	{
		var types = AccessTools.GetTypesFromAssembly(mod.GetType().Assembly);
		RegisterMod(mod);
		// Do a prepass to gather and create category definitions
		List<Type> excludedTypes = [];
		foreach (var type in types)
		{
			try
			{
				AnalyzeDefinitions(mod, type);
			} catch
			{
				excludedTypes.Add(type);
				mod.Logger.LogError($"Failed to analyze type `{type.FullName}' for patches. It will be excluded from patch collection.");
				continue;
			}
		}
		// Now do the actual patch collection and patch creation.
		foreach (var type in types)
		{
			if(excludedTypes.Contains(type))
				continue;
			var patchCategoryDefinitions = type.GetCustomAttributes<ConfigCategoryDefinitionAttribute>();
			ConfigCategory category;
			if (patchCategoryDefinitions != null && patchCategoryDefinitions.Count() == 1)
				category = ConfigCategory.Get(mod, patchCategoryDefinitions.First().Name)!;
			else
				category = ConfigCategory.GetDefault(mod);
			foreach (var patch in HarmonyPatchInfo.Create(mod, category.Harmony, type))
				AddPatch(mod, patch);
			foreach(var method in type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				AddPropertyHandlers(mod, method);
			}
		}
	}

	/// <summary>
	/// Configures a mod by enabling or disabling its patches depending on loaded configuration file.
	/// </summary>
	/// <remarks>
	/// When <paramref name="enabled"/> is <see langword="true"/>, only patches in categories that are enabled in config are applied.
	/// When it's <see langword="false" />, all patches are unpatched.
	/// </remarks>
	/// <param name="mod">The mod to configure.</param>
	/// <param name="enabled">Flag indicating if the mod needs to be enabled or disabled.</param>
	internal static void ConfigureMod(EntropyModBase mod, bool enabled)
	{
		if(!Patches.TryGetValue(mod, out var categories))
			return;
		foreach(var category in categories.Keys)
			ConfigureCategory(mod, category, true, true);

		mod.Logger.LogInfo(enabled ? $"`{mod.Info.Name}' mod enabled." : $"`{mod.Info.Name}' mod disabled.");
	}

	internal static void AddPatch(EntropyModBase mod, HarmonyPatchInfo patch)
	{
		if(!Patches.TryGetValue(mod, out var modPatches))
		{
			modPatches = [];
			Patches.Add(mod, modPatches);
		}
		if(!modPatches.TryGetValue(patch.Category, out var categoryPatches))
		{
			categoryPatches = [];
			modPatches.Add(patch.Category, categoryPatches);
		}
		if(!categoryPatches.Contains(patch))
			categoryPatches.Add(patch);
	}

	private static void OnConfigurationChanged(EntropyModBase mod, SettingChangedEventArgs e)
	{
		if(e.Entry is PatchConfigEntry patchEntry)
		{
			ConfigurePatch(mod, patchEntry, (bool) patchEntry.Value);
		}
	}

	private static void ConfigureCategory(EntropyModBase mod, ConfigCategory category, bool enabled, bool silent = false)
	{
		if(!Patches.TryGetValue(mod, out var categories))
			return;
		if(!categories.TryGetValue(category, out var patches))
			return;
		var applied = false;
		var harmony = category.Harmony;

		// Filter out patches that are already in the desired state
		foreach (var patch in patches.Where(x => x.IsPatched != enabled))
		{
			if (enabled)
			{
				if (patch.ConfigEntry?.Value ?? true)
					applied |= patch.Patch(harmony);
			} else
			{
				applied |= patch.Unpatch(harmony);
			}
		}

		// Nothing to log
		if(silent || !applied)
			return;

		if (enabled)
		{
			mod.Logger.LogInfo(category.IsDefault
				? $"`{mod.Info.ModID}' feature enabled."
				: $"`{mod.Info.ModID}.{category.Name}' feature enabled.");
		} else
		{
			mod.Logger.LogInfo(category.IsDefault
				? $"`{mod.Info.ModID}' feature disabled."
				: $"`{mod.Info.ModID}.{category.Name}' feature disabled.");
		}
	}

	private static void ConfigurePatch(EntropyModBase mod, PatchConfigEntry entry, bool enabled, bool silent = false)
	{
		if (!Patches.TryGetValue(mod, out var categories))
			return;
		if (!categories.TryGetValue(entry.Category, out var patches))
			return;
		var patch = patches.FirstOrDefault(x => x.ConfigEntry == entry);
		if (patch == null || patch.IsPatched == enabled)
			return;
		var harmony = entry.Category.Harmony;
		var applied = false;
		if (enabled)
			applied = patch.Patch(harmony);
		else
			applied = patch.Unpatch(harmony);
		
		// Nothing to log
		if (silent || !applied)
			return;

		if (enabled)
			mod.Logger.LogInfo($"`{mod.Info.ModID}.{entry.Category.Name}.{entry.Name}' patch enabled.");
		else
			mod.Logger.LogInfo($"`{mod.Info.ModID}.{entry.Category.Name}.{entry.Name}' patch disabled.");
	}

	[SuppressMessage("Performance", "CA1851:Possible multiple enumerations of 'IEnumerable' collection")]
	private static void AnalyzeDefinitions(EntropyModBase mod, Type type)
	{
		DefineCategories(mod, type);
		DefineEntries(mod, type);

		static void DefineCategories(EntropyModBase mod, Type type)
		{
			var patchCategoryDefinitions = type.GetCustomAttributes<ConfigCategoryDefinitionAttribute>();
			if (patchCategoryDefinitions != null && patchCategoryDefinitions.Any())
			{
				foreach (var definiton in patchCategoryDefinitions)
					ConfigCategory.Define(mod, definiton.Name, definiton.DisplayName, definiton.Description);
			}

			foreach(var member in type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).OfType<MemberInfo>()
				.Concat(type.GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				.Concat(type.GetFields(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)))
			{
				patchCategoryDefinitions = member.GetCustomAttributes<ConfigCategoryDefinitionAttribute>();
				if (patchCategoryDefinitions != null && patchCategoryDefinitions.Any())
				{
					foreach (var definition in patchCategoryDefinitions)
						ConfigCategory.Define(mod, definition.Name, definition.DisplayName, definition.Description);
				}
			}
		}
		static void DefineEntries(EntropyModBase mod, Type type)
		{

			var patchConfigDefinitions = type.GetCustomAttributes<PatchConfigDefinitionAttribute>();
			var typeCategory = LookupCategoryByType(mod, type);
			if (patchConfigDefinitions != null && patchConfigDefinitions.Any())
			{
				foreach (var definiton in patchConfigDefinitions)
					PatchConfigEntry.Define(mod, definiton.Category ?? typeCategory, definiton);
			}

			foreach (var member in type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).OfType<MemberInfo>()
				.Concat(type.GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				.Concat(type.GetFields(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)))
			{
				patchConfigDefinitions = member.GetCustomAttributes<PatchConfigDefinitionAttribute>();
				var methodCategory = LookupCategoryByMember(mod, member) ?? typeCategory;
				if (patchConfigDefinitions != null && patchConfigDefinitions.Any())
				{
					foreach (var definition in patchConfigDefinitions)
						PatchConfigEntry.Define(mod, definition.Category ?? methodCategory, definition);
				}
			}
			// Unlike patch entry definitions, auto config definitions can only be applied to static properties.
			foreach (var property in type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
			{
				var autoConfigs = property.GetCustomAttributes<AutoConfigDefinitionAttribute>();
				var propertyCategory = LookupCategoryByMember(mod, property) ?? typeCategory;
				var count = autoConfigs.Count();
				if (autoConfigs != null && count > 0)
				{
					if (count > 1)
					{
						mod.Logger.LogError($"Property `{property.DeclaringType?.FullName}.{property.Name}' has multiple AutoConfigDefinition attributes. Only one is allowed. This property will be ignored for auto config generation.");
						continue;
					}
					var getter = property.GetGetMethod(true);
					var setter = property.GetSetMethod(true);
					if (getter is null || setter is null)
					{
						mod.Logger.LogError($"Property `{property.DeclaringType?.FullName}.{property.Name}' should have both getter and setter to be used for auto config generation. This property will be ignored.");
						continue;
					}
					var definition = autoConfigs.FirstOrDefault();
					var methodInfo = typeof(AutoConfigEntry<>).MakeGenericType(property.PropertyType).GetMethod(
						nameof(AutoConfigEntry<>.Define),
						[
							typeof(EntropyModBase),
							typeof(PropertyInfo),
							typeof(string),
							typeof(AutoConfigDefinitionAttribute),
						]);
					if (methodInfo is null)
						throw new ApplicationException($"Failed to find `Define' method for AutoConfigEntry of type `{property.PropertyType.FullName}' for property `{property.DeclaringType?.FullName}.{property.Name}'! Entropy.Common has a bug that needs to be fixed.");
					methodInfo.Invoke(null, [mod, property, definition.Category ?? propertyCategory, definition]);
				}
			}
		}
	}

	[SuppressMessage("Performance", "CA1851:Possible multiple enumerations of 'IEnumerable' collection")]
	private static string? LookupCategoryByType(EntropyModBase mod, Type type)
	{
		var categoryAttrs = type.GetCustomAttributes<ConfigCategoryAttribute>();
		if (categoryAttrs.Count() == 1)
			return categoryAttrs.First().Category;
		else if (categoryAttrs.Count() > 1)
			mod.Logger.LogError($"Type `{type.FullName}' has multiple ConfigCategoryAttribute attributes. Only one is allowed.");
		var definitions = type.GetCustomAttributes<ConfigCategoryDefinitionAttribute>();
		if (definitions != null && definitions.Count() == 1)
			return definitions.First().Name;
		return null;
	}

	[SuppressMessage("Performance", "CA1851:Possible multiple enumerations of 'IEnumerable' collection")]
	private static string? LookupCategoryByMember(EntropyModBase mod, MemberInfo method)
	{
		var categoryAttrs = method.GetCustomAttributes<ConfigCategoryAttribute>();
		if (categoryAttrs.Count() == 1)
			return categoryAttrs.First().Category;
		else if (categoryAttrs.Count() > 1)
			mod.Logger.LogError($"Method `{method.DeclaringType?.FullName}.{method.Name}' has multiple ConfigCategoryAttribute attributes. Only one is allowed.");
		var definitions = method.GetCustomAttributes<ConfigCategoryDefinitionAttribute>();
		if (definitions != null && definitions.Count() == 1)
			return definitions.First().Name;
		return null;
	}
	private static void AddPropertyHandlers(EntropyModBase mod, MethodInfo method)
	{
		AddPropertyHandlersBody<ConfigPropertyChangingAttribute>(
			mod,
			method,
			(attr) => attr.Category,
			(attr) => attr.Name,
			(entry, method) => entry.SubscribeOnPropertyChanging(method),
			typeof(bool),
			2);
		AddPropertyHandlersBody<ConfigPropertyChangedAttribute>(
			mod,
			method,
			(attr) => attr.Category,
			(attr) => attr.Name,
			(entry, method) => entry.SubscribeOnPropertyChanged(method),
			typeof(void),
			1);
		static void AddPropertyHandlersBody<TAttribute>(
			EntropyModBase mod,
			MethodInfo method,
			Func<TAttribute, string?> getCategory,
			Func<TAttribute, string> getName,
			Action<ConfigEntryBase, MethodInfo> subscription,
			Type returnType,
			int parametersLength)
			where TAttribute : Attribute
		{
			var propertyChanging = method.GetCustomAttributes<TAttribute>();
			var attrName = typeof(TAttribute).Name;
			if (propertyChanging.Any())
			{
				if (propertyChanging.Count() > 1)
				{
					mod.Logger.LogError($"Method `{method.DeclaringType?.FullName}.{method.Name}' has multiple {attrName} attributes. Only one is allowed. This method will be ignored for config change notifications.");
					return;
				}
				if (!method.IsStatic)
				{
					mod.Logger.LogError($"Method `{method.DeclaringType?.FullName}.{method.Name}' has {attrName} but is not static. This method will be ignored for config change notifications.");
					return;
				}
				var parameters = method.GetParameters();
				var attr = propertyChanging.First();
				var category = ConfigCategory.Get(mod, getCategory(attr));
				if (category is null)
				{
					mod.Logger.LogError($"Method `{method.DeclaringType?.FullName}.{method.Name}' has {attrName} with Category `{category}' that does not exist. This method will be ignored for config change notifications.");
					return;
				}
				var name = getName(attr);
				var entry = ConfigEntryBase.Get(mod, name, category);
				if (entry is null)
				{
					mod.Logger.LogError($"Method `{method.DeclaringType?.FullName}.{method.Name}' has {attrName} entry `{category}.{name}' that does not exist. This method will be ignored for config change notifications.");
					return;
				}
				if (method.ReturnType != returnType || parameters.Length != parametersLength ||
					!parameters[0].ParameterType.IsAssignableFrom(entry.GetType()) ||
					(parametersLength > 1 && !parameters[1].ParameterType.IsAssignableFrom(entry.Type)))
				{
					mod.Logger.LogError($"Method `{method.DeclaringType?.FullName}.{method.Name}' has {attrName} `{category}.{name}' but does not have the correct signature.");
					return;
				}
				subscription(entry, method);
			}
		}
	}
}