#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using System.Runtime.InteropServices;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiMultiSelectIO
{
	private ImVector<ImGuiSelectionRequest> _requests;
	private ImGuiSelectionUserData _rangeSrcItem;
	private ImGuiSelectionUserData _mavIdItem;
	[MarshalAs(UnmanagedType.U1)]
	private bool _navIdSelected;
	[MarshalAs(UnmanagedType.U1)]
	private bool _rangeSrcReset;
	private int _itemsCount;

	public ref ImVector<ImGuiSelectionRequest> Requests => ref this._requests;
	public ref ImGuiSelectionUserData RangeSrcItem => ref this._rangeSrcItem;
	public ref ImGuiSelectionUserData NavIdItem => ref this._mavIdItem;
	public ref bool NavIdSelected => ref this._navIdSelected;
	public ref bool RangeSrcReset => ref this._rangeSrcReset;
	public ref int ItemsCount => ref this._itemsCount;
}