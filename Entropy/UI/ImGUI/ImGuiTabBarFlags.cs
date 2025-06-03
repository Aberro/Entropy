#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
namespace Entropy.UI.ImGUI;

[Flags]
public enum ImGuiTabBarFlags
{
	None = 0,
	/// <summary>
	/// Allow manually dragging tabs to re-order them + New tabs are appended at the end of list
	/// </summary>
	Reorderable = 1 << 0,
	/// <summary>
	/// Automatically select new tabs when they appear
	/// </summary>
	AutoSelectNewTabs = 1 << 1,
	/// <summary>
	/// Disable buttons to open the tab list popup
	/// </summary>
	TabListPopupButton = 1 << 2,
	/// <summary>
	/// Disable behavior of closing tabs (that are submitted with p_open != NULL) with middle mouse button. You can still repro this behavior on user's side with if (IsItemHovered() && IsMouseClicked(2)) *p_open = false.
	/// </summary>
	NoCloseWithMiddleMouseButton = 1 << 3,
	/// <summary>
	/// Disable scrolling buttons (apply when fitting policy is ImGuiTabBarFlags_FittingPolicyScroll)
	/// </summary>
	NoTabListScrollingButtons = 1 << 4,
	/// <summary>
	/// Disable tooltips when hovering a tab
	/// </summary>
	NoTooltip = 1 << 5,
	/// <summary>
	/// Resize tabs when they don't fit
	/// </summary>
	FittingPolicyResizeDown = 1 << 6,
	/// <summary>
	/// Add scroll buttons when tabs don't fit
	/// </summary>
	FittingPolicyScroll = 1 << 7,
	/// <summary>
	/// PRIVATE! Part of a dock node [we don't use this in the master branch but it facilitate branch syncing to keep this around]
	/// </summary>
	DockNode = 1 << 20,
	/// <summary>
	/// PRIVATE!
	/// </summary>
	IsFocused = 1 << 21,
	/// <summary>
	/// PRIVATE!
	/// </summary>
	SaveSettings = 1 << 22, // FIXME: Settings are handled by the docking system, this only request the tab bar to mark settings dirty when reordering tabs
	FittingPolicyMask = FittingPolicyResizeDown | FittingPolicyScroll,
	FittingPolicyDefault = FittingPolicyResizeDown
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member