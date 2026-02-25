namespace Entropy.Common.Attributes;

/// <summary>
/// An attribute to specify a method as a callback for when a configuration property is changing.
/// The method should have the signature of <c>bool MethodName(ConfigEntryBase entry, object value)</c>,
/// or <c>bool MethodName(TEntry entry, TValue value)</c> for a specific type TValue same as the config entry value type TEntry.
/// The method will be called before the configuration property value is changed, and if it returns false, the change will be cancelled.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class ConfigPropertyChangingAttribute : Attribute
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

	public ConfigPropertyChangingAttribute(string name, string? category = null)
	{
		Name = name;
		Category = category;
	}
}