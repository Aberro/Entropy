#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImGuiNET;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Entropy.UI.ImGUI;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct ImGuiStyleMod
{
	[FieldOffset(0)]
	public ImGuiStyleVar VarIdx;

	[FieldOffset(4)]
	public fixed int BackupInt[2];

	[FieldOffset(4)]
	public fixed float BackupFloat[2];

	public ImGuiStyleMod(ImGuiStyleVar idx, int v)
	{
		this.VarIdx = idx;
		this.BackupInt[0] = v;
	}

	public ImGuiStyleMod(ImGuiStyleVar idx, float v)
	{
		this.VarIdx = idx;
		this.BackupFloat[0] = v;
	}

	public ImGuiStyleMod(ImGuiStyleVar idx, ImVec2 v)
	{
		this.VarIdx = idx;
		this.BackupFloat[0] = v.X;
		this.BackupFloat[1] = v.Y;
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member