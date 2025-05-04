using BepInEx.Configuration;
using Entropy.Patches;

namespace Entropy.Mods;

/// <summary>
/// A class to store the configuration for the mod.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Config"/> class.
/// </remarks>
/// <param name="mod">The mod that this configuration belongs to.</param>
/// <param name="configFilePath">The path to the configuration file.</param>
public class Config(EntropyMod mod, string configFilePath) : ConfigFile(configFilePath, false)
{
	private readonly EntropyMod mod = mod;

	private readonly Dictionary<PatchCategory, ConfigEntry<bool>> categories = [];

	/// <summary>
	/// Returns a category definition for a given configuration entry, or <see langword="null"/> if the entry is not a category.
	/// </summary>
	/// <param name="entry">The configuration entry to get the category for.</param>
	/// <returns>The category definition for the given configuration entry, or <see langword="null"/> if the entry is not a category.</returns>
	public PatchCategory? GetCategory(ConfigEntryBase entry)
	{
		foreach(var category in this.categories)
		{
			if(category.Value == entry)
				return category.Key;
		}
		return null;
	}

	/// <summary>
	/// Returns the configuration entry for a given category.
	/// </summary>
	/// <param name="category">The category to get the configuration entry for.</param>
	/// <returns>The configuration entry for the given category.</returns>
	/// <exception cref="ArgumentException">Thrown when the category is not bound to the mod.</exception>
	public ConfigEntry<bool> GetCategoryEntry(PatchCategory category)
	{
		if(this.categories.TryGetValue(category, out var entry))
			return entry;
		throw new ArgumentException($"The category {category.Mod.Name}.{category.Name} is not bound to the mod {this.mod.Name}.");
	}

	/// <summary>
	/// Binds the category with a configuration entry.
	/// </summary>
	/// <param name="category">The category to bind.</param>
	public void BindCategory(PatchCategory category)
	{
		if(this.mod != category.Mod)
			throw new ArgumentException($"The category {category.Mod.Name}.{category.Name} does not belong to the mod {this.mod.Name}.");
		if(this.categories.ContainsKey(category))
			return;
		var entry = Bind(category.Name, "Enabled", true, category.Description ?? "");
		this.categories.Add(category, entry);
		this.mod.Config.BindCategory(category);
		this.mod.Config.Save();
	}
}