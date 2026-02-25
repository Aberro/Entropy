#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CS8656 // Call to non-readonly member from a 'readonly' member results in an implicit copy.
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImGuiNET;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiViewportP
{
	private ImGuiViewportFlags _flags;
	private ImVec2 _pos;
	private ImVec2 _size;
	private ImVec2 _workPos;
	private ImVec2 _workSize;
	private nint _platformHandleRaw;

	private fixed int _drawListsLastFrame[2];
	private ImDrawList* _drawLists_0;
	private ImDrawList* _drawLists_1;
	private ImDrawData _drawDataP;
	private ImDrawDataBuilder _drawDataBuilder;
	private ImVec2 _workOffsetMin;
	private ImVec2 _workOffsetMax;
	private ImVec2 _buildWorkOffsetMin;
	private ImVec2 _buildWorkOffsetMax;

	// Helpers to retrieve ImRect (we don't need to store BuildWorkRect as every access tend to change it, hence the code asymmetry)
	public readonly ImRect MainRect => new(this._pos.X, this._pos.Y, this._pos.X + this._size.X, this._pos.Y + this._size.Y);
	public readonly ImRect WorkRect => new(this._workPos.X, this._workPos.Y, this._workPos.X + this._workSize.X, this._workPos.Y + this._workSize.Y);

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

	/// <summary>
	/// Last frame number the background draw list were used
	/// </summary>
	public ref int DrawListLastFrameBackground => ref this._drawListsLastFrame[0];
	/// <summary>
	/// Last frame number the foreground draw list were used
	/// </summary>
	public ref int DrawListLastFrameForeground => ref this._drawListsLastFrame[1];
	/// <summary>
	/// Convenience background draw list. We use it to draw software mouser cursor when io.MouseDrawCursor is set and to draw most debug overlays.
	/// </summary>
	public ref ImDrawList DrawListBackground => ref *this._drawLists_0;
	public bool HasDrawListBackground => this._drawLists_0 != null;
	public void SetDrawListBackground(ref ImDrawList drawList)
	{
		fixed(ImDrawList* ptr = &drawList)
		{
			this._drawLists_0 = ptr;
		}
	}
	public void ClearDrawListBackgroup() => this._drawLists_0 = null;
	/// <summary>
	/// Convenience foreground draw list. We use it to draw software mouser cursor when io.MouseDrawCursor is set and to draw most debug overlays.
	/// </summary>
	public ref ImDrawList DrawListForeground => ref *this._drawLists_1;
	public bool HasDrawListForeground => this._drawLists_1 != null;
	public void SetDrawListForeground(ref ImDrawList drawList)
	{
		fixed(ImDrawList* ptr = &drawList)
		{
			this._drawLists_1 = ptr;
		}
	}
	public void ClearDrawListForeground() => this._drawLists_1 = null;
	public ref ImDrawData DrawDataP => ref this._drawDataP;
	public ref ImDrawDataBuilder DrawDataBuilder => ref this._drawDataBuilder;

	/// <summary>
	/// Work Area: Offset from Pos to top-left corner of Work Area. Generally (0,0) or (0,+main_menu_bar_height). Work Area is Full Area but without menu-bars/status-bars (so WorkArea always fit inside Pos/Size!)
	/// </summary>
	public ref ImVec2 WorkOffsetMin => ref this._workOffsetMin;
	/// <summary>
	/// Work Area: Offset from Pos+Size to bottom-right corner of Work Area. Generally (0,0) or (0,-status_bar_height).
	/// </summary>
	public ref ImVec2 WorkOffsetMax => ref this._workOffsetMax;
	/// <summary>
	/// Work Area: Offset being built during current frame. Generally >= 0.0f.
	/// </summary>
	public ref ImVec2 BuildWorkOffsetMin => ref this._buildWorkOffsetMin;
	/// <summary>
	/// Work Area: Offset being built during current frame. Generally <= 0.0f.
	/// </summary>
	public ref ImVec2 BuildWorkOffsetMax => ref this._buildWorkOffsetMax;

	public readonly ImVec2 Center => new(this._pos.X + (this._size.X * 0.5f), this._pos.Y + (this._size.Y * 0.5f));
	public readonly ImVec2 WorkCenter => new(this._workPos.X + (this._workSize.X * 0.5f), this._workPos.Y + (this._workSize.Y * 0.5f));
	/// <summary>
	/// Calculate work rect pos/size given a set of offset (we have 1 pair of offset for rect locked from last frame data, and 1 pair for currently building rect)
	/// </summary>
	public readonly ImVec2 CalcWorkRectPos(ref ImVec2 off_min) => new(this._pos.X + off_min.X, this._pos.Y + off_min.Y);
	public readonly ImVec2 CalcWorkRectSize(ref ImVec2 off_min, in ImVec2 off_max) => new(Math.Max(0.0f, this._size.X - off_min.X + off_max.X), Math.Max(0.0f, this._size.Y - off_min.Y + off_max.Y));
	/// <summary>
	/// Update public fields
	/// </summary>
	public void UpdateWorkRect()
	{
		WorkPos = CalcWorkRectPos(ref WorkOffsetMin);
		WorkSize = CalcWorkRectSize(ref WorkOffsetMin, WorkOffsetMax);
	}

	public readonly ImRect GetBuildWorkRect()
	{
		var pos = CalcWorkRectPos(ref BuildWorkOffsetMin);
		var size = CalcWorkRectSize(ref BuildWorkOffsetMin, BuildWorkOffsetMax);
		return new ImRect(pos.X, pos.Y, pos.X + size.X, pos.Y + size.Y);
	}
};
