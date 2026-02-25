#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using UnityEngine;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImVec2(float x, float y) : IEquatable<ImVec2>
{
	private float _x = x;
	private float _y = y;
	public ref float X => ref this._x;
	public ref float Y => ref this._y;

	public static implicit operator Vector2(ImVec2 v) => new(v._x, v._y);
	public static implicit operator ImVec2(Vector2 v) => new(v.x, v.y);

	public static bool operator ==(ImVec2 left, ImVec2 right) => left._x == right._x && left._y == right._y;
	public static bool operator !=(ImVec2 left, ImVec2 right) => left._x != right._x || left._y != right._y;
	public override bool Equals(object? obj) => obj is ImVec2 v && this == v;
	public bool Equals(ImVec2 other) => this == other;
	public override int GetHashCode() => HashCode.Combine(_x, _y);
}