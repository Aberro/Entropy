using Entropy.Common.Mods;
using Entropy.Common.Utils;
using LaunchPadBooster;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Entropy.Common.Configs;

/// <summary>
/// A class representing a configuration entry.
/// </summary>
/// <typeparam name="T">The type of the configuration entry value.</typeparam>
public class ConfigEntry<T> : ConfigEntryBase
{

	/// <inheritdoc/>
	public override Type Type => typeof(T);
	[MaybeNull]
	public Optional<T> Default { get; }
	public Optional<T> MinValue { get; }
	public Optional<T> MaxValue { get; }
	public new T? Value
	{
		get => base.Value is null ? default : (T?)base.Value;
		set
		{
			if (value is IComparable comparable)
			{
				if (MinValue != null)
				{
					if (comparable.CompareTo(MinValue) < 0)
						return;
				}
				if (MaxValue != null)
				{
					if (comparable.CompareTo(MaxValue) > 0)
						return;
				}
			}
			base.Value = value!;
		}
	}

	/// <summary>
	/// An event that is triggered before the value of this configuration entry is changed.
	/// </summary>
	public PropertyChangingEventHandler<T>? PropertyChanging { get; set; }
	/// <summary>
	/// An event that is triggered after the value of this configuration entry is changed.
	/// </summary>
	public PropertyChangedEventHandler<T>? PropertyChanged { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="PatchConfigEntry"/> class with the specified name.
	/// </summary>
	/// <param name="mod"> The mod that this entry belongs to.</param>
	/// <param name="name">The name of the patch category.</param>
	/// <param name="category">The category this entry belongs to.</param>
	internal ConfigEntry(EntropyModBase mod, string name, string category) : this(mod, name, "", category) { }
	/// <summary>
	/// Initializes a new instance of the <see cref="PatchConfigEntry"/> class with the specified name and description.
	/// </summary>
	/// <param name="mod"> The mod that this category belongs to.</param>
	/// <param name="name">The name of the patch category.</param>
	/// <param name="description">The description of the patch category.</param>
	/// <param name="category">The category this entry belongs to.</param>
	internal ConfigEntry(EntropyModBase mod, string name, string description, string category) : this(mod, name, description, category, default, default, default) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="PatchConfigEntry"/> class with the specified name, display name, and description.
	/// </summary>
	/// <param name="mod"> The mod that this category belongs to.</param>
	/// <param name="name">The name of the patch category, as used in <see cref="ConfigCategoryAttribute"/>.</param>
	/// <param name="displayName">The display name used to display the category name.</param>
	/// <param name="description">The description of the patch category used to display in hints.</param>
	/// <param name="category">The category this entry belongs to.</param>
	/// <param name="defaultValue">The default value of this config entry. This is used to initialize the config entry when it is first created.</param>
	internal ConfigEntry(EntropyModBase mod, string name, string description, string? category, Optional<T> defaultValue, Optional<T> minValue, Optional<T> maxValue)
		: base(mod, name, description, category)
	{
		ArgumentNullException.ThrowIfNull(mod);
		Default = defaultValue;
		MinValue = minValue;
		MaxValue = maxValue;
	}

	/// <summary>
	/// Gets a <see cref="PatchConfigEntry"/> instance by its name. If the category does not exist, it creates a new one.
	/// </summary>
	/// <param name="mod"> The mod that this category belongs to.</param>
	/// <param name="entry">The name of the entry to get.</param>
	/// <param name="category">The name of the category the entry belongs to.</param>
	/// <returns>A <see cref="PatchConfigEntry"/> instance with the specified name.</returns>
	public static ConfigEntry<T>? Get(EntropyModBase mod, string entry, string? category) => 
		Get<T>(mod, entry, ConfigCategory.Get(mod, category) ?? ConfigCategory.GetDefault(mod));

	/// <summary>
	/// Defines a new patch category with the specified name, display name, and description.
	/// </summary>
	/// <param name="mod">The mod category of which to define.</param>
	/// <param name="name">The name of the patch category.</param>
	/// <param name="description">The description of the patch category used to display in hints.</param>
	/// <param name="category">The name of the category the entry belongs to.</param>
	/// <param name="defaultValue">The default value of this config entry.</param>
	public static ConfigEntry<T> Define(EntropyModBase mod,
		string name,
		string description,
		string? category,
		Optional<T> defaultValue,
		Optional<T> minValue,
		Optional<T> maxValue)
	{
		ArgumentNullException.ThrowIfNull(mod);
		ArgumentNullException.ThrowIfNull(name);
		ArgumentNullException.ThrowIfNull(description);

		var categoryObj = ConfigCategory.Get(mod, category) ?? throw new ApplicationException("Tried to use category that is not defined! Use PatchCategoryDefinitionAttribute to define a category");
		if(Get<ConfigEntry<T>>(mod, name, categoryObj) is ConfigEntry<T> existingEntry)
			return existingEntry;
		var result = new ConfigEntry<T>(mod, name, description, category, defaultValue, minValue, maxValue);
		mod.Config.BindConfigEntry(result, result.Default, result.MinValue, result.MaxValue);
		return result;
	}

	internal override bool OnPropertyChanging(object value)
	{
		var args = new PropertyChangingEventArgs<T>(this, (T) value);
		PropertyChanging?.Invoke(this, args);
		return !args.Cancel;
	}
	internal override void OnPropertyChanged()
	{
		base.OnPropertyChanged();
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs<T>(this));
	}
	internal override void SubscribeOnPropertyChanging(MethodInfo handler)
	{
		// This invocation should happen with handlers already validated, so the expected signature should
		// be bool Method(TEntry entry, T newValue), where TEntry is the type of this config entry (either exact type, or base type)
		//var handlerDelegate = (Func<ConfigEntryBase, T, bool>) handler.CreateDelegate(typeof(Func<ConfigEntryBase, T, bool>));
		PropertyChanging += (sender, args) => args.Cancel = !(bool)handler.Invoke(null, [args.Entry, args.Value]);
	}
	internal override void SubscribeOnPropertyChanged(MethodInfo handler)
	{
		// This invocation should happen with handlers already validated, so the expected signature should
		// be void Method(TEntry entry), where TEntry is the type of this config entry (either exact type, or base type)
		//var handlerDelegate = (Action<ConfigEntryBase>) handler.CreateDelegate(typeof(Action<ConfigEntryBase>));
		PropertyChanged += (sender, args) => handler.Invoke(null, [args.Entry]);
	}
}