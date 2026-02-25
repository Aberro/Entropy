using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Entropy.Common.Attributes;

/// <summary>
/// An attribute to define automated configuration entry that patches the property it is applied to to get and set the value of the config entry.
/// That is - this attribute should be declared on a auto-property without a backing field, which getter and setter would be patched to access config entry value.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
[MeansImplicitUse]
public sealed class AutoConfigDefinitionAttribute : ConfigDefinitionAttributeBase
{
	private object? _defaultValue;
	public bool HasDefaultValue { get; private set; }
	public object? DefaultValue
	{
		get => _defaultValue;
		set
		{
			_defaultValue = value;
			HasDefaultValue = true;
		}
	}

	/// <summary>
	/// The minimum value of the config entry. Applicable only to numeric values.
	/// </summary>
	public object? MinValue { get; set; }
	/// <summary>
	/// The maximum value of the config entry. Applicable only to numeric values.
	/// </summary>
	public object? MaxValue { get; set; }
	public AutoConfigDefinitionAttribute(string description)
		: base("$MemberName", description, null)
	{
	}
	public AutoConfigDefinitionAttribute(string description, string category)
		: base("$MemberName", description, category)
	{
	}
	public AutoConfigDefinitionAttribute(string name, string description, string category)
		: base(name, description, category)
	{
	}
	public AutoConfigDefinitionAttribute(string description, float defaultValueX, float defalutValueY)
		: this("$MemberName", description, null!)
	{
		DefaultValue = new Vector2(defaultValueX, defalutValueY);
	}
	public AutoConfigDefinitionAttribute(string description, string category, float defaultValueX, float defalutValueY)
		: this("$MemberName", description, category!)
	{
		DefaultValue = new Vector2(defaultValueX, defalutValueY);
	}
	public AutoConfigDefinitionAttribute(string name, string description, string? category, float defaultValueX, float defalutValueY)
		: this(name, description, category!)
	{
		DefaultValue = new Vector2(defaultValueX, defalutValueY);
	}
	public AutoConfigDefinitionAttribute(string description, float defaultValueX, float defaultValueY, float defaultValueZ)
		: this("$MemberName", description, null!)
	{
		DefaultValue = new Vector3(defaultValueX, defaultValueY, defaultValueZ);
	}
	public AutoConfigDefinitionAttribute(string description, string category, float defaultValueX, float defaultValueY, float defaultValueZ)
		: this("$MemberName", description, category)
	{
		DefaultValue = new Vector3(defaultValueX, defaultValueY, defaultValueZ);
	}
	public AutoConfigDefinitionAttribute(string name, string description, float defaultValueX, float defaultValueY, float defaultValueZ, string? category = null)
		: this(name, description, category!)
	{
		DefaultValue = new Vector3(defaultValueX, defaultValueY, defaultValueZ);
	}
	public AutoConfigDefinitionAttribute(string description, float defaultValueX, float defaultValueY, float defaultValueZ, float defaultValueW)
		: this("$MemberName", description, null!)
	{
		DefaultValue = new Vector4(defaultValueX, defaultValueY, defaultValueZ, defaultValueW);
	}
	public AutoConfigDefinitionAttribute(string description, string category, float defaultValueX, float defaultValueY, float defaultValueZ, float defaultValueW)
		: this("$MemberName", description, category)
	{
		DefaultValue = new Vector4(defaultValueX, defaultValueY, defaultValueZ, defaultValueW);
	}
	public AutoConfigDefinitionAttribute(string name, string description, float defaultValueX, float defaultValueY, float defaultValueZ, float defaultValueW, string? category = null)
		: this(name, description, category!)
	{
		DefaultValue = new Vector4(defaultValueX, defaultValueY, defaultValueZ, defaultValueW);
	}
}