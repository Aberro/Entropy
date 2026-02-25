namespace Entropy.Common.Attributes;

/// <summary>
/// Attribute that is used to define a configuration category.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
public sealed class ConfigCategoryDefinitionAttribute : Attribute
{
	/// <summary>
	/// The name of the patch category, the name that is used in <see cref="ConfigCategoryAttribute"/>.
	/// </summary>
	public string Name { get; }
	/// <summary>
	/// The display name used to display the category name.
	/// </summary>
	public string DisplayName { get; }
	/// <summary>
	/// The description of the patch category used to display in hints.
	/// </summary>
	public string Description { get; }

	/// <summary>
	/// A flag indicating if this category has an enable config entry to enable/disable all patches in this category.
	/// </summary>
	public bool HasEnableToggle { get; set; }

	/// <summary>
	/// Creates a new instance of the <see cref="ConfigCategoryDefinitionAttribute"/> attribute.
	/// </summary>
	/// <param name="name">Patch category name, the name that is used in <see cref="ConfigCategoryAttribute"/>.</param>
	/// <param name="displayName">Display name used to display the category name.</param>
	/// <param name="description">Description of the patch category used to display in hints.</param>
	public ConfigCategoryDefinitionAttribute(string name, string displayName, string description)
	{
		Name = name;
		DisplayName = displayName;
		Description = description;
	}
}