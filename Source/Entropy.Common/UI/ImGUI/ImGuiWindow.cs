#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CS8656 // Call to non-readonly member from a 'readonly' member results in an implicit copy.
#pragma warning disable CA1815 // Override equals and operator equals on value types

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using ImGuiID = System.UInt32;
using ImS8 = System.SByte;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiWindow
{
	private byte* _name;
	private ImGuiID _id;
	private ImGuiWindowFlags _flags;
	private ImGuiViewportP* _viewport;
	private ImVec2 _pos;
	private ImVec2 _size;
	private ImVec2 _sizeFull;
	private ImVec2 _contentSize;
	private ImVec2 _contentSizeIdeal;
	private ImVec2 _contentSizeExplicit;
	private ImVec2 _windowPadding;
	private float _windowRounding;
	private float _windowBorderSize;
	private int _nameBufLen;
	private ImGuiID _moveId;
	private ImGuiID _childId;
	private ImVec2 _scroll;
	private ImVec2 _scrollMax;
	private ImVec2 _scrollTarget;
	private ImVec2 _scrollTargetCenterRatio;
	private ImVec2 _scrollTargetEdgeSnapDist;
	private ImVec2 _scrollbarSizes;
	[MarshalAs(UnmanagedType.U1)]
	private bool _scrollbarX;
	[MarshalAs(UnmanagedType.U1)]
	private bool _scrollbarY;
	[MarshalAs(UnmanagedType.U1)]
	private bool _active;
	[MarshalAs(UnmanagedType.U1)]
	private bool _wasActive;
	[MarshalAs(UnmanagedType.U1)]
	private bool _writeAccessed;
	[MarshalAs(UnmanagedType.U1)]
	private bool _collapsed;
	[MarshalAs(UnmanagedType.U1)]
	private bool _wantCollapseToggle;
	[MarshalAs(UnmanagedType.U1)]
	private bool _skipItems;
	[MarshalAs(UnmanagedType.U1)]
	private bool _appearing;
	[MarshalAs(UnmanagedType.U1)]
	private bool _hidden;
	[MarshalAs(UnmanagedType.U1)]
	private bool _isFallbackWindow;
	[MarshalAs(UnmanagedType.U1)]
	private bool _isExplicitChild;
	[MarshalAs(UnmanagedType.U1)]
	private bool _hasCloseButton;
	private sbyte _resizeBorderHeld;
	private short _beginCount;
	private short _beginOrderWithinParent;
	private short _beginOrderWithinContext;
	private short _focusOrder;
	private ImGuiID _popupId;
	private ImS8 _autoFitFramesX;
	private ImS8 _autoFitFramesY;
	private ImS8 _autoFitChildAxises;
	[MarshalAs(UnmanagedType.U1)]
	private bool _autoFitOnlyGrows;
	ImGuiNET.ImGuiDir _autoPosLastDirection;
	private ImS8 _hiddenFramesCanSkipItems;
	private ImS8 _hiddenFramesCannotSkipItems;
	private ImS8 _hiddenFramesForRenderOnly;
	private ImS8 _disableInputsFrames;
	[MarshalAs(UnmanagedType.U1)]
	private ImGuiNET.ImGuiCond _setWindowAllowFlags;
	private ImVec2 _setWindowPosVal;
	private ImVec2 _setWindowPosPivot;
	private ImVector<ImGuiID> _idStack;
	ImGuiNET.ImGuiWindowTempData _dc;
	private ImRect _outerRectClipped;
	private ImRect _innerRect;
	private ImRect _innerClipRect;
	private ImRect _workRect;
	private ImRect _parentWorkRect;
	private ImRect _clipRect;
	private ImRect _contentRegionRect;
	private ImGuiNET.Vector2ih _hitTestHoleSize;
	private ImGuiNET.Vector2ih _hitTestHoleOffset;
	private int _lastFrameActive;
	private float _lastTimeActive;
	private float _itemWidthDefault;
	private ImGuiStorage _stateStorage;
	private ImVector<ImGuiNET.ImGuiOldColumns> _columnsStorage;
	private float _fontWindowScale;
	private int _settingsOffset;
	private ImDrawList* _drawList;
	private ImDrawList _drawListInst;
	private ImGuiWindow* _parentWindow;
	private ImGuiWindow* _parentWindowInBeginStack;
	private ImGuiWindow* _rootWindow;
	private ImGuiWindow* _rootWindowPopupTree;
	private ImGuiWindow* _rootWindowForTitleBarHighlight;
	private ImGuiWindow* _rootWindowForNav;
	private ImGuiWindow* _navLastChildNavWindow;
	private fixed ImGuiID _navLastIds[(int)ImGuiNET.ImGuiNavLayer.COUNT];
	private ImRect _navRectRelMain;
	private ImRect _navRectRelMenu;
	private int _memoryDrawListIdxCapacity;
	private int _memoryDrawListVtxCapacity;
	[MarshalAs(UnmanagedType.U1)]
	private bool _memoryCompacted;

	//ImGuiNET.ImGuiWindow
	/// <summary>
	/// Window name, owned by the window.
	/// </summary>
	public string? Name
	{
		get => this._name != null ? ImGuiHelper.GetString(this._name) : null;
		set => this._name = value != null ? ImGuiHelper.GetStringPointer(value) : null;
	}
	/// <summary>
	/// == ImHashStr(Name)
	/// </summary>
	[SuppressMessage("Naming", "CA1721:Property names should not match get methods")]
	public ref ImGuiID ID => ref this._id;
	/// <summary>
	/// See enum ImGuiWindowFlags_
	/// </summary>
	public ref ImGuiWindowFlags Flags => ref this._flags;
	/// <summary>
	/// Always set in Begin(). Inactive windows may have a NULL value here if their viewport was discarded.
	/// </summary>
	public ref ImGuiViewportP Viewport => ref *this._viewport;
	public bool HasViewport => this._viewport != null;
	public void SetViewport(ref ImGuiViewportP viewport) => this._viewport = ImGuiHelper.GetPointer(ref viewport);
	public void ClearViewport() => this._viewport = null;
	/// <summary>
	/// Position (always rounded-up to nearest pixel)
	/// </summary>
	public ref ImVec2 Pos => ref this._pos;
	/// <summary>
	/// Current size (==SizeFull or collapsed title bar size)
	/// </summary>
	public ref ImVec2 Size => ref this._size;
	/// <summary>
	/// Size when non collapsed
	/// </summary>
	public ref ImVec2 SizeFull => ref this._sizeFull;
	/// <summary>
	/// Size of contents/scrollable client area (calculated from the extents reach of the cursor) from previous frame. Does not include window decoration or window padding.
	/// </summary>
	public ref ImVec2 ContentSize => ref this._contentSize;
	public ref ImVec2 ContentSizeIdeal => ref this._contentSizeIdeal;
	/// <summary>
	/// Size of contents/scrollable client area explicitly request by the user via SetNextWindowContentSize().
	/// </summary>
	public ref ImVec2 ContentSizeExplicit => ref this._contentSizeExplicit;
	/// <summary>
	/// Window padding at the time of Begin().
	/// </summary>
	public ref ImVec2 WindowPadding => ref this._windowPadding;
	/// <summary>
	/// Window rounding at the time of Begin(). May be clamped lower to avoid rendering artifacts with title bar, menu bar etc.
	/// </summary>
	public ref float WindowRounding => ref this._windowRounding;
	/// <summary>
	/// Window border size at the time of Begin().
	/// </summary>
	public ref float WindowBorderSize => ref this._windowBorderSize;
	/// <summary>
	/// Size of buffer storing Name. May be larger than strlen(Name)!
	/// </summary>
	public ref int NameBufLen => ref this._nameBufLen;
	/// <summary>
	/// == window->GetID("#MOVE")
	/// </summary>
	public ref ImGuiID MoveId => ref this._moveId;
	/// <summary>
	/// ID of corresponding item in parent window (for navigation to return from child window to parent window)
	/// </summary>
	public ref ImGuiID ChildId => ref this._childId;
	public ref ImVec2 Scroll => ref this._scroll;
	public ref ImVec2 ScrollMax => ref this._scrollMax;
	/// <summary>
	/// target scroll position. stored as cursor position with scrolling canceled out, so the highest point is always 0.0f. (FLT_MAX for no change)
	/// </summary>
	public ref ImVec2 ScrollTarget => ref this._scrollTarget;
	/// <summary>
	/// 0.0f = scroll so that target position is at top, 0.5f = scroll so that target position is centered
	/// </summary>
	public ref ImVec2 ScrollTargetCenterRatio => ref this._scrollTargetCenterRatio;
	/// <summary>
	/// 0.0f = no snapping, >0.0f snapping threshold
	/// </summary>
	public ref ImVec2 ScrollTargetEdgeSnapDist => ref this._scrollTargetEdgeSnapDist;
	/// <summary>
	/// Size taken by each scrollbars on their smaller axis. Pay attention! ScrollbarSizes.x == width of the vertical scrollbar, ScrollbarSizes.y = height of the horizontal scrollbar.
	/// </summary>
	public ref ImVec2 ScrollbarSizes => ref this._scrollbarSizes;
	/// <summary>
	/// Is horizontal scrollbar visible?
	/// </summary>
	public ref bool ScrollbarX => ref this._scrollbarX;
	/// <summary>
	/// Is vertical scrollbar visible?
	/// </summary>
	public ref bool ScrollbarY => ref this._scrollbarY;
	/// <summary>
	/// Set to true on Begin(), unless Collapsed
	/// </summary>
	public ref bool Active => ref this._active;
	public ref bool WasActive => ref this._wasActive;
	/// <summary>
	/// Set to true when any widget access the current window
	/// </summary>
	public ref bool WriteAccessed => ref this._writeAccessed;
	/// <summary>
	/// Set when collapsing window to become only title-bar
	/// </summary>
	public ref bool Collapsed => ref this._collapsed;
	public ref bool WantCollapseToggle => ref this._wantCollapseToggle;
	/// <summary>
	/// Set when items can safely be all clipped (e.g. window not visible or collapsed)
	/// </summary>
	public ref bool SkipItems => ref this._skipItems;
	/// <summary>
	/// Set during the frame where the window is appearing (or re-appearing)
	/// </summary>
	public ref bool Appearing => ref this._appearing;
	/// <summary>
	/// Do not display (== HiddenFrames*** > 0)
	/// </summary>
	public ref bool Hidden => ref this._hidden;
	/// <summary>
	/// Set on the "Debug##Default" window.
	/// </summary>
	public ref bool IsFallbackWindow => ref this._isFallbackWindow;
	/// <summary>
	/// Set when passed _ChildWindow, left to false by BeginDocked()
	/// </summary>
	public ref bool IsExplicitChild => ref this._isExplicitChild;
	/// <summary>
	/// Set when the window has a close button (p_open != NULL)
	/// </summary>
	public ref bool HasCloseButton => ref this._hasCloseButton;
	/// <summary>
	/// Current border being held for resize (-1: none, otherwise 0-3)
	/// </summary>
	public ref sbyte ResizeBorderHeld => ref this._resizeBorderHeld;
	/// <summary>
	/// Number of Begin() during the current frame (generally 0 or 1, 1+ if appending via multiple Begin/End pairs)
	/// </summary>
	public ref short BeginCount => ref this._beginCount;
	/// <summary>
	/// Begin() order within immediate parent window, if we are a child window. Otherwise 0.
	/// </summary>
	public ref short BeginOrderWithinParent => ref this._beginOrderWithinParent;
	/// <summary>
	/// Begin() order within entire imgui context. This is mostly used for debugging submission order related issues.
	/// </summary>
	public ref short BeginOrderWithinContext => ref this._beginOrderWithinContext;
	/// <summary>
	/// Order within WindowsFocusOrder[], altered when windows are focused.
	/// </summary>
	public ref short FocusOrder => ref this._focusOrder;
	/// <summary>
	/// ID in the popup stack when this window is used as a popup/menu (because we use generic Name/ID for recycling)
	/// </summary>
	public ref ImGuiID PopupId => ref this._popupId;
	public ref ImS8 AutoFitFramesX => ref this._autoFitFramesX;
	public ref ImS8 AutoFitFramesY => ref this._autoFitFramesY;
	public ref ImS8 AutoFitChildAxises => ref this._autoFitChildAxises;
	public ref bool AutoFitOnlyGrows => ref this._autoFitOnlyGrows;
	public ref ImGuiNET.ImGuiDir AutoPosLastDirection => ref this._autoPosLastDirection;
	/// <summary>
	/// Hide the window for N frames
	/// </summary>
	public ref ImS8 HiddenFramesCanSkipItems => ref this._hiddenFramesCanSkipItems;
	/// <summary>
	/// Hide the window for N frames while allowing items to be submitted so we can measure their size
	/// </summary>
	public ref ImS8 HiddenFramesCannotSkipItems => ref this._hiddenFramesCannotSkipItems;
	/// <summary>
	/// Hide the window until frame N at Render() time only
	/// </summary>
	public ref ImS8 HiddenFramesForRenderOnly => ref this._hiddenFramesForRenderOnly;
	/// <summary>
	/// Disable window interactions for N frames
	/// </summary>
	public ref ImS8 DisableInputsFrames => ref this._disableInputsFrames;
	/// <summary>
	/// store acceptable condition flags for SetNextWindowPos() use.
	/// </summary>
	public ref ImGuiNET.ImGuiCond SetWindowPosAllowFlags => ref this._setWindowAllowFlags;
	/// <summary>
	/// store acceptable condition flags for SetNextWindowSize() use.
	/// </summary>
	public ref ImGuiNET.ImGuiCond SetWindowSizeAllowFlags => ref this._setWindowAllowFlags;
	/// <summary>
	/// store acceptable condition flags for SetNextWindowCollapsed() use.
	/// </summary>
	public ref ImGuiNET.ImGuiCond SetWindowCollapsedAllowFlags => ref this._setWindowAllowFlags;
	/// <summary>
	/// store window position when using a non-zero Pivot (position set needs to be processed when we know the window size)
	/// </summary>
	public ref ImVec2 SetWindowPosVal => ref this._setWindowPosVal;
	/// <summary>
	/// store window pivot for positioning. ImVec2(0, 0) when positioning from top-left corner; ImVec2(0.5f, 0.5f) for centering; ImVec2(1, 1) for bottom right.
	/// </summary>
	public ref ImVec2 SetWindowPosPivot => ref this._setWindowPosPivot;
	/// <summary>
	/// ID stack. ID are hashes seeded with the value at the top of the stack. (In theory this should be in the TempData structure)
	/// </summary>
	public ref ImVector<ImGuiID> IDStack => ref this._idStack;
	/// <summary>
	/// Temporary per-window data, reset at the beginning of the frame. This used to be called ImGuiDrawContext, hence the "DC" variable name.
	/// </summary>
	public ref ImGuiNET.ImGuiWindowTempData DC => ref this._dc;

	// The best way to understand what those rectangles are is to use the 'Metrics->Tools->Show Windows Rectangles' viewer.
	// The main 'OuterRect', omitted as a field, is window->Rect().
	/// <summary>
	/// == Window->Rect() just after setup in Begin(). == window->Rect() for root window.
	/// </summary>
	public ref ImRect OuterRectClipped => ref this._outerRectClipped;
	/// <summary>
	/// Inner rectangle (omit title bar, menu bar, scroll bar)
	/// </summary>
	public ref ImRect InnerRect => ref this._innerRect;
	/// <summary>
	/// == InnerRect shrunk by WindowPadding*0.5f on each side, clipped within viewport or parent clip rect.
	/// </summary>
	public ref ImRect InnerClipRect => ref this._innerClipRect;
	//Initially covers the whole scrolling region. Reduced by containers e.g columns/tables when active. Shrunk by WindowPadding*1.0f on each side. This is meant to replace ContentRegionRect over time (from 1.71+ onward).
	public ref ImRect WorkRect => ref this._workRect;
	/// <summary>
	/// Backup of WorkRect before entering a container such as columns/tables. Used by e.g. SpanAllColumns functions to easily access. Stacked containers are responsible for maintaining this. // FIXME-WORKRECT: Could be a stack?
	/// </summary>
	public ref ImRect ParentWorkRect => ref this._parentWorkRect;
	/// <summary>
	/// Current clipping/scissoring rectangle, evolve as we are using PushClipRect(), etc. == DrawList->clip_rect_stack.back().
	/// </summary>
	public ref ImRect ClipRect => ref this._clipRect;
	/// <summary>
	/// FIXME: This is currently confusing/misleading. It is essentially WorkRect but not handling of scrolling. We currently rely on it as right/bottom aligned sizing operation need some size to rely on.
	/// </summary>
	public ref ImRect ContentRegionRect => ref this._contentRegionRect;
	/// <summary>
	/// Define an optional rectangular hole where mouse will pass-through the window.
	/// </summary>
	public ref ImGuiNET.Vector2ih HitTestHoleSize => ref this._hitTestHoleSize;
	public ref ImGuiNET.Vector2ih HitTestHoleOffset => ref this._hitTestHoleOffset;
	/// <summary>
	/// Last frame number the window was Active.
	/// </summary>
	public ref int LastFrameActive => ref this._lastFrameActive;
	/// <summary>
	/// Last timestamp the window was Active (using float as we don't need high precision there)
	/// </summary>
	public ref float LastTimeActive => ref this._lastTimeActive;
	public ref float ItemWidthDefault => ref this._itemWidthDefault;
	public ref ImGuiStorage StateStorage => ref this._stateStorage;
	public ref ImVector<ImGuiNET.ImGuiOldColumns> ColumnsStorage => ref this._columnsStorage;
	/// <summary>
	/// User scale multiplier per-window, via SetWindowFontScale()
	/// </summary>
	public ref float FontWindowScale => ref this._fontWindowScale;
	/// <summary>
	/// Offset into SettingsWindows[] (offsets are always valid as we only grow the array from the back)
	/// </summary>
	public ref int SettingsOffset => ref this._settingsOffset;
	/// <summary>
	/// == &DrawListInst (for backward compatibility reason with code using imgui_internal.h we keep this a pointer)
	/// </summary>
	/// <summary>
	/// == &DrawListInst (for backward compatibility reason with code using imgui_internal.h we keep this a pointer)
	/// </summary>
	public ref ImDrawList DrawList => ref *this._drawList;
	public bool HasDrawList => this._drawList != null;
	public void SetDrawList(ref ImDrawList drawList) => this._drawList = ImGuiHelper.GetPointer(ref drawList);
	/// <summary>
	/// Clears the draw list reference, resetting it to null.
	/// </summary>
	public void ClearDrawList() => this._drawList = null;

	/// <summary>
	/// For backward compatibility with code using imgui_internal.h, we keep this as a pointer.
	/// </summary>
	public ref ImDrawList DrawListInst => ref this._drawListInst;
	/// <summary>
	/// If we are a child _or_ popup _or_ docked window, this is pointing to our parent. Otherwise NULL.
	/// </summary>
	public ref ImGuiWindow ParentWindow => ref *this._parentWindow;
	public bool HasParentWindow => this._parentWindow != null;
	public void SetParentWindow(ref ImGuiWindow parentWindow) => this._parentWindow = ImGuiHelper.GetPointer(ref parentWindow);
	public void ClearParentWindow() => this._parentWindow = null;
	public ref ImGuiWindow ParentWindowInBeginStack => ref *this._parentWindowInBeginStack;
	public bool HasParentWindowInBeginStack => this._parentWindowInBeginStack != null;
	public void SetParentWindowInBeginStack(ref ImGuiWindow parentWindowInBeginStack) => this._parentWindowInBeginStack = ImGuiHelper.GetPointer(ref parentWindowInBeginStack);
	public void ClearParentWindowInBeginStack() => this._parentWindowInBeginStack = null;
	/// <summary>
	/// Point to ourself or first ancestor that is not a child window. Doesn't cross through popups/dock nodes.
	/// </summary>
	public ref ImGuiWindow RootWindow => ref *this._rootWindow;
	public bool HasRootWindow => this._rootWindow != null;
	public void SetRootWindow(ref ImGuiWindow rootWindow) => this._rootWindow = ImGuiHelper.GetPointer(ref rootWindow);
	public void ClearRootWindow() => this._rootWindow = null;
	/// <summary>
	/// Point to ourself or first ancestor that is not a child window. Cross through popups parent<>child.
	/// </summary>
	public ref ImGuiWindow RootWindowPopupTree => ref *this._rootWindowPopupTree;
	public bool HasRootWindowPopupTree => this._rootWindowPopupTree != null;
	public void SetRootWindowPopupTree(ref ImGuiWindow rootWindowPopupTree) => this._rootWindowPopupTree = ImGuiHelper.GetPointer(ref rootWindowPopupTree);
	public void ClearRootWindowPopupTree() => this._rootWindowPopupTree = null;
	/// <summary>
	/// Point to ourself or first ancestor which will display TitleBgActive color when this window is active.
	/// </summary>
	public ref ImGuiWindow RootWindowForTitleBarHighlight => ref *this._rootWindowForTitleBarHighlight;
	public bool HasRootWindowForTitleBarHighlight => this._rootWindowForTitleBarHighlight != null;
	public void SetRootWindowForTitleBarHighlight(ref ImGuiWindow rootWindowForTitleBarHighlight) => this._rootWindowForTitleBarHighlight = ImGuiHelper.GetPointer(ref rootWindowForTitleBarHighlight);
	public void ClearRootWindowForTitleBarHighlight() => this._rootWindowForTitleBarHighlight = null;
	/// <summary>
	/// Point to ourself or first ancestor which doesn't have the NavFlattened flag.
	/// </summary>
	public ref ImGuiWindow RootWindowForNav => ref *this._rootWindowForNav;
	public bool HasRootWindowForNav => this._rootWindowForNav != null;
	public void SetRootWindowForNav(ref ImGuiWindow rootWindowForNav) => this._rootWindowForNav = ImGuiHelper.GetPointer(ref rootWindowForNav);
	public void ClearRootWindowForNav() => this._rootWindowForNav = null;
	/// <summary>
	/// When going to the menu bar, we remember the child window we came from. (This could probably be made implicit if we kept g.Windows sorted by last focused including child window.)
	/// </summary>
	public ref ImGuiWindow NavLastChildNavWindow => ref *this._navLastChildNavWindow;
	public bool HasNavLastChildNavWindow => this._navLastChildNavWindow != null;
	public void SetNavLastChildNavWindow(ref ImGuiWindow navLastChildNavWindow) => this._navLastChildNavWindow = ImGuiHelper.GetPointer(ref navLastChildNavWindow);
	public void ClearNavLastChildNavWindow() => this._navLastChildNavWindow = null;
	/// <summary>
	/// Last known NavId for this window, per layer (0/1)
	/// </summary>
	public Span<ImGuiID> NavLastIds
	{
		get
		{
			fixed (ImGuiID* ptr = this._navLastIds)
			{
				return new Span<ImGuiID>(ptr, (int)ImGuiNET.ImGuiNavLayer.COUNT);
			}
		}
	}
	/// <summary>
	/// Reference rectangle, in window relative space
	/// </summary>
	public ref ImRect NavRectRelMain => ref this._navRectRelMain;
	/// <summary>
	/// Reference rectangle, in window relative space
	/// </summary>
	public ref ImRect NavRectRelMenu => ref this._navRectRelMenu;

	/// <summary>
	/// Backup of last idx/vtx count, so when waking up the window we can preallocate and avoid iterative alloc/copy
	/// </summary>
	public ref int MemoryDrawListIdxCapacity => ref this._memoryDrawListIdxCapacity;
	public ref int MemoryDrawListVtxCapacity => ref this._memoryDrawListVtxCapacity;
	/// <summary>
	/// Set when window extraneous data have been garbage collected
	/// </summary>
	public ref bool MemoryCompacted => ref this._memoryCompacted;

	//public unsafe ImGuiWindow(ImGuiContext* context, string name)
	//{
	//	ImGuiWindow_ImGuiWindow(ref this, context, name != null ? (byte*)Marshal.StringToHGlobalAnsi(name) : null);
	//}

	public ImGuiID GetID(string s)
	{
		return ImGuiWindow_GetID_Str(ref this, ImGuiHelper.GetStringPointer(s), null);
	}
	ImGuiID GetID(IntPtr ptr) => ImGuiWindow_GetID_Ptr(ref this, (void*)ptr);
	ImGuiID GetID(int n) => ImGuiWindow_GetID_Int(ref this, n);
	ImGuiID GetIDFromRectangle(ImRect r_abs) => ImGuiWindow_GetIDFromRectangle(ref this, ref r_abs);

	// We don't use g.FontSize because the window may be != g.CurrentWidow.
	public readonly ImRect Rect() => new(this._pos.X, this._pos.Y, this._pos.X + this._size.X, this._pos.Y + this._size.Y);
	public readonly unsafe float CalcFontSize() 
	{
		ref var g = ref ImGuiHelper.GetCurrentContext();
		var scale = g.FontBaseSize * this._fontWindowScale;
		if (HasParentWindow)
			scale *= ParentWindow.FontWindowScale;
		return scale;
	}
	public readonly float TitleBarHeight()
	{
		ref var g = ref ImGuiHelper.GetCurrentContext();
		return (this._flags & ImGuiWindowFlags.NoTitleBar) != 0 
			? 0.0f 
			: CalcFontSize() + (g.Style.FramePadding.Y * 2.0f);
	}
	public readonly ImRect TitleBarRect() => new(this._pos, new ImVec2(this._pos.X + this._sizeFull.X, this._pos.Y + TitleBarHeight()));
	public readonly float MenuBarHeight() 
	{
		ref var g = ref ImGuiHelper.GetCurrentContext();
		return (this._flags & ImGuiWindowFlags.MenuBar) != 0
			? this._dc.MenuBarOffset.y + CalcFontSize() + (g.Style.FramePadding.Y * 2.0f) 
			: 0.0f;
	}
	public readonly ImRect MenuBarRect()
	{
		var y1 = this._pos.Y + TitleBarHeight();
		return new ImRect(this._pos.X, y1, this._pos.X + this._sizeFull.X, y1 + MenuBarHeight());
	}
	
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImGuiWindow_ImGuiWindow(ref ImGuiWindow self, ImGuiContext* context, byte* name);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern ImGuiID ImGuiWindow_GetID_Int(ref ImGuiWindow self, int n);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern ImGuiID ImGuiWindow_GetID_Str(ref ImGuiWindow self, byte* str_begin, byte* str_end);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern ImGuiID ImGuiWindow_GetID_Ptr(ref ImGuiWindow self, void* ptr);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern ImGuiID ImGuiWindow_GetIDFromRectangle(ref ImGuiWindow self, ref readonly ImRect r_abs);
}