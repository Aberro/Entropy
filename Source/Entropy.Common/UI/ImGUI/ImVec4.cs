#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using UnityEngine;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImVec4 : IEquatable<ImVec4>
{
	private float _x;
	private float _y;
	private float _z;
	private float _w;
	public ref float X => ref this._x;
	public ref float Y => ref this._y;
	public ref float Z => ref this._z;
	public ref float W => ref this._w;
	public static implicit operator Vector4(ImVec4 v) => new(v._x, v._y, v._z, v._w);
	public static implicit operator ImVec4(Vector4 v) => new(v.x, v.y, v.z, v.w);
	public static bool operator ==(ImVec4 lhs, ImVec4 rhs) => lhs._x == rhs._x && lhs._y == rhs._y && lhs._z == rhs._z && lhs._w == rhs._w;
	public static bool operator !=(ImVec4 lhs, ImVec4 rhs) => lhs._x != rhs._x || lhs._y != rhs._y || lhs._z != rhs._z || lhs._w != rhs._w;

	public ImVec4(float x, float y, float z, float w)
	{
		this._x = x;
		this._y = y;
		this._z = z;
		this._w = w;
	}
	public override bool Equals(object? obj) => obj is ImVec4 v && this == v;
	public bool Equals(ImVec4 other) => this == other;
	public override int GetHashCode() => HashCode.Combine(this._x, this._y, this._z, this._w);
}