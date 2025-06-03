#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
using Entropy.UI.ImGUI;
using System.Runtime.InteropServices;
using System.Text;
using ImGuiID = System.UInt32;
using ImS8 = System.SByte;

namespace Entropy.UI.ImGUI;
public unsafe struct ImGuiWindow
{
	/// <summary>
	/// Window name, owned by the window.
	/// </summary>
	public byte* Name;
	/// <summary>
	/// == ImHashStr(Name)
	/// </summary>
	public ImGuiID ID;
	/// <summary>
	/// See enum ImGuiWindowFlags_
	/// </summary>
	public ImGuiWindowFlags Flags;
	/// <summary>
	/// Always set in Begin(). Inactive windows may have a NULL value here if their viewport was discarded.
	/// </summary>
	public ImGuiNET.ImGuiViewportP* Viewport;
	/// <summary>
	/// Position (always rounded-up to nearest pixel)
	/// </summary>
	public ImVec2 Pos;
	/// <summary>
	/// Current size (==SizeFull or collapsed title bar size)
	/// </summary>
	public ImVec2 Size;
	/// <summary>
	/// Size when non collapsed
	/// </summary>
	public ImVec2 SizeFull;
	/// <summary>
	/// Size of contents/scrollable client area (calculated from the extents reach of the cursor) from previous frame. Does not include window decoration or window padding.
	/// </summary>
	public ImVec2 ContentSize;
	public ImVec2 ContentSizeIdeal;
	/// <summary>
	/// Size of contents/scrollable client area explicitly request by the user via SetNextWindowContentSize().
	/// </summary>
	public ImVec2 ContentSizeExplicit;
	/// <summary>
	/// Window padding at the time of Begin().
	/// </summary>
	public ImVec2 WindowPadding;
	/// <summary>
	/// Window rounding at the time of Begin(). May be clamped lower to avoid rendering artifacts with title bar, menu bar etc.
	/// </summary>
	public float WindowRounding;
	/// <summary>
	/// Window border size at the time of Begin().
	/// </summary>
	public float WindowBorderSize;
	/// <summary>
	/// Size of buffer storing Name. May be larger than strlen(Name)!
	/// </summary>
	public int NameBufLen;
	/// <summary>
	/// == window->GetID("#MOVE")
	/// </summary>
	public ImGuiID MoveId;
	/// <summary>
	/// ID of corresponding item in parent window (for navigation to return from child window to parent window)
	/// </summary>
	public ImGuiID ChildId;
	public ImVec2 Scroll;
	public ImVec2 ScrollMax;
	/// <summary>
	/// target scroll position. stored as cursor position with scrolling canceled out, so the highest point is always 0.0f. (FLT_MAX for no change)
	/// </summary>
	public ImVec2 ScrollTarget;
	/// <summary>
	/// 0.0f = scroll so that target position is at top, 0.5f = scroll so that target position is centered
	/// </summary>
	public ImVec2 ScrollTargetCenterRatio;
	/// <summary>
	/// 0.0f = no snapping, >0.0f snapping threshold
	/// </summary>
	public ImVec2 ScrollTargetEdgeSnapDist;
	/// <summary>
	/// Size taken by each scrollbars on their smaller axis. Pay attention! ScrollbarSizes.x == width of the vertical scrollbar, ScrollbarSizes.y = height of the horizontal scrollbar.
	/// </summary>
	public ImVec2 ScrollbarSizes;
	/// <summary>
	/// Is horizontal scrollbar visible?
	/// </summary>
	public bool ScrollbarX;
	/// <summary>
	/// Is vertical scrollbar visible?
	/// </summary>
	public bool ScrollbarY;
	/// <summary>
	/// Set to true on Begin(), unless Collapsed
	/// </summary>
	public bool Active;
	public bool WasActive;
	/// <summary>
	/// Set to true when any widget access the current window
	/// </summary>
	public bool WriteAccessed;
	/// <summary>
	/// Set when collapsing window to become only title-bar
	/// </summary>
	public bool Collapsed;
	public bool WantCollapseToggle;
	/// <summary>
	/// Set when items can safely be all clipped (e.g. window not visible or collapsed)
	/// </summary>
	public bool SkipItems;
	/// <summary>
	/// Set during the frame where the window is appearing (or re-appearing)
	/// </summary>
	public bool Appearing;
	/// <summary>
	/// Do not display (== HiddenFrames*** > 0)
	/// </summary>
	public bool Hidden;
	/// <summary>
	/// Set on the "Debug##Default" window.
	/// </summary>
	public bool IsFallbackWindow;
	/// <summary>
	/// Set when passed _ChildWindow, left to false by BeginDocked()
	/// </summary>
	public bool IsExplicitChild;
	/// <summary>
	/// Set when the window has a close button (p_open != NULL)
	/// </summary>
	public bool HasCloseButton;
	///Current border being held for resize (-1: none, otherwise 0-3)
	public sbyte ResizeBorderHeld;
	/// <summary>
	/// Number of Begin() during the current frame (generally 0 or 1, 1+ if appending via multiple Begin/End pairs)
	/// </summary>
	public short BeginCount;
	/// <summary>
	/// Begin() order within immediate parent window, if we are a child window. Otherwise 0.
	/// </summary>
	public short BeginOrderWithinParent;
	/// <summary>
	/// Begin() order within entire imgui context. This is mostly used for debugging submission order related issues.
	/// </summary>
	public short BeginOrderWithinContext;
	/// <summary>
	/// Order within WindowsFocusOrder[], altered when windows are focused.
	/// </summary>
	public short FocusOrder;
	/// <summary>
	/// ID in the popup stack when this window is used as a popup/menu (because we use generic Name/ID for recycling)
	/// </summary>
	public ImGuiID PopupId;
	public ImS8 AutoFitFramesX, AutoFitFramesY;
	public ImS8 AutoFitChildAxises;
	public bool AutoFitOnlyGrows;
	public ImGuiNET.ImGuiDir AutoPosLastDirection;
	/// <summary>
	/// Hide the window for N frames
	/// </summary>
	public ImS8 HiddenFramesCanSkipItems;
	/// <summary>
	/// Hide the window for N frames while allowing items to be submitted so we can measure their size
	/// </summary>
	public ImS8 HiddenFramesCannotSkipItems;
	/// <summary>
	/// Hide the window until frame N at Render() time only
	/// </summary>
	public ImS8 HiddenFramesForRenderOnly;
	/// <summary>
	/// Disable window interactions for N frames
	/// </summary>
	public ImS8 DisableInputsFrames;
	private byte _setWindowAllowFlags;
	/// <summary>
	/// store acceptable condition flags for SetNextWindowPos() use.
	/// </summary>
	public ImGuiNET.ImGuiCond SetWindowPosAllowFlags => (ImGuiNET.ImGuiCond)this._setWindowAllowFlags;
	/// <summary>
	/// store acceptable condition flags for SetNextWindowSize() use.
	/// </summary>
	public ImGuiNET.ImGuiCond SetWindowSizeAllowFlags => (ImGuiNET.ImGuiCond)this._setWindowAllowFlags;
	/// <summary>
	/// store acceptable condition flags for SetNextWindowCollapsed() use.
	/// </summary>
	public ImGuiNET.ImGuiCond SetWindowCollapsedAllowFlags => (ImGuiNET.ImGuiCond)this._setWindowAllowFlags;
	/// <summary>
	/// store window position when using a non-zero Pivot (position set needs to be processed when we know the window size)
	/// </summary>
	public ImVec2 SetWindowPosVal;
	/// <summary>
	/// store window pivot for positioning. ImVec2(0, 0) when positioning from top-left corner; ImVec2(0.5f, 0.5f) for centering; ImVec2(1, 1) for bottom right.
	/// </summary>
	public ImVec2 SetWindowPosPivot;
	/// <summary>
	/// ID stack. ID are hashes seeded with the value at the top of the stack. (In theory this should be in the TempData structure)
	/// </summary>
	public ImVector<ImGuiID> IDStack;
	/// <summary>
	/// Temporary per-window data, reset at the beginning of the frame. This used to be called ImGuiDrawContext, hence the "DC" variable name.
	/// </summary>
	public ImGuiNET.ImGuiWindowTempData DC;

	// The best way to understand what those rectangles are is to use the 'Metrics->Tools->Show Windows Rectangles' viewer.
	// The main 'OuterRect', omitted as a field, is window->Rect().
	/// <summary>
	/// == Window->Rect() just after setup in Begin(). == window->Rect() for root window.
	/// </summary>
	public ImRect OuterRectClipped;
	/// <summary>
	/// Inner rectangle (omit title bar, menu bar, scroll bar)
	/// </summary>
	public ImRect InnerRect;
	/// <summary>
	/// == InnerRect shrunk by WindowPadding*0.5f on each side, clipped within viewport or parent clip rect.
	/// </summary>
	public ImRect InnerClipRect;
	//Initially covers the whole scrolling region. Reduced by containers e.g columns/tables when active. Shrunk by WindowPadding*1.0f on each side. This is meant to replace ContentRegionRect over time (from 1.71+ onward).
	public ImRect WorkRect;
	/// <summary>
	/// Backup of WorkRect before entering a container such as columns/tables. Used by e.g. SpanAllColumns functions to easily access. Stacked containers are responsible for maintaining this. // FIXME-WORKRECT: Could be a stack?
	/// </summary>
	public ImRect ParentWorkRect;
	/// <summary>
	/// Current clipping/scissoring rectangle, evolve as we are using PushClipRect(), etc. == DrawList->clip_rect_stack.back().
	/// </summary>
	public ImRect ClipRect;
	/// <summary>
	/// FIXME: This is currently confusing/misleading. It is essentially WorkRect but not handling of scrolling. We currently rely on it as right/bottom aligned sizing operation need some size to rely on.
	/// </summary>
	public ImRect ContentRegionRect;
	/// <summary>
	/// Define an optional rectangular hole where mouse will pass-through the window.
	/// </summary>
	public ImGuiNET.Vector2ih HitTestHoleSize;
	public ImGuiNET.Vector2ih HitTestHoleOffset;
	/// <summary>
	/// Last frame number the window was Active.
	/// </summary>
	public int LastFrameActive;
	/// <summary>
	/// Last timestamp the window was Active (using float as we don't need high precision there)
	/// </summary>
	public float LastTimeActive;
	public float ItemWidthDefault;
	public ImGuiStorage StateStorage;
	public ImVector<ImGuiNET.ImGuiOldColumns> ColumnsStorage;
	/// <summary>
	/// User scale multiplier per-window, via SetWindowFontScale()
	/// </summary>
	public float FontWindowScale;
	/// <summary>
	/// Offset into SettingsWindows[] (offsets are always valid as we only grow the array from the back)
	/// </summary>
	public int SettingsOffset;
	/// <summary>
	/// == &DrawListInst (for backward compatibility reason with code using imgui_internal.h we keep this a pointer)
	/// </summary>
	public ImDrawList* DrawList;
	/// <summary>
	/// == &DrawListInst (for backward compatibility reason with code using imgui_internal.h we keep this a pointer)
	/// </summary>
	public ImDrawListPtr DrawListPtr => this.DrawList;
	public ImDrawList DrawListInst;
	/// <summary>
	/// If we are a child _or_ popup _or_ docked window, this is pointing to our parent. Otherwise NULL.
	/// </summary>
	public ImGuiWindow* ParentWindow;
	public ImGuiWindow* ParentWindowInBeginStack;
	/// <summary>
	/// Point to ourself or first ancestor that is not a child window. Doesn't cross through popups/dock nodes.
	/// </summary>
	public ImGuiWindow* RootWindow;
	/// <summary>
	/// Point to ourself or first ancestor that is not a child window. Cross through popups parent<>child.
	/// </summary>
	public ImGuiWindow* RootWindowPopupTree;
	/// <summary>
	/// Point to ourself or first ancestor which will display TitleBgActive color when this window is active.
	/// </summary>
	public ImGuiWindow* RootWindowForTitleBarHighlight;
	/// <summary>
	/// Point to ourself or first ancestor which doesn't have the NavFlattened flag.
	/// </summary>
	public ImGuiWindow* RootWindowForNav;
	/// <summary>
	/// When going to the menu bar, we remember the child window we came from. (This could probably be made implicit if we kept g.Windows sorted by last focused including child window.)
	/// </summary>
	public ImGuiWindow* NavLastChildNavWindow;
	/// <summary>
	/// Last known NavId for this window, per layer (0/1)
	/// </summary>
	public fixed ImGuiID NavLastIds[(int)ImGuiNET.ImGuiNavLayer.COUNT];
	/// <summary>
	/// Reference rectangle, in window relative space
	/// </summary>
	ImRect NavRectRelMain;
	/// <summary>
	/// Reference rectangle, in window relative space
	/// </summary>
	ImRect NavRectRelMenu;

	/// <summary>
	/// Backup of last idx/vtx count, so when waking up the window we can preallocate and avoid iterative alloc/copy
	/// </summary>
	public int MemoryDrawListIdxCapacity;
	public int MemoryDrawListVtxCapacity;
	/// <summary>
	/// Set when window extraneous data have been garbage collected
	/// </summary>
	public bool MemoryCompacted;

	//public unsafe ImGuiWindow(ImGuiContext* context, string name)
	//{
	//	ImGuiWindow_ImGuiWindow(ref this, context, name != null ? (byte*)Marshal.StringToHGlobalAnsi(name) : null);
	//}

	public ImGuiID GetID(string s)
	{
		var byteCount = Encoding.UTF8.GetByteCount(s) + 1;
		var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(byteCount);
		Encoding.UTF8.GetBytes(s, buffer.AsSpan<byte>(0, byteCount));
		buffer[byteCount-1] = 0; // Explicit null-terminator
		fixed(byte* textPtr = buffer)
		{
			return ImGuiWindow_GetID_Str(ref this, textPtr, null);
		}
	}
	ImGuiID GetID(IntPtr ptr) => ImGuiWindow_GetID_Ptr(this, (void*)ptr);
	ImGuiID GetID(int n) => ImGuiWindow_GetID_Int(this, n);
	ImGuiID GetIDFromRectangle(ImRect r_abs) => ImGuiWindow_GetIDFromRectangle(this, ref r_abs);

	// We don't use g.FontSize because the window may be != g.CurrentWidow.
	public readonly ImRect Rect() => new ImRect(this.Pos.X, this.Pos.Y, this.Pos.X + this.Size.X, this.Pos.Y + this.Size.Y);
	public readonly float CalcFontSize() 
	{
		ImGuiContextPtr g = ImGuiNET.ImGui.GetCurrentContext();
		float scale = g.FontBaseSize * this.FontWindowScale;
		if (this.ParentWindow != null)
			scale *= this.ParentWindow->FontWindowScale;
		return scale;
	}
	public readonly float TitleBarHeight()
	{
		ImGuiContextPtr g = ImGuiNET.ImGui.GetCurrentContext();
		return (this.Flags & ImGuiWindowFlags.NoTitleBar) != 0 
			? 0.0f 
			: CalcFontSize() + g.Style.FramePadding.Y * 2.0f;
	}
	public readonly ImRect TitleBarRect() => new ImRect(this.Pos, new ImVec2(this.Pos.X + this.SizeFull.X, this.Pos.Y + TitleBarHeight()));
	public readonly float MenuBarHeight() 
	{
		ImGuiContextPtr g = ImGuiNET.ImGui.GetCurrentContext();
		return (this.Flags & ImGuiWindowFlags.MenuBar) != 0
			? this.DC.MenuBarOffset.y + CalcFontSize() + g.Style.FramePadding.Y * 2.0f 
			: 0.0f;
	}
	public readonly ImRect MenuBarRect()
	{
		float y1 = this.Pos.Y + TitleBarHeight();
		return new ImRect(this.Pos.X, y1, this.Pos.X + this.SizeFull.X, y1 + MenuBarHeight());
	}
	
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImGuiWindow_ImGuiWindow(in ImGuiWindow self, ImGuiContext* context, byte* name);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern ImGuiID ImGuiWindow_GetID_Int(in ImGuiWindow self, int n);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern ImGuiID ImGuiWindow_GetID_Str(ref ImGuiWindow self, byte* str_begin, byte* str_end);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern ImGuiID ImGuiWindow_GetID_Ptr(in ImGuiWindow self, void* ptr);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern ImGuiID ImGuiWindow_GetIDFromRectangle(in ImGuiWindow self, ref readonly ImRect r_abs);
}
