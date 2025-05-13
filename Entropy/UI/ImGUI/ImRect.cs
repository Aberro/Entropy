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
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member