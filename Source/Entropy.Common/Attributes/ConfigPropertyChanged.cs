namespace Entropy.Common.Attributes;

/// <summary>
/// An attribute to specify a method as a callback for when a configuration property is changed.
/// The method should have the signature of <c>void MethodName(ConfigEntryBase entry)</c>
/// The method will be called after the configuration property value is changed.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class ConfigPropertyChangedAttribute : Attribute
{
	/// <summary>
	/// Config entry name.
	/// </summary>
	public string Name { get; }
	/// <summary>
	/// The category to which the config entry belongs.
	/// If not specified, the default category will be used.
	/// </summary>
	public string? Category { get; }

	public ConfigPropertyChangedAttribute(string name, string? category = null)
	{
		Name = name;
		Category = category;
	}
}