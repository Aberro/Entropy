#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImGuiNET;
using System.Runtime.InteropServices;
using ImGuiID = System.UInt32;
using ImS16 = System.Int16;
using ImS32 = System.Int32;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiTabItem
{
	private ImGuiID _id;
	private ImGuiTabItemFlags _flags;
	private int _lastFrameVisible;
	private int _lastFrameSelected;
	private float _offset;
	private float _width;
	private float _contentWidth;
	private float _requestedWidth;
	private ImS32 _nameOffset;
	private ImS16 _beginOrder;
	private ImS16 _indexDuringLayout;
	[MarshalAs(UnmanagedType.U1)]
	private bool _wantClose;

	public ref ImGuiID ID => ref this._id;
	public ref ImGuiTabItemFlags Flags => ref this._flags;
	public ref int LastFrameVisible => ref this._lastFrameVisible;
	/// <summary>
	/// This allows us to infer an ordered list of the last activated tabs with little maintenance
	/// </summary>
	public ref int LastFrameSelected => ref this._lastFrameSelected;
	/// <summary>
	/// Position relative to beginning of tab
	/// </summary>
	public ref float Offset => ref this._offset;
	/// <summary>
	/// Width currently displayed
	/// </summary>
	public ref float Width => ref this._width;
	/// <summary>
	/// Width of label, stored during BeginTabItem() call
	/// </summary>
	public ref float ContentWidth => ref this._contentWidth;
	/// <summary>
	/// Width optionally requested by caller, -1.0f is unused
	/// </summary>
	public ref float RequestedWidth => ref this._requestedWidth;
	/// <summary>
	/// When Window==NULL, offset to name within parent ImGuiTabBar::TabsNames
	/// </summary>
	public ref ImS32 NameOffset => ref this._nameOffset;
	/// <summary>
	/// BeginTabItem() order, used to re-order tabs after toggling ImGuiTabBarFlags_Reorderable
	/// </summary>
	public ref ImS16 BeginOrder => ref this._beginOrder;
	/// <summary>
	/// Index only used during TabBarLayout()
	/// </summary>
	public ref ImS16 IndexDuringLayout => ref this._indexDuringLayout;
	/// <summary>
	/// Marked as closed by SetTabItemClosed()
	/// </summary>
	public ref bool WantClose => ref this._wantClose;

	public ImGuiTabItem()
	{
		this._lastFrameVisible = this._lastFrameSelected = -1;
		this._nameOffset = -1;
		this._beginOrder = this._indexDuringLayout = -1;
	}
}