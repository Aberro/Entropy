#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.ComponentModel;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// A simple facade for actual DisplayNameAttribute, to apply it to enum values.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="DisplayNameAttribute"/> class with the specified display name.
/// </remarks>
/// <param name="displayName"></param>
public class DisplayNameAttribute(string displayName) : Attribute
{
	/// <summary>
	/// The name to display.
	/// </summary>
	public string DisplayName { get; } = displayName;
}