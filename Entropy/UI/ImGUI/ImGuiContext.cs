#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Runtime.InteropServices;
using ImGuiNET;
using UnityEngine;
using ImU32 = uint;
using ImGuiID = uint;
using ImU8 = byte;
using ImS8 = sbyte;
using ImGuiKeyChord = int;
using ImFileHandle = System.IntPtr;
using ImGuiErrorCallback = System.IntPtr;

namespace Entropy.UI.ImGUI;

public struct ImGuiContextPtr
{
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
	public unsafe ImGuiContext* NativePtr { get; }
	public unsafe ImGuiContextPtr(ImGuiContext* nativePtr) => NativePtr = nativePtr;
	public unsafe ImGuiContextPtr(IntPtr nativePtr) => NativePtr = (ImGuiContext*)(void*)nativePtr;
	public static unsafe implicit operator ImGuiContextPtr(ImGuiContext* nativePtr) => new(nativePtr);
	public static unsafe implicit operator ImGuiContext*(ImGuiContextPtr wrappedPtr) => wrappedPtr.NativePtr;
	public static implicit operator ImGuiContextPtr(IntPtr nativePtr) => new(nativePtr);
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
}

public class TestUnmanaged<T> where T : unmanaged { }

public class test
{
	TestUnmanaged<ImGuiContext>? asdf;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ImGuiContext
{
	public bool Initialized;
	public bool FontAtlasOwnedByContext;            // IO.Fonts-> is owned by the ImGuiContext and will be destructed along with it.
	public ImGuiIO IO;
	public ImVector<ImGuiInputEvent> InputEventsQueue;                 // Input events which will be tricked/written into IO structure.
	public ImVector<ImGuiInputEvent> InputEventsTrail;                 // Past input events processed in NewFrame(). This is to allow domain-specific application to access e.g mouse/pen trail.
	public ImGuiStyle Style;
	public ImFontPtr Font;                               // (Shortcut) == FontStack.empty() ? IO.Font : FontStack.back()
	public float FontSize;                           // (Shortcut) == FontBaseSize Ptr g.CurrentWindow->FontWindowScale == window->FontSize(). Text height for current window.
	public float FontBaseSize;                       // (Shortcut) == IO.FontGlobalScale Ptr Font->Scale Ptr Font->FontSize. Base text height.
	public ImDrawListSharedData DrawListSharedData;
	public double Time;
	public int FrameCount;
	public int FrameCountEnded;
	public int FrameCountRendered;
	public bool WithinFrameScope;                   // Set by NewFrame(), cleared by EndFrame()
	public bool WithinFrameScopeWithImplicitWindow; // Set by NewFrame(), cleared by EndFrame() when the implicit debug window has been pushed
	public bool WithinEndChild;                     // Set within EndChild()
	public bool GcCompactAll;                       // Request full GC
	public bool TestEngineHookItems;                // Will call test engine hooks: ImGuiTestEngineHook_ItemAdd(), ImGuiTestEngineHook_ItemInfo(), ImGuiTestEngineHook_Log()
	public IntPtr TestEngine;                         // Test engine user data

	// Windows state
	public ImVector<ImGuiWindowPtr> Windows;                            // Windows, sorted in display order, back to front
	public ImVector<ImGuiWindowPtr> WindowsFocusOrder;                  // Root windows, sorted in focus order, back to front.
	public ImVector<ImGuiWindowPtr> WindowsTempSortBuffer;              // Temporary buffer used in EndFrame() to reorder windows so parents are kept before their child
	public ImVector<ImGuiWindowStackData> CurrentWindowStack;
	public ImGuiStorage WindowsById;                        // Map window's ImGuiID to ImGuiWindowPtr
	public int WindowsActiveCount;                 // Number of unique windows submitted by frame
	public ImVec2 WindowsHoverPadding;                // Padding around resizable windows for which hovering on counts as hovering the window == ImMax(style.TouchExtraPadding, WINDOWS_HOVER_PADDING)
	public ImGuiWindow* CurrentWindow;                      // Window being drawn into
	public ImGuiWindow* HoveredWindow;                      // Window the mouse is hovering. Will typically catch mouse inputs.
	public ImGuiWindow* HoveredWindowUnderMovingWindow;     // Hovered window ignoring MovingWindow. Only set if MovingWindow is set.
	public ImGuiWindow* MovingWindow;                       // Track the window we clicked on (in order to preserve focus). The actual window that is moved is generally MovingWindow->RootWindow.
	public ImGuiWindow* WheelingWindow;                     // Track the window we started mouse-wheeling on. Until a timer elapse or mouse has moved, generally keep scrolling the same window even if during the course of scrolling the mouse ends up hovering a child window.
	public ImVec2 WheelingWindowRefMousePos;
	public float WheelingWindowTimer;

	// Item/widgets state and tracking information
	public ImGuiID DebugHookIdInfo;                    // Will call core hooks: DebugHookIdInfo() from GetID functions, used by Stack Tool [next HoveredId/ActiveId to not pull in an extra cache-line]
	public ImGuiID HoveredId;                          // Hovered widget, filled during the frame
	public ImGuiID HoveredIdPreviousFrame;
	public bool HoveredIdAllowOverlap;
	public bool HoveredIdUsingMouseWheel;           // Hovered widget will use mouse wheel. Blocks scrolling the underlying window.
	public bool HoveredIdPreviousFrameUsingMouseWheel;
	public bool HoveredIdDisabled;                  // At least one widget passed the rect test, but has been discarded by disabled flag or popup inhibit. May be true even if HoveredId == 0.
	public float HoveredIdTimer;                     // Measure contiguous hovering time
	public float HoveredIdNotActiveTimer;            // Measure contiguous hovering time where the item has not been active
	public ImGuiID ActiveId;                           // Active widget
	public ImGuiID ActiveIdIsAlive;                    // Active widget has been seen this frame (we can't use a bool as the ActiveId may change within the frame)
	public float ActiveIdTimer;
	public bool ActiveIdIsJustActivated;            // Set at the time of activation for one frame
	public bool ActiveIdAllowOverlap;               // Active widget allows another widget to steal active id (generally for overlapping widgets, but not always)
	public bool ActiveIdNoClearOnFocusLoss;         // Disable losing active id if the active id window gets unfocused.
	public bool ActiveIdHasBeenPressedBefore;       // Track whether the active id led to a press (this is to allow changing between PressOnClick and PressOnRelease without pressing twice). Used by range_select branch.
	public bool ActiveIdHasBeenEditedBefore;        // Was the value associated to the widget Edited over the course of the Active state.
	public bool ActiveIdHasBeenEditedThisFrame;
	public ImVec2 ActiveIdClickOffset;                // Clicked offset from upper-left corner, if applicable (currently only set by ButtonBehavior)
	public ImGuiWindow* ActiveIdWindow;
	public ImGuiInputSource ActiveIdSource;                     // Activating with mouse or nav (gamepad/keyboard)
	public int ActiveIdMouseButton;
	public ImGuiID ActiveIdPreviousFrame;
	public bool ActiveIdPreviousFrameIsAlive;
	public bool ActiveIdPreviousFrameHasBeenEditedBefore;
	public ImGuiWindow* ActiveIdPreviousFrameWindow;
	public ImGuiID LastActiveId;                       // Store the last non-zero ActiveId, useful for animation.
	public float LastActiveIdTimer;                  // Store the last non-zero ActiveId timer since the beginning of activation, useful for animation.

	// Input Ownership
	public bool ActiveIdUsingMouseWheel;            // Active widget will want to read mouse wheel. Blocks scrolling the underlying window.
	public ImU32 ActiveIdUsingNavDirMask;            // Active widget will want to read those nav move requests (e.g. can activate a button and move away from it)
	public ImU32 ActiveIdUsingNavInputMask;          // Active widget will want to read those nav inputs.
	public ImBitArrayForNamedKeys ActiveIdUsingKeyInputMask;          // Active widget will want to read those key inputs. When we grow the ImGuiKey enum we'll need to either to order the enum to make useful keys come first, either redesign this into e.g. a small array.

	// Next window/item data
	public ImGuiItemFlags CurrentItemFlags;                      // == g.ItemFlagsStack.back()
	public ImGuiNextItemData NextItemData;                       // Storage for SetNextItemPtrPtr functions
	public ImGuiLastItemData LastItemData;                       // Storage for last submitted item (setup by ItemAdd)
	public ImGuiNextWindowData NextWindowData;                     // Storage for SetNextWindowPtrPtr functions

	// Shared stacks
	public ImVector<ImGuiColorMod> ColorStack;                         // Stack for PushStyleColor()/PopStyleColor() - inherited by Begin()
	public ImVector<ImGuiStyleMod> StyleVarStack;                      // Stack for PushStyleVar()/PopStyleVar() - inherited by Begin()
	public ImVector<ImFontPtr> FontStack;                          // Stack for PushFont()/PopFont() - inherited by Begin()
	public ImVector<ImGuiID> FocusScopeStack;                    // Stack for PushFocusScope()/PopFocusScope() - not inherited by Begin(), unless child window
	public ImVector<ImGuiItemFlags> ItemFlagsStack;                     // Stack for PushItemFlag()/PopItemFlag() - inherited by Begin()
	public ImVector<ImGuiGroupData> GroupStack;                         // Stack for BeginGroup()/EndGroup() - not inherited by Begin()
	public ImVector<ImGuiPopupData> OpenPopupStack;                     // Which popups are open (persistent)
	public ImVector<ImGuiPopupData> BeginPopupStack;                    // Which level of BeginPopup() we are in (reset every frame)
	public int BeginMenuCount;

	// Viewports
	public ImVector<ImGuiViewportPPtr> Viewports;                        // Active viewports (Size==1 in 'master' branch). Each viewports hold their copy of ImDrawData.

	// Gamepad/keyboard Navigation
	public ImGuiWindow* NavWindow;                          // Focused window for navigation. Could be called 'FocusedWindow'
	public ImGuiID NavId;                              // Focused item for navigation
	public ImGuiID NavFocusScopeId;                    // Identify a selection scope (selection code often wants to "clear other items" when landing on an item of the selection set)
	public ImGuiID NavActivateId;                      // ~~ (g.ActiveId == 0) && IsNavInputPressed(ImGuiNavInput_Activate) ? NavId : 0, also set when calling ActivateItem()
	public ImGuiID NavActivateDownId;                  // ~~ IsNavInputDown(ImGuiNavInput_Activate) ? NavId : 0
	public ImGuiID NavActivatePressedId;               // ~~ IsNavInputPressed(ImGuiNavInput_Activate) ? NavId : 0
	public ImGuiID NavActivateInputId;                 // ~~ IsNavInputPressed(ImGuiNavInput_Input) ? NavId : 0; ImGuiActivateFlags_PreferInput will be set and NavActivateId will be 0.
	public ImGuiActivateFlags NavActivateFlags;
	public ImGuiID NavJustMovedToId;                   // Just navigated to this id (result of a successfully MoveRequest).
	public ImGuiID NavJustMovedToFocusScopeId;         // Just navigated to this focus scope id (result of a successfully MoveRequest).
	public ImGuiModFlags NavJustMovedToKeyMods;
	public ImGuiID NavNextActivateId;                  // Set by ActivateItem(), queued until next frame.
	public ImGuiActivateFlags NavNextActivateFlags;
	public ImGuiInputSource NavInputSource;                     // Keyboard or Gamepad mode? THIS WILL ONLY BE None or NavGamepad or NavKeyboard.
	public ImGuiNavLayer NavLayer;                           // Layer we are navigating on. For now the system is hard-coded for 0=main contents and 1=menu/title bar, may expose layers later.
	public bool NavIdIsAlive;                       // Nav widget has been seen this frame ~~ NavRectRel is valid
	public bool NavMousePosDirty;                   // When set we will update mouse position if (io.ConfigFlags & ImGuiConfigFlags_NavEnableSetMousePos) if set (NB: this not enabled by default)
	public bool NavDisableHighlight;                // When user starts using mouse, we hide gamepad/keyboard highlight (NB: but they are still available, which is why NavDisableHighlight isn't always != NavDisableMouseHover)
	public bool NavDisableMouseHover;               // When user starts using gamepad/keyboard, we hide mouse hovering highlight until mouse is touched again.

	// Navigation: Init & Move Requests
	public bool NavAnyRequest;                      // ~~ NavMoveRequest || NavInitRequest this is to perform early out in ItemAdd()
	public bool NavInitRequest;                     // Init request for appearing window to select first item
	public bool NavInitRequestFromMove;
	public ImGuiID NavInitResultId;                    // Init request result (first item of the window, or one for which SetItemDefaultFocus() was called)
	public ImRect NavInitResultRectRel;               // Init request result rectangle (relative to parent window)
	public bool NavMoveSubmitted;                   // Move request submitted, will process result on next NewFrame()
	public bool NavMoveScoringItems;                // Move request submitted, still scoring incoming items
	public bool NavMoveForwardToNextFrame;
	public ImGuiNavMoveFlags NavMoveFlags;
	public ImGuiScrollFlags NavMoveScrollFlags;
	public ImGuiModFlags NavMoveKeyMods;
	public ImGuiDir NavMoveDir;                         // Direction of the move request (left/right/up/down)
	public ImGuiDir NavMoveDirForDebug;
	public ImGuiDir NavMoveClipDir;                     // FIXME-NAV: Describe the purpose of this better. Might want to rename?
	public ImRect NavScoringRect;                     // Rectangle used for scoring, in screen space. Based of window->NavRectRel[], modified for directional navigation scoring.
	public ImRect NavScoringNoClipRect;               // Some nav operations (such as PageUp/PageDown) enforce a region which clipper will attempt to always keep submitted
	public int NavScoringDebugCount;               // Metrics for debugging
	public int NavTabbingDir;                      // Generally -1 or +1, 0 when tabbing without a nav id
	public int NavTabbingCounter;                  // >0 when counting items for tabbing
	public ImGuiNavItemData NavMoveResultLocal;                 // Best move request candidate within NavWindow
	public ImGuiNavItemData NavMoveResultLocalVisible;          // Best move request candidate within NavWindow that are mostly visible (when using ImGuiNavMoveFlags_AlsoScoreVisibleSet flag)
	public ImGuiNavItemData NavMoveResultOther;                 // Best move request candidate within NavWindow's flattened hierarchy (when using ImGuiWindowFlags_NavFlattened flag)
	public ImGuiNavItemData NavTabbingResultFirst;              // First tabbing request candidate within NavWindow and flattened hierarchy

	// Navigation: Windowing (CTRL+TAB for list, or Menu button + keys or directional pads to move/resize)
	public ImGuiWindow* NavWindowingTarget;                 // Target window when doing CTRL+Tab (or Pad Menu + FocusPrev/Next), this window is temporarily displayed top-most!
	public ImGuiWindow* NavWindowingTargetAnim;             // Record of last valid NavWindowingTarget until DimBgRatio and NavWindowingHighlightAlpha becomes 0.0f, so the fade-out can stay on it.
	public ImGuiWindow* NavWindowingListWindow;             // Internal window actually listing the CTRL+Tab contents
	public float NavWindowingTimer;
	public float NavWindowingHighlightAlpha;
	public bool NavWindowingToggleLayer;

	// Render
	public float DimBgRatio;                         // 0.0..1.0 animation when fading in a dimming background (for modal window and CTRL+TAB list)
	public ImGuiMouseCursor MouseCursor;

	// Drag and Drop
	public bool DragDropActive;
	public bool DragDropWithinSource;               // Set when within a BeginDragDropXXX/EndDragDropXXX block for a drag source.
	public bool DragDropWithinTarget;               // Set when within a BeginDragDropXXX/EndDragDropXXX block for a drag target.
	public ImGuiDragDropFlags DragDropSourceFlags;
	public int DragDropSourceFrameCount;
	public int DragDropMouseButton;
	public ImGuiPayload DragDropPayload;
	public ImRect DragDropTargetRect;                 // Store rectangle of current target candidate (we favor small targets when overlapping)
	public ImGuiID DragDropTargetId;
	public ImGuiDragDropFlags DragDropAcceptFlags;
	public float DragDropAcceptIdCurrRectSurface;    // Target item surface (we resolve overlapping targets by prioritizing the smaller surface)
	public ImGuiID DragDropAcceptIdCurr;               // Target item id (set at the time of accepting the payload)
	public ImGuiID DragDropAcceptIdPrev;               // Target item id from previous frame (we need to store this to allow for overlapping drag and drop targets)
	public int DragDropAcceptFrameCount;           // Last time a target expressed a desire to accept the source
	public ImGuiID DragDropHoldJustPressedId;          // Set when holding a payload just made ButtonBehavior() return a press.
	public ImVector<byte> DragDropPayloadBufHeap;             // We don't expose the ImVector<> directly, ImGuiPayload only holds pointer+size
	public fixed byte DragDropPayloadBufLocal[16];        // Local buffer for small payloads

	// Clipper
	public int ClipperTempDataStacked;
	public ImVector<ImGuiListClipperData> ClipperTempData;

	// Tables
	public ImGuiTablePtr CurrentTable;
	public int TablesTempDataStacked;      // Temporary table data size (because we leave previous instances undestructed, we generally don't use TablesTempData.Size)
	public ImVector<ImGuiTableTempData> TablesTempData;             // Temporary table data (buffers reused/shared across instances, support nesting)
	public ImPool<ImGuiTable> Tables;                     // Persistent table data
	public ImVector<float> TablesLastTimeActive;       // Last used timestamp of each tables (SOA, for efficient GC)
	public ImVector<ImDrawChannel> DrawChannelsTempMergeBuffer;

	// Tab bars
	public ImGuiTabBar* CurrentTabBar;
	public ImPool<ImGuiTabBar> TabBars;
	public ImVector<ImGuiPtrOrIndex> CurrentTabBarStack;
	public ImVector<ImGuiShrinkWidthItem> ShrinkWidthBuffer;

	// Widget state
	public ImVec2 MouseLastValidPos;
	public ImGuiInputTextState InputTextState;
	public ImFont InputTextPasswordFont;
	public ImGuiID TempInputId;                        // Temporary text input when CTRL+clicking on a slider, etc.
	public ImGuiColorEditFlags ColorEditOptions;                   // Store user options for color edit widgets
	public float ColorEditLastHue;                   // Backup of last Hue associated to LastColor, so we can restore Hue in lossy RGB<>HSV round trips
	public float ColorEditLastSat;                   // Backup of last Saturation associated to LastColor, so we can restore Saturation in lossy RGB<>HSV round trips
	public ImU32 ColorEditLastColor;                 // RGB value with alpha set to 0.
	public ImVec4 ColorPickerRef;                     // Initial/reference color at the time of opening the color picker.
	public ImGuiComboPreviewData ComboPreviewData;
	public float SliderGrabClickOffset;
	public float SliderCurrentAccum;                 // Accumulated slider delta when using navigation controls.
	public bool SliderCurrentAccumDirty;            // Has the accumulated slider delta changed since last time we tried to apply it?
	public bool DragCurrentAccumDirty;
	public float DragCurrentAccum;                   // Accumulator for dragging modification. Always high-precision, not rounded by end-user precision settings
	public float DragSpeedDefaultRatio;              // If speed == 0.0f, uses (max-min) Ptr DragSpeedDefaultRatio
	public float ScrollbarClickDeltaToGrabCenter;    // Distance between mouse and center of grab box, normalized in parent space. Use storage?
	public float DisabledAlphaBackup;                // Backup for style.Alpha for BeginDisabled()
	public short DisabledStackSize;
	public short TooltipOverrideCount;
	public float TooltipSlowDelay;                   // Time before slow tooltips appears (FIXME: This is temporary until we merge in tooltip timer+priority work)
	public ImVector<char> ClipboardHandlerData;               // If no custom clipboard handler is defined
	public ImVector<ImGuiID> MenusIdSubmittedThisFrame;          // A list of menu IDs that were rendered at least once

	// Platform support
	public ImGuiPlatformImeData PlatformImeData;                    // Data updated by current frame
	public ImGuiPlatformImeData PlatformImeDataPrev;                // Previous frame data (when changing we will call io.SetPlatformImeDataFn
	public char PlatformLocaleDecimalPoint;         // '.' or Ptrlocaleconv()->decimal_point

	// Settings
	public bool SettingsLoaded;
	public float SettingsDirtyTimer;                 // Save .ini Settings to memory when time reaches zero
	public ImGuiTextBuffer SettingsIniData;                    // In memory .ini settings
	public ImVector<ImGuiSettingsHandler> SettingsHandlers;       // List of .ini settings handlers
	public ImChunkStream<ImGuiWindowSettings> SettingsWindows;        // ImGuiWindow .ini settings entries
	public ImChunkStream<ImGuiTableSettings> SettingsTables;         // ImGuiTable .ini settings entries
	public ImVector<ImGuiContextHook> Hooks;                  // Hooks for extensions (e.g. test engine)
	public ImGuiID HookIdNext;             // Next available HookId

	// Capture/Logging
	public bool LogEnabled;                         // Currently capturing
	public ImGuiLogType LogType;                            // Capture target
	public ImFileHandle LogFile;                            // If != NULL log to stdout/ file
	public ImGuiTextBuffer LogBuffer;                          // Accumulation buffer when log to clipboard. This is pointer so our GImGui static constructor doesn't call heap allocators.
	public IntPtr LogNextPrefix;
	public IntPtr LogNextSuffix;
	public float LogLinePosY;
	public bool LogLineFirstItem;
	public int LogDepthRef;
	public int LogDepthToExpand;
	public int LogDepthToExpandDefault;            // Default/stored value for LogDepthMaxExpand if not specified in the LogXXX function call.

	// Debug Tools
	public ImGuiDebugLogFlags DebugLogFlags;
	public ImGuiTextBuffer DebugLogBuf;
	public bool DebugItemPickerActive;              // Item picker is active (started with DebugStartItemPicker())
	public ImGuiID DebugItemPickerBreakId;             // Will call IM_DEBUG_BREAK() when encountering this ID
	public ImGuiMetricsConfig DebugMetricsConfig;
	public ImGuiStackTool DebugStackTool;

	// Misc
	public fixed float FramerateSecPerFrame[120];          // Calculate estimate of framerate for user over the last 2 seconds.
	public int FramerateSecPerFrameIdx;
	public int FramerateSecPerFrameCount;
	public float FramerateSecPerFrameAccum;
	public int WantCaptureMouseNextFrame;          // Explicit capture override via SetNextFrameWantCaptureMouse()/SetNextFrameWantCaptureKeyboard(). Default to -1.
	public int WantCaptureKeyboardNextFrame;       // "
	public int WantTextInputNextFrame;
	public ImVector<char> TempBuffer;                         // Temporary text buffer
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member