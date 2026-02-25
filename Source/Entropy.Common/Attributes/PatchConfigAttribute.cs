using Entropy.Common.Configs;
using HarmonyLib;

namespace Entropy.Common.Attributes;

/// <summary>
/// Attribute to specify the automated patch configuration entry, used to enable/disable the patch.
/// </summary>
/// <remarks>
/// The config entry is defined by the <see cref="PatchConfigEntry"/> class and should be defined by the <see cref="PatchConfigDefinitionAttribute"/> attribute somewhere (anywhere would do).
/// If the patch has a single <see cref="PatchConfigDefinitionAttribute"/>, <see cref="PatchConfigAttribute"/> is not required, and the patch will use the config entry specified in the definition.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class PatchConfigAttribute : Attribute
{
	public string Name { get; }
	public PatchConfigAttribute(string name) : base()
	{
		Name = name;
	}
}