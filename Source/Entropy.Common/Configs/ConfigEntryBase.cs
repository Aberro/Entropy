using Cysharp.Threading.Tasks;
using Entropy.Common.Mods;
using Entropy.Common.Utils;
using HarmonyLib;
using ImGuiNET.Unity;
using LaunchPadBooster;
using System.Reflection;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace Entropy.Common.Configs;

/// <summary>
/// A base class representing a configuration entry. This is used to store the configuration entries in the <see cref="Config"/> class, and to define the common properties of the configuration entries.
/// </summary>
public abstract class ConfigEntryBase
{
	private enum TagsEntry
	{
		Order = 0,
		DisplayName = 1,
		Visible = 2,
		Disabled = 3,
		CustomDrawer = 4,
		RequiresRestart = 5,
		Format = 6,
	}
	// Not as much 'cached' as 'copied', because we want PropertyChanging functionality, but BepInEx does not provide us with such,
	// so instead we store the original value and restore it if PropertyChanging returns false.
	private object? _valueCached;
	private BepInEx.Configuration.ConfigEntryBase _configEntry = null!;
	private static readonly Dictionary<ConfigCategory, Dictionary<string, ConfigEntryBase>> _existingEntries = [];
	private object[] _tags = null!;
	private MethodInfo _onSettingChanged = null!;
	private object[] _onSettingChangedParameters;

	/// <summary>
	/// The mod that this entry belongs to.
	/// </summary>
	public EntropyModBase Mod { get; private set; }

	/// <summary>
	/// The category this entry belongs to.
	/// </summary>
	public ConfigCategory Category { get; private set; }

	/// <summary>
	/// The type of the configuration entry value.
	/// </summary>
	public abstract Type Type { get; }

	/// <summary>
	/// The name of the configuration entry, as used internally.
	/// </summary>
	public string Name { get; private set; }

	public object? Value
	{
		get => _valueCached;
		set => _configEntry.BoxedValue = value;
	}


	/// <summary>
	/// Whether this entry is visible in the configuration panel.
	/// </summary>
	public bool Visible
	{
		get;
		set
		{
			field = value;
			SetTag(TagsEntry.Visible, value);
		}
	} = DefaultTagValues.Visible;

	/// <summary>
	/// Whether this entry is enabled in configuration panel.
	/// </summary>
	public bool Disabled
	{
		get;
		set
		{
			field = value;
			SetTag(TagsEntry.Disabled, value);
		}
	} = DefaultTagValues.Disabled;
	/// <summary>
	/// Custom format string for this entry values in configuration panel. Applicable only for vector and floating point values.
	/// </summary>
	public string Format
	{
		get;
		set
		{
			field = value;
			SetTag(TagsEntry.Format, value);
		}
	} = DefaultTagValues.Format;

	/// <summary>
	/// The display name of the configuration entry, used to display the category name. If null - the <see cref="Name"/> will be used as display name.
	/// </summary>
	public string DisplayName
	{
		get => field ?? Name;
		set
		{
			field = value;
			SetTag(TagsEntry.DisplayName, value ?? Name);
		}
	} = DefaultTagValues.DisplayName;
	/// <summary>
	/// The description of the patch category used to display in hints.
	/// </summary>
	public string? Description { get; private set; }
	/// <summary>
	/// Ordering value for this entry in configuration panel. Entries with lower order value will be displayed first. Default value is 0.
	/// </summary>
	public int Order
	{
		get;
		set
		{
			field = value;
			SetTag(TagsEntry.Order, value);
		}
	} = DefaultTagValues.Order;
	/// <summary>
	/// Custom drawer for this entry in configuration panel.
	/// This is a function that takes a <see cref="ConfigEntryBase"/> instance and returns a boolean value indicating 
	/// whether the value has changed.
	/// </summary>
	public Func<ConfigEntryBase, bool>? CustomDrawer
	{
		get;
		set
		{
			field = value;
			SetTag(TagsEntry.CustomDrawer, Mod.Config.ConvertCustomDrawer(value));
		}
	} = DefaultTagValues.CustomDrawer;
	/// <summary>
	/// Whether changing this entry's value requires a game restart to take effect.
	/// This is used to display a warning in the configuration panel when the value has changed.
	/// </summary>
	public bool RequireRestart
	{ 
		get;
		set
		{
			field = value;
			SetTag(TagsEntry.RequiresRestart, value);
		}
	} = DefaultTagValues.RequireRestart;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2214:Do not call overridable methods in constructors", Justification = "There's no other way")]
	protected internal ConfigEntryBase(EntropyModBase mod, string name, string description, string? category)
	{
		ArgumentNullException.ThrowIfNull(mod);
		ArgumentNullException.ThrowIfNull(name);
		ArgumentNullException.ThrowIfNull(description);
		Mod = mod;
		Name = name;
		Description = description;
		Category = ConfigCategory.Get(mod, category) ?? throw new ApplicationException("Tried to use category '{category}' that is not defined! Use PatchCategoryDefinitionAttribute to define a category");
		_onSettingChangedParameters = [this];
	}

	/// <summary>
	/// Gets a configuration entry by its name and default category. Returns null if the entry does not exist.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="mod"></param>
	/// <param name="entry"></param>
	/// <returns></returns>
	/// <exception cref="ApplicationException">Thrown when entry exists but is of a different type.</exception>
	public static ConfigEntry<T>? Get<T>(EntropyModBase mod, string entry) => Get<T>(mod, entry, ConfigCategory.GetDefault(mod));
	/// <summary>
	/// Gets a configuration entry by its name and category. Returns null if the entry does not exist.
	/// </summary>
	/// <typeparam name="T">Entry value type</typeparam>
	/// <param name="mod"></param>
	/// <param name="entry"></param>
	/// <param name="category"></param>
	/// <returns></returns>
	/// <exception cref="ApplicationException">Thrown when entry exists but is of a different type.</exception>
	public static ConfigEntry<T>? Get<T>(EntropyModBase mod, string entry, ConfigCategory category)
	{
		ArgumentNullException.ThrowIfNull(mod);
		ArgumentNullException.ThrowIfNull(entry);
		ArgumentNullException.ThrowIfNull(category);
		var baseEntry = Get(mod, entry, category);
		if (baseEntry is null)
			return null;
		if (baseEntry is not ConfigEntry<T> typedEntry)
			throw new ApplicationException($"Tried to get config entry {category.Name}.{entry} for mod {mod.Info.Name} with type {typeof(ConfigEntry<T>).FullName}, but it is of type {baseEntry.Type.FullName}!");
		return typedEntry;
	}
	public static ConfigEntryBase? Get(EntropyModBase mod, string entry) => Get(mod, entry, ConfigCategory.GetDefault(mod));
	public static ConfigEntryBase? Get(EntropyModBase mod, string entry, ConfigCategory category)
	{
		ArgumentNullException.ThrowIfNull(mod);
		ArgumentNullException.ThrowIfNull(entry);
		ArgumentNullException.ThrowIfNull(category);
		if (!_existingEntries.TryGetValue(category, out var modEntries))
		{
			if (modEntries is null)
				return null;
		}
		modEntries.TryGetValue(entry, out var existingEntry);
		return existingEntry;
	}

	public void Save()
	{
		_onSettingChanged.Invoke(_configEntry, _onSettingChangedParameters);
	}

	internal abstract bool OnPropertyChanging(object value);
	internal virtual void OnPropertyChanged()
	{
		_valueCached = _configEntry.BoxedValue;
	}
	internal abstract void SubscribeOnPropertyChanging(MethodInfo handler);
	internal abstract void SubscribeOnPropertyChanged(MethodInfo handler);

	internal void BindBepInExEntry(BepInEx.Configuration.ConfigEntryBase entry)
	{
		_configEntry = entry;
		
		_onSettingChanged = typeof(BepInEx.Configuration.ConfigEntryBase).GetMethod("OnSettingChanged");
		_valueCached = _configEntry.BoxedValue;
		_tags = _configEntry.Description.Tags;
		if (_tags is null || _tags.Length < Enum.GetValues(typeof(TagsEntry)).Length)
			throw new ApplicationException("BepInEx entry tags array is not properly initialized! This is a bug in Entropy's config system, please report it to the mod author!");
		Register(this);
	}

	private static void Register(ConfigEntryBase entry)
	{
		if (!_existingEntries.TryGetValue(entry.Category, out var modEntries))
		{
			modEntries = [];
			_existingEntries[entry.Category] = modEntries;
		}
		if(modEntries.ContainsKey(entry.Name))
			throw new ApplicationException($"Tried to register config entry {entry.Category.Name}.{entry.Name} for mod {entry.Mod.Info.Name}, but it already exists!");
		modEntries[entry.Name] = entry;
	}
	private void SetTag<TTag>(TagsEntry index, TTag value)
	{
		_tags[(int) index] = new KeyValuePair<string, TTag>(index.ToString(), value);
	}
}