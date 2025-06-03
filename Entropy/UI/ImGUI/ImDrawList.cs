#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
using Entropy.UI.ImGUI;
using ImGuiNET;
using System.Runtime.InteropServices;
using ImDrawIdx = System.UInt16;
using ImTextureID = System.IntPtr;
using ImU32 = System.UInt32;

namespace Entropy.UI.ImGUI;
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate void ImDrawCallback(ImDrawList* parent_list, ImDrawCmd* cmd);
public unsafe struct ImDrawList
{
	// This is what you have to render
	public ImVector<ImDrawCmd> CmdBuffer; // Draw commands. Typically 1 command = 1 GPU draw call, unless the command is a callback.
	public ImVector<ImDrawIdx> IdxBuffer; // Index buffer. Each command consume ImDrawCmd::ElemCount of those
	public ImVector<ImDrawVert> VtxBuffer; // Vertex buffer.
	public ImDrawListFlags Flags; // Flags, you may poke into these to adjust anti-aliasing settings per-primitive.

	// [Internal, used while building lists]
	public uint _VtxCurrentIdx; // [Internal] generally == VtxBuffer.Size unless we are past 64K vertices, in which case this gets reset to 0.
	public ImDrawListSharedData* _Data; // Pointer to shared draw data (you can use ImGui::GetDrawListSharedData() to get the one from current ImGui context)
	public char* _OwnerName; // Pointer to owner window's name for debugging
	public ImDrawVert* _VtxWritePtr; // [Internal] point within VtxBuffer.Data after each add command (to avoid using the ImVector<> operators too much)
	public ImDrawIdx* _IdxWritePtr; // [Internal] point within IdxBuffer.Data after each add command (to avoid using the ImVector<> operators too much)
	public ImVector<ImVec4> _ClipRectStack; // [Internal]
	public ImVector<ImTextureID> _TextureIdStack; // [Internal]
	public ImVector<ImVec2> _Path; // [Internal] current path building
	public ImDrawCmdHeader _CmdHeader; // [Internal] template of active commands. Fields should match those of CmdBuffer.back().
	public ImDrawListSplitter _Splitter; // [Internal] for channels api (note: prefer using your own persistent instance of ImDrawListSplitter!)
	public float _FringeScale; // [Internal] anti-alias fringe is scaled by this value, this helps to keep things sharp while zooming at vertex buffer content

	/// <summary>
	/// If you want to create ImDrawList instances, pass them ImGui::GetDrawListSharedData() or create and use your own ImDrawListSharedData (so you can use ImDrawList without ImGui)
	/// </summary>
	/// <param name="shared_data"></param>
	public ImDrawList(ImDrawListSharedData* shared_data) { this._Data = shared_data; }
	/// <summary>
	/// Render-level scissoring. This is passed down to your render function but not used for CPU-side coarse clipping. Prefer using higher-level ImGui::PushClipRect() to affect logic (hit-testing and widget culling)
	/// </summary>
	public void PushClipRect(in ImVec2 clip_rect_min, in ImVec2 clip_rect_max, bool intersect_with_current_clip_rect = false)
		=> ImDrawList_PushClipRect(this, clip_rect_min, clip_rect_max, intersect_with_current_clip_rect);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_PushClipRect(in ImDrawList self, in ImVec2 clip_rect_min, in ImVec2 clip_rect_max, bool intersect_with_current_clip_rect);
	public void PushClipRectFullScreen()
		=> ImDrawList_PushClipRectFullScreen(this);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_PushClipRectFullScreen(in ImDrawList self);
	public void PopClipRect()
		=> ImDrawList_PopClipRect(this);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_PopClipRect(in ImDrawList self);
	public void PushTextureID(ImTextureID texture_id)
		=> ImDrawList_PushTextureID(this, texture_id);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_PushTextureID(in ImDrawList self, ImTextureID texture_id);
	public void PopTextureID()
		=> ImDrawList_PopTextureID(this);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_PopTextureID(in ImDrawList self);
	public readonly ImVec2 GetClipRectMin() 
	{
		var cr = this._ClipRectStack.GetLast();
		return new ImVec2(cr.x, cr.y);
	}
	public readonly ImVec2 GetClipRectMax()
	{
		var cr = this._ClipRectStack.GetLast();
		return new ImVec2(cr.z, cr.w);
	}

	// Primitives
	// - Filled shapes must always use clockwise winding order. The anti-aliasing fringe depends on it. Counter-clockwise shapes will have "inward" anti-aliasing.
	// - For rectangular primitives, "p_min" and "p_max" represent the upper-left and lower-right corners.
	// - For circle primitives, use "num_segments == 0" to automatically calculate tessellation (preferred).
	// In older versions (until Dear ImGui 1.77) the AddCircle functions defaulted to num_segments == 12.
	// In future versions we will use textures to provide cheaper and higher-quality circles.
	// Use AddNgon() and AddNgonFilled() functions if you need to guaranteed a specific number of sides.
	public void AddLine(in ImVec2 p1, in ImVec2 p2, ImU32 col, float thickness = 1.0f)
		=> ImDrawList_AddLine(this, p1, p2, col, thickness);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_AddLine(in ImDrawList self, in ImVec2 p1, in ImVec2 p2, ImU32 col, float thickness);
	/// <summary>
	/// a: upper-left, b: lower-right (== upper-left + size)
	/// </summary>
	public void AddRect(in ImVec2 p_min, in ImVec2 p_max, ImU32 col, float rounding = 0.0f, ImDrawFlags flags = 0, float thickness = 1.0f)
		=> ImDrawList_AddRect(this, p_min, p_max, col, rounding, flags, thickness);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_AddRect(in ImDrawList self, in ImVec2 p_min, in ImVec2 p_max, ImU32 col, float rounding, ImDrawFlags flags, float thickness);
	/// <summary>
	/// a: upper-left, b: lower-right (== upper-left + size)
	/// </summary>
	public void AddRectFilled(in ImVec2 p_min, in ImVec2 p_max, ImU32 col, float rounding = 0.0f, ImDrawFlags flags = 0)
		=> ImDrawList_AddRectFilled(this, p_min, p_max, col, rounding, flags);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_AddRectFilled(in ImDrawList self, in ImVec2 p_min, in ImVec2 p_max, ImU32 col, float rounding, ImDrawFlags flags);
	public void AddRectFilledMultiColor(in ImVec2 p_min, in ImVec2 p_max, ImU32 col_upr_left, ImU32 col_upr_right, ImU32 col_bot_right, ImU32 col_bot_left)
		=> ImDrawList_AddRectFilledMultiColor(this, p_min, p_max, col_upr_left, col_upr_right, col_bot_right, col_bot_left);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_AddRectFilledMultiColor(in ImDrawList self, in ImVec2 p_min, in ImVec2 p_max, ImU32 col_upr_left, ImU32 col_upr_right, ImU32 col_bot_right, ImU32 col_bot_left);
	public void AddQuad(in ImVec2 p1, in ImVec2 p2, in ImVec2 p3, in ImVec2 p4, ImU32 col, float thickness = 1.0f)
		=> ImDrawList_AddQuad(this, p1, p2, p3, p4, col, thickness);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_AddQuad(in ImDrawList self, in ImVec2 p1, in ImVec2 p2, in ImVec2 p3, in ImVec2 p4, ImU32 col, float thickness);
	public void AddQuadFilled(in ImVec2 p1, in ImVec2 p2, in ImVec2 p3, in ImVec2 p4, ImU32 col)
		=> ImDrawList_AddQuadFilled(this, p1, p2, p3, p4, col);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_AddQuadFilled(in ImDrawList self, in ImVec2 p1, in ImVec2 p2, in ImVec2 p3, in ImVec2 p4, ImU32 col);
	public void AddTriangle(in ImVec2 p1, in ImVec2 p2, in ImVec2 p3, ImU32 col, float thickness = 1.0f)
		=> ImDrawList_AddTriangle(this, p1, p2, p3, col, thickness);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_AddTriangle(in ImDrawList self, in ImVec2 p1, in ImVec2 p2, in ImVec2 p3, ImU32 col, float thickness);
	public void AddTriangleFilled(in ImVec2 p1, in ImVec2 p2, in ImVec2 p3, ImU32 col)
		=> ImDrawList_AddTriangleFilled(this, p1, p2, p3, col);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_AddTriangleFilled(in ImDrawList self, in ImVec2 p1, in ImVec2 p2, in ImVec2 p3, ImU32 col);
	public void AddCircle(in ImVec2 center, float radius, ImU32 col, int num_segments = 0, float thickness = 1.0f)
		=> ImDrawList_AddCircle(this, center, radius, col, num_segments, thickness);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_AddCircle(in ImDrawList self, in ImVec2 center, float radius, ImU32 col, int num_segments, float thickness);
	public void AddCircleFilled(in ImVec2 center, float radius, ImU32 col, int num_segments = 0)
		=> ImDrawList_AddCircleFilled(this, center, radius, col, num_segments);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_AddCircleFilled(in ImDrawList self, in ImVec2 center, float radius, ImU32 col, int num_segments);
	public void AddNgon(in ImVec2 center, float radius, ImU32 col, int num_segments, float thickness = 1.0f)
		=> ImDrawList_AddNgon(this, center, radius, col, num_segments, thickness);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_AddNgon(in ImDrawList self, in ImVec2 center, float radius, ImU32 col, int num_segments, float thickness);
	public void AddNgonFilled(in ImVec2 center, float radius, ImU32 col, int num_segments)
		=> ImDrawList_AddNgonFilled(this, center, radius, col, num_segments);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_AddNgonFilled(in ImDrawList self, in ImVec2 center, float radius, ImU32 col, int num_segments);
	/// <summary>
	/// Whenever possible, do not use! This has abyssal performance due to marshalling and memory allocation.
	/// </summary>
	public void AddText(in ImVec2 pos, ImU32 col, string text)
	{
		var textPtr = Marshal.StringToCoTaskMemUTF8(text);
		try
		{
			ImDrawList_AddText_Vec2(this, pos, col, (char*)textPtr, null);
		}
		finally
		{
			Marshal.FreeCoTaskMem(textPtr);
		}
	}
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_AddText_Vec2(in ImDrawList self, in ImVec2 pos, ImU32 col, char* text_begin, char* text_end);
	public void AddText(ImFont* font, float font_size, in ImVec2 pos, ImU32 col, string text, float wrap_width = 0.0f)
	{
		var textPtr = Marshal.StringToCoTaskMemUTF8(text);
		try
		{
			ImDrawList_AddText_FontPtr(this, font, font_size, pos, col, (char*)textPtr, null, wrap_width, null);
		}
		finally
		{
			Marshal.FreeCoTaskMem(textPtr);
		}
	}
	public unsafe void AddText(ImFont* font, float font_size, in ImVec2 pos, ImU32 col, string text, in ImVec4 cpu_fine_clip_rect, float wrap_width = 0.0f)
	{
		var textPtr = Marshal.StringToCoTaskMemUTF8(text);
		try
		{
			fixed(ImVec4* clipRectPtr = &cpu_fine_clip_rect)
			{
				ImDrawList_AddText_FontPtr(this, font, font_size, pos, col, (char*)textPtr, null, wrap_width, clipRectPtr);
			}
		}
		finally
		{
			Marshal.FreeCoTaskMem(textPtr);
		}
	}
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_AddText_FontPtr(in ImDrawList self, ImFont* font, float font_size, in ImVec2 pos, ImU32 col, char* text_begin, char* text_end, float wrap_width, ImVec4* cpu_fine_clip_rect);
	public void AddPolyline(ImVec2[] points, ImU32 col, ImDrawFlags flags, float thickness)
	{
		if(points == null || points.Length == 0)
			return;
		fixed(ImVec2* pointsPtr = points)
		{
			ImDrawList_AddPolyline(this, pointsPtr, points.Length, col, flags, thickness);
		}
	}
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_AddPolyline(in ImDrawList self, ImVec2* points, int num_points, ImU32 col, ImDrawFlags flags, float thickness);
	public void AddConvexPolyFilled(in ImVec2 points, int num_points, ImU32 col)
	{
		fixed(ImVec2* pointsPtr = &points)
		{
			ImDrawList_AddConvexPolyFilled(this, pointsPtr, num_points, col);
		}
	}

	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_AddConvexPolyFilled(in ImDrawList self, ImVec2* points, int num_points, ImU32 col);
	/// <summary>
	/// Cubic Bezier (4 control points)
	/// </summary>
	public void AddBezierCubic(in ImVec2 p1, in ImVec2 p2, in ImVec2 p3, in ImVec2 p4, ImU32 col, float thickness, int num_segments = 0)
		=> ImDrawList_AddBezierCubic(this, p1, p2, p3, p4, col, thickness, num_segments);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_AddBezierCubic(in ImDrawList self, in ImVec2 p1, in ImVec2 p2, in ImVec2 p3, in ImVec2 p4, ImU32 col, float thickness, int num_segments);
	//Quadratic Bezier (3 control points)
	public void AddBezierQuadratic(in ImVec2 p1, in ImVec2 p2, in ImVec2 p3, ImU32 col, float thickness, int num_segments = 0)
		=> ImDrawList_AddBezierQuadratic(this, p1, p2, p3, col, thickness, num_segments);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_AddBezierQuadratic(in ImDrawList self, in ImVec2 p1, in ImVec2 p2, in ImVec2 p3, ImU32 col, float thickness, int num_segments);

	// Image primitives
	// - Read FAQ to understand what ImTextureID is.
	// - "p_min" and "p_max" represent the upper-left and lower-right corners of the rectangle.
	// - "uv_min" and "uv_max" represent the normalized texture coordinates to use for those corners. Using (0,0)->(1,1) texture coordinates will generally display the entire texture.
	public void AddImage(ImTextureID user_texture_id, in ImVec2 p_min, in ImVec2 p_max, ImU32 col = 0xFFFFFFFF)
		=> AddImage(user_texture_id, p_min, p_max, new ImVec2(0, 0), new ImVec2(1, 1), col);
	public void AddImage(ImTextureID user_texture_id, in ImVec2 p_min, in ImVec2 p_max, in ImVec2 uv_min, in ImVec2 uv_max, ImU32 col = 0xFFFFFFFF)
		=> ImDrawList_AddImage(this, user_texture_id, p_min, p_max, uv_min, uv_max, col);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_AddImage(in ImDrawList self, ImTextureID user_texture_id, in ImVec2 p_min, in ImVec2 p_max, in ImVec2 uv_min, in ImVec2 uv_max, ImU32 col);
	public void AddImageQuad(ImTextureID user_texture_id, in ImVec2 p1, in ImVec2 p2, in ImVec2 p3, in ImVec2 p4, ImU32 col = 0xFFFFFFFF)
		=> AddImageQuad(user_texture_id, p1, p2, p3, p4, new ImVec2(0, 0), new ImVec2(1, 0), new ImVec2(1, 1), new ImVec2(0, 1), col);
	public void AddImageQuad(ImTextureID user_texture_id, in ImVec2 p1, in ImVec2 p2, in ImVec2 p3, in ImVec2 p4, in ImVec2 uv1, in ImVec2 uv2, in ImVec2 uv3, in ImVec2 uv4, ImU32 col = 0xFFFFFFFF)
		=> ImDrawList_AddImageQuad(this, user_texture_id, p1, p2, p3, p4, uv1, uv2, uv3, uv4, col);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_AddImageQuad(in ImDrawList self, ImTextureID user_texture_id, in ImVec2 p1, in ImVec2 p2, in ImVec2 p3, in ImVec2 p4, in ImVec2 uv1, in ImVec2 uv2, in ImVec2 uv3, in ImVec2 uv4, ImU32 col);
	public void AddImageRounded(ImTextureID user_texture_id, in ImVec2 p_min, in ImVec2 p_max, in ImVec2 uv_min, in ImVec2 uv_max, ImU32 col, float rounding, ImDrawFlags flags = 0)
		=> ImDrawList_AddImageRounded(this, user_texture_id, p_min, p_max, uv_min, uv_max, col, rounding, flags);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_AddImageRounded(in ImDrawList self, ImTextureID user_texture_id, in ImVec2 p_min, in ImVec2 p_max, in ImVec2 uv_min, in ImVec2 uv_max, ImU32 col, float rounding, ImDrawFlags flags);

	// Stateful path API, add points then finish with PathFillConvex() or PathStroke()
	// - Filled shapes must always use clockwise winding order. The anti-aliasing fringe depends on it. Counter-clockwise shapes will have "inward" anti-aliasing.
	public void PathClear() { this._Path.Shrink(0); }
	public void PathLineTo(in ImVec2 pos)
	{
		fixed(ImVec2* posPtr = &pos)
		{
			this._Path.Add(posPtr);
		}
	}
	public void PathLineToMergeDuplicate(in ImVec2 pos)
	{ 
		if (this._Path.Size == 0 || this._Path.GetLast() != pos)
			fixed(ImVec2* posPtr = &pos)
			{
				this._Path.Add(posPtr);
			}
	}
	public void PathFillConvex(ImU32 col) 
	{
		ImDrawList_AddConvexPolyFilled(this, this._Path.GetPtr(0), this._Path.Size, col);
		this._Path.Shrink(0);
	}
	public void PathStroke(ImU32 col, ImDrawFlags flags = 0, float thickness = 1.0f)
	{
		ImDrawList_AddPolyline(this, this._Path.GetPtr(0), this._Path.Size, col, flags, thickness);
		this._Path.Shrink(0);
	}
	public void PathArcTo(in ImVec2 center, float radius, float a_min, float a_max, int num_segments = 0)
		=> ImDrawList_PathArcTo(this, center, radius, a_min, a_max, num_segments);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_PathArcTo(in ImDrawList self, in ImVec2 center, float radius, float a_min, float a_max, int num_segments);
	/// <summary>
	/// Use precomputed angles for a 12 steps circle
	/// </summary>
	public void PathArcToFast(in ImVec2 center, float radius, int a_min_of_12, int a_max_of_12)
		=> ImDrawList_PathArcToFast(this, center, radius, a_min_of_12, a_max_of_12);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_PathArcToFast(in ImDrawList self, in ImVec2 center, float radius, int a_min_of_12, int a_max_of_12);
	/// <summary>
	/// Cubic Bezier (4 control points)
	/// </summary>
	void PathBezierCubicCurveTo(in ImVec2 p2, in ImVec2 p3, in ImVec2 p4, int num_segments = 0)
		=> ImDrawList_PathBezierCubicCurveTo(this, p2, p3, p4, num_segments);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_PathBezierCubicCurveTo(in ImDrawList self, in ImVec2 p2, in ImVec2 p3, in ImVec2 p4, int num_segments);
	/// <summary>
	/// Quadratic Bezier (3 control points)
	/// </summary>
	void PathBezierQuadraticCurveTo(in ImVec2 p2, in ImVec2 p3, int num_segments = 0)
		=> ImDrawList_PathBezierQuadraticCurveTo(this, p2, p3, num_segments);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_PathBezierQuadraticCurveTo(in ImDrawList self, in ImVec2 p2, in ImVec2 p3, int num_segments);
	void PathRect(in ImVec2 rect_min, in ImVec2 rect_max, float rounding = 0.0f, ImDrawFlags flags = 0)
		=> ImDrawList_PathRect(this, rect_min, rect_max, rounding, flags);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_PathRect(in ImDrawList self, in ImVec2 rect_min, in ImVec2 rect_max, float rounding, ImDrawFlags flags);

	// Advanced
	/// <summary>
	/// Your rendering function must check for 'UserCallback' in ImDrawCmd and call the function instead of rendering triangles.
	/// </summary>
	public void AddCallback(ImDrawCallback callback, void* callback_data)
		=> ImDrawList_AddCallback(this, callback, callback_data);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_AddCallback(in ImDrawList self, ImDrawCallback callback, void* callback_data);
	/// <summary>
	/// This is useful if you need to forcefully create a new draw call (to allow for dependent rendering / blending). Otherwise primitives are merged into the same draw-call as much as possible
	/// </summary>
	public void AddDrawCmd()
		=> ImDrawList_AddDrawCmd(this);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_AddDrawCmd(in ImDrawList self);
	/// <summary>
	/// Create a clone of the CmdBuffer/IdxBuffer/VtxBuffer.
	/// </summary>
	/// <returns></returns>
	public readonly ImDrawList* CloneOutput()
		=> ImDrawList_CloneOutput(this);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern ImDrawList* ImDrawList_CloneOutput(in ImDrawList self);

	// Advanced: Channels
	// - Use to split render into layers. By switching channels to can render out-of-order (e.g. submit FG primitives before BG primitives)
	// - Use to minimize draw calls (e.g. if going back-and-forth between multiple clipping rectangles, prefer to append into separate channels then merge at the end)
	// - FIXME-OBSOLETE: This API shouldn't have been in ImDrawList in the first place!
	// Prefer using your own persistent instance of ImDrawListSplitter as you can stack them.
	// Using the ImDrawList::ChannelsXXXX you cannot stack a split over another.
	public void ChannelsSplit(int count) => this._Splitter.Split(this, count);
	public void ChannelsMerge() => this._Splitter.Merge(this);
	public void ChannelsSetCurrent(int n) => this._Splitter.SetCurrentChannel(this, n);

	// Advanced: Primitives allocations
	// - We render triangles (three vertices)
	// - All primitives needs to be reserved via PrimReserve() beforehand.
	public void PrimReserve(int idx_count, int vtx_count)
		=> ImDrawList_PrimReserve(this, idx_count, vtx_count);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_PrimReserve(in ImDrawList self, int idx_count, int vtx_count);
	public void PrimUnreserve(int idx_count, int vtx_count)
		=> ImDrawList_PrimUnreserve(this, idx_count, vtx_count);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_PrimUnreserve(in ImDrawList self, int idx_count, int vtx_count);
	/// <summary>
	/// Axis aligned rectangle (composed of two triangles)
	/// </summary>
	public void PrimRect(in ImVec2 a, in ImVec2 b, ImU32 col)
		=> ImDrawList_PrimRect(this, a, b, col);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_PrimRect(in ImDrawList self, in ImVec2 a, in ImVec2 b, ImU32 col);
	public void PrimRectUV(in ImVec2 a, in ImVec2 b, in ImVec2 uv_a, in ImVec2 uv_b, ImU32 col)
		=> ImDrawList_PrimRectUV(this, a, b, uv_a, uv_b, col);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_PrimRectUV(in ImDrawList self, in ImVec2 a, in ImVec2 b, in ImVec2 uv_a, in ImVec2 uv_b, ImU32 col);
	public void PrimQuadUV(in ImVec2 a, in ImVec2 b, in ImVec2 c, in ImVec2 d, in ImVec2 uv_a, in ImVec2 uv_b, in ImVec2 uv_c, in ImVec2 uv_d, ImU32 col)
		=> ImDrawList_PrimQuadUV(this, a, b, c, d, uv_a, uv_b, uv_c, uv_d, col);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawList_PrimQuadUV(in ImDrawList self, in ImVec2 a, in ImVec2 b, in ImVec2 c, in ImVec2 d, in ImVec2 uv_a, in ImVec2 uv_b, in ImVec2 uv_c, in ImVec2 uv_d, ImU32 col);
	public void PrimWriteVtx(in ImVec2 pos, in ImVec2 uv, ImU32 col)
	{
		this._VtxWritePtr->pos = pos;
		this._VtxWritePtr->uv = uv;
		this._VtxWritePtr->col = col;
		this._VtxWritePtr++;
		this._VtxCurrentIdx++;
	}
	public void PrimWriteIdx(ImDrawIdx idx)
	{
		*this._IdxWritePtr = idx;
		this._IdxWritePtr++;
	}
	/// <summary>
	/// Write vertex with unique index
	/// </summary>
	public void PrimVtx(in ImVec2 pos, in ImVec2 uv, ImU32 col)
	{ 
		PrimWriteIdx((ImDrawIdx)this._VtxCurrentIdx); 
		PrimWriteVtx(pos, uv, col);
	}
}