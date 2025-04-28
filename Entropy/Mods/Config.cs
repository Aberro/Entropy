using BepInEx.Configuration;
using Entropy.Patches;

namespace Entropy.Mods;

/// <summary>
/// A class to store the configuration for the mod.
/// </summary>
public class Config : ConfigFile
{
	private readonly EntropyModData mod;

	private readonly Dictionary<PatchCategory, ConfigEntry<bool>> categories = [];

	/// <summary>
	/// Initializes a new instance of the <see cref="Config"/> class.
	/// </summary>
	/// <param name="mod">The mod that this configuration belongs to.</param>
	/// <param name="configFilePath">The path to the configuration file.</param>
	public Config(EntropyModData mod, string configFilePath) : base(configFilePath, false)
	{
		this.mod = mod;
	}

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
}