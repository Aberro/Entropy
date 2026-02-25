#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImGuiNET;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Entropy.Common.UI.ImGUI;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct ImGuiStyleMod
{
	[FieldOffset(0)]
	ImGuiStyleVar _varIdx;
	[FieldOffset(4)]
	fixed int _backupInt[2];
	[FieldOffset(4)]
	fixed float _backupFloat[2];

	public ref ImGuiStyleVar VarIdx => ref this._varIdx;
	public Span<int> BackupInt
	{
		get
		{
			fixed (int* ptr = this._backupInt)
			{
				return new Span<int>(ptr, 2);
			}
		}
	}
	public Span<float> BackupFloat
	{
		get
		{
			fixed (float* ptr = this._backupFloat)
			{
				return new Span<float>(ptr, 2);
			}
		}
	}

	public ImGuiStyleMod(ImGuiStyleVar idx, int v)
	{
		this._varIdx = idx;
		this._backupInt[0] = v;
	}

	public ImGuiStyleMod(ImGuiStyleVar idx, float v)
	{
		this._varIdx = idx;
		this._backupFloat[0] = v;
	}

	public ImGuiStyleMod(ImGuiStyleVar idx, ref ImVec2 v)
	{
		this._varIdx = idx;
		this._backupFloat[0] = v.X;
		this._backupFloat[1] = v.Y;
	}
}