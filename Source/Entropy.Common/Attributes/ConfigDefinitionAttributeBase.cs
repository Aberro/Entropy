using Entropy.Common.Configs;
using Entropy.Common.Utils;
using System.Diagnostics.CodeAnalysis;

namespace Entropy.Common.Attributes;

[SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix")]
public abstract class ConfigDefinitionAttributeBase : Attribute
{
	/// <summary>
	/// The internal name of the configuration entry.
	/// </summary>
	public string Name { get; }
	/// <summary>
	/// The description of the entry used to display in hints.
	/// </summary>
	public string Description { get; }
	/// <summary>
	/// Category name to which this configuration entry belongs to. If not specified, it will be added to the default category.
	/// </summary>
	public string? Category { get; }
	/// <summary>
	/// Ordering value for this entry in configuration panel. Entries with lower order value will be displayed first. Default value is 0.
	/// </summary>
	public int Order { get; set; } = DefaultTagValues.Order;
	/// <summary>
	/// The display name used to display the entry name.
	/// </summary>
	public string? DisplayName { get; set; } = DefaultTagValues.DisplayName;
	/// <summary>
	/// Whether this entry is visible in the configuration panel.
	/// </summary>
	public bool Visible { get; set; } = DefaultTagValues.Visible;
	/// <summary>
	/// Whether this entry is enabled in configuration panel.
	/// </summary>
	public bool Enabled { get; set; } = DefaultTagValues.Disabled;
	/// <summary>
	/// Whether changing this entry value requires a restart to take effect.
	/// </summary>
	public bool RequireRestart { get; set; } = DefaultTagValues.RequireRestart;
	/// <summary>
	/// Custom format string for this entry values in configuration panel. Applicable only for vector and floating point values.
	/// </summary>
	public string Format { get; set; } = DefaultTagValues.Format;
	/// <summary>
	/// The type where the custom drawer method for this entry is declared.
	/// The method must be static and have the signature `bool CustomDrawer(ConfigEntryBase entry)`.
	/// </summary>
	public Type? CustomDrawerType { get; set; }
	/// <summary>
	/// The name of the custom drawer method for this entry in configuration panel.
	/// The method must be static and have the signature `bool CustomDrawer(ConfigEntryBase entry)`.
	/// </summary>
	public string? CustomDrawer { get; set; }

	protected internal ConfigDefinitionAttributeBase(string name, string description, string? category = null!)
	{
		ArgumentNullException.ThrowIfNull(name);
		ArgumentNullException.ThrowIfNull(description);

		Name = name;
		Description = description;
		Category = category;
	}
}