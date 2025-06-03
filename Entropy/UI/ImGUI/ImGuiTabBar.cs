#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Runtime.InteropServices;
using ImGuiID = uint;
using ImS16 = short;
using ImS8 = sbyte;

namespace Entropy.UI.ImGUI;

public struct ImGuiTabBar
{
	public ImVector<ImGuiTabItem> Tabs;
	public ImGuiTabBarFlags Flags;
	/// <summary>
	/// Zero for tab-bars used by docking
	/// </summary>
	public ImGuiID ID;
	/// <summary>
	/// Selected tab/window
	/// </summary>
	public ImGuiID SelectedTabId;
	/// <summary>
	/// Next selected tab/window. Will also trigger a scrolling animation
	/// </summary>
	public ImGuiID NextSelectedTabId;
	/// <summary>
	/// Can occasionally be != SelectedTabId (e.g. when previewing contents for CTRL+TAB preview)
	/// </summary>
	public ImGuiID VisibleTabId;
	public int CurrFrameVisible;
	public int PrevFrameVisible;
	public ImRect BarRect;
	public float CurrTabsContentsHeight;
	/// <summary>
	/// Record the height of contents submitted below the tab bar
	/// </summary>
	public float PrevTabsContentsHeight;
	/// <summary>
	/// Actual width of all tabs (locked during layout)
	/// </summary>
	public float WidthAllTabs;
	/// <summary>
	/// Ideal width if all tabs were visible and not clipped
	/// </summary>
	public float WidthAllTabsIdeal;
	public float ScrollingAnim;
	public float ScrollingTarget;
	public float ScrollingTargetDistToVisibility;
	public float ScrollingSpeed;
	public float ScrollingRectMinX;
	public float ScrollingRectMaxX;
	public ImGuiID ReorderRequestTabId;
	public ImS16 ReorderRequestOffset;
	public ImS8 BeginCount;
	public bool WantLayout;
	public bool VisibleTabWasSubmitted;
	/// <summary>
	/// Set to true when a new tab item or button has been added to the tab bar during last frame
	/// </summary>
	public bool TabsAddedNew;
	/// <summary>
	/// Number of tabs submitted this frame.
	/// </summary>
	public ImS16 TabsActiveCount;
	/// <summary>
	/// Index of last BeginTabItem() tab for use by EndTabItem()
	/// </summary>
	public ImS16 LastTabItemIdx;
	public float ItemSpacingY;
	/// <summary>
	/// style.FramePadding locked at the time of BeginTabBar()
	/// </summary>
	public ImVec2 FramePadding;
	public ImVec2 BackupCursorPos;
	/// <summary>
	/// For non-docking tab bar we re-append names in a contiguous buffer.
	/// </summary>
	public ImGuiTextBuffer TabsNames;

	public ImGuiTabBar()
	{
		this.CurrFrameVisible = this.PrevFrameVisible = -1;
		this.LastTabItemIdx = -1;
	}
	public readonly unsafe int GetTabOrder(ImGuiTabItemPtr tab) { return this.Tabs.IndexFromPtr(tab); }
	public unsafe readonly string GetTabName(ImGuiTabItemPtr tab)
	{
		if(tab.NameOffset == -1 || tab.NameOffset >= this.TabsNames.Buf.Size)
			throw new ArgumentException("Tab is invalid.");
		return Marshal.PtrToStringUTF8((IntPtr)this.TabsNames.Buf.GetPtr(tab.NameOffset));
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member