#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using UnityEngine;

namespace Entropy.UI.ImGUI;

public struct ImVec4
{
	public float x;
	public float y;
	public float z;
	public float w;
	public ImVec4(float x, float y, float z, float w)
	{
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}
	public static implicit operator Vector4(ImVec4 v) => new Vector4(v.x, v.y, v.z, v.w);
	public static implicit operator ImVec4(Vector4 v) => new ImVec4(v.x, v.y, v.z, v.w);
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member