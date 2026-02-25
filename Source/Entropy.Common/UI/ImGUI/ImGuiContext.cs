#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using System.Runtime.InteropServices;
using ImGuiNET;
using ImU32 = uint;
using ImGuiID = uint;
using ImFileHandle = System.IntPtr;
using Unity.Collections.LowLevel.Unsafe;

namespace Entropy.Common.UI.ImGUI;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ImGuiContext
{
	[MarshalAs(UnmanagedType.U1)]
	private bool _initialized;
	[MarshalAs(UnmanagedType.U1)]
	private bool _fontAtlasOwnedByContext;
	private ImGuiIO _io;
	private ImVector<ImGuiInputEvent> _inputEventsQueue;
	private ImVector<ImGuiInputEvent> _inputEventsTrail;
	private ImGuiStyle _style;
	private ImFont* _font;
	private float _fontSize;
	private float _fontBaseSize;
	private ImDrawListSharedData _DrawListSharedData;
	private double _time;
	private int _frameCount;
	private int _frameCountEnded;
	private int _frameCountRendered;
	[MarshalAs(UnmanagedType.U1)]
	private bool _withinFrameScope;
	[MarshalAs(UnmanagedType.U1)]
	private bool _withinFrameScopeWithImplicitWindow;
	[MarshalAs(UnmanagedType.U1)]
	private bool _withinEndChild;
	[MarshalAs(UnmanagedType.U1)]
	private bool _gcCompactAll;
	[MarshalAs(UnmanagedType.U1)]
	private bool _testEngineHookItems;
	private nint _testEngine;
	private ImVector<ImGuiWindowPtr> _windows;
	private ImVector<ImGuiWindowPtr> _windowsFocusOrder;
	private ImVector<ImGuiWindowPtr> _windowsTempSortBuffer;
	private ImVector<ImGuiWindowStackData> _currentWindowStack;
	private ImGuiStorage _windowsById;
	private int _windowsActiveCount;
	private ImVec2 _windowsHoverPadding;
	private ImGuiWindow* _currentWindow;
	private ImGuiWindow* _hoveredWindow;
	private ImGuiWindow* _hoveredWindowUnderMovingWindow;
	private ImGuiWindow* _movingWindow;
	private ImGuiWindow* _wheelingWindow;
	private ImVec2 _wheelingWindowRefMousePos;
	private float _wheelingWindowTimer;
	private ImGuiID _debugHookIdInfo;
	private ImGuiID _hoveredId;
	private ImGuiID _hoveredIdPreviousFrame;
	[MarshalAs(UnmanagedType.U1)]
	private bool _hoveredIdAllowOverlap;
	[MarshalAs(UnmanagedType.U1)]
	private bool _hoveredIdUsingMouseWheel;
	[MarshalAs(UnmanagedType.U1)]
	private bool _hoveredIdPreviousFrameUsingMouseWheel;
	[MarshalAs(UnmanagedType.U1)]
	private bool _hoveredIdDisabled;
	private float _hoveredIdTimer;
	private float _hoveredIdNotActiveTimer;
	private ImGuiID _activeId;
	private ImGuiID _activeIdIsAlive;
	private float _activeIdTimer;
	[MarshalAs(UnmanagedType.U1)]
	private bool _activeIdIsJustActivated;
	[MarshalAs(UnmanagedType.U1)]
	private bool _activeIdAllowOverlap;
	[MarshalAs(UnmanagedType.U1)]
	private bool _activeIdNoClearOnFocusLoss;
	[MarshalAs(UnmanagedType.U1)]
	private bool _activeIdHasBeenPressedBefore;
	[MarshalAs(UnmanagedType.U1)]
	private bool _activeIdHasBeenEditedBefore;
	[MarshalAs(UnmanagedType.U1)]
	private bool _activeIdHasBeenEditedThisFrame;
	private ImVec2 _activeIdClickOffset;
	private ImGuiWindow* _activeIdWindow;
	private ImGuiInputSource _activeIdSource;
	private int _activeIdMouseButton;
	private ImGuiID _activeIdPreviousFrame;
	[MarshalAs(UnmanagedType.U1)]
	private bool _activeIdPreviousFrameIsAlive;
	[MarshalAs(UnmanagedType.U1)]
	private bool _activeIdPreviousFrameHasBeenEditedBefore;
	private ImGuiWindow* _activeIdPreviousFrameWindow;
	private ImGuiID _lastActiveId;
	private float _lastActiveIdTimer;
	[MarshalAs(UnmanagedType.U1)]
	private bool _activeIdUsingMouseWheel;
	private ImU32 _activeIdUsingNavDirMask;
	private ImU32 _activeIdUsingNavInputMask;
	private ImBitArrayForNamedKeys _activeIdUsingKeyInputMask;
	private ImGuiItemFlags _currentItemFlags;
	private ImGuiNextItemData _nextItemData;
	private ImGuiLastItemData _lastItemData;
	private ImGuiNextWindowData _nextWindowData;
	private ImVector<ImGuiColorMod> _colorStack;
	private ImVector<ImGuiStyleMod> _styleVarStack;
	private ImVector<ImFontPtr> _fontStack;
	private ImVector<ImGuiID> _focusScopeStack;
	private ImVector<ImGuiItemFlags> _itemFlagsStack;
	private ImVector<ImGuiGroupData> _groupStack;
	private ImVector<ImGuiPopupData> _openPopupStack;
	private ImVector<ImGuiPopupData> _beginPopupStack;
	private int _beginMenuCount;
	private ImVector<ImGuiViewportPPtr> _viewports;
	private ImGuiWindow* _navWindow;
	private ImGuiID _navId;
	private ImGuiID _navFocusScopeId;
	private ImGuiID _navActivateId;
	private ImGuiID _navActivateDownId;
	private ImGuiID _navActivatePressedId;
	private ImGuiID _navActivateInputId;
	private ImGuiActivateFlags _navActivateFlags;
	private ImGuiID _navJustMovedToId;
	private ImGuiID _navJustMovedToFocusScopeId;
	private ImGuiModFlags _navJustMovedToKeyMods;
	private ImGuiID _navNextActivateId;
	private ImGuiActivateFlags _navNextActivateFlags;
	private ImGuiInputSource _navInputSource;
	private ImGuiNavLayer _navLayer;
	[MarshalAs(UnmanagedType.U1)]
	private bool _navIdIsAlive;
	[MarshalAs(UnmanagedType.U1)]
	private bool _navMousePosDirty;
	[MarshalAs(UnmanagedType.U1)]
	private bool _navDisableHighlight;
	[MarshalAs(UnmanagedType.U1)]
	private bool _navDisableMouseHover;
	[MarshalAs(UnmanagedType.U1)]
	private bool _navAnyRequest;
	[MarshalAs(UnmanagedType.U1)]
	private bool _navInitRequest;
	[MarshalAs(UnmanagedType.U1)]
	private bool _navInitRequestFromMove;
	private ImGuiID _navInitResultId;
	private ImRect _navInitResultRectRel;
	[MarshalAs(UnmanagedType.U1)]
	private bool _navMoveSubmitted;
	[MarshalAs(UnmanagedType.U1)]
	private bool _navMoveScoringItems;
	[MarshalAs(UnmanagedType.U1)]
	private bool _navMoveForwardToNextFrame;
	private ImGuiNavMoveFlags _navMoveFlags;
	private ImGuiScrollFlags _navMoveScrollFlags;
	private ImGuiModFlags _navMoveKeyMods;
	private ImGuiDir _navMoveDir;
	private ImGuiDir _navMoveDirForDebug;
	private ImGuiDir _navMoveClipDir;
	private ImRect _navScoringRect;
	private ImRect _navScoringNoClipRect;
	private int _navScoringDebugCount;
	private int _navTabbingDir;
	private int _navTabbingCounter;
	private ImGuiNavItemData _navMoveResultLocal;
	private ImGuiNavItemData _navMoveResultLocalVisible;
	private ImGuiNavItemData _navMoveResultOther;
	private ImGuiNavItemData _navTabbingResultFirst;
	private ImGuiWindow* _navWindowingTarget;
	private ImGuiWindow* _navWindowingTargetAnim;
	private ImGuiWindow* _navWindowingListWindow;
	private float _navWindowingTimer;
	private float _navWindowingHighlightAlpha;
	[MarshalAs(UnmanagedType.U1)]
	bool _navWindowingToggleLayer;
	private float _dimBgRatio;
	private ImGuiMouseCursor _mouseCursor;
	[MarshalAs(UnmanagedType.U1)]
	private bool _dragDropActive;
	[MarshalAs(UnmanagedType.U1)]
	private bool _dragDropWithinSource;
	[MarshalAs(UnmanagedType.U1)]
	private bool _dragDropWithinTarget;
	private ImGuiDragDropFlags _dragDropSourceFlags;
	private int _dragDropSourceFrameCount;
	private int _dragDropMouseButton;
	private ImGuiPayload _dragDropPayload;
	private ImRect _dragDropTargetRect;
	private ImGuiID _dragDropTargetId;
	private ImGuiDragDropFlags _dragDropAcceptFlags;
	private float _dragDropAcceptIdCurrRectSurface;
	private ImGuiID _dragDropAcceptIdCurr;
	private ImGuiID _dragDropAcceptIdPrev;
	private int _dragDropAcceptFrameCount;
	private ImGuiID _dragDropHoldJustPressedId;
	private ImVector<byte> _dragDropPayloadBufHeap;
	private fixed byte _dragDropPayloadBufLocal[16];
	private int _clipperTempDataStacked;
	private ImVector<ImGuiListClipperData> _clipperTempData;
	private ImGuiTable* _currentTable;
	private int _tablesTempDataStacked;
	private ImVector<ImGuiTableTempData> _tablesTempData;
	private ImPool<ImGuiTable> _tables;
	private ImVector<float> _tablesLastTimeActive;
	private ImVector<ImDrawChannel> _drawChannelsTempMergeBuffer;
	private ImGuiTabBar* _currentTabBar;
	private ImPool<ImGuiTabBar> _tabBars;
	private ImVector<ImGuiPtrOrIndex> _currentTabBarStack;
	private ImVector<ImGuiShrinkWidthItem> _shrinkWidthBuffer;
	private ImVec2 _mouseLastValidPos;
	private ImGuiInputTextState _inputTextState;
	private ImFont _inputTextPasswordFont;
	private ImGuiID _tempInputId;
	private ImGuiColorEditFlags _colorEditOptions;
	private float _colorEditLastHue;
	private float _colorEditLastSat;
	private ImU32 _colorEditLastColor;
	private ImVec4 _colorPickerRef;
	private ImGuiComboPreviewData _comboPreviewData;
	private float _sliderGrabClickOffset;
	private float _sliderCurrentAccum;
	[MarshalAs(UnmanagedType.U1)]
	private bool _sliderCurrentAccumDirty;
	[MarshalAs(UnmanagedType.U1)]
	private bool _dragCurrentAccumDirty;
	private float _dragCurrentAccum;
	private float _dragSpeedDefaultRatio;
	private float _scrollbarClickDeltaToGrabCenter;
	private float _disabledAlphaBackup;
	private short _disabledStackSize;
	private short _tooltipOverrideCount;
	private float _tooltipSlowDelay;
	private ImVector<byte> _clipboardHandlerData;
	private ImVector<ImGuiID> _menusIdSubmittedThisFrame;
	private ImGuiPlatformImeData _platformImeData;
	private ImGuiPlatformImeData _platformImeDataPrev;
	private byte _platformLocaleDecimalPoint;
	[MarshalAs(UnmanagedType.U1)]
	private bool _settingsLoaded;
	private float _settingsDirtyTimer;
	private ImGuiTextBuffer _settingsIniData;
	private ImVector<ImGuiSettingsHandler> _settingsHandlers;
	private ImChunkStream<ImGuiWindowSettings> _settingsWindows;
	private ImChunkStream<ImGuiTableSettings> _settingsTables;
	private ImVector<ImGuiContextHook> _hooks;
	private ImGuiID _hookIdNext;
	[MarshalAs(UnmanagedType.U1)]
	private bool _logEnabled;
	private ImGuiLogType _logType;
	private ImFileHandle _logFile;
	private ImGuiTextBuffer _logBuffer;
	private byte* _logNextPrefix;
	private byte* _logNextSuffix;
	private float _logLinePosY;
	[MarshalAs(UnmanagedType.U1)]
	private bool _logLineFirstItem;
	private int _logDepthRef;
	private int _logDepthToExpand;
	private int _logDepthToExpandDefault;
	private ImGuiDebugLogFlags _debugLogFlags;
	private ImGuiTextBuffer _debugLogBuf;
	[MarshalAs(UnmanagedType.U1)]
	private bool _debugItemPickerActive;
	private ImGuiID _debugItemPickerBreakId;
	private ImGuiMetricsConfig _debugMetricsConfig;
	private ImGuiStackTool _debugStackTool;
	private fixed float _framerateSecPerFrame[120];
	private int _framerateSecPerFrameIdx;
	private int _framerateSecPerFrameCount;
	private float _framerateSecPerFrameAccum;
	private int _wantCaptureMouseNextFrame;
	private int _wantCaptureKeyboardNextFrame;
	private int _wantTextInputNextFrame;
	private ImVector<byte> _tempBuffer;

	public ref bool Initialized => ref this._initialized;

	/// <summary>
	/// IO.Fonts-> is owned by the ImGuiContext and will be destructed along with it.
	/// </summary>
	public ref bool FontAtlasOwnedByContext => ref this._fontAtlasOwnedByContext;

	public ref ImGuiIO IO => ref this._io;

	/// <summary>
	/// Input events which will be tricked/written into IO structure.
	/// </summary>
	public ref ImVector<ImGuiInputEvent> InputEventsQueue => ref this._inputEventsQueue;

	/// <summary>
	/// Past input events processed in NewFrame(). This is to allow domain-specific application to access e.g mouse/pen trail.
	/// </summary>
	public ref ImVector<ImGuiInputEvent> InputEventsTrail => ref this._inputEventsTrail;

	public ref ImGuiStyle Style => ref this._style;

	/// <summary>
	/// (Shortcut) == FontStack.empty() ? IO.Font : FontStack.back()
	/// </summary>
	public ref ImFont Font => ref ImGuiHelper.ValidatePointer(this._font);
	public bool HasFont => this._font != null;
	public void SetFont(ref ImFont font) => this._font = ImGuiHelper.GetPointer(ref font);
	public void ClearFont() => this._font = null;


	/// <summary>
	/// (Shortcut) == FontBaseSize Ptr g.CurrentWindow->FontWindowScale == window->FontSize(). Text height for current window.
	/// </summary>
	public ref float FontSize => ref this._fontSize;

	/// <summary>
	/// (Shortcut) == IO.FontGlobalScale Ptr Font->Scale Ptr Font->FontSize. Base text height.
	/// </summary>
	public ref float FontBaseSize => ref this._fontBaseSize;

	public ref ImDrawListSharedData DrawListSharedData => ref this._DrawListSharedData;

	public ref double Time => ref this._time;

	public ref int FrameCount => ref this._frameCount;

	public ref int FrameCountEnded => ref this._frameCountEnded;

	public ref int FrameCountRendered => ref this._frameCountRendered;

	/// <summary>
	/// Set by NewFrame(), cleared by EndFrame()
	/// </summary>
	public ref bool WithinFrameScope => ref this._withinFrameScope;

	/// <summary>
	/// Set by NewFrame(), cleared by EndFrame() when the implicit debug window has been pushed
	/// </summary>
	public ref bool WithinFrameScopeWithImplicitWindow => ref this._withinFrameScopeWithImplicitWindow;

	/// <summary>
	/// Set within EndChild()
	/// </summary>
	public ref bool WithinEndChild => ref this._withinEndChild;

	/// <summary>
	/// Request full GC
	/// </summary>
	public ref bool GcCompactAll => ref this._gcCompactAll;

	/// <summary>
	/// Will call test engine hooks: ImGuiTestEngineHook_ItemAdd(), ImGuiTestEngineHook_ItemInfo(), ImGuiTestEngineHook_Log()
	/// </summary>
	public ref bool TestEngineHookItems => ref this._testEngineHookItems;

	/// <summary>
	/// Test engine user data
	/// </summary>
	public ref nint TestEngine => ref this._testEngine;

	// Windows state
	/// <summary>
	/// Windows, sorted in display order, back to front
	/// </summary>
	public ref ImVector<ImGuiWindowPtr> Windows => ref this._windows;

	/// <summary>
	/// Root windows, sorted in focus order, back to front.
	/// </summary>
	public ref ImVector<ImGuiWindowPtr> WindowsFocusOrder => ref this._windowsFocusOrder;

	/// <summary>
	/// Temporary buffer used in EndFrame() to reorder windows so parents are kept before their child
	/// </summary>
	public ref ImVector<ImGuiWindowPtr> WindowsTempSortBuffer => ref this._windowsTempSortBuffer;

	public ref ImVector<ImGuiWindowStackData> CurrentWindowStack => ref this._currentWindowStack;

	/// <summary>
	/// Map window's ImGuiID to ImGuiWindowPtr
	/// </summary>
	public ref ImGuiStorage WindowsById => ref this._windowsById;

	/// <summary>
	/// Number of unique windows submitted by frame
	/// </summary>
	public ref int WindowsActiveCount => ref this._windowsActiveCount;

	/// <summary>
	/// Padding around resizable windows for which hovering on counts as hovering the window == ImMax(style.TouchExtraPadding, WINDOWS_HOVER_PADDING)
	/// </summary>
	public ref ImVec2 WindowsHoverPadding => ref this._windowsHoverPadding;

	/// <summary>
	/// Window being drawn into
	/// </summary>
	public ref ImGuiWindow CurrentWindow => ref ImGuiHelper.ValidatePointer(this._currentWindow);
	public bool HasCurrentWindow => this._currentWindow != null;
	public void SetCurrentWindow(ref ImGuiWindow window) => this._currentWindow = ImGuiHelper.GetPointer(ref window);
	public void ClearCurrentWindow() => this._currentWindow = null;

	/// <summary>
	/// Window the mouse is hovering. Will typically catch mouse inputs.
	/// </summary>
	public ref ImGuiWindow HoveredWindow => ref ImGuiHelper.ValidatePointer(this._hoveredWindow);
	public bool HasHoveredWindow => this._hoveredWindow != null;
	public void SetHoveredWindow(ref ImGuiWindow window) => this._hoveredWindow = ImGuiHelper.GetPointer(ref window);
	public void ClearHoveredWindow() => this._hoveredWindow = null;

	/// <summary>
	/// Hovered window ignoring MovingWindow. Only set if MovingWindow is set.
	/// </summary>
	public ref ImGuiWindow HoveredWindowUnderMovingWindow => ref ImGuiHelper.ValidatePointer(this._hoveredWindowUnderMovingWindow);

	/// <summary>
	/// Track the window we clicked on (in order to preserve focus). The actual window that is moved is generally MovingWindow->RootWindow.
	/// </summary>
	public ref ImGuiWindow MovingWindow => ref ImGuiHelper.ValidatePointer(this._movingWindow);
	public bool HasMovingWindow => this._movingWindow != null;
	public void SetMovingWindow(ref ImGuiWindow window) => this._movingWindow = ImGuiHelper.GetPointer(ref window);
	public void ClearMovingWindow() => this._movingWindow = null;

	/// <summary>
	/// Track the window we started mouse-wheeling on. Until a timer elapse or mouse has moved, generally keep scrolling the same window even if during the course of scrolling the mouse ends up hovering a child window.
	/// </summary>
	public ref ImGuiWindow WheelingWindow => ref ImGuiHelper.ValidatePointer(this._wheelingWindow);
	public bool HasWheelingWindow => this._wheelingWindow != null;
	public void SetWheelingWindow(ref ImGuiWindow window) => this._wheelingWindow = ImGuiHelper.GetPointer(ref window);
	public void ClearWheelingWindow() => this._wheelingWindow = null;

	public ref ImVec2 WheelingWindowRefMousePos => ref this._wheelingWindowRefMousePos;

	public ref float WheelingWindowTimer => ref this._wheelingWindowTimer;

	// Item/widgets state and tracking information
	/// <summary>
	/// Will call core hooks: DebugHookIdInfo() from GetID functions, used by Stack Tool [next HoveredId/ActiveId to not pull in an extra cache-line]
	/// </summary>
	public ref ImGuiID DebugHookIdInfo => ref this._debugHookIdInfo;

	/// <summary>
	/// Hovered widget, filled during the frame
	/// </summary>
	public ref ImGuiID HoveredId => ref this._hoveredId;

	public ref ImGuiID HoveredIdPreviousFrame => ref this._hoveredIdPreviousFrame;

	public ref bool HoveredIdAllowOverlap => ref this._hoveredIdAllowOverlap;

	/// <summary>
	/// Hovered widget will use mouse wheel. Blocks scrolling the underlying window.
	/// </summary>
	public ref bool HoveredIdUsingMouseWheel => ref this._hoveredIdUsingMouseWheel;

	public ref bool HoveredIdPreviousFrameUsingMouseWheel => ref this._hoveredIdPreviousFrameUsingMouseWheel;

	/// <summary>
	/// At least one widget passed the rect test, but has been discarded by disabled flag or popup inhibit. May be true even if HoveredId == 0.
	/// </summary>
	public ref bool HoveredIdDisabled => ref this._hoveredIdDisabled;

	/// <summary>
	/// Measure contiguous hovering time
	/// </summary>
	public ref float HoveredIdTimer => ref this._hoveredIdTimer;

	/// <summary>
	/// Measure contiguous hovering time where the item has not been active
	/// </summary>
	public ref float HoveredIdNotActiveTimer => ref this._hoveredIdNotActiveTimer;

	/// <summary>
	/// Active widget
	/// </summary>
	public ref ImGuiID ActiveId => ref this._activeId;

	/// <summary>
	/// Active widget has been seen this frame (we can't use a bool as the ActiveId may change within the frame)
	/// </summary>
	public ref ImGuiID ActiveIdIsAlive => ref this._activeIdIsAlive;

	public ref float ActiveIdTimer => ref this._activeIdTimer;

	/// <summary>
	/// Set at the time of activation for one frame
	/// </summary>
	public ref bool ActiveIdIsJustActivated => ref this._activeIdIsJustActivated;

	/// <summary>
	/// Active widget allows another widget to steal active id (generally for overlapping widgets, but not always)
	/// </summary>
	public ref bool ActiveIdAllowOverlap => ref this._activeIdAllowOverlap;

	/// <summary>
	/// Disable losing active id if the active id window gets unfocused.
	/// </summary>
	public ref bool ActiveIdNoClearOnFocusLoss => ref this._activeIdNoClearOnFocusLoss;

	/// <summary>
	/// Track whether the active id led to a press (this is to allow changing between PressOnClick and PressOnRelease without pressing twice). Used by range_select branch.
	/// </summary>
	public ref bool ActiveIdHasBeenPressedBefore => ref this._activeIdHasBeenPressedBefore;

	/// <summary>
	/// Was the value associated to the widget Edited over the course of the Active state.
	/// </summary>
	public ref bool ActiveIdHasBeenEditedBefore => ref this._activeIdHasBeenEditedBefore;

	public ref bool ActiveIdHasBeenEditedThisFrame => ref this._activeIdHasBeenEditedThisFrame;

	/// <summary>
	/// Clicked offset from upper-left corner, if applicable (currently only set by ButtonBehavior)
	/// </summary>
	public ref ImVec2 ActiveIdClickOffset => ref this._activeIdClickOffset;

	public ref ImGuiWindow ActiveIdWindow => ref ImGuiHelper.ValidatePointer(this._activeIdWindow);
	/// <summary>
	/// Activating with mouse or nav (gamepad/keyboard)
	/// </summary>
	public ref ImGuiInputSource ActiveIdSource => ref this._activeIdSource;

	public ref int ActiveIdMouseButton => ref this._activeIdMouseButton;

	public ref ImGuiID ActiveIdPreviousFrame => ref this._activeIdPreviousFrame;

	public ref bool ActiveIdPreviousFrameIsAlive => ref this._activeIdPreviousFrameIsAlive;

	public ref bool ActiveIdPreviousFrameHasBeenEditedBefore => ref this._activeIdPreviousFrameHasBeenEditedBefore;

	public ref ImGuiWindow ActiveIdPreviousFrameWindow => ref ImGuiHelper.ValidatePointer(this._activeIdPreviousFrameWindow);
	public bool HasActiveIdPreviousFrameWindow => this._activeIdPreviousFrameWindow != null;
	public void SetActiveIdPreviousFrameWindow(ref ImGuiWindow window) => this._activeIdPreviousFrameWindow = ImGuiHelper.GetPointer(ref window);
	public void ClearActiveIdPreviousFrameWindow() => this._activeIdPreviousFrameWindow = null;

	/// <summary>
	/// Store the last non-zero ActiveId, useful for animation.
	/// </summary>
	public ref ImGuiID LastActiveId => ref this._lastActiveId;

	/// <summary>
	/// Store the last non-zero ActiveId timer since the beginning of activation, useful for animation.
	/// </summary>
	public ref float LastActiveIdTimer => ref this._lastActiveIdTimer;

	// Input Ownership
	/// <summary>
	/// Active widget will want to read mouse wheel. Blocks scrolling the underlying window.
	/// </summary>
	public ref bool ActiveIdUsingMouseWheel => ref this._activeIdUsingMouseWheel;

	/// <summary>
	/// Active widget will want to read those nav move requests (e.g. can activate a button and move away from it)
	/// </summary>
	public ref ImU32 ActiveIdUsingNavDirMask => ref this._activeIdUsingNavDirMask;

	/// <summary>
	/// Active widget will want to read those nav inputs.
	/// </summary>
	public ref ImU32 ActiveIdUsingNavInputMask => ref this._activeIdUsingNavInputMask;

	/// <summary>
	/// Active widget will want to read those key inputs. When we grow the ImGuiKey enum we'll need to either to order the enum to make useful keys come first, either redesign this into e.g. a small array.
	/// </summary>
	public ref ImBitArrayForNamedKeys ActiveIdUsingKeyInputMask => ref this._activeIdUsingKeyInputMask;

	// Next window/item data
	/// <summary>
	/// == g.ItemFlagsStack.back()
	/// </summary>
	public ref ImGuiItemFlags CurrentItemFlags => ref this._currentItemFlags;

	/// <summary>
	/// Storage for SetNextItemPtrPtr functions
	/// </summary>
	public ref ImGuiNextItemData NextItemData => ref this._nextItemData;

	/// <summary>
	/// Storage for last submitted item (setup by ItemAdd)
	/// </summary>
	public ref ImGuiLastItemData LastItemData => ref this._lastItemData;

	/// <summary>
	/// Storage for SetNextWindowPtrPtr functions
	/// </summary>
	public ref ImGuiNextWindowData NextWindowData => ref this._nextWindowData;

	// Shared stacks
	/// <summary>
	/// Stack for PushStyleColor()/PopStyleColor() - inherited by Begin()
	/// </summary>
	public ref ImVector<ImGuiColorMod> ColorStack => ref this._colorStack;

	/// <summary>
	/// Stack for PushStyleVar()/PopStyleVar() - inherited by Begin()
	/// </summary>
	public ref ImVector<ImGuiStyleMod> StyleVarStack => ref this._styleVarStack;

	/// <summary>
	/// Stack for PushFont()/PopFont() - inherited by Begin()
	/// </summary>
	public ref ImVector<ImFontPtr> FontStack => ref this._fontStack;

	/// <summary>
	/// Stack for PushFocusScope()/PopFocusScope() - not inherited by Begin(), unless child window
	/// </summary>
	public ref ImVector<ImGuiID> FocusScopeStack => ref this._focusScopeStack;

	/// <summary>
	/// Stack for PushItemFlag()/PopItemFlag() - inherited by Begin()
	/// </summary>
	public ref ImVector<ImGuiItemFlags> ItemFlagsStack => ref this._itemFlagsStack;

	/// <summary>
	/// Stack for BeginGroup()/EndGroup() - not inherited by Begin()
	/// </summary>
	public ref ImVector<ImGuiGroupData> GroupStack => ref this._groupStack;

	/// <summary>
	/// Which popups are open (persistent)
	/// </summary>
	public ref ImVector<ImGuiPopupData> OpenPopupStack => ref this._openPopupStack;

	/// <summary>
	/// Which level of BeginPopup() we are in (reset every frame)
	/// </summary>
	public ref ImVector<ImGuiPopupData> BeginPopupStack => ref this._beginPopupStack;

	public ref int BeginMenuCount => ref this._beginMenuCount;

	// Viewports
	/// <summary>
	/// Active viewports (Size==1 in 'master' branch). Each viewports hold their copy of ImDrawData.
	/// </summary>
	public ref ImVector<ImGuiViewportPPtr> Viewports => ref this._viewports;

	// Gamepad/keyboard Navigation
	/// <summary>
	/// Focused window for navigation. Could be called 'FocusedWindow'
	/// </summary>
	public ref ImGuiWindow NavWindow => ref ImGuiHelper.ValidatePointer(this._navWindow);

	/// <summary>
	/// Focused item for navigation
	/// </summary>
	public ref ImGuiID NavId => ref this._navId;

	/// <summary>
	/// Identify a selection scope (selection code often wants to "clear other items" when landing on an item of the selection set)
	/// </summary>
	public ref ImGuiID NavFocusScopeId => ref this._navFocusScopeId;

	/// <summary>
	/// ~~ (g.ActiveId == 0) && IsNavInputPressed(ImGuiNavInput_Activate) ? NavId : 0, also set when calling ActivateItem()
	/// </summary>
	public ref ImGuiID NavActivateId => ref this._navActivateId;

	/// <summary>
	/// ~~ IsNavInputDown(ImGuiNavInput_Activate) ? NavId : 0
	/// </summary>
	public ref ImGuiID NavActivateDownId => ref this._navActivateDownId;

	/// <summary>
	/// ~~ IsNavInputPressed(ImGuiNavInput_Activate) ? NavId : 0
	/// </summary>
	public ref ImGuiID NavActivatePressedId => ref this._navActivatePressedId;

	/// <summary>
	/// ~~ IsNavInputPressed(ImGuiNavInput_Input) ? NavId : 0; ImGuiActivateFlags_PreferInput will be set and NavActivateId will be 0.
	/// </summary>
	public ref ImGuiID NavActivateInputId => ref this._navActivateInputId;

	public ref ImGuiActivateFlags NavActivateFlags => ref this._navActivateFlags;

	/// <summary>
	/// Just navigated to this id (result of a successfully MoveRequest).
	/// </summary>
	public ref ImGuiID NavJustMovedToId => ref this._navJustMovedToId;

	/// <summary>
	/// Just navigated to this focus scope id (result of a successfully MoveRequest).
	/// </summary>
	public ref ImGuiID NavJustMovedToFocusScopeId => ref this._navJustMovedToFocusScopeId;

	public ref ImGuiModFlags NavJustMovedToKeyMods => ref this._navJustMovedToKeyMods;

	/// <summary>
	/// Set by ActivateItem(), queued until next frame.
	/// </summary>
	public ref ImGuiID NavNextActivateId => ref this._navNextActivateId;

	public ref ImGuiActivateFlags NavNextActivateFlags => ref this._navNextActivateFlags;

	/// <summary>
	/// Keyboard or Gamepad mode? THIS WILL ONLY BE None or NavGamepad or NavKeyboard.
	/// </summary>
	public ref ImGuiInputSource NavInputSource => ref this._navInputSource;

	/// <summary>
	/// Layer we are navigating on. For now the system is hard-coded for 0=main contents and 1=menu/title bar, may expose layers later.
	/// </summary>
	public ref ImGuiNavLayer NavLayer => ref this._navLayer;

	/// <summary>
	/// Nav widget has been seen this frame ~~ NavRectRel is valid
	/// </summary>
	public ref bool NavIdIsAlive => ref this._navIdIsAlive;

	/// <summary>
	/// When set we will update mouse position if (io.ConfigFlags & ImGuiConfigFlags_NavEnableSetMousePos) if set (NB: this not enabled by default)
	/// </summary>
	public ref bool NavMousePosDirty => ref this._navMousePosDirty;

	/// <summary>
	/// When user starts using mouse, we hide gamepad/keyboard highlight (NB: but they are still available, which is why NavDisableHighlight isn't always != NavDisableMouseHover)
	/// </summary>
	public ref bool NavDisableHighlight => ref this._navDisableHighlight;

	/// <summary>
	/// When user starts using gamepad/keyboard, we hide mouse hovering highlight until mouse is touched again.
	/// </summary>
	public ref bool NavDisableMouseHover => ref this._navDisableMouseHover;

	// Navigation: Init & Move Requests
	/// <summary>
	/// ~~ NavMoveRequest || NavInitRequest this is to perform early out in ItemAdd()
	/// </summary>
	public ref bool NavAnyRequest => ref this._navAnyRequest;

	/// <summary>
	/// Init request for appearing window to select first item
	/// </summary>
	public ref bool NavInitRequest => ref this._navInitRequest;
	public ref bool NavInitRequestFromMove => ref this._navInitRequestFromMove;

	/// <summary>
	/// Init request result (first item of the window, or one for which SetItemDefaultFocus() was called)
	/// </summary>
	public ref ImGuiID NavInitResultId => ref this._navInitResultId;

	/// <summary>
	/// Init request result rectangle (relative to parent window)
	/// </summary>
	public ref ImRect NavInitResultRectRel => ref this._navInitResultRectRel;

	/// <summary>
	/// Move request submitted, will process result on next NewFrame()
	/// </summary>
	public ref bool NavMoveSubmitted => ref this._navMoveSubmitted;

	/// <summary>
	/// Move request submitted, still scoring incoming items
	/// </summary>
	public ref bool NavMoveScoringItems => ref this._navMoveScoringItems;

	public ref bool NavMoveForwardToNextFrame => ref this._navMoveForwardToNextFrame;

	public ref ImGuiNavMoveFlags NavMoveFlags => ref this._navMoveFlags;

	public ref ImGuiScrollFlags NavMoveScrollFlags => ref this._navMoveScrollFlags;

	public ref ImGuiModFlags NavMoveKeyMods => ref this._navMoveKeyMods;

	/// <summary>
	/// Direction of the move request (left/right/up/down)
	/// </summary>
	public ref ImGuiDir NavMoveDir => ref this._navMoveDir;

	public ref ImGuiDir NavMoveDirForDebug => ref this._navMoveDirForDebug;

	/// <summary>
	/// FIXME-NAV: Describe the purpose of this better. Might want to rename?
	/// </summary>
	public ref ImGuiDir NavMoveClipDir => ref this._navMoveClipDir;

	/// <summary>
	/// Rectangle used for scoring, in screen space. Based of window->NavRectRel[], modified for directional navigation scoring.
	/// </summary>
	public ref ImRect NavScoringRect => ref this._navScoringRect;

	/// <summary>
	/// Some nav operations (such as PageUp/PageDown) enforce a region which clipper will attempt to always keep submitted
	/// </summary>
	public ref ImRect NavScoringNoClipRect => ref this._navScoringNoClipRect;

	/// <summary>
	/// Metrics for debugging
	/// </summary>
	public ref int NavScoringDebugCount => ref this._navScoringDebugCount;

	/// <summary>
	/// Generally -1 or +1, 0 when tabbing without a nav id
	/// </summary>
	public ref int NavTabbingDir => ref this._navTabbingDir;

	/// <summary>
	/// >0 when counting items for tabbing
	/// </summary>
	public ref int NavTabbingCounter => ref this._navTabbingCounter;

	/// <summary>
	/// Best move request candidate within NavWindow
	/// </summary>
	public ref ImGuiNavItemData NavMoveResultLocal => ref this._navMoveResultLocal;

	/// <summary>
	/// Best move request candidate within NavWindow that are mostly visible (when using ImGuiNavMoveFlags_AlsoScoreVisibleSet flag)
	/// </summary>
	public ref ImGuiNavItemData NavMoveResultLocalVisible => ref this._navMoveResultLocalVisible;

	/// <summary>
	/// Best move request candidate within NavWindow's flattened hierarchy (when using ImGuiWindowFlags_NavFlattened flag)
	/// </summary>
	public ref ImGuiNavItemData NavMoveResultOther => ref this._navMoveResultOther;

	/// <summary>
	/// First tabbing request candidate within NavWindow and flattened hierarchy
	/// </summary>
	public ref ImGuiNavItemData NavTabbingResultFirst => ref this._navTabbingResultFirst;

	// Navigation: Windowing (CTRL+TAB for list, or Menu button + keys or directional pads to move/resize)
	/// <summary>
	/// Target window when doing CTRL+Tab (or Pad Menu + FocusPrev/Next), this window is temporarily displayed top-most!
	/// </summary>
	public ref ImGuiWindow NavWindowingTarget => ref ImGuiHelper.ValidatePointer(this._navWindowingTarget);
	public bool HasNavWindowingTarget => this._navWindowingTarget != null;
	public void SetNavWindowingTarget(ref ImGuiWindow window) => this._navWindowingTarget = ImGuiHelper.GetPointer(ref window);
	public void ClearNavWindowingTarget() => this._navWindowingTarget = null;

	/// <summary>
	/// Record of last valid NavWindowingTarget until DimBgRatio and NavWindowingHighlightAlpha becomes 0.0f, so the fade-out can stay on it.
	/// </summary>
	public ref ImGuiWindow NavWindowingTargetAnim => ref ImGuiHelper.ValidatePointer(this._navWindowingTargetAnim);
	public bool HasNavWindowingTargetAnim => this._navWindowingTargetAnim != null;
	public void SetNavWindowingTargetAnim(ref ImGuiWindow window) => this._navWindowingTargetAnim = ImGuiHelper.GetPointer(ref window);
	public void ClearNavWindowingTargetAnim() => this._navWindowingTargetAnim = null;

	/// <summary>
	/// Internal window actually listing the CTRL+Tab contents
	/// </summary>
	public ref ImGuiWindow NavWindowingListWindow => ref ImGuiHelper.ValidatePointer(this._navWindowingListWindow);
	public bool HasNavWindowingListWindow => this._navWindowingListWindow != null;
	public void SetNavWindowingListWindow(ref ImGuiWindow window) => this._navWindowingListWindow = ImGuiHelper.GetPointer(ref window);
	public void ClearNavWindowingListWindow() => this._navWindowingListWindow = null;

	public ref float NavWindowingTimer => ref this._navWindowingTimer;

	public ref float NavWindowingHighlightAlpha => ref this._navWindowingHighlightAlpha;

	public ref bool NavWindowingToggleLayer => ref this._navWindowingToggleLayer;

	// Render
	/// <summary>
	/// 0.0..1.0 animation when fading in a dimming background (for modal window and CTRL+TAB list)
	/// </summary>
	public ref float DimBgRatio => ref this._dimBgRatio;

	public ref ImGuiMouseCursor MouseCursor => ref this._mouseCursor;

	// Drag and Drop
	public ref bool DragDropActive => ref this._dragDropActive;

	/// <summary>
	/// Set when within a BeginDragDropXXX/EndDragDropXXX block for a drag source.
	/// </summary>
	public ref bool DragDropWithinSource => ref this._dragDropWithinSource;

	/// <summary>
	/// Set when within a BeginDragDropXXX/EndDragDropXXX block for a drag target.
	/// </summary>
	public ref bool DragDropWithinTarget => ref this._dragDropWithinTarget;

	public ref ImGuiDragDropFlags DragDropSourceFlags => ref this._dragDropSourceFlags;

	public ref int DragDropSourceFrameCount => ref this._dragDropSourceFrameCount;

	public ref int DragDropMouseButton => ref this._dragDropMouseButton;

	public ref ImGuiPayload DragDropPayload => ref this._dragDropPayload;

	/// <summary>
	/// Store rectangle of current target candidate (we favor small targets when overlapping)
	/// </summary>
	public ref ImRect DragDropTargetRect => ref this._dragDropTargetRect;

	public ref ImGuiID DragDropTargetId => ref this._dragDropTargetId;

	public ref ImGuiDragDropFlags DragDropAcceptFlags => ref this._dragDropAcceptFlags;

	/// <summary>
	/// Target item surface (we resolve overlapping targets by prioritizing the smaller surface)
	/// </summary>
	public ref float DragDropAcceptIdCurrRectSurface => ref this._dragDropAcceptIdCurrRectSurface;

	/// <summary>
	/// Target item id (set at the time of accepting the payload)
	/// </summary>
	public ref ImGuiID DragDropAcceptIdCurr => ref this._dragDropAcceptIdCurr;

	/// <summary>
	/// Target item id from previous frame (we need to store this to allow for overlapping drag and drop targets)
	/// </summary>
	public ref ImGuiID DragDropAcceptIdPrev => ref this._dragDropAcceptIdPrev;

	/// <summary>
	/// Last time a target expressed a desire to accept the source
	/// </summary>
	public ref int DragDropAcceptFrameCount => ref this._dragDropAcceptFrameCount;

	/// <summary>
	/// Set when holding a payload just made ButtonBehavior() return a press.
	/// </summary>
	public ref ImGuiID DragDropHoldJustPressedId => ref this._dragDropHoldJustPressedId;

	/// <summary>
	/// We don't expose the ImVector<> directly, ImGuiPayload only holds pointer+size
	/// </summary>
	public ref ImVector<byte> DragDropPayloadBufHeap => ref this._dragDropPayloadBufHeap;

	/// <summary>
	/// Local buffer for small payloads
	/// </summary>
	public Span<byte> DragDropPayloadBufLocal
	{
		get
		{
			fixed (byte* ptr = this._dragDropPayloadBufLocal)
			{
				return new Span<byte>(ptr, 16);
			}
		}
	}

	// Clipper
	public ref int ClipperTempDataStacked => ref this._clipperTempDataStacked;

	public ref ImVector<ImGuiListClipperData> ClipperTempData => ref this._clipperTempData;

	// Tables
	public ref ImGuiTable CurrentTable => ref ImGuiHelper.ValidatePointer(this._currentTable);
	public bool HasCurrentTable => this._currentTable != null;
	public void SetCurrentTable(ref ImGuiTable table) => this._currentTable = ImGuiHelper.GetPointer(ref table);
	public void ClearCurrentTable() => this._currentTable = null;

	/// <summary>
	/// Temporary table data size (because we leave previous instances undestructed, we generally don't use TablesTempData.Size)
	/// </summary>
	public ref int TablesTempDataStacked => ref this._tablesTempDataStacked;

	/// <summary>
	/// Temporary table data (buffers reused/shared across instances, support nesting)
	/// </summary>
	public ref ImVector<ImGuiTableTempData> TablesTempData => ref this._tablesTempData;

	/// <summary>
	/// Persistent table data
	/// </summary>
	public ref ImPool<ImGuiTable> Tables => ref this._tables;

	/// <summary>
	/// Last used timestamp of each tables (SOA, for efficient GC)
	/// </summary>
	public ref ImVector<float> TablesLastTimeActive => ref this._tablesLastTimeActive;

	public ref ImVector<ImDrawChannel> DrawChannelsTempMergeBuffer => ref this._drawChannelsTempMergeBuffer;

	// Tab bars
	public ref ImGuiTabBar CurrentTabBar => ref ImGuiHelper.ValidatePointer(this._currentTabBar);
	public bool HasCurrentTabBar => this._currentTabBar != null;
	public void SetCurrentTabBar(ref ImGuiTabBar tabBar) => this._currentTabBar = ImGuiHelper.GetPointer(ref tabBar);
	public void ClearCurrentTabBar() => this._currentTabBar = null;

	public ref ImPool<ImGuiTabBar> TabBars => ref this._tabBars;

	public ref ImVector<ImGuiPtrOrIndex> CurrentTabBarStack => ref this._currentTabBarStack;

	public ref ImVector<ImGuiShrinkWidthItem> ShrinkWidthBuffer => ref this._shrinkWidthBuffer;

	// Widget state
	public ref ImVec2 MouseLastValidPos => ref this._mouseLastValidPos;

	public ref ImGuiInputTextState InputTextState => ref this._inputTextState;

	public ref ImFont InputTextPasswordFont => ref this._inputTextPasswordFont;
	/// <summary>
	/// Temporary text input when CTRL+clicking on a slider, etc.
	/// </summary>
	public ref ImGuiID TempInputId => ref this._tempInputId;

	/// <summary>
	/// Store user options for color edit widgets
	/// </summary>
	public ref ImGuiColorEditFlags ColorEditOptions => ref this._colorEditOptions;

	/// <summary>
	/// Backup of last Hue associated to LastColor, so we can restore Hue in lossy RGB<>HSV round trips
	/// </summary>
	public ref float ColorEditLastHue => ref this._colorEditLastHue;

	/// <summary>
	/// Backup of last Saturation associated to LastColor, so we can restore Saturation in lossy RGB<>HSV round trips
	/// </summary>
	public ref float ColorEditLastSat => ref this._colorEditLastSat;

	/// <summary>
	/// RGB value with alpha set to 0.
	/// </summary>
	public ref ImU32 ColorEditLastColor => ref this._colorEditLastColor;

	/// <summary>
	/// Initial/reference color at the time of opening the color picker.
	/// </summary>
	public ref ImVec4 ColorPickerRef => ref this._colorPickerRef;

	public ref ImGuiComboPreviewData ComboPreviewData => ref this._comboPreviewData;

	public ref float SliderGrabClickOffset => ref this._sliderGrabClickOffset;

	/// <summary>
	/// Accumulated slider delta when using navigation controls.
	/// </summary>
	public ref float SliderCurrentAccum => ref this._sliderCurrentAccum;

	/// <summary>
	/// Has the accumulated slider delta changed since last time we tried to apply it?
	/// </summary>
	public ref bool SliderCurrentAccumDirty => ref this._sliderCurrentAccumDirty;

	public ref bool DragCurrentAccumDirty => ref this._dragCurrentAccumDirty;

	/// <summary>
	/// Accumulator for dragging modification. Always high-precision, not rounded by end-user precision settings
	/// </summary>
	public ref float DragCurrentAccum => ref this._dragCurrentAccum;

	/// <summary>
	/// If speed == 0.0f, uses (max-min) Ptr DragSpeedDefaultRatio
	/// </summary>
	public ref float DragSpeedDefaultRatio => ref this._dragSpeedDefaultRatio;

	/// <summary>
	/// Distance between mouse and center of grab box, normalized in parent space. Use storage?
	/// </summary>
	public ref float ScrollbarClickDeltaToGrabCenter => ref this._scrollbarClickDeltaToGrabCenter;

	/// <summary>
	/// Backup for style.Alpha for BeginDisabled()
	/// </summary>
	public ref float DisabledAlphaBackup => ref this._disabledAlphaBackup;

	public ref short DisabledStackSize => ref this._disabledStackSize;

	public ref short TooltipOverrideCount => ref this._tooltipOverrideCount;

	/// <summary>
	/// Time before slow tooltips appears (FIXME: This is temporary until we merge in tooltip timer+priority work)
	/// </summary>
	public ref float TooltipSlowDelay => ref this._tooltipSlowDelay;

	/// <summary>
	/// If no custom clipboard handler is defined
	/// </summary>
	public ref ImVector<byte> ClipboardHandlerData => ref this._clipboardHandlerData;

	/// <summary>
	/// A list of menu IDs that were rendered at least once
	/// </summary>
	public ref ImVector<ImGuiID> MenusIdSubmittedThisFrame => ref this._menusIdSubmittedThisFrame;

	// Platform support
	/// <summary>
	/// Data updated by current frame
	/// </summary>
	public ref ImGuiPlatformImeData PlatformImeData => ref this._platformImeData;

	/// <summary>
	/// Previous frame data (when changing we will call io.SetPlatformImeDataFn
	/// </summary>
	public ref ImGuiPlatformImeData PlatformImeDataPrev => ref this._platformImeDataPrev;

	/// <summary>
	/// '.' or Ptrlocaleconv()->decimal_point
	/// </summary>
	public ref byte PlatformLocaleDecimalPoint => ref this._platformLocaleDecimalPoint;

	// Settings
	public ref bool SettingsLoaded => ref this._settingsLoaded;

	/// <summary>
	/// Save .ini Settings to memory when time reaches zero
	/// </summary>
	public ref float SettingsDirtyTimer => ref this._settingsDirtyTimer;

	/// <summary>
	/// In memory .ini settings
	/// </summary>
	public ref ImGuiTextBuffer SettingsIniData => ref this._settingsIniData;

	/// <summary>
	/// List of .ini settings handlers
	/// </summary>
	public ref ImVector<ImGuiSettingsHandler> SettingsHandlers => ref this._settingsHandlers;

	/// <summary>
	/// ImGuiWindow .ini settings entries
	/// </summary>
	public ref ImChunkStream<ImGuiWindowSettings> SettingsWindows => ref this._settingsWindows;

	/// <summary>
	/// ImGuiTable .ini settings entries
	/// </summary>
	public ref ImChunkStream<ImGuiTableSettings> SettingsTables => ref this._settingsTables;

	/// <summary>
	/// Hooks for extensions (e.g. test engine)
	/// </summary>
	public ref ImVector<ImGuiContextHook> Hooks => ref this._hooks;

	/// <summary>
	/// Next available HookId
	/// </summary>
	public ref ImGuiID HookIdNext => ref this._hookIdNext;

	// Capture/Logging
	/// <summary>
	/// Currently capturing
	/// </summary>
	public ref bool LogEnabled => ref this._logEnabled;

	/// <summary>
	/// Capture target
	/// </summary>
	public ref ImGuiLogType LogType => ref this._logType;

	/// <summary>
	/// If != NULL log to stdout/ file
	/// </summary>
	public ref ImFileHandle LogFile => ref this._logFile;

	/// <summary>
	/// Accumulation buffer when log to clipboard. This is pointer so our GImGui static constructor doesn't call heap allocators.
	/// </summary>
	public ref ImGuiTextBuffer LogBuffer => ref this._logBuffer;

	public string? LogNextPrefix
	{
		get => this._logNextPrefix != null ? ImGuiHelper.GetString(this._logNextPrefix) : null;
		set => this._logNextPrefix = value != null ? ImGuiHelper.GetStringPointer(value) : null;
	}

	public string LogNextSuffix
	{
		get => this._logNextSuffix != null ? ImGuiHelper.GetString(this._logNextSuffix) : string.Empty;
		set => this._logNextSuffix = value != null ? ImGuiHelper.GetStringPointer(value) : null;
	}

	public ref float LogLinePosY => ref this._logLinePosY;

	public ref bool LogLineFirstItem => ref this._logLineFirstItem;

	public ref int LogDepthRef => ref this._logDepthRef;

	public ref int LogDepthToExpand => ref this._logDepthToExpand;

	/// <summary>
	/// Default/stored value for LogDepthMaxExpand if not specified in the LogXXX function call.
	/// </summary>
	public ref int LogDepthToExpandDefault => ref this._logDepthToExpandDefault;

	// Debug Tools
	public ref ImGuiDebugLogFlags DebugLogFlags => ref this._debugLogFlags;

	public ref ImGuiTextBuffer DebugLogBuf => ref this._debugLogBuf;

	/// <summary>
	/// Item picker is active (started with DebugStartItemPicker())
	/// </summary>
	public ref bool DebugItemPickerActive => ref this._debugItemPickerActive;

	/// <summary>
	/// Will call IM_DEBUG_BREAK() when encountering this ID
	/// </summary>
	public ref ImGuiID DebugItemPickerBreakId => ref this._debugItemPickerBreakId;

	public ref ImGuiMetricsConfig DebugMetricsConfig => ref this._debugMetricsConfig;

	public ref ImGuiStackTool DebugStackTool => ref this._debugStackTool;

	// Misc
	/// <summary>
	/// Calculate estimate of framerate for user over the last 2 seconds.
	/// </summary>
	public Span<float> FramerateSecPerFrame 
	{
		get
		{
			fixed (float* ptr = this._framerateSecPerFrame)
			{
				return new Span<float>(ptr, 120);
			}
		}
	}

	public ref int FramerateSecPerFrameIdx => ref this._framerateSecPerFrameIdx;

	public ref int FramerateSecPerFrameCount => ref this._framerateSecPerFrameCount;

	public ref float FramerateSecPerFrameAccum => ref this._framerateSecPerFrameAccum;

	/// <summary>
	/// Explicit capture override via SetNextFrameWantCaptureMouse()/SetNextFrameWantCaptureKeyboard(). Default to -1.
	/// </summary>
	public ref int WantCaptureMouseNextFrame => ref this._wantCaptureMouseNextFrame;

	public ref int WantCaptureKeyboardNextFrame => ref this._wantCaptureKeyboardNextFrame;

	public ref int WantTextInputNextFrame => ref this._wantTextInputNextFrame;

	/// <summary>
	/// Temporary text buffer
	/// </summary>
	public ref ImVector<byte> TempBuffer => ref this._tempBuffer;
}