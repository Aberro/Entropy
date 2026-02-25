#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
using UnityEngine;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImRect : IEquatable<ImRect>
{
	private ImVec2 _min;    // Upper-left
	private ImVec2 _max;    // Lower-right

	/// <summary>
	/// Upper-left
	/// </summary>
	public ref ImVec2 Min => ref this._min;
	/// <summary>
	/// Lower-right
	/// </summary>
	public ref ImVec2 Max => ref this._max;

	public ImRect()
	{
		this._min = default;
		this._max = default;
	}
	public ImRect(Vector2 min, Vector2 max)
	{
		this._min = min;
		this._max = max;
	}
	public ImRect(Vector4 v)
	{
		this._min = new ImVec2(v.x, v.y);
		this._max = new ImVec2(v.z, v.w);
	}
	public ImRect(float x1, float y1, float x2, float y2)
	{
		this._min = new ImVec2(x1, y1);
		this._max = new ImVec2(x2, y2);
	}
	public ImRect(Rect r)
	{
		this._min = new ImVec2(r.xMin, r.yMin);
		this._max = new ImVec2(r.xMax, r.yMax);
	}

	public static implicit operator Rect(ImRect r) => new(r._min.X, r._min.Y, r._max.X - r._min.X, r._max.Y - r._min.Y);
	public static implicit operator ImRect(Rect r) => new(r.xMin, r.yMin, r.xMax, r.yMax);
	public static bool operator ==(ImRect a, ImRect b) => a._min.X == b._min.X && a._min.Y == b._min.Y && a._max.X == b._max.X && a._max.Y == b._max.Y;
	public static bool operator !=(ImRect a, ImRect b) => a._min.X != b._min.X || a._min.Y != b._min.Y || a._max.X != b._max.X || a._max.Y != b._max.Y;

	public ImVec2 Center => new((this._min.X + this._max.X) * 0.5f, (this._min.Y + this._max.Y) * 0.5f);
	public ImVec2 Size => new(this._max.X - this._min.X, this._max.Y - this._min.Y);
	public float Width => this._max.X - this._min.X;
	public float Height => this._max.Y - this._min.Y;
	public float Area => (this._max.X - this._min.X) * (this._max.Y - this._min.Y);
	/// <summary>
	/// Top-left
	/// </summary>
	/// <returns></returns>
	public ImVec2 TopLeft => this._min;
	/// <summary>
	/// Top-right
	/// </summary>
	/// <returns></returns>
	public ImVec2 TopRight => new(this._max.X, this._min.Y);
	/// <summary>
	/// Bottom-left
	/// </summary>
	/// <returns></returns>
	public ImVec2 BottomLeft => new(this._min.X, this._max.Y);
	/// <summary>
	/// Bottom-right
	/// </summary>
	/// <returns></returns>
	public ImVec2 BottomRight => this._max;
	public bool Contains(ImVec2 p) => p.X >= this._min.X && p.Y >= this._min.Y && p.X < this._max.X && p.Y < this._max.Y;
	public bool Contains(ImRect r) => r._min.X >= this._min.X && r._min.Y >= this._min.Y && r._max.X <= this._max.X && r._max.Y <= this._max.Y;
	public bool Overlaps(ImRect r) => r._min.Y < this._max.Y && r._max.Y > this._min.Y && r._min.X < this._max.X && r._max.X > this._min.X;
	public void Add(ImVec2 p)
	{
		if(this._min.X > p.X)
			this._min.X = p.X;
		if(this._min.Y > p.Y)
			this._min.Y = p.Y;
		if(this._max.X < p.X)
			this._max.X = p.X;
		if(this._max.Y < p.Y)
			this._max.Y = p.Y;
	}
	public void Add(ImRect r)
	{
		if(this._min.X > r._min.X)
			this._min.X = r._min.X;
		if(this._min.Y > r._min.Y)
			this._min.Y = r._min.Y;
		if(this._max.X < r._max.X)
			this._max.X = r._max.X;
		if(this._max.Y < r._max.Y)
			this._max.Y = r._max.Y;
	}
	public void Expand(float amount)
	{
		this._min.X -= amount;
		this._min.Y -= amount;
		this._max.X += amount;
		this._max.Y += amount;
	}
	public void Expand(ImVec2 amount)
	{
		this._min.X -= amount.X;
		this._min.Y -= amount.Y;
		this._max.X += amount.X;
		this._max.Y += amount.Y;
	}
	public void Translate(ImVec2 d)
	{
		this._min.X += d.X;
		this._min.Y += d.Y;
		this._max.X += d.X;
		this._max.Y += d.Y;
	}
	public void TranslateX(float dx)
	{
		this._min.X += dx;
		this._max.X += dx;
	}
	public void TranslateY(float dy)
	{
		this._min.Y += dy;
		this._max.Y += dy;
	}
	/// <summary>
	/// Simple version, may lead to an inverted rectangle, which is fine for Contains/Overlaps test but not for display.
	/// </summary>
	/// <param name="r"></param>
	public void ClipWith(ImRect r)
	{
		this._min = new ImVec2(Math.Max(this._min.X, r._min.X), Math.Max(this._min.Y, r._min.Y));
		this._max = new ImVec2(Math.Min(this._max.X, r._max.X), Math.Min(this._max.Y, r._max.Y));
	}
	/// <summary>
	/// Full version, ensure both points are fully clipped.
	/// </summary>
	/// <param name="r"></param>
	public void ClipWithFull(ImRect r)
	{
		this._min = new ImVec2(Math.Clamp(this._min.X, r._min.X, r._max.X), Math.Clamp(this._min.Y, r._min.Y, r._max.Y));
		this._max = new ImVec2(Math.Clamp(this._max.X, r._min.X, r._max.X), Math.Clamp(this._max.Y, r._min.Y, r._max.Y));
	}
	public void Floor()
	{
		this._min.X = (float)Math.Floor(this._min.X);
		this._min.Y = (float)Math.Floor(this._min.Y);
		this._max.X = (float)Math.Floor(this._max.X);
		this._max.Y = (float)Math.Floor(this._max.Y);
	}
	public bool IsInverted() => this._min.X > this._max.X || this._min.Y > this._max.Y;
	public ImVec4 ToVec4() => new(this._min.X, this._min.Y, this._max.X, this._max.Y);
	public override bool Equals(object? obj) => obj is ImRect rect && this == rect;
	public bool Equals(ImRect other) => this == other;
	public override int GetHashCode() => HashCode.Combine(this._min.X, this._min.Y, this._max.X, this._max.Y);
}