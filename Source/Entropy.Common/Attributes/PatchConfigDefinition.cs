using Entropy.Common.Patching;
using Entropy.Common.Utils;

namespace Entropy.Common.Attributes;

/// <summary>
/// Attribute that is used to define an automated patch configuration entry.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
public sealed class PatchConfigDefinitionAttribute : ConfigDefinitionAttribute<bool>
{

	/// <summary>
	/// Creates a new instance of the <see cref="ConfigCategoryDefinitionAttribute"/> attribute.
	/// </summary>
	/// <param name="name">Patch category name, the name that is used in <see cref="ConfigCategoryAttribute"/>.</param>
	/// <param name="description">Description of the patch category used to display in hints.</param>
	public PatchConfigDefinitionAttribute(string name, string description, string category = null!)
		: base(name, description, category)
	{
		DefaultValue = true;
	}
}