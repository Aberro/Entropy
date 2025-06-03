#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImGuiNET;
using ImGuiID = System.UInt32;
using ImS16 = System.Int16;
using ImS32 = System.Int32;

namespace Entropy.UI.ImGUI;

public struct ImGuiTabItem
{
	public ImGuiID ID;
	public ImGuiTabItemFlags Flags;
	public int LastFrameVisible;
	/// <summary>
	/// This allows us to infer an ordered list of the last activated tabs with little maintenance
	/// </summary>
	public int LastFrameSelected;
	/// <summary>
	/// Position relative to beginning of tab
	/// </summary>
	public float Offset;
	/// <summary>
	/// Width currently displayed
	/// </summary>
	public float Width;
	/// <summary>
	/// Width of label, stored during BeginTabItem() call
	/// </summary>
	public float ContentWidth;
	/// <summary>
	/// Width optionally requested by caller, -1.0f is unused
	/// </summary>
	public float RequestedWidth;
	/// <summary>
	/// When Window==NULL, offset to name within parent ImGuiTabBar::TabsNames
	/// </summary>
	public ImS32 NameOffset;
	/// <summary>
	/// BeginTabItem() order, used to re-order tabs after toggling ImGuiTabBarFlags_Reorderable
	/// </summary>
	public ImS16 BeginOrder;
	/// <summary>
	/// Index only used during TabBarLayout()
	/// </summary>
	public ImS16 IndexDuringLayout;
	/// <summary>
	/// Marked as closed by SetTabItemClosed()
	/// </summary>
	public bool WantClose;

	public ImGuiTabItem()
	{
		this.LastFrameVisible = this.LastFrameSelected = -1;
		this.NameOffset = -1;
		this.BeginOrder = this.IndexDuringLayout = -1;
	}
}