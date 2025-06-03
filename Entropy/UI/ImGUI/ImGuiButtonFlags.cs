#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML

namespace Entropy.UI.ImGUI;

public enum ImGuiButtonFlags
{
	None = 0,
	/// <summary>
	/// React on left mouse button (default)
	/// </summary>
	MouseButtonLeft = 1 << 0,
	/// <summary>
	/// React on right mouse button
	/// </summary>
	MouseButtonRight = 1 << 1,
	/// <summary>
	/// React on center mouse button
	/// </summary>
	MouseButtonMiddle = 1 << 2,

	// [Internal]
	MouseButtonMask = MouseButtonLeft | MouseButtonRight | MouseButtonMiddle,
	MouseButtonDefault = MouseButtonLeft,
	/// <summary>
	/// return true on click (mouse down event)
	/// </summary>
	PressedOnClick = 1 << 4,
	/// <summary>
	/// [Default] return true on click + release on same item <-- this is what the majority of Button are using
	/// </summary>
	PressedOnClickRelease = 1 << 5,
	/// <summary>
	/// return true on click + release even if the release event is not done while hovering the item
	/// </summary>
	PressedOnClickReleaseAnywhere = 1 << 6,
	/// <summary>
	/// return true on release (default requires click+release)
	/// </summary>
	PressedOnRelease = 1 << 7,
	/// <summary>
	/// return true on double-click (default requires click+release)
	/// </summary>
	PressedOnDoubleClick = 1 << 8,
	/// <summary>
	/// return true when held into while we are drag and dropping another item (used by e.g. tree nodes, collapsing headers)
	/// </summary>
	PressedOnDragDropHold = 1 << 9,
	/// <summary>
	/// hold to repeat
	/// </summary>
	Repeat = 1 << 10,
	/// <summary>
	/// allow interactions even if a child window is overlapping
	/// </summary>
	FlattenChildren = 1 << 11,
	/// <summary>
	/// require previous frame HoveredId to either match id or be null before being usable, use along with SetItemAllowOverlap()
	/// </summary>
	AllowItemOverlap = 1 << 12,
	/// <summary>
	/// disable automatically closing parent popup on press // [UNUSED]
	/// </summary>
	DontClosePopups = 1 << 13,
	///// <summary>
	///// disable interactions -> use BeginDisabled() or ImGuiItemFlags_Disabled
	///// </summary>
	//Disabled = 1 << 14,
	/// <summary>
	/// vertically align button to match text baseline - ButtonEx() only // FIXME: Should be removed and handled by SmallButton(), not possible currently because of DC.CursorPosPrevLine
	/// </summary>
	AlignTextBaseLine = 1 << 15,
	/// <summary>
	/// disable mouse interaction if a key modifier is held
	/// </summary>
	NoKeyModifiers = 1 << 16,
	/// <summary>
	/// don't set ActiveId while holding the mouse (PressedOnClick only)
	/// </summary>
	NoHoldingActiveId = 1 << 17,
	/// <summary>
	/// don't override navigation focus when activated
	/// </summary>
	NoNavFocus = 1 << 18,
	/// <summary>
	/// don't report as hovered when nav focus is on this item
	/// </summary>
	NoHoveredOnFocus = 1 << 19,
	PressedOnMask = PressedOnClick | PressedOnClickRelease | PressedOnClickReleaseAnywhere | PressedOnRelease | PressedOnDoubleClick | PressedOnDragDropHold,
	PressedOnDefault = PressedOnClickRelease
}