
using Entropy.Attributes;
using Entropy.Mods;

namespace Entropy.Patches;

/// <summary>
/// A class that represents a category of a patch by a name, which is used to group patches and enable/disable them all at once.
/// </summary>
public class PatchCategory
{
	private const string DefaultCategoryName = "Main";
	private static readonly List<PatchCategory> _existingCategories = [];
	private static readonly Dictionary<EntropyMod, List<PatchCategory>> _modPatchCategories = [];
	private static IReadOnlyDictionary<EntropyMod, IReadOnlyList<PatchCategory>>? _modPatchCategoriesCache;
	private string? displayName;

	/// <summary>
	/// A list of all currently existing patch categories.
	/// </summary>
	public static IReadOnlyList<PatchCategory> ExistingCategories => _existingCategories.AsReadOnly();

	/// <summary>
	/// A dictionary of all patch categories, where the key is the <see cref="EntropyMod"/> and the value is a list of <see cref="PatchCategory"/> instances.
	/// </summary>
	public static IReadOnlyDictionary<EntropyMod, IReadOnlyList<PatchCategory>> ModPatchCategories
	{
		get
		{
			if(_modPatchCategoriesCache is null)
			{
				var result = new Dictionary<EntropyMod, IReadOnlyList<PatchCategory>>();
				foreach(var mod in _modPatchCategories.Keys)
				{
					result.Add(mod, _modPatchCategories[mod].AsReadOnly());
				}
				_modPatchCategoriesCache = result;
			}
			return _modPatchCategoriesCache;
		}
	}

	/// <summary>
	/// The mod that this category belongs to.
	/// </summary>
	public EntropyMod Mod { get; private set; }

	/// <summary>
	/// The name of the patch category, as used in <see cref="HarmonyPatchCategoryAttribute"/>.
	/// </summary>
	public string Name { get; private set; }

	/// <summary>
	/// A flag indicating if this is the default category. The default category is used when no category is specified in the <see cref="HarmonyPatchCategoryAttribute"/> or <see cref="PatchCategoryDefinitionAttribute"/>.
	/// </summary>
	public bool IsDefault { get; private set; }

	/// <summary>
	/// A flag indicating if this category is enabled. If the category is disabled, all patches in this category will be ignored.
	/// </summary>
	public bool Enabled
	{
		get => Mod.Config.GetCategoryEntry(this).Value;
		set => Mod.Config.GetCategoryEntry(this).Value = value;
	}

	/// <summary>
	/// The display name of the patch category, used to display the category name.
	/// </summary>
	public string DisplayName 
	{
		get => this.displayName ?? Name;
		private set => this.displayName = value;
	}

	/// <summary>
	/// The description of the patch category used to display in hints.
	/// </summary>
	public string? Description { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="PatchCategory"/> class with the specified name.
	/// </summary>
	/// <param name="mod"> The mod that this category belongs to.</param>
	/// <param name="name">The name of the patch category.</param>
	public PatchCategory(EntropyMod mod, string name) : this(mod, name, name, null!) { }
	/// <summary>
	/// Initializes a new instance of the <see cref="PatchCategory"/> class with the specified name and description.
	/// </summary>
	/// <param name="mod"> The mod that this category belongs to.</param>
	/// <param name="name">The name of the patch category.</param>
	/// <param name="description">The description of the patch category.</param>
	public PatchCategory(EntropyMod mod, string name, string description) : this(mod, name, name, description) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="PatchCategory"/> class with the specified name, display name, and description.
	/// </summary>
	/// <param name="mod"> The mod that this category belongs to.</param>
	/// <param name="name">The name of the patch category, as used in <see cref="HarmonyPatchCategoryAttribute"/>.</param>
	/// <param name="displayName">The display name used to display the category name.</param>
	/// <param name="description">The description of the patch category used to display in hints.</param>
	public PatchCategory(EntropyMod mod, string name, string displayName, string description)
	{
		Mod = mod;
		Name = name;
		IsDefault = name == DefaultCategoryName;
		DisplayName = displayName;
		Description = description;
	}

	/// <summary>
	/// Gets a <see cref="PatchCategory"/> instance by its name. If the category does not exist, it creates a new one.
	/// </summary>
	/// <param name="mod"> The mod that this category belongs to.</param>
	/// <param name="category">The name of the category to get.</param>
	/// <returns>A <see cref="PatchCategory"/> instance with the specified name.</returns>
	public static PatchCategory Get(EntropyMod mod, string category)
	{
		foreach(var existingCategory in _existingCategories)
		{
			if(existingCategory.Mod == mod && existingCategory.Name.Equals(category, StringComparison.OrdinalIgnoreCase))
			{
				return existingCategory;
			}
		}
		if(!_modPatchCategories.TryGetValue(mod, out var modCategories))
		{
			modCategories = [];
			_modPatchCategories.Add(mod, modCategories);
		}
		var result = new PatchCategory(mod, category);
		modCategories.Add(result);
		_existingCategories.Add(result);
		mod.Config.BindCategory(result);
		mod.AddCategory(result);
		return result;
	}

	/// <summary>
	/// Gets default patch category for the specified mod. This category is used when no category is specified in the <see cref="HarmonyPatchCategoryAttribute"/> or <see cref="PatchCategoryDefinitionAttribute"/>.
	/// </summary>
	/// <param name="mod">The mod default category of which to get.</param>
	/// <returns>A default patch category.</returns>
	public static PatchCategory GetDefault(EntropyMod mod) => Get(mod, DefaultCategoryName);

	/// <summary>
	/// Defines a new patch category with the specified name, display name, and description.
	/// </summary>
	/// <param name="mod">The mod category of which to define.</param>
	/// <param name="name">The name of the patch category, as used in <see cref="HarmonyPatchCategoryAttribute"/>.</param>
	/// <param name="displayName">The display name used to display the category name.</param>
	/// <param name="description">The description of the patch category used to display in hints.</param>
	public static void Define(EntropyMod mod, string name, string displayName, string description)
	{
		var category = Get(mod, name);
		category.DisplayName = displayName;
		category.Description = description;
		category.Mod = mod;
	}
}