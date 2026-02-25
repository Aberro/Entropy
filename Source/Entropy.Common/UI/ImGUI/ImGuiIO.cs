#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1823 // Avoid unused private fields
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImGuiNET;
using System;
using ImU16 = ushort;
using ImS8 = sbyte;
using System.Runtime.InteropServices;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiIO
{
	public const int SizeOfImGuiKeyData = 16;

	private ImGuiConfigFlags _configFlags;
	private ImGuiBackendFlags _backendFlags;
	private ImVec2 _displaySize;
	private float _deltaTime;
	private float _iniSavingRate;
	private byte* _iniFilename;
	private byte* _logFilename;
	private float _mouseDoubleClickTime;
	private float _mouseDoubleClickMaxDist;
	private float _mouseDragThreshold;
	private float _keyRepeatDelay;
	private float _keyRepeatRate;
	private void* _userData;
	private ImFontAtlas* _fonts;
	private float _fontGlobalScale;
	[MarshalAs(UnmanagedType.U1)]
	private bool _fontAllowUserScaling;
	private ImFont* _fontDefault;
	private ImVec2 _displayFramebufferScale;
	[MarshalAs(UnmanagedType.U1)]
	private bool _mouseDrawCursor;
	[MarshalAs(UnmanagedType.U1)]
	private bool _configMacOSXBehaviors;
	[MarshalAs(UnmanagedType.U1)]
	private bool _configInputTrickleEventQueue;
	[MarshalAs(UnmanagedType.U1)]
	private bool _configInputTextCursorBlink;
	[MarshalAs(UnmanagedType.U1)]
	private bool _configDragClickToInputText;
	[MarshalAs(UnmanagedType.U1)]
	private bool _configWindowsResizeFromEdges;
	[MarshalAs(UnmanagedType.U1)]
	private bool _configWindowsMoveFromTitleBarOnly;
	private float _configMemoryCompactTimer;
	private byte* _backendPlatformName;
	private byte* _backendRendererName;
	private void* _backendPlatformUserData;
	private void* _backendRendererUserData;
	private void* _backendLanguageUserData;
	private IntPtr _getClipboardTextFn;
	private IntPtr _setClipboardTextFn;
	private void* _clipboardUserData;
	private IntPtr _setPlatformImeDataFn;
	private IntPtr _unusedPadding;
	[MarshalAs(UnmanagedType.U1)]
	private bool _wantCaptureMouse;
	[MarshalAs(UnmanagedType.U1)]
	private bool _wantCaptureKeyboard;
	[MarshalAs(UnmanagedType.U1)]
	private bool _wantTextInput;
	[MarshalAs(UnmanagedType.U1)]
	private bool _wantSetMousePos;
	[MarshalAs(UnmanagedType.U1)]
	private bool _wantSaveIniSettings;
	[MarshalAs(UnmanagedType.U1)]
	private bool _navActive;
	[MarshalAs(UnmanagedType.U1)]
	private bool _navVisible;
	private float _framerate;
	private int _metricsRenderVertices;
	private int _metricsRenderIndices;
	private int _metricsRenderWindows;
	private int _metricsActiveWindows;
	private int _metricsActiveAllocations;
	private ImVec2 _mouseDelta;
	private fixed int _keyMap[(int)ImGuiKey.COUNT];
	private fixed bool _keysDown[(int)ImGuiKey.COUNT];
	private ImVec2 _mousePos;
	private fixed bool _mouseDown[5];
	private float _mouseWheel;
	private float _mouseWheelH;
	[MarshalAs(UnmanagedType.U1)]
	private bool _keyCtrl;
	[MarshalAs(UnmanagedType.U1)]
	private bool _keyShift;
	[MarshalAs(UnmanagedType.U1)]
	private bool _keyAlt;
	[MarshalAs(UnmanagedType.U1)]
	private bool _keySuper;
	private fixed float _navInputs[(int)ImGuiNavInput.COUNT];
	private ImGuiModFlags _keyMods;
	private fixed byte _keysData[SizeOfImGuiKeyData * (int)ImGuiKey.KeysData_SIZE];
	[MarshalAs(UnmanagedType.U1)]
	private bool _wantCaptureMouseUnlessPopupClose;
	private ImVec2 _mousePosPrev;
	private ImVec2 _mouseClickedPos0;
	private ImVec2 _mouseClickedPos1;
	private ImVec2 _mouseClickedPos2;
	private ImVec2 _mouseClickedPos3;
	private ImVec2 _mouseClickedPos4;
	private fixed double _mouseClickedTime[5];
	private fixed bool _mouseClicked[5];
	private fixed bool _mouseDoubleClicked[5];
	private fixed ImU16 _mouseClickedCount[5];
	private fixed ImU16 _mouseClickedLastCount[5];
	private fixed bool _mouseReleased[5];
	private fixed bool _mouseDownOwned[5];
	private fixed bool _mouseDownOwnedUnlessPopupClose[5];
	private fixed float _mouseDownDuration[5];
	private fixed float _mouseDownDurationPrev[5];
	private fixed float _mouseDragMaxDistanceSqr[5];
	private fixed float _navInputsDownDuration[(int)ImGuiNavInput.COUNT];
	private fixed float _navInputsDownDurationPrev[(int)ImGuiNavInput.COUNT];
	private float _penPressure;
	[MarshalAs(UnmanagedType.U1)]
	private bool _appFocusLost;
	[MarshalAs(UnmanagedType.U1)]
	private bool _appAcceptingEvents;
	private ImS8 _backendUsingLegacyKeyArrays;
	[MarshalAs(UnmanagedType.U1)]
	private bool _backendUsingLegacyNavInputArray;
	private byte _inputQueueSurrogate;
	private ImVector<byte> _inputQueueCharacters;

	//------------------------------------------------------------------
	// Configuration                            // Default value
	//------------------------------------------------------------------
	/// <summary>
	/// <0> See ImGuiConfigFlags_ enum. Set by user/application. Gamepad/keyboard navigation options, etc.
	/// </summary>
	public ref ImGuiConfigFlags ConfigFlags => ref this._configFlags;

	/// <summary>
	/// <0> See ImGuiBackendFlags_ enum. Set by backend (imgui_impl_xxx files or custom backend) to communicate features supported by the backend.
	/// </summary>
	public ref ImGuiBackendFlags BackendFlags => ref this._backendFlags;

	/// <summary>
	/// <unset> Main display size, in pixels (generally == GetMainViewport()->Size). May change every frame.
	/// </summary>
	public ref ImVec2 DisplaySize => ref this._displaySize;

	/// <summary>
	/// <1.0f/60.0f> Time elapsed since last frame, in seconds. May change every frame.
	/// </summary>
	public ref float DeltaTime => ref this._deltaTime;

	/// <summary>
	/// <5.0f> Minimum time between saving positions/sizes to .ini file, in seconds.
	/// </summary>
	public ref float IniSavingRate => ref this._iniSavingRate;

	/// <summary>
	/// <"imgui.ini"> Path to .ini file (important: default "imgui.ini" is relative to current working dir!). Set NULL to disable automatic .ini loading/saving or if you want to manually call LoadIniSettingsXXX() / SaveIniSettingsXXX() functions.
	/// </summary>

	public string? IniFilename
	{
		get => ImGuiHelper.GetString(this._iniFilename);
		set
		{
			if(value == null)
				this._iniFilename = null;
			else
				this._iniFilename = ImGuiHelper.GetStringPointer(value);
		}
	}
	/// <summary>
	/// <"imgui_log.txt"> Path to .log file (default parameter to ImGui::LogToFile when no file is specified).
	/// </summary>
	public string? LogFilename
	{
		get => ImGuiHelper.GetString(this._logFilename);
		set
		{
			if(value == null)
				this._logFilename = null;
			else
				this._logFilename = ImGuiHelper.GetStringPointer(value);
		}
	}

	/// <summary>
	/// <0.30f> Time for a double-click, in seconds.
	/// </summary>
	public ref float MouseDoubleClickTime => ref this._mouseDoubleClickTime;

	/// <summary>
	/// <6.0f> Distance threshold to stay in to validate a double-click, in pixels.
	/// </summary>
	public ref float MouseDoubleClickMaxDist => ref this._mouseDoubleClickMaxDist;

	/// <summary>
	/// <6.0f> Distance threshold before considering we are dragging.
	/// </summary>
	public ref float MouseDragThreshold => ref this._mouseDragThreshold;

	/// <summary>
	/// <0.250f> When holding a key/button, time before it starts repeating, in seconds (for buttons in Repeat mode, etc.).
	/// </summary>
	public ref float KeyRepeatDelay => ref this._keyRepeatDelay;

	/// <summary>
	/// <0.050f> When holding a key/button, rate at which it repeats, in seconds.
	/// </summary>	
	public ref float KeyRepeatRate => ref this._keyRepeatRate;
	/// <summary>
	/// <NULL> Store your own data for retrieval by callbacks.
	/// </summary>
	public ref void* UserData => ref this._userData;

	/// <summary>
	/// <auto> Font atlas: load, rasterize and pack one or more fonts into a single texture.
	/// </summary>
	public ref ImFontAtlas Fonts => ref *this._fonts;
	public bool HasFonts => this._fonts != null;
	public void SetFonts(ref ImFontAtlas fonts)
	{
		fixed(ImFontAtlas* ptr = &fonts)
		{
			this._fonts = ptr;
		}
	}
	public void ClearFonts() => this._fonts = null;

	/// <summary>
	/// <1.0f> Global scale all fonts
	/// </summary>
	public ref float FontGlobalScale => ref this._fontGlobalScale;

	/// <summary>
	/// <false> Allow user scaling text of individual window with CTRL+Wheel.
	/// </summary>
	public ref bool FontAllowUserScaling => ref this._fontAllowUserScaling;

	/// <summary>
	/// <NULL> Font to use on NewFrame(). Use NULL to uses Fonts->Fonts[0].
	/// </summary>
	public ref ImFont FontDefault => ref *this._fontDefault;
	public bool HasFontDefault => this._fontDefault != null;
	public void SetFontDefault(ImFont* font)
	{
		if(font == null)
			this._fontDefault = null;
		else
			this._fontDefault = font;
	}
	public void ClearFontDefault() => this._fontDefault = null;

	/// <summary>
	/// <(1, 1)> For retina display or other situations where window coordinates are different from framebuffer coordinates. This generally ends up in ImDrawData::FramebufferScale.
	/// </summary>
	public ref ImVec2 DisplayFramebufferScale => ref this._displayFramebufferScale;

	// Miscellaneous options
	/// <summary>
	/// <false> Request ImGui to draw a mouse cursor for you (if you are on a platform without a mouse cursor). Cannot be easily renamed to 'io.ConfigXXX' because this is frequently used by backend implementations.
	/// </summary>
	public ref bool MouseDrawCursor => ref this._mouseDrawCursor;

	/// <summary>
	/// <defined(__APPLE__)> OS X style: Text editing cursor movement using Alt instead of Ctrl, Shortcuts using Cmd/Super instead of Ctrl, Line/Text Start and End using Cmd+Arrows instead of Home/End, Double click selects by word instead of selecting whole text, Multi-selection in lists uses Cmd/Super instead of Ctrl.
	/// </summary>
	public ref bool ConfigMacOSXBehaviors => ref this._configMacOSXBehaviors;

	/// <summary>
	/// <true> Enable input queue trickling: some types of events submitted during the same frame (e.g. button down + up) will be spread over multiple frames, improving interactions with low framerates.
	/// </summary>
	public ref bool ConfigInputTrickleEventQueue => ref this._configInputTrickleEventQueue;

	/// <summary>
	/// <true> Enable blinking cursor (optional as some users consider it to be distracting).
	/// </summary>
	public ref bool ConfigInputTextCursorBlink => ref this._configInputTextCursorBlink;

	/// <summary>
	/// <false> [BETA] Enable turning DragXXX widgets into text input with a simple mouse click-release (without moving). Not desirable on devices without a keyboard.
	/// </summary>
	public ref bool ConfigDragClickToInputText => ref this._configDragClickToInputText;

	/// <summary>
	/// <true> Enable resizing of windows from their edges and from the lower-left corner. This requires (io.BackendFlags & ImGuiBackendFlags_HasMouseCursors) because it needs mouse cursor feedback. (This used to be a per-window ImGuiWindowFlags_ResizeFromAnySide flag)
	/// </summary>
	public ref bool ConfigWindowsResizeFromEdges => ref this._configWindowsResizeFromEdges;

	/// <summary>
	/// <false> Enable allowing to move windows only when clicking on their title bar. Does not apply to windows without a title bar.
	/// </summary>
	public ref bool ConfigWindowsMoveFromTitleBarOnly => ref this._configWindowsMoveFromTitleBarOnly;

	/// <summary>
	/// <60.0f> Timer (in seconds) to free transient windows/tables memory buffers when unused. Set to -1.0f to disable.
	/// </summary>
	public ref float ConfigMemoryCompactTimer => ref this._configMemoryCompactTimer;

	//------------------------------------------------------------------
	// Platform Functions
	// (the imgui_impl_xxxx backend files are setting those up for you)
	//------------------------------------------------------------------

	/// <summary>
	/// <NULL> Optional: Platform/Renderer backend name (informational only! will be displayed in About Window) + User data for backend/wrappers to store their own stuff.
	/// </summary>
	public string? BackendPlatformName
	{
		get => ImGuiHelper.GetString(this._backendPlatformName);
		set
		{
			if(value == null)
				this._backendPlatformName = null;
			else
				this._backendPlatformName = ImGuiHelper.GetStringPointer(value);
		}
	}

	/// <summary>
	/// <NULL>
	/// </summary>
	public string? BackendRendererName
	{
		get => ImGuiHelper.GetString(this._backendRendererName);
		set
		{
			if(value == null)
				this._backendRendererName = null;
			else
				this._backendRendererName = ImGuiHelper.GetStringPointer(value);
		}
	}

	/// <summary>
	/// <NULL> User data for platform backend
	/// </summary>
	public ref void* BackendPlatformUserData => ref this._backendPlatformUserData;
	/// <summary>
	/// <NULL> User data for renderer backend
	/// </summary>
	public ref void* BackendRendererUserData => ref this._backendRendererUserData;
	/// <summary>
	/// <NULL> User data for non C++ programming language backend
	/// </summary>
	public ref void* BackendLanguageUserData => ref this._backendLanguageUserData;

	// Optional: Access OS clipboard
	// (default to use native Win32 clipboard on Windows, otherwise uses a private clipboard. Override to access OS clipboard on other architectures)
	public ref nint GetClipboardTextFn => ref this._getClipboardTextFn;
	public ref nint SetClipboardTextFn => ref this._setClipboardTextFn;
	public ref void* ClipboardUserData => ref this._clipboardUserData;

	// Optional: Notify OS Input Method Editor of the screen position of your cursor for text input position (e.g. when using Japanese/Chinese IME on Windows)
	// (default to use native imm32 api on Windows)
	public ref nint SetPlatformImeDataFn => ref this._setPlatformImeDataFn;

	//------------------------------------------------------------------
	// Output - Updated by NewFrame() or EndFrame()/Render()
	// (when reading from the io.WantCaptureMouse, io.WantCaptureKeyboard flags to dispatch your inputs, it is
	//  generally easier and more correct to use their state BEFORE calling NewFrame(). See FAQ for details!)
	//------------------------------------------------------------------

	/// <summary>
	/// Set when Dear ImGui will use mouse inputs, in this case do not dispatch them to your main game/application (either way, always pass on mouse inputs to imgui). (e.g. unclicked mouse is hovering over an imgui window, widget is active, mouse was clicked over an imgui window, etc.).
	/// </summary>
	public ref bool WantCaptureMouse => ref this._wantCaptureMouse;
	/// <summary>
	/// Set when Dear ImGui will use keyboard inputs, in this case do not dispatch them to your main game/application (either way, always pass keyboard inputs to imgui). (e.g. InputText active, or an imgui window is focused and navigation is enabled, etc.).
	/// </summary>
	public ref bool WantCaptureKeyboard => ref this._wantCaptureKeyboard;
	/// <summary>
	/// Mobile/console: when set, you may display an on-screen keyboard. This is set by Dear ImGui when it wants textual keyboard input to happen (e.g. when a InputText widget is active).
	/// </summary>
	public ref bool WantTextInput => ref this._wantTextInput;
	/// <summary>
	/// MousePos has been altered, backend should reposition mouse on next frame. Rarely used! Set only when ImGuiConfigFlags_NavEnableSetMousePos flag is enabled.
	/// </summary>
	public ref bool WantSetMousePos => ref this._wantSetMousePos;
	/// <summary>
	/// When manual .ini load/save is active (io.IniFilename == NULL), this will be set to notify your application that you can call SaveIniSettingsToMemory() and save yourself. Important: clear io.WantSaveIniSettings yourself after saving!
	/// </summary>
	public ref bool WantSaveIniSettings => ref this._wantSaveIniSettings;
	/// <summary>
	/// Keyboard/Gamepad navigation is currently allowed (will handle ImGuiKey_NavXXX events) = a window is focused and it doesn't use the ImGuiWindowFlags_NoNavInputs flag.
	/// </summary>
	public ref bool NavActive => ref this._navActive;
	/// <summary>
	/// Keyboard/Gamepad navigation is visible and allowed (will handle ImGuiKey_NavXXX events).
	/// </summary>
	public ref bool NavVisible => ref this._navVisible;
	/// <summary>
	/// Rough estimate of application framerate, in frame per second. Solely for convenience. Rolling average estimation based on io.DeltaTime over 120 frames.
	/// </summary>
	public ref float Framerate => ref this._framerate;
	/// <summary>
	/// Vertices output during last call to Render()
	/// </summary>
	public ref int MetricsRenderVertices => ref this._metricsRenderVertices;
	/// <summary>
	/// Indices output during last call to Render() = number of triangles * 3
	/// </summary>
	public ref int MetricsRenderIndices => ref this._metricsRenderIndices;
	/// <summary>
	/// Number of visible windows
	/// </summary>
	public ref int MetricsRenderWindows => ref this._metricsRenderWindows;
	/// <summary>
	/// Number of active windows
	/// </summary>
	public ref int MetricsActiveWindows => ref this._metricsActiveWindows;
	/// <summary>
	/// Number of active allocations, updated by MemAlloc/MemFree based on current context. May be off if you have multiple imgui contexts.
	/// </summary>
	public ref int MetricsActiveAllocations => ref this._metricsActiveAllocations;

	/// <summary>
	/// Mouse delta. Note that this is zero if either current or previous position are invalid (-FLT_MAX,-FLT_MAX), so a disappearing/reappearing mouse won't have a huge delta.
	/// </summary>
	public ref ImVec2 MouseDelta => ref this._mouseDelta;

	// Legacy: before 1.87, we required backend to fill io.KeyMap[] (imgui->native map) during initialization and io.KeysDown[] (native indices) every frame.
	// This is still temporarily supported as a legacy feature. However the new preferred scheme is for backend to call io.AddKeyEvent().
	/// <summary>
	/// [LEGACY] Input: map of indices into the KeysDown[512] entries array which represent your "native" keyboard state. The first 512 are now unused and should be kept zero. Legacy backend will write into KeyMap[] using ImGuiKey_ indices which are always >512.
	/// </summary>
	[Obsolete("[LEGACY] Input: map of indices into the KeysDown[512] entries array which represent your \"native\" keyboard state. The first 512 are now unused and should be kept zero. Legacy backend will write into KeyMap[] using ImGuiKey_ indices which are always >512.")]
	public Span<int> KeyMap
	{
		get
		{
			fixed (int* ptr = this._keyMap)
			{
				return new Span<int>(ptr, (int)ImGuiKey.COUNT);
			}
		}
	}
	/// <summary>
	/// [LEGACY] Input: Keyboard keys that are pressed (ideally left in the "native" order your engine has access to keyboard keys, so you can use your own defines/enums for keys). This used to be [512] sized. It is now ImGuiKey_COUNT to allow legacy io.KeysDown[GetKeyIndex(...)] to work without an overflow.
	/// </summary>
	[Obsolete("[LEGACY] Input: Keyboard keys that are pressed (ideally left in the \"native\" order your engine has access to keyboard keys, so you can use your own defines/enums for keys). This used to be [512] sized. It is now ImGuiKey_COUNT to allow legacy io.KeysDown[GetKeyIndex(...)] to work without an overflow.")]
	public Span<bool> KeysDown
	{
		get
		{
			fixed (bool* ptr = this._keysDown)
			{
				return new Span<bool>(ptr, (int)ImGuiKey.COUNT);
			}
		}
	}

	//------------------------------------------------------------------
	// [Internal] Dear ImGui will maintain those fields. Forward compatibility not guaranteed!
	//------------------------------------------------------------------

	// Main Input State
	// (this block used to be written by backend, since 1.87 it is best to NOT write to those directly, call the AddXXX functions above instead)
	// (reading from those variables is fair game, as they are extremely unlikely to be moving anywhere)
	/// <summary>
	/// Mouse position, in pixels. Set to ImVec2(-FLT_MAX, -FLT_MAX) if mouse is unavailable (on another screen, etc.)
	/// </summary>
	public ref ImVec2 MousePos => ref this._mousePos;
	/// <summary>
	/// Mouse buttons: 0=left, 1=right, 2=middle + extras (ImGuiMouseButton_COUNT == 5). Dear ImGui mostly uses left and right buttons. Others buttons allows us to track if the mouse is being used by your application + available to user as a convenience via IsMouse** API.
	/// </summary>
	public Span<bool> MouseDown
	{
		get
		{
			fixed (bool* ptr = this._mouseDown)
			{
				return new Span<bool>(ptr, 5);
			}
		}
	}
	/// <summary>
	/// Mouse wheel Vertical: 1 unit scrolls about 5 lines text.
	/// </summary>
	public ref float MouseWheel => ref this._mouseWheel;
	/// <summary>
	/// Mouse wheel Horizontal. Most users don't have a mouse with an horizontal wheel, may not be filled by all backends.
	/// </summary>
	public ref float MouseWheelH => ref this._mouseWheelH;
	/// <summary>
	/// Keyboard modifier down: Control
	/// </summary>
	public ref bool KeyCtrl => ref this._keyCtrl;
	/// <summary>
	/// Keyboard modifier down: Shift
	/// </summary>
	public ref bool KeyShift => ref this._keyShift;
	/// <summary>
	/// Keyboard modifier down: Alt
	/// </summary>
	public ref bool KeyAlt => ref this._keyAlt;
	/// <summary>
	/// Keyboard modifier down: Cmd/Super/Windows
	/// </summary>
	public ref bool KeySuper => ref this._keySuper;
	/// <summary>
	/// Gamepad inputs. Cleared back to zero by EndFrame(). Keyboard keys will be auto-mapped and be written here by NewFrame().
	/// </summary>
	public Span<float> NavInputs
	{
		get
		{
			fixed (float* ptr = this._navInputs)
			{
				return new Span<float>(ptr, (int)ImGuiNavInput.COUNT);
			}
		}
	}

	// Other state maintained from data above + IO function calls
	/// <summary>
	/// Key mods flags (same as io.KeyCtrl/KeyShift/KeyAlt/KeySuper but merged into flags), updated by NewFrame()
	/// </summary>
	public ref ImGuiModFlags KeyMods => ref this._keyMods;
	/// <summary>
	/// Key state for all known keys. Use IsKeyXXX() functions to access this.
	/// </summary>
	public Span<ImGuiKeyData> KeysData
	{
		get
		{
			fixed (byte* ptr = this._keysData)
			{
				return new Span<ImGuiKeyData>((ImGuiKeyData*)ptr, (int)ImGuiKey.KeysData_SIZE);
			}
		}
	}
	/// <summary>
	/// Alternative to WantCaptureMouse: (WantCaptureMouse == true && WantCaptureMouseUnlessPopupClose == false) when a click over void is expected to close a popup.
	/// </summary>
	public ref bool WantCaptureMouseUnlessPopupClose => ref this._wantCaptureMouseUnlessPopupClose;

	/// <summary>
	/// Previous mouse position (note that MouseDelta is not necessary == MousePos-MousePosPrev, in case either position is invalid)
	/// </summary>
	public ref ImVec2 MousePosPrev => ref this._mousePosPrev;

	public Span<ImVec2 > MouseClickedPos
	{
		get
		{
			fixed (ImVec2* ptr = &this._mouseClickedPos0)
			{
				return new Span<ImVec2>(ptr, 5);
			}
		}
	}

	/// <summary>
	/// Time of last click (used to figure out double-click)
	/// </summary>
	public Span<double> MouseClickedTime
	{
		get
		{
			fixed (double* ptr = this._mouseClickedTime)
			{
				return new Span<double>(ptr, 5);
			}
		}
	}
	/// <summary>
	/// Mouse button went from !Down to Down (same as MouseClickedCount[x] != 0)
	/// </summary>
	public Span<bool> MouseClicked
	{
		get
		{
			fixed (bool* ptr = this._mouseClicked)
			{
				return new Span<bool>(ptr, 5);
			}
		}
	}
	/// <summary>
	/// Has mouse button been double-clicked? (same as MouseClickedCount[x] == 2)
	/// </summary>
	public Span<bool> MouseDoubleClicked
	{
		get
		{
			fixed (bool* ptr = this._mouseDoubleClicked)
			{
				return new Span<bool>(ptr, 5);
			}
		}
	}
	/// <summary>
	/// == 0 (not clicked), == 1 (same as MouseClicked[]), == 2 (double-clicked), == 3 (triple-clicked) etc. when going from !Down to Down
	/// </summary>
	public Span<ImU16> MouseClickedCount
	{
		get
		{
			fixed (ImU16* ptr = this._mouseClickedCount)
			{
				return new Span<ImU16>(ptr, 5);
			}
		}
	}
	/// <summary>
	/// Count successive number of clicks. Stays valid after mouse release. Reset after another click is done.
	/// </summary>
	public Span<ImU16> MouseClickedLastCount
	{
		get
		{
			fixed (ImU16* ptr = this._mouseClickedLastCount)
			{
				return new Span<ImU16>(ptr, 5);
			}
		}
	}
	/// <summary>
	/// Mouse button went from Down to !Down
	/// </summary>
	public Span<bool> MouseReleased
	{
		get
		{
			fixed (bool* ptr = this._mouseReleased)
			{
				return new Span<bool>(ptr, 5);
			}
		}
	}
	/// <summary>
	/// Track if button was clicked inside a dear imgui window or over void blocked by a popup. We don't request mouse capture from the application if click started outside ImGui bounds.
	/// </summary>
	public Span<bool> MouseDownOwned
	{
		get
		{
			fixed (bool* ptr = this._mouseDownOwned)
			{
				return new Span<bool>(ptr, 5);
			}
		}
	}
	/// <summary>
	/// Track if button was clicked inside a dear imgui window.
	/// </summary>
	public Span<bool> MouseDownOwnedUnlessPopupClose
	{
		get
		{
			fixed (bool* ptr = this._mouseDownOwnedUnlessPopupClose)
			{
				return new Span<bool>(ptr, 5);
			}
		}
	}
	/// <summary>
	/// Duration the mouse button has been down (0.0f == just clicked)
	/// </summary>
	public Span<float> MouseDownDuration
	{
		get
		{
			fixed (float* ptr = this._mouseDownDuration)
			{
				return new Span<float>(ptr, 5);
			}
		}
	}
	/// <summary>
	/// Previous time the mouse button has been down
	/// </summary>
	public Span<float> MouseDownDurationPrev
	{
		get
		{
			fixed (float* ptr = this._mouseDownDurationPrev)
			{
				return new Span<float>(ptr, 5);
			}
		}
	}
	/// <summary>
	/// Squared maximum distance of how much mouse has traveled from the clicking point (used for moving thresholds)
	/// </summary>
	public Span<float> MouseDragMaxDistanceSqr
	{
		get
		{
			fixed (float* ptr = this._mouseDragMaxDistanceSqr)
			{
				return new Span<float>(ptr, 5);
			}
		}
	}
	public Span<float> NavInputsDownDuration
	{
		get
		{
			fixed (float* ptr = this._navInputsDownDuration)
			{
				return new Span<float>(ptr, (int)ImGuiNavInput.COUNT);
			}
		}
	}
	public Span<float> NavInputsDownDurationPrev
	{
		get
		{
			fixed (float* ptr = this._navInputsDownDurationPrev)
			{
				return new Span<float>(ptr, (int)ImGuiNavInput.COUNT);
			}
		}
	}
	/// <summary>
	/// Touch/Pen pressure (0.0f to 1.0f, should be >0.0f only when MouseDown[0] == true). Helper storage currently unused by Dear ImGui.
	/// </summary>
	public ref float PenPressure => ref this._penPressure;
	/// <summary>
	/// Only modify via AddFocusEvent()
	/// </summary>
	public ref bool AppFocusLost => ref this._appFocusLost;
	/// <summary>
	/// Only modify via SetAppAcceptingEvents()
	/// </summary>
	public ref bool AppAcceptingEvents => ref this._appAcceptingEvents;
	/// <summary>
	/// -1: unknown, 0: using AddKeyEvent(), 1: using legacy io.KeysDown[]
	/// </summary>
	public ref ImS8 BackendUsingLegacyKeyArrays => ref this._backendUsingLegacyKeyArrays;
	/// <summary>
	/// 0: using AddKeyAnalogEvent(), 1: writing to legacy io.NavInputs[] directly
	/// </summary>
	public ref bool BackendUsingLegacyNavInputArray => ref this._backendUsingLegacyNavInputArray;
	/// <summary>
	/// For AddInputCharacterUTF16()
	/// </summary>
	public ref byte InputQueueSurrogate => ref this._inputQueueSurrogate;

	/// <summary>
	/// Queue of _characters_ input (obtained by platform backend). Fill using AddInputCharacter() helper.
	/// </summary>
	public ref ImVector<byte> InputQueueCharacters => ref this._inputQueueCharacters;
}