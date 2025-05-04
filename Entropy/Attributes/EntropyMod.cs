namespace Entropy.Attributes;

/// <summary>
/// An attribute indicating that the assembly is an Entropy mod.
/// </summary>
/// <remarks>
/// Technically, this attribute is not cessessary as a mod is identified by having a reference to Entropy assembly, but since just adding a reference does not guarantee
/// it'll be included into built assembly metadata, and a mod doesn't have to use any of Entropy framework types, this could result in compiler excluding the assembly,
/// thus the mod won't be otherwise identified as an Entropy mod.
/// Using this attribute helps to force inclusion of Entropy assembly reference and by that identify a mod as an Entropy mod.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
public class EntropyModAttribute : Attribute
{
}