using Entropy.Common.Patching;
using Entropy.Common.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Entropy.Common.Attributes;

/// <summary>
/// An attribute to define configuration entry.
/// </summary>
/// <typeparam name="T"></typeparam>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
[SuppressMessage("Performance", "CA1813:Avoid unsealed attributes")]
public class ConfigDefinitionAttribute<T> : ConfigDefinitionAttributeBase
{
	public T? DefaultValue { get; set; }
	public T? MinValue { get; set; }
	public T? MaxValue { get; set; }
	/// <summary>
	/// Creates a new instance of the <see cref="ConfigCategoryDefinitionAttribute"/> attribute.
	/// </summary>
	/// <param name="name">Patch category name, the name that is used in <see cref="ConfigCategoryAttribute"/>.</param>
	/// <param name="description">Description of the patch category used to display in hints.</param>
	/// <param name="category">Category name where this configuration belongs to. If not specified, it will be added to the default category.</param>
	/// <param name="defaultValue">The default value of this config entry. This is used to initialize the config entry when it is first created.</param>
	public ConfigDefinitionAttribute(string name, string description, string category = null!, T? defaultValue = default)
		: base(name, description, category)
	{
		DefaultValue = defaultValue;
	}
}