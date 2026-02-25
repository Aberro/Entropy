#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using System.Runtime.InteropServices;
using ImS8 = sbyte;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiSelectionRequest
{
	private ImGuiSelectionRequestType _type;
	[MarshalAs(UnmanagedType.U1)]
	private bool _selected;
	private ImS8 _rangeDirection;
	private ImGuiSelectionUserData _rangeFirstItem;
	private ImGuiSelectionUserData _rangeLastItem;

	public ref ImGuiSelectionRequestType Type => ref this._type;
	public ref bool Selected => ref this._selected;
	public ref ImS8 RangeDirection => ref this._rangeDirection;
	public ref ImGuiSelectionUserData RangeFirstItem => ref this._rangeFirstItem;
	public ref ImGuiSelectionUserData RangeLastItem => ref this._rangeLastItem;
}

public enum ImGuiSelectionRequestType
{
	None = 0,
	/// <summary>
	/// Request app to clear selection (if Selected==false) or select all items (if Selected==true). We cannot set RangeFirstItem/RangeLastItem as its contents is entirely up to user (not necessarily an index)
	/// </summary>
	SetAll,
	/// <summary>
	/// Request app to select/unselect [RangeFirstItem..RangeLastItem] items (inclusive) based on value of Selected. Only EndMultiSelect() request this, app code can read after BeginMultiSelect() and it will always be false.
	/// </summary>
	SetRange,
}