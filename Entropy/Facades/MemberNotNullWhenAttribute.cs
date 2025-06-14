﻿#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Diagnostics.CodeAnalysis;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Indicates that a member of a class or struct is not null when the method or property returns a specified value.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
public sealed class MemberNotNullWhenAttribute : Attribute
{
 	/// <summary>Initializes the attribute with the specified return value condition and a field or property member.</summary>
	/// <param name="returnValue">
	/// The return value condition. If the method returns this value, the associated field or property member will not be null.
	/// </param>
	/// <param name="member">
	/// The field or property member that is promised to be not-null.
	/// </param>
	public MemberNotNullWhenAttribute(bool returnValue, string member)
	{
		ReturnValue = returnValue;
		Members = [member];
	}

	/// <summary>Initializes the attribute with the specified return value condition and list of field and property members.</summary>
	/// <param name="returnValue">
	/// The return value condition. If the method returns this value, the associated field and property members will not be null.
	/// </param>
	/// <param name="members">
	/// The list of field and property members that are promised to be not-null.
	/// </param>
	public MemberNotNullWhenAttribute(bool returnValue, params string[] members)
	{
		ReturnValue = returnValue;
		Members = members;
	}

	/// <summary>Gets the return value condition.</summary>
	public bool ReturnValue { get; }

	/// <summary>Gets field or property member names.</summary>
	public string[] Members { get; }
}