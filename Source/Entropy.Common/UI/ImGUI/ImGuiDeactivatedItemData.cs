#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using System.Runtime.InteropServices;
using ImGuiID = uint;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiDeactivatedItemData
{
	private ImGuiID _id;
	private int _elapseFrame;
	[MarshalAs(UnmanagedType.U1)]
	private bool _hasBeenEditedBefore;
	[MarshalAs(UnmanagedType.U1)]
	private bool _isAlive;

	public ref ImGuiID ID => ref this._id;
	public ref int ElapseFrame => ref this._elapseFrame;
	public ref bool HasBeenEditedBefore => ref this._hasBeenEditedBefore;
	public ref bool IsAlive => ref this._isAlive;
}