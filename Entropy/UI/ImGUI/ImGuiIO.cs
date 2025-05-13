#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImGuiNET;
using System;
using ImU16 = ushort;
using ImS8 = sbyte;
using System.Runtime.InteropServices;

namespace Entropy.UI.ImGUI;

public unsafe struct ImGuiIO
{
	public const int SizeOfImGuiKeyData = 16;
	//------------------------------------------------------------------
	// Configuration                            // Default value
	//------------------------------------------------------------------

	public ImGuiConfigFlags ConfigFlags;             // = 0              // See ImGuiConfigFlags_ enum. Set by user/application. Gamepad/keyboard navigation options, etc.
	public ImGuiBackendFlags BackendFlags;            // = 0              // See ImGuiBackendFlags_ enum. Set by backend (imgui_impl_xxx files or custom backend) to communicate features supported by the backend.
	public ImVec2 DisplaySize;                    // <unset>          // Main display size, in pixels (generally == GetMainViewport()->Size). May change every frame.
	public float DeltaTime;                      // = 1.0f/60.0f     // Time elapsed since last frame, in seconds. May change every frame.
	public float IniSavingRate;                  // = 5.0f           // Minimum time between saving positions/sizes to .ini file, in seconds.
	private char* _iniFilename;                    // = "imgui.ini"    // Path to .ini file (important: default "imgui.ini" is relative to current working dir!). Set NULL to disable automatic .ini loading/saving or if you want to manually call LoadIniSettingsXXX() / SaveIniSettingsXXX() functions.
	public string? IniFilename
	{
		get => Marshal.PtrToStringAnsi((IntPtr)this._iniFilename);
		set
		{
			if(value == null)
				this._iniFilename = null;
			else
				this._iniFilename = (char*)Marshal.StringToHGlobalAnsi(value);
		}
	}
	private char* _logFilename;                    // = "imgui_log.txt"// Path to .log file (default parameter to ImGui::LogToFile when no file is specified).
	public string? LogFilename
	{
		get => Marshal.PtrToStringAnsi((IntPtr)this._logFilename);
		set
		{
			if(value == null)
				this._logFilename = null;
			else
				this._logFilename = (char*)Marshal.StringToHGlobalAnsi(value);
		}
	}

	public float MouseDoubleClickTime;           // = 0.30f          // Time for a double-click, in seconds.
	public float MouseDoubleClickMaxDist;        // = 6.0f           // Distance threshold to stay in to validate a double-click, in pixels.
	public float MouseDragThreshold;             // = 6.0f           // Distance threshold before considering we are dragging.
	public float KeyRepeatDelay;                 // = 0.250f         // When holding a key/button, time before it starts repeating, in seconds (for buttons in Repeat mode, etc.).
	public float KeyRepeatRate;                  // = 0.050f         // When holding a key/button, rate at which it repeats, in seconds.
	public void* UserData;                       // = NULL           // Store your own data for retrieval by callbacks.

	public ImFontAtlas* Fonts;                          // <auto>           // Font atlas: load, rasterize and pack one or more fonts into a single texture.
	public float FontGlobalScale;                // = 1.0f           // Global scale all fonts
	public bool FontAllowUserScaling;           // = false          // Allow user scaling text of individual window with CTRL+Wheel.
	public ImFont* FontDefault;                    // = NULL           // Font to use on NewFrame(). Use NULL to uses Fonts->Fonts[0].
	public ImVec2 DisplayFramebufferScale;        // = (1, 1)         // For retina display or other situations where window coordinates are different from framebuffer coordinates. This generally ends up in ImDrawData::FramebufferScale.

	// Miscellaneous options
	public bool MouseDrawCursor;                // = false          // Request ImGui to draw a mouse cursor for you (if you are on a platform without a mouse cursor). Cannot be easily renamed to 'io.ConfigXXX' because this is frequently used by backend implementations.
	public bool ConfigMacOSXBehaviors;          // = defined(__APPLE__) // OS X style: Text editing cursor movement using Alt instead of Ctrl, Shortcuts using Cmd/Super instead of Ctrl, Line/Text Start and End using Cmd+Arrows instead of Home/End, Double click selects by word instead of selecting whole text, Multi-selection in lists uses Cmd/Super instead of Ctrl.
	public bool ConfigInputTrickleEventQueue;   // = true           // Enable input queue trickling: some types of events submitted during the same frame (e.g. button down + up) will be spread over multiple frames, improving interactions with low framerates.
	public bool ConfigInputTextCursorBlink;     // = true           // Enable blinking cursor (optional as some users consider it to be distracting).
	public bool ConfigDragClickToInputText;     // = false          // [BETA] Enable turning DragXXX widgets into text input with a simple mouse click-release (without moving). Not desirable on devices without a keyboard.
	public bool ConfigWindowsResizeFromEdges;   // = true           // Enable resizing of windows from their edges and from the lower-left corner. This requires (io.BackendFlags & ImGuiBackendFlags_HasMouseCursors) because it needs mouse cursor feedback. (This used to be a per-window ImGuiWindowFlags_ResizeFromAnySide flag)
	public bool ConfigWindowsMoveFromTitleBarOnly; // = false       // Enable allowing to move windows only when clicking on their title bar. Does not apply to windows without a title bar.
	public float ConfigMemoryCompactTimer;       // = 60.0f          // Timer (in seconds) to free transient windows/tables memory buffers when unused. Set to -1.0f to disable.

	//------------------------------------------------------------------
	// Platform Functions
	// (the imgui_impl_xxxx backend files are setting those up for you)
	//------------------------------------------------------------------

	// Optional: Platform/Renderer backend name (informational only! will be displayed in About Window) + User data for backend/wrappers to store their own stuff.
	private char* _backendPlatformName;            // = NULL
	public string? BackendPlatformName
	{
		get => Marshal.PtrToStringAnsi((IntPtr)this._backendPlatformName);
		set
		{
			if(value == null)
				this._backendPlatformName = null;
			else
				this._backendPlatformName = (char*)Marshal.StringToHGlobalAnsi(value);
		}
	}
	private char* _backendRendererName;            // = NULL
	public string? BackendRendererName
	{
		get => Marshal.PtrToStringAnsi((IntPtr)this._backendRendererName);
		set
		{
			if(value == null)
				this._backendRendererName = null;
			else
				this._backendRendererName = (char*)Marshal.StringToHGlobalAnsi(value);
		}
	}
	public void* BackendPlatformUserData;        // = NULL           // User data for platform backend
	public void* BackendRendererUserData;        // = NULL           // User data for renderer backend
	public void* BackendLanguageUserData;        // = NULL           // User data for non C++ programming language backend

	// Optional: Access OS clipboard
	// (default to use native Win32 clipboard on Windows, otherwise uses a private clipboard. Override to access OS clipboard on other architectures)
	public IntPtr GetClipboardTextFn;
	public IntPtr SetClipboardTextFn;
	public void* ClipboardUserData;

	// Optional: Notify OS Input Method Editor of the screen position of your cursor for text input position (e.g. when using Japanese/Chinese IME on Windows)
	// (default to use native imm32 api on Windows)
	public IntPtr SetPlatformImeDataFn;
	public IntPtr _UnusedPadding;

	//------------------------------------------------------------------
	// Output - Updated by NewFrame() or EndFrame()/Render()
	// (when reading from the io.WantCaptureMouse, io.WantCaptureKeyboard flags to dispatch your inputs, it is
	//  generally easier and more correct to use their state BEFORE calling NewFrame(). See FAQ for details!)
	//------------------------------------------------------------------

	public bool WantCaptureMouse;                   // Set when Dear ImGui will use mouse inputs, in this case do not dispatch them to your main game/application (either way, always pass on mouse inputs to imgui). (e.g. unclicked mouse is hovering over an imgui window, widget is active, mouse was clicked over an imgui window, etc.).
	public bool WantCaptureKeyboard;                // Set when Dear ImGui will use keyboard inputs, in this case do not dispatch them to your main game/application (either way, always pass keyboard inputs to imgui). (e.g. InputText active, or an imgui window is focused and navigation is enabled, etc.).
	public bool WantTextInput;                      // Mobile/console: when set, you may display an on-screen keyboard. This is set by Dear ImGui when it wants textual keyboard input to happen (e.g. when a InputText widget is active).
	public bool WantSetMousePos;                    // MousePos has been altered, backend should reposition mouse on next frame. Rarely used! Set only when ImGuiConfigFlags_NavEnableSetMousePos flag is enabled.
	public bool WantSaveIniSettings;                // When manual .ini load/save is active (io.IniFilename == NULL), this will be set to notify your application that you can call SaveIniSettingsToMemory() and save yourself. Important: clear io.WantSaveIniSettings yourself after saving!
	public bool NavActive;                          // Keyboard/Gamepad navigation is currently allowed (will handle ImGuiKey_NavXXX events) = a window is focused and it doesn't use the ImGuiWindowFlags_NoNavInputs flag.
	public bool NavVisible;                         // Keyboard/Gamepad navigation is visible and allowed (will handle ImGuiKey_NavXXX events).
	public float Framerate;                          // Rough estimate of application framerate, in frame per second. Solely for convenience. Rolling average estimation based on io.DeltaTime over 120 frames.
	public int MetricsRenderVertices;              // Vertices output during last call to Render()
	public int MetricsRenderIndices;               // Indices output during last call to Render() = number of triangles * 3
	public int MetricsRenderWindows;               // Number of visible windows
	public int MetricsActiveWindows;               // Number of active windows
	public int MetricsActiveAllocations;           // Number of active allocations, updated by MemAlloc/MemFree based on current context. May be off if you have multiple imgui contexts.
	public ImVec2 MouseDelta;                         // Mouse delta. Note that this is zero if either current or previous position are invalid (-FLT_MAX,-FLT_MAX), so a disappearing/reappearing mouse won't have a huge delta.

	// Legacy: before 1.87, we required backend to fill io.KeyMap[] (imgui->native map) during initialization and io.KeysDown[] (native indices) every frame.
	// This is still temporarily supported as a legacy feature. However the new preferred scheme is for backend to call io.AddKeyEvent().
	//# ifndef IMGUI_DISABLE_OBSOLETE_KEYIO
	public fixed int KeyMap[(int)ImGuiKey.COUNT];             // [LEGACY] Input: map of indices into the KeysDown[512] entries array which represent your "native" keyboard state. The first 512 are now unused and should be kept zero. Legacy backend will write into KeyMap[] using ImGuiKey_ indices which are always >512.
	public fixed bool KeysDown[(int)ImGuiKey.COUNT];           // [LEGACY] Input: Keyboard keys that are pressed (ideally left in the "native" order your engine has access to keyboard keys, so you can use your own defines/enums for keys). This used to be [512] sized. It is now ImGuiKey_COUNT to allow legacy io.KeysDown[GetKeyIndex(...)] to work without an overflow.
	//#endif

	//------------------------------------------------------------------
	// [Internal] Dear ImGui will maintain those fields. Forward compatibility not guaranteed!
	//------------------------------------------------------------------

	// Main Input State
	// (this block used to be written by backend, since 1.87 it is best to NOT write to those directly, call the AddXXX functions above instead)
	// (reading from those variables is fair game, as they are extremely unlikely to be moving anywhere)
	public ImVec2 MousePos;                           // Mouse position, in pixels. Set to ImVec2(-FLT_MAX, -FLT_MAX) if mouse is unavailable (on another screen, etc.)
	public fixed bool MouseDown[5];                       // Mouse buttons: 0=left, 1=right, 2=middle + extras (ImGuiMouseButton_COUNT == 5). Dear ImGui mostly uses left and right buttons. Others buttons allows us to track if the mouse is being used by your application + available to user as a convenience via IsMouse** API.
	public float MouseWheel;                         // Mouse wheel Vertical: 1 unit scrolls about 5 lines text.
	public float MouseWheelH;                        // Mouse wheel Horizontal. Most users don't have a mouse with an horizontal wheel, may not be filled by all backends.
	public bool KeyCtrl;                            // Keyboard modifier down: Control
	public bool KeyShift;                           // Keyboard modifier down: Shift
	public bool KeyAlt;                             // Keyboard modifier down: Alt
	public bool KeySuper;                           // Keyboard modifier down: Cmd/Super/Windows
	public fixed float NavInputs[(int)ImGuiNavInput.COUNT];     // Gamepad inputs. Cleared back to zero by EndFrame(). Keyboard keys will be auto-mapped and be written here by NewFrame().

	// Other state maintained from data above + IO function calls
	public ImGuiModFlags KeyMods;                          // Key mods flags (same as io.KeyCtrl/KeyShift/KeyAlt/KeySuper but merged into flags), updated by NewFrame()
	public fixed byte KeysData[SizeOfImGuiKeyData * (int)ImGuiKey.KeysData_SIZE];  // Key state for all known keys. Use IsKeyXXX() functions to access this.
	public bool WantCaptureMouseUnlessPopupClose;   // Alternative to WantCaptureMouse: (WantCaptureMouse == true && WantCaptureMouseUnlessPopupClose == false) when a click over void is expected to close a popup.
	public ImVec2 MousePosPrev;                       // Previous mouse position (note that MouseDelta is not necessary == MousePos-MousePosPrev, in case either position is invalid)
	public ImVec2 MouseClickedPos0; // Position at time of clicking
	public ImVec2 MouseClickedPos1;
	public ImVec2 MouseClickedPos2;
	public ImVec2 MouseClickedPos3;
	public ImVec2 MouseClickedPos4;
	public fixed double MouseClickedTime[5];                // Time of last click (used to figure out double-click)
	public fixed bool MouseClicked[5];                    // Mouse button went from !Down to Down (same as MouseClickedCount[x] != 0)
	public fixed bool MouseDoubleClicked[5];              // Has mouse button been double-clicked? (same as MouseClickedCount[x] == 2)
	public fixed ImU16 MouseClickedCount[5];               // == 0 (not clicked), == 1 (same as MouseClicked[]), == 2 (double-clicked), == 3 (triple-clicked) etc. when going from !Down to Down
	public fixed ImU16 MouseClickedLastCount[5];           // Count successive number of clicks. Stays valid after mouse release. Reset after another click is done.
	public fixed bool MouseReleased[5];                   // Mouse button went from Down to !Down
	public fixed bool MouseDownOwned[5];                  // Track if button was clicked inside a dear imgui window or over void blocked by a popup. We don't request mouse capture from the application if click started outside ImGui bounds.
	public fixed bool MouseDownOwnedUnlessPopupClose[5];  // Track if button was clicked inside a dear imgui window.
	public fixed float MouseDownDuration[5];               // Duration the mouse button has been down (0.0f == just clicked)
	public fixed float MouseDownDurationPrev[5];           // Previous time the mouse button has been down
	public fixed float MouseDragMaxDistanceSqr[5];         // Squared maximum distance of how much mouse has traveled from the clicking point (used for moving thresholds)
	public fixed float NavInputsDownDuration[(int)ImGuiNavInput.COUNT];
	public fixed float NavInputsDownDurationPrev[(int)ImGuiNavInput.COUNT];
	public float PenPressure;                        // Touch/Pen pressure (0.0f to 1.0f, should be >0.0f only when MouseDown[0] == true). Helper storage currently unused by Dear ImGui.
	public bool AppFocusLost;                       // Only modify via AddFocusEvent()
	public bool AppAcceptingEvents;                 // Only modify via SetAppAcceptingEvents()
	public ImS8 BackendUsingLegacyKeyArrays;        // -1: unknown, 0: using AddKeyEvent(), 1: using legacy io.KeysDown[]
	public bool BackendUsingLegacyNavInputArray;    // 0: using AddKeyAnalogEvent(), 1: writing to legacy io.NavInputs[] directly
	public char InputQueueSurrogate;                // For AddInputCharacterUTF16()
	public ImVector<char> InputQueueCharacters;         // Queue of _characters_ input (obtained by platform backend). Fill using AddInputCharacter() helper.
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member