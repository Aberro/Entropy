#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using UnityEngine;

namespace Entropy.UI.ImGUI;

public struct ImRect
{
	public ImVec2 Min;    // Upper-left
	public ImVec2 Max;    // Lower-right

	public ImRect()
	{
		this.Min = default;
		this.Max = default;
	}
	public ImRect(Vector2 min, Vector2 max)
	{
		this.Min = min;
		this.Max = max;
	}
	public ImRect(Vector4 v)
	{
		this.Min = new ImVec2(v.x, v.y);
		this.Max = new ImVec2(v.z, v.w);
	}
	public ImRect(float x1, float y1, float x2, float y2)
	{
		this.Min = new ImVec2(x1, y1);
		this.Max = new ImVec2(x2, y2);
	}
	public ImRect(Rect r)
	{
		this.Min = new ImVec2(r.xMin, r.yMin);
		this.Max = new ImVec2(r.xMax, r.yMax);
	}

	public static implicit operator Rect(ImRect r) => new(r.Min.X, r.Min.Y, r.Max.X - r.Min.X, r.Max.Y - r.Min.Y);
	public static implicit operator ImRect(Rect r) => new(r.xMin, r.yMin, r.xMax, r.yMax);


	public ImVec2 GetCenter() => new ImVec2((this.Min.X + this.Max.X) * 0.5f, (this.Min.Y + this.Max.Y) * 0.5f);
	public ImVec2 GetSize() => new ImVec2(this.Max.X - this.Min.X, this.Max.Y - this.Min.Y);
	public float GetWidth() => this.Max.X - this.Min.X;
	public float GetHeight() => this.Max.Y - this.Min.Y;
	public float GetArea() => (this.Max.X - this.Min.X) * (this.Max.Y - this.Min.Y);
	/// <summary>
	/// Top-left
	/// </summary>
	/// <returns></returns>
	public ImVec2 GetTL() => this.Min;
	/// <summary>
	/// Top-right
	/// </summary>
	/// <returns></returns>
	public ImVec2 GetTR() => new ImVec2(this.Max.X, this.Min.Y);
	/// <summary>
	/// Bottom-left
	/// </summary>
	/// <returns></returns>
	public ImVec2 GetBL() => new ImVec2(this.Min.X, this.Max.Y);
	/// <summary>
	/// Bottom-right
	/// </summary>
	/// <returns></returns>
	public ImVec2 GetBR() => this.Max;
	public bool Contains(ImVec2 p) => p.X >= this.Min.X && p.Y >= this.Min.Y && p.X < this.Max.X && p.Y < this.Max.Y;
	public bool Contains(ImRect r) => r.Min.X >= this.Min.X && r.Min.Y >= this.Min.Y && r.Max.X <= this.Max.X && r.Max.Y <= this.Max.Y;
	public bool Overlaps(ImRect r) => r.Min.Y < this.Max.Y && r.Max.Y > this.Min.Y && r.Min.X < this.Max.X && r.Max.X > this.Min.X;
	public void Add(ImVec2 p)
	{
		if(this.Min.X > p.X)
			this.Min.X = p.X;
		if(this.Min.Y > p.Y)
			this.Min.Y = p.Y;
		if(this.Max.X < p.X)
			this.Max.X = p.X;
		if(this.Max.Y < p.Y)
			this.Max.Y = p.Y;
	}
	public void Add(ImRect r)
	{
		if(this.Min.X > r.Min.X)
			this.Min.X = r.Min.X;
		if(this.Min.Y > r.Min.Y)
			this.Min.Y = r.Min.Y;
		if(this.Max.X < r.Max.X)
			this.Max.X = r.Max.X;
		if(this.Max.Y < r.Max.Y)
			this.Max.Y = r.Max.Y;
	}
	public void Expand(float amount)
	{
		this.Min.X -= amount;
		this.Min.Y -= amount;
		this.Max.X += amount;
		this.Max.Y += amount;
	}
	public void Expand(ImVec2 amount)
	{
		this.Min.X -= amount.X;
		this.Min.Y -= amount.Y;
		this.Max.X += amount.X;
		this.Max.Y += amount.Y;
	}
	public void Translate(ImVec2 d)
	{
		this.Min.X += d.X;
		this.Min.Y += d.Y;
		this.Max.X += d.X;
		this.Max.Y += d.Y;
	}
	public void TranslateX(float dx)
	{
		this.Min.X += dx;
		this.Max.X += dx;
	}
	public void TranslateY(float dy)
	{
		this.Min.Y += dy;
		this.Max.Y += dy;
	}
	/// <summary>
	/// Simple version, may lead to an inverted rectangle, which is fine for Contains/Overlaps test but not for display.
	/// </summary>
	/// <param name="r"></param>
	public void ClipWith(ImRect r)
	{
		this.Min = new ImVec2(Math.Max(this.Min.X, r.Min.X), Math.Max(this.Min.Y, r.Min.Y));
		this.Max = new ImVec2(Math.Min(this.Max.X, r.Max.X), Math.Min(this.Max.Y, r.Max.Y));
	}
	/// <summary>
	/// Full version, ensure both points are fully clipped.
	/// </summary>
	/// <param name="r"></param>
	public void ClipWithFull(ImRect r)
	{
		this.Min = new ImVec2(Math.Clamp(this.Min.X, r.Min.X, r.Max.X), Math.Clamp(this.Min.Y, r.Min.Y, r.Max.Y));
		this.Max = new ImVec2(Math.Clamp(this.Max.X, r.Min.X, r.Max.X), Math.Clamp(this.Max.Y, r.Min.Y, r.Max.Y));
	}
	public void Floor()
	{
		this.Min.X = (float)Math.Floor(this.Min.X);
		this.Min.Y = (float)Math.Floor(this.Min.Y);
		this.Max.X = (float)Math.Floor(this.Max.X);
		this.Max.Y = (float)Math.Floor(this.Max.Y);
	}
	public bool IsInverted() => this.Min.X > this.Max.X || this.Min.Y > this.Max.Y;
	public ImVec4 ToVec4() => new ImVec4(this.Min.X, this.Min.Y, this.Max.X, this.Max.Y);
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member