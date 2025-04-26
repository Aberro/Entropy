namespace System.ComponentModel;

/// <summary>
/// A simple facade for actual DisplayNameAttribute, to apply it to enum values.
/// </summary>
public class DisplayNameAttribute : Attribute
{
    public string DisplayName { get; }

    public DisplayNameAttribute(string displayName)
    {
        DisplayName = displayName;
    }
}