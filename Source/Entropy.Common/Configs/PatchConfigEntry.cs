using Entropy.Common.Attributes;
using Entropy.Common.Mods;
using Entropy.Common.Utils;
using HarmonyLib;

namespace Entropy.Common.Configs;

/// <summary>
/// A class that represents a configuration file entry, which is used to enable/disable patch(es).
/// </summary>
public class PatchConfigEntry : ConfigEntry<bool>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="PatchConfigEntry"/> class with the specified name, display name, and description.
	/// </summary>
	/// <param name="mod"> The mod that this category belongs to.</param>
	/// <param name="name">The name of the patch category, as used in <see cref="ConfigCategoryAttribute"/>.</param>
	/// <param name="description">The description of the patch category used to display in hints.</param>
	/// <param name="category">The category this entry belongs to.</param>
	/// <param name="defaultValue">The default value of this config entry.</param>
	protected PatchConfigEntry(EntropyModBase mod, string name, string description, string? category, bool defaultValue)
		: base(mod, name, description, category, defaultValue, default, default)
	{
	}

	public static new PatchConfigEntry? Get (EntropyModBase mod, string name, string? category) =>
		(PatchConfigEntry?)Get<bool>(mod, name, ConfigCategory.Get(mod, category) ?? ConfigCategory.GetDefault(mod));
	public static new PatchConfigEntry? Get(EntropyModBase mod, string name, ConfigCategory category) =>
		(PatchConfigEntry?)Get<bool>(mod, name, category);

	/// <summary>
	/// Defines a new patch category with the specified name, display name, and description.
	/// </summary>
	/// <param name="mod">The mod category of which to define.</param>
	/// <param name="category">The name of the category the entry belongs to.</param>
	/// <param name="attribute">The attribute containing the definition of the patch category.</param>
	/// <exception cref="ApplicationException"></exception>
	public static PatchConfigEntry Define(EntropyModBase mod, string? category, PatchConfigDefinitionAttribute attribute)
	{
		ArgumentNullException.ThrowIfNull(mod);
		ArgumentNullException.ThrowIfNull(attribute);

		var categoryObj = ConfigCategory.Get(mod, category) ?? throw new ApplicationException($"Tried to use category '{category}'that is not defined! Use PatchCategoryDefinitionAttribute to define a category");
		if (Get<bool>(mod, attribute.Name, categoryObj) is PatchConfigEntry existingEntry)
			return existingEntry;
		var result = new PatchConfigEntry(mod, attribute.Name, attribute.Description, category, attribute.DefaultValue);
		mod.Config.BindConfigEntry(result, result.Default, result.MinValue, result.MaxValue);
		result.Order = attribute.Order;
		result.DisplayName = attribute.DisplayName!;
		result.Disabled = attribute.Enabled;
		result.Visible = attribute.Visible;
		result.RequireRestart = attribute.RequireRestart;
		return result;
	}
}