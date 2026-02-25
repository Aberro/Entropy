#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImGuiNET;
using System.Runtime.InteropServices;
using ImGuiID = uint;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiWindowSettings
{
	private ImGuiID _id;
	private Vector2ih _pos;
	private Vector2ih _size;
	[MarshalAs(UnmanagedType.U1)]
	private bool _collapsed;
	[MarshalAs(UnmanagedType.U1)]
	private bool _isChild;
	[MarshalAs(UnmanagedType.U1)]
	private bool _wantApply;
	[MarshalAs(UnmanagedType.U1)]
	private bool _wantDelete;

	public ref ImGuiID ID => ref this._id;
	public ref Vector2ih Pos => ref this._pos;
	public ref Vector2ih Size => ref this._size;
	public ref bool Collapsed => ref this._collapsed;
	public ref bool IsChild => ref this._isChild;
	/// <summary>
	/// Set when loaded from .ini data (to enable merging/loading .ini data into an already running context)
	/// </summary>
	public ref bool WantApply => ref this._wantApply;
	/// <summary>
	/// Set to invalidate/delete the settings entry
	/// </summary>
	public ref bool WantDelete => ref this._wantDelete;
}