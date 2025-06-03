#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
namespace Entropy.UI.ImGUI;

public enum ImGuiWindowFlags
{ 
	None = 0,
	/// <summary>
	/// Disable title-bar
	/// </summary>
	NoTitleBar = 1 << 0,
	/// <summary>
	/// Disable user resizing with the lower-right grip
	/// </summary>
	NoResize = 1 << 1,
	/// <summary>
	/// Disable user moving the window
	/// </summary>
	NoMove = 1 << 2,
	/// <summary>
	/// Disable scrollbars (window can still scroll with mouse or programmatically)
	/// </summary>
	NoScrollbar = 1 << 3,
	/// <summary>
	/// Disable user vertically scrolling with mouse wheel. On child window, mouse wheel will be forwarded to the parent unless NoScrollbar is also set.
	/// </summary>
	NoScrollWithMouse = 1 << 4,
	/// <summary>
	/// Disable user collapsing window by double-clicking on it. Also referred to as Window Menu Button (e.g. within a docking node).
	/// </summary>
	NoCollapse = 1 << 5,
	/// <summary>
	/// Resize every window to its content every frame
	/// </summary>
	AlwaysAutoResize = 1 << 6,
	/// <summary>
	/// Disable drawing background color (WindowBg, etc.) and outside border. Similar as using SetNextWindowBgAlpha(0.0f).
	/// </summary>
	NoBackground = 1 << 7,
	/// <summary>
	/// Never load/save settings in .ini file
	/// </summary>
	NoSavedSettings = 1 << 8,
	/// <summary>
	/// Disable catching mouse, hovering test with pass through.
	/// </summary>
	NoMouseInputs = 1 << 9,
	/// <summary>
	/// Has a menu-bar
	/// </summary>
	MenuBar = 1 << 10,
	/// <summary>
	/// Allow horizontal scrollbar to appear (off by default). You may use SetNextWindowContentSize(ImVec2(width,0.0f)); prior to calling Begin() to specify width. Read code in imgui_demo in the "Horizontal Scrolling" section.
	/// </summary>
	HorizontalScrollbar = 1 << 11,
	/// <summary>
	/// Disable taking focus when transitioning from hidden to visible state
	/// </summary>
	NoFocusOnAppearing = 1 << 12,
	/// <summary>
	/// Disable bringing window to front when taking focus (e.g. clicking on it or programmatically giving it focus)
	/// </summary>
	NoBringToFrontOnFocus = 1 << 13,

	/// <summary>
	/// Always show vertical scrollbar (even if ContentSize.y < Size.y)
	/// </summary>
	AlwaysVerticalScrollbar = 1 << 14,
	/// <summary>
	/// Always show horizontal scrollbar (even if ContentSize.x < Size.x)
	/// </summary>
	AlwaysHorizontalScrollbar = 1<< 15,
	/// <summary>
	/// Ensure child windows without border uses style.WindowPadding (ignored by default for non-bordered child windows, because more convenient)
	/// </summary>
	AlwaysUseWindowPadding = 1 << 16,
	/// <summary>
	/// No gamepad/keyboard navigation within the window
	/// </summary>
	NoNavInputs = 1 << 18,
	/// <summary>
	/// No focusing toward this window with gamepad/keyboard navigation (e.g. skipped by CTRL+TAB)
	/// </summary>
	NoNavFocus = 1 << 19,
	/// <summary>
	/// Display a dot next to the title. When used in a tab/docking context, tab is selected when clicking the X + closure is not assumed (will wait for user to stop submitting the tab). Otherwise closure is assumed when pressing the X, so if you keep submitting the tab may reappear at end of tab bar.
	/// </summary>
	UnsavedDocument = 1 << 20,
	NoNav = NoNavInputs | NoNavFocus,
	NoDecoration = NoTitleBar | NoResize | NoScrollbar | NoCollapse,
	NoInputs = NoMouseInputs | NoNavInputs | NoNavFocus,

	// [Internal]
	/// <summary>
	/// [BETA] On child window: allow gamepad/keyboard navigation to cross over parent border to this child or between sibling child windows.
	/// </summary>
	[Obsolete]
	NavFlattened = 1 << 23,
	/// <summary>
	/// Don't use! For internal use by BeginChild()
	/// </summary>
	[Obsolete]
	ChildWindow = 1 << 24,
	/// <summary>
	/// Don't use! For internal use by BeginTooltip()
	/// </summary>
	[Obsolete]
	Tooltip = 1 << 25,
	/// <summary>
	/// Don't use! For internal use by BeginPopup()
	/// </summary>
	[Obsolete]
	Popup = 1 << 26,
	/// <summary>
	/// Don't use! For internal use by BeginPopupModal()
	/// </summary>
	[Obsolete]
	Modal = 1 << 27,
	/// <summary>
	/// Don't use! For internal use by BeginMenu()
	/// </summary>
	[Obsolete]
	ChildMenu = 1 << 28,
	/// <summary>
	/// [Obsolete] Set io.ConfigWindowsResizeFromEdges=true and make sure mouse cursors are supported by backend (io.BackendFlags & ImGuiBackendFlags_HasMouseCursors)
	/// </summary>
	[Obsolete]
	ResizeFromAnySide = 1 << 17,
}