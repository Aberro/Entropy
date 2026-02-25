#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using System.Runtime.InteropServices;
using ImGuiID = uint;
using ImS16 = short;
using ImS8 = sbyte;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiTabBar
{
	private ImVector<ImGuiTabItem> _tabs;
	private ImGuiTabBarFlags _flags;
	private ImGuiID _id;
	private ImGuiID _selectedTabId;
	private ImGuiID _nextSelectedTabId;
	private ImGuiID _visibleTabId;
	private int _currFrameVisible;
	private int _prevFrameVisible;
	private ImRect _barRect;
	private float _currTabsContentsHeight;
	private float _prevTabsContentsHeight;
	private float _widthAllTabs;
	private float _widthAllTabsIdeal;
	private float _scrollingAnim;
	private float _scrollingTarget;
	private float _scrollingTargetDistToVisibility;
	private float _scrollingSpeed;
	private float _scrollingRectMinX;
	private float _scrollingRectMaxX;
	private ImGuiID _reorderRequestTabId;
	private ImS16 _reorderRequestOffset;
	private ImS8 _beginCount;
	[MarshalAs(UnmanagedType.U1)]
	private bool _wantLayout;
	[MarshalAs(UnmanagedType.U1)]
	private bool _visibleTabWasSubmitted;
	[MarshalAs(UnmanagedType.U1)]
	private bool _tabsAddedNew;
	private ImS16 _tabsActiveCount;
	private ImS16 _lastTabItemIdx;
	private float _itemSpacingY;
	private ImVec2 _framePadding;
	private ImVec2 _backupCursorPos;
	private ImGuiTextBuffer _tabsNames;

	public ref ImVector<ImGuiTabItem> Tabs => ref this._tabs;
	public ref ImGuiTabBarFlags Flags => ref this._flags;
	/// <summary>
	/// Zero for tab-bars used by docking
	/// </summary>
	public ref ImGuiID ID => ref this._id;
	/// <summary>
	/// Selected tab/window
	/// </summary>
	public ref ImGuiID SelectedTabId => ref this._selectedTabId;
	/// <summary>
	/// Next selected tab/window. Will also trigger a scrolling animation
	/// </summary>
	public ref ImGuiID NextSelectedTabId => ref this._nextSelectedTabId;
	/// <summary>
	/// Can occasionally be != SelectedTabId (e.g. when previewing contents for CTRL+TAB preview)
	/// </summary>
	public ref ImGuiID VisibleTabId => ref this._visibleTabId;
	public ref int CurrFrameVisible => ref this._currFrameVisible;
	public ref int PrevFrameVisible => ref this._prevFrameVisible;
	public ref ImRect BarRect => ref this._barRect;
	public ref float CurrTabsContentsHeight => ref this._currTabsContentsHeight;
	/// <summary>
	/// Record the height of contents submitted below the tab bar
	/// </summary>
	public ref float PrevTabsContentsHeight => ref this._prevTabsContentsHeight;
	/// <summary>
	/// Actual width of all tabs (locked during layout)
	/// </summary>
	public ref float WidthAllTabs => ref this._widthAllTabs;
	/// <summary>
	/// Ideal width if all tabs were visible and not clipped
	/// </summary>
	public ref float WidthAllTabsIdeal => ref this._widthAllTabsIdeal;
	public ref float ScrollingAnim => ref this._scrollingAnim;
	public ref float ScrollingTarget => ref this._scrollingTarget;
	public ref float ScrollingTargetDistToVisibility => ref this._scrollingTargetDistToVisibility;
	public ref float ScrollingSpeed => ref this._scrollingSpeed;
	public ref float ScrollingRectMinX => ref this._scrollingRectMinX;
	public ref float ScrollingRectMaxX => ref this._scrollingRectMaxX;
	public ref ImGuiID ReorderRequestTabId => ref this._reorderRequestTabId;
	public ref ImS16 ReorderRequestOffset => ref this._reorderRequestOffset;
	public ref ImS8 BeginCount => ref this._beginCount;
	public ref bool WantLayout => ref this._wantLayout;
	public ref bool VisibleTabWasSubmitted => ref this._visibleTabWasSubmitted;
	/// <summary>
	/// Set to true when a new tab item or button has been added to the tab bar during last frame
	/// </summary>
	public ref bool TabsAddedNew => ref this._tabsAddedNew;
	/// <summary>
	/// Number of tabs submitted this frame.
	/// </summary>
	public ref ImS16 TabsActiveCount => ref this._tabsActiveCount;
	/// <summary>
	/// Index of last BeginTabItem() tab for use by EndTabItem()
	/// </summary>
	public ref ImS16 LastTabItemIdx => ref this._lastTabItemIdx;
	public ref float ItemSpacingY => ref this._itemSpacingY;
	/// <summary>
	/// style.FramePadding locked at the time of BeginTabBar()
	/// </summary>
	public ref ImVec2 FramePadding => ref this._framePadding;
	public ref ImVec2 BackupCursorPos => ref this._backupCursorPos;
	/// <summary>
	/// For non-docking tab bar we re-append names in a contiguous buffer.
	/// </summary>
	public ref ImGuiTextBuffer TabsNames => ref this._tabsNames;

	public ImGuiTabBar()
	{
		this._currFrameVisible = this._prevFrameVisible = -1;
		this._lastTabItemIdx = -1;
	}
	public readonly unsafe int GetTabOrder(ref ImGuiTabItem tab) { return this._tabs.IndexOf(ref tab); }
	public unsafe readonly string GetTabName(ref ImGuiTabItem tab)
	{
		if(tab.NameOffset == -1 || tab.NameOffset >= this._tabsNames.Buf.Size)
			throw new ArgumentException("Tab is invalid.");
		return ImGuiHelper.GetString(ImGuiHelper.GetPointer(ref this._tabsNames.Buf.Get(tab.NameOffset)));
	}
}