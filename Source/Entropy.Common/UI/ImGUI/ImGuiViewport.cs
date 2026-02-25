#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImGuiNET;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiViewport
{
	private ImGuiViewportFlags _flags;
	private ImVec2 _pos;
	private ImVec2 _size;
	private ImVec2 _workPos;
	private ImVec2 _workSize;
	private nint _platformHandleRaw;

	// Helpers
	public readonly ImVec2 Center => new(this._pos.X + (this._size.X * 0.5f), this._pos.Y + (this._size.Y * 0.5f));
	public readonly ImVec2 WorkCenter => new(this._workPos.X + (this._workSize.X * 0.5f), this._workPos.Y + (this._workSize.Y * 0.5f));

	/// <summary>
	/// See ImGuiViewportFlags_
	/// </summary>
	public ref ImGuiViewportFlags Flags => ref this._flags;
	/// <summary>
	/// Main Area: Position of the viewport (Dear ImGui coordinates are the same as OS desktop/native coordinates)
	/// </summary>
	public ref ImVec2 Pos => ref this._pos;
	/// <summary>
	/// Main Area: Size of the viewport.
	/// </summary>
	public ref ImVec2 Size => ref this._size;
	/// <summary>
	/// Work Area: Position of the viewport minus task bars, menus bars, status bars (>= Pos)
	/// </summary>
	public ref ImVec2 WorkPos => ref this._workPos;
	/// <summary>
	/// Work Area: Size of the viewport minus task bars, menu bars, status bars (<= Size)
	/// </summary>
	public ref ImVec2 WorkSize => ref this._workSize;
	// Platform/Backend Dependent Data
	/// <summary>
	/// void* to hold lower-level, platform-native window handle (under Win32 this is expected to be a HWND, unused for other platforms)
	/// </summary>
	public ref nint PlatformHandleRaw => ref this._platformHandleRaw;
};