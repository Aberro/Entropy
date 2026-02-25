using Entropy.Common.Attributes;
using Entropy.Common.Mods;
using Entropy.Common.Utils;
using HarmonyLib;

namespace Entropy.Common.Configs;

/// <summary>
/// A class that represents a category of a patch by a name, which is used to group patches and enable/disable them all at once.
/// </summary>
public class ConfigCategory : IDisposable
{
	public const string DefaultCategoryName = "Common";
	public const string DefaultCategoryDisplayName = "Common";
	public const string DefaultCategoryDescription = "Common";
	public const string DefaultCategoryEnableName = "Enable";
	public const string DefaultCategoryEnableDescription = "Enable or disable all common patches.";
	private static readonly Dictionary<EntropyModBase, Dictionary<string, ConfigCategory>> _existingCategories = [];
	private string? _displayName;
	private Harmony _harmony;

	/// <summary>
	/// The Harmony instance used to patch this category.
	/// </summary>
	public Harmony Harmony => _harmony;

	/// <summary>
	/// The mod that this category belongs to.
	/// </summary>
	public EntropyModBase Mod { get; private set; }

	/// <summary>
	/// The name of the patch category, as used in <see cref="ConfigCategoryAttribute"/>.
	/// </summary>
	public string Name { get; private set; }

	/// <summary>
	/// A flag indicating if this is the default category. The default category is used when no category is specified in the <see cref="ConfigCategoryAttribute"/> or <see cref="ConfigCategoryDefinitionAttribute"/>.
	/// </summary>
	public bool IsDefault { get; private set; }

	/// <summary>
	/// The display name of the patch category, used to display the category name.
	/// </summary>
	public string DisplayName
	{
		get => this._displayName ?? Name;
		private set => this._displayName = value;
	}

	/// <summary>
	/// The description of the patch category used to display in hints.
	/// </summary>
	public string Description { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ConfigCategory"/> class with the specified name.
	/// </summary>
	/// <param name="mod"> The mod that this category belongs to.</param>
	/// <param name="name">The name of the patch category.</param>
	public ConfigCategory(EntropyModBase mod, string name) : this(mod, name, name, DefaultCategoryDescription) { }
	/// <summary>
	/// Initializes a new instance of the <see cref="ConfigCategory"/> class with the specified name and description.
	/// </summary>
	/// <param name="mod"> The mod that this category belongs to.</param>
	/// <param name="name">The name of the patch category.</param>
	/// <param name="description">The description of the patch category.</param>
	public ConfigCategory(EntropyModBase mod, string name, string description) : this(mod, name, name, description) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="ConfigCategory"/> class with the specified name, display name, and description.
	/// </summary>
	/// <param name="mod"> The mod that this category belongs to.</param>
	/// <param name="name">The name of the patch category, as used in <see cref="ConfigCategoryAttribute"/>.</param>
	/// <param name="displayName">The display name used to display the category name.</param>
	/// <param name="description">The description of the patch category used to display in hints.</param>
	public ConfigCategory(EntropyModBase mod, string name, string displayName, string description)
	{
		ArgumentNullException.ThrowIfNull(mod);
		ArgumentNullException.ThrowIfNull(name);
		ArgumentNullException.ThrowIfNull(displayName);
		ArgumentNullException.ThrowIfNull(description);
		Mod = mod;
		Name = name;
		IsDefault = name == DefaultCategoryName;
		DisplayName = displayName;
		Description = description;
		_harmony = new Harmony($"{mod.Info.ModID}.{Name}");
	}

	void IDisposable.Dispose()
	{
		GC.SuppressFinalize(this);
		(_harmony as IDisposable)?.Dispose();
	}

	/// <summary>
	/// Gets a <see cref="ConfigCategory"/> instance by its name. If the category does not exist, it creates a new one.
	/// </summary>
	/// <param name="mod"> The mod that this category belongs to.</param>
	/// <param name="category">The name of the category to get.</param>
	/// <returns>A <see cref="ConfigCategory"/> instance with the specified name.</returns>
	public static ConfigCategory? Get(EntropyModBase mod, string? category)
	{
		ArgumentNullException.ThrowIfNull(mod);
		if (!_existingCategories.TryGetValue(mod, out var modCategories))
		{
			if(category is null || category == DefaultCategoryName)
				return Define(mod, DefaultCategoryName, DefaultCategoryDisplayName, DefaultCategoryDescription);
			return null;
		}
		category ??= DefaultCategoryName;
		if (modCategories.TryGetValue(category, out var existingCategory))
			return existingCategory;
		return null;
	}

	/// <summary>
	/// Gets default patch category for the specified mod. This category is used when no category is specified in the <see cref="ConfigCategoryAttribute"/> or <see cref="ConfigCategoryDefinitionAttribute"/>.
	/// </summary>
	/// <param name="mod">The mod default category of which to get.</param>
	/// <returns>A default patch category.</returns>
	public static ConfigCategory GetDefault(EntropyModBase mod) => Get(mod, DefaultCategoryName) ?? Define(mod, DefaultCategoryName, DefaultCategoryDisplayName, DefaultCategoryDescription);

	/// <summary>
	/// Defines a new patch category with the specified name, display name, and description.
	/// </summary>
	/// <param name="mod">The mod category of which to define.</param>
	/// <param name="name">The name of the patch category, as used in <see cref="ConfigCategoryAttribute"/>.</param>
	/// <param name="displayName">The display name used to display the category name.</param>
	/// <param name="description">The description of the patch category used to display in hints.</param>
	public static ConfigCategory Define(EntropyModBase mod, string name, string displayName, string description)
	{
		ArgumentNullException.ThrowIfNull(mod);
		ArgumentNullException.ThrowIfNull(name);
		ArgumentNullException.ThrowIfNull(displayName);
		ArgumentNullException.ThrowIfNull(description);
		if (!_existingCategories.TryGetValue(mod, out var modCategories))
		{
			modCategories = [];
			_existingCategories.Add(mod, modCategories);
		}
		if (modCategories.TryGetValue(name, out var existingCategory))
			return existingCategory;
		var category = new ConfigCategory(mod, name, displayName, description);
		modCategories.Add(name, category);
		mod.AddCategory(category);
		return category;
	}
}