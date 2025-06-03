#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
namespace Entropy.UI.ImGUI;

public enum ImGuiTabItemFlags
{
	None = 0,
	/// <summary>
	/// Display a dot next to the title + tab is selected when clicking the X + closure is not assumed (will wait for user to stop submitting the tab). Otherwise closure is assumed when pressing the X, so if you keep submitting the tab may reappear at end of tab bar.
	/// </summary>
	UnsavedDocument = 1 << 0,
	/// <summary>
	/// Trigger flag to programmatically make the tab selected when calling BeginTabItem()
	/// </summary>
	SetSelected = 1 << 1,
	/// <summary>
	/// Disable behavior of closing tabs (that are submitted with p_open != NULL) with middle mouse button. You can still repro this behavior on user's side with if (IsItemHovered() && IsMouseClicked(2)) *p_open = false.
	/// </summary>
	NoCloseWithMiddleMouseButton = 1 << 2,
	/// <summary>
	/// Don't call PushID(tab->ID)/PopID() on BeginTabItem()/EndTabItem()
	/// </summary>
	NoPushId = 1 << 3,
	/// <summary>
	/// Disable tooltip for the given tab
	/// </summary>
	NoTooltip = 1 << 4,
	/// <summary>
	/// Disable reordering this tab or having another tab cross over this tab
	/// </summary>
	NoReorder = 1 << 5,
	/// <summary>
	/// Enforce the tab position to the left of the tab bar (after the tab list popup button)
	/// </summary>
	Leading = 1 << 6,
	/// <summary>
	/// Enforce the tab position to the right of the tab bar (before the scrolling buttons)
	/// </summary>
	Trailing = 1 << 7,
	SectionMask = Leading | Trailing,
	/// <summary>
	/// Track whether p_open was set or not (we'll need this info on the next frame to recompute ContentWidth during layout)
	/// </summary>
	NoCloseButton = 1 << 20,
	/// <summary>
	/// Used by TabItemButton, change the tab item behavior to mimic a button
	/// </summary>
	Button = 1 << 21,
}