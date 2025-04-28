namespace Entropy.Attributes;

/// <summary>
/// Attribute to mark a patch as unpatchable, i.e. it can be safely disabled and will revert any changes made by it.
/// </summary>
/// <remarks>
/// This attribute is unnecessary if the patch class have an Unpatch method.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class Unpatchable : Attribute { }