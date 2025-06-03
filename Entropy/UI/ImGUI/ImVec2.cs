#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using UnityEngine;

namespace Entropy.UI.ImGUI;

public struct ImVec2(float x, float y)
{
	public float X = x;
	public float Y = y;

	public static implicit operator Vector2(ImVec2 v) => new Vector2(v.X, v.Y);
	public static implicit operator ImVec2(Vector2 v) => new ImVec2(v.x, v.y);

	public static bool operator ==(ImVec2 left, ImVec2 right)
	{
		return left.X == right.X && left.Y == right.Y;
	}
	public static bool operator !=(ImVec2 left, ImVec2 right)
	{
		return left.X != right.X || left.Y != right.Y;
	}
	public override bool Equals(object? obj)
	{
		if(obj is ImVec2 v)
		{
			return this == v;
		}
		return false;
	}
	public override int GetHashCode()
	{
		unchecked
		{
			return (X.GetHashCode() * 397) ^ Y.GetHashCode();
		}
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member