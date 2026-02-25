#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImGuiNET;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using ImDrawIdx = System.UInt16;
using ImTextureID = System.IntPtr;
using ImU32 = System.UInt32;

namespace Entropy.Common.UI.ImGUI;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate void ImDrawCallback(ImDrawList* parent_list, ImDrawCmd* cmd);
public unsafe struct ImDrawList
{
	// This is what you have to render
	private ImVector<ImDrawCmd> _cmdBuffer;
	private ImVector<ImDrawIdx> _idxBuffer;
	private ImVector<ImDrawVert> _vtxBuffer;
	private ImDrawListFlags _flags;
	private uint _vtxCurrentIdx;
	private ImDrawListSharedData* _data;
	private byte* _ownerName;
	private ImDrawVert* _vtxWritePtr;
	private ImDrawIdx* _idxWritePtr;
	private ImVector<ImVec4> _clipRectStack;
	private ImVector<ImTextureID> _textureIdStack;
	private ImVector<ImVec2> _path;
	private ImDrawCmdHeader _cmdHeader;
	private ImDrawListSplitter _splitter;
	private float _fringeScale;


	/// <summary>
	/// Draw commands. Typically 1 command = 1 GPU draw call, unless the command is a callback.
	/// </summary>
	public ref ImVector<ImDrawCmd> CmdBuffer => ref this._cmdBuffer;
	/// <summary>
	/// Index buffer. Each command consume ImDrawCmd::ElemCount of those
	/// </summary>
	public ref ImVector<ImDrawIdx> IdxBuffer => ref this._idxBuffer;
	/// <summary>
	/// Vertex buffer.
	/// </summary>
	public ref ImVector<ImDrawVert> VtxBuffer => ref this._vtxBuffer;
	/// <summary>
	/// Flags, you may poke into these to adjust anti-aliasing settings per-primitive.
	/// </summary>
	public ref ImDrawListFlags Flags => ref this._flags;

	// [Internal, used while building lists]
	/// <summary>
	/// [Internal] generally == VtxBuffer.Size unless we are past 64K vertices, in which case this gets reset to 0.
	/// </summary>
	public ref uint VtxCurrentIdx => ref this._vtxCurrentIdx;

	/// <summary>
	/// Pointer to shared draw data (you can use ImGui::GetDrawListSharedData() to get the one from current ImGui context)
	/// </summary>
	public ref ImDrawListSharedData Data => ref *this._data;

	/// <summary>
	/// Pointer to owner window's name for debugging
	/// </summary>
	public string OwnerName => ImGuiHelper.GetString(this._ownerName);

	/// <summary>
	/// [Internal] point within VtxBuffer.Data after each add command (to avoid using the ImVector<> operators too much)
	/// </summary>
	public ref ImDrawVert VtxWritePtr => ref *this._vtxWritePtr;

	/// <summary>
	/// [Internal] point within IdxBuffer.Data after each add command (to avoid using the ImVector<> operators too much)
	/// </summary>
	public ref ImDrawIdx IdxWritePtr => ref *this._idxWritePtr;

	/// <summary>
	/// [Internal]
	/// </summary>
	public ref ImVector<ImVec4> ClipRectStack => ref this._clipRectStack;

	/// <summary>
	/// [Internal]
	/// </summary>
	public ref ImVector<ImTextureID> TextureIdStack => ref this._textureIdStack;

	/// <summary>
	/// [Internal] current path building
	/// </summary>
	public ref ImVector<ImVec2> Path => ref this._path;

	/// <summary>
	/// [Internal] template of active commands. Fields should match those of CmdBuffer.back().
	/// </summary>
	public ref ImDrawCmdHeader CmdHeader => ref this._cmdHeader;

	/// <summary>
	/// [Internal] for channels api (note: prefer using your own persistent instance of ImDrawListSplitter!)
	/// </summary>
	public ref ImDrawListSplitter Splitter => ref this._splitter;

	/// <summary>
	/// [Internal] anti-alias fringe is scaled by this value, this helps to keep things sharp while zooming at vertex buffer content
	/// </summary>
	public ref float FringeScale => ref this._fringeScale;

	/// <summary>
	/// If you want to create ImDrawList instances, pass them ImGui::GetDrawListSharedData() or create and use your own ImDrawListSharedData (so you can use ImDrawList without ImGui)
	/// </summary>
	/// <param name="shared_data"></param>
	public ImDrawList(ref ImDrawListSharedData shared_data) => this._data = (ImDrawListSharedData*)UnsafeUtility.AddressOf(ref shared_data);
	/// <summary>
	/// Render-level scissoring. This is passed down to your render function but not used for CPU-side coarse clipping. Prefer using higher-level ImGui::PushClipRect() to affect logic (hit-testing and widget culling)
	/// </summary>
	public void PushClipRect(ref ImVec2 clip_rect_min, ref ImVec2 clip_rect_max, bool intersect_with_current_clip_rect = false)
		=> ImDrawList_PushClipRect(ref this, ref clip_rect_min, ref clip_rect_max, intersect_with_current_clip_rect);
	public void PushClipRectFullScreen()
		=> ImDrawList_PushClipRectFullScreen(ref this);
	public void PopClipRect()
		=> ImDrawList_PopClipRect(ref this);
	public void PushTextureID(ImTextureID texture_id)
		=> ImDrawList_PushTextureID(ref this, texture_id);
	public void PopTextureID()
		=> ImDrawList_PopTextureID(ref this);
	public ImVec2 GetClipRectMin() 
	{
		ref var cr = ref ClipRectStack.GetLast();
		return new ImVec2(cr.X, cr.Y);
	}
	public ImVec2 GetClipRectMax()
	{
		ref var cr = ref ClipRectStack.GetLast();
		return new ImVec2(cr.Z, cr.W);
	}

	// Primitives
	// - Filled shapes must always use clockwise winding order. The anti-aliasing fringe depends on it. Counter-clockwise shapes will have "inward" anti-aliasing.
	// - For rectangular primitives, "p_min" and "p_max" represent the upper-left and lower-right corners.
	// - For circle primitives, use "num_segments == 0" to automatically calculate tessellation (preferred).
	// In older versions (until Dear ImGui 1.77) the AddCircle functions defaulted to num_segments == 12.
	// In future versions we will use textures to provide cheaper and higher-quality circles.
	// Use AddNgon() and AddNgonFilled() functions if you need to guaranteed a specific number of sides.
	public void AddLine(ref ImVec2 p1, ref ImVec2 p2, ImU32 col, float thickness = 1.0f)
		=> ImDrawList_AddLine(ref this, ref p1, ref p2, col, thickness);
	/// <summary>
	/// a: upper-left, b: lower-right (== upper-left + size)
	/// </summary>
	public void AddRect(ref ImVec2 p_min, ref ImVec2 p_max, ImU32 col, float rounding = 0.0f, ImDrawFlags flags = 0, float thickness = 1.0f)
		=> ImDrawList_AddRect(ref this, ref p_min, ref p_max, col, rounding, flags, thickness);
	/// <summary>
	/// a: upper-left, b: lower-right (== upper-left + size)
	/// </summary>
	public void AddRectFilled(ref ImVec2 p_min, ref ImVec2 p_max, ImU32 col, float rounding = 0.0f, ImDrawFlags flags = 0)
		=> ImDrawList_AddRectFilled(ref this, ref p_min, ref p_max, col, rounding, flags);
	public void AddRectFilledMultiColor(ref ImVec2 p_min, ref ImVec2 p_max, ImU32 col_upr_left, ImU32 col_upr_right, ImU32 col_bot_right, ImU32 col_bot_left)
		=> ImDrawList_AddRectFilledMultiColor(ref this, ref p_min, ref p_max, col_upr_left, col_upr_right, col_bot_right, col_bot_left);
	public void AddQuad(ref ImVec2 p1, ref ImVec2 p2, ref ImVec2 p3, ref ImVec2 p4, ImU32 col, float thickness = 1.0f)
		=> ImDrawList_AddQuad(ref this, ref p1, ref p2, ref p3, ref p4, col, thickness);
	public void AddQuadFilled(ref ImVec2 p1, ref ImVec2 p2, ref ImVec2 p3, ref ImVec2 p4, ImU32 col)
		=> ImDrawList_AddQuadFilled(ref this, ref p1, ref p2, ref p3, ref p4, col);
	public void AddTriangle(ref ImVec2 p1, ref ImVec2 p2, ref ImVec2 p3, ImU32 col, float thickness = 1.0f)
		=> ImDrawList_AddTriangle(ref this, ref p1, ref p2, ref p3, col, thickness);
	public void AddTriangleFilled(ref ImVec2 p1, ref ImVec2 p2, ref ImVec2 p3, ImU32 col)
		=> ImDrawList_AddTriangleFilled(ref this, ref p1, ref p2, ref p3, col);
	public void AddCircle(ref ImVec2 center, float radius, ImU32 col, int num_segments = 0, float thickness = 1.0f)
		=> ImDrawList_AddCircle(ref this, ref center, radius, col, num_segments, thickness);
	public void AddCircleFilled(ref ImVec2 center, float radius, ImU32 col, int num_segments = 0)
		=> ImDrawList_AddCircleFilled(ref this, ref center, radius, col, num_segments);
	public void AddNgon(ref ImVec2 center, float radius, ImU32 col, int num_segments, float thickness = 1.0f)
		=> ImDrawList_AddNgon(ref this, ref center, radius, col, num_segments, thickness);
	public void AddNgonFilled(ref ImVec2 center, float radius, ImU32 col, int num_segments)
		=> ImDrawList_AddNgonFilled(ref this, ref center, radius, col, num_segments);
	/// <summary>
	/// Whenever possible, do not use! This has abyssal performance due to marshalling and memory allocation.
	/// </summary>
	public void AddText(ref ImVec2 pos, ImU32 col, string text) =>
		ImDrawList_AddText_Vec2(ref this, ref pos, col, ImGuiHelper.GetStringPointer(text), null);
	public unsafe void AddText(ref ImFont font, float font_size, ref ImVec2 pos, ImU32 col, string text, float wrap_width = 0.0f) =>
			ImDrawList_AddText_FontPtr(ref this, ref font, font_size, ref pos, col, ImGuiHelper.GetStringPointer(text), null, wrap_width, ref ImGuiHelper.GetRef<ImVec4>(null));
	public unsafe void AddText(ref ImFont font, float font_size, ref ImVec2 pos, ImU32 col, string text, ref ImVec4 cpu_fine_clip_rect, float wrap_width = 0.0f) =>
			ImDrawList_AddText_FontPtr(ref this, ref font, font_size, ref pos, col, ImGuiHelper.GetStringPointer(text), null, wrap_width, ref cpu_fine_clip_rect);
	public void AddPolyline(ImVec2[] points, ImU32 col, ImDrawFlags flags, float thickness)
	{
		if(points == null || points.Length == 0)
			return;
		fixed(ImVec2* pointsPtr = points)
		{
			ImDrawList_AddPolyline(ref this, pointsPtr, points.Length, col, flags, thickness);
		}
	}
	public void AddConvexPolyFilled(ImVec2[] points, int num_points, ImU32 col)
	{
		fixed(ImVec2* pointsPtr = points)
		{
			ImDrawList_AddConvexPolyFilled(ref this, pointsPtr, num_points, col);
		}
	}

	/// <summary>
	/// Cubic Bezier (4 control points)
	/// </summary>
	public void AddBezierCubic(ref ImVec2 p1, ref ImVec2 p2, ref ImVec2 p3, ref ImVec2 p4, ImU32 col, float thickness, int num_segments = 0)
		=> ImDrawList_AddBezierCubic(ref this, ref p1, ref p2, ref p3, ref p4, col, thickness, num_segments);
	/// <summary>
	/// Quadratic Bezier (3 control points)
	/// </summary>
	public void AddBezierQuadratic(ref ImVec2 p1, ref ImVec2 p2, ref ImVec2 p3, ImU32 col, float thickness, int num_segments = 0)
		=> ImDrawList_AddBezierQuadratic(ref this, ref p1, ref p2, ref p3, col, thickness, num_segments);

	// Image primitives
	// - Read FAQ to understand what ImTextureID is.
	// - "p_min" and "p_max" represent the upper-left and lower-right corners of the rectangle.
	// - "uv_min" and "uv_max" represent the normalized texture coordinates to use for those corners. Using (0,0)->(1,1) texture coordinates will generally display the entire texture.
	public void AddImage(ImTextureID user_texture_id, ref ImVec2 p_min, ref ImVec2 p_max, ImU32 col = 0xFFFFFFFF)
	{
		var uv_min = new ImVec2(0, 0);
		var uv_max = new ImVec2(1, 1);
		AddImage(user_texture_id, ref p_min, ref p_max, ref uv_min, ref uv_max, col);
	}
	public void AddImage(ImTextureID user_texture_id, ref ImVec2 p_min, ref ImVec2 p_max, ref ImVec2 uv_min, ref ImVec2 uv_max, ImU32 col = 0xFFFFFFFF)
		=> ImDrawList_AddImage(ref this, user_texture_id, ref p_min, ref p_max, ref uv_min, ref uv_max, col);
	public void AddImageQuad(ImTextureID user_texture_id, ref ImVec2 p1, ref ImVec2 p2, ref ImVec2 p3, ref ImVec2 p4, ImU32 col = 0xFFFFFFFF)
	{
		var uv1 = new ImVec2(0, 0);
		var uv2 = new ImVec2(1, 0);
		var uv3 = new ImVec2(1, 1);
		var uv4 = new ImVec2(0, 1);
		AddImageQuad(user_texture_id, ref p1, ref p2, ref p3, ref p4, ref uv1, ref uv2, ref uv3, ref uv4, col);
	}
	public void AddImageQuad(ImTextureID user_texture_id, ref ImVec2 p1, ref ImVec2 p2, ref ImVec2 p3, ref ImVec2 p4, ref ImVec2 uv1, ref ImVec2 uv2, ref ImVec2 uv3, ref ImVec2 uv4, ImU32 col = 0xFFFFFFFF)
		=> ImDrawList_AddImageQuad(ref this, user_texture_id, ref p1, ref p2, ref p3, ref p4, ref uv1, ref uv2, ref uv3, ref uv4, col);
	public void AddImageRounded(ImTextureID user_texture_id, ref ImVec2 p_min, ref ImVec2 p_max, ref ImVec2 uv_min, ref ImVec2 uv_max, ImU32 col, float rounding, ImDrawFlags flags = 0)
		=> ImDrawList_AddImageRounded(ref this, user_texture_id, ref p_min, ref p_max, ref uv_min, ref uv_max, col, rounding, flags);

	// Stateful path API, add points then finish with PathFillConvex() or PathStroke()
	// - Filled shapes must always use clockwise winding order. The anti-aliasing fringe depends on it. Counter-clockwise shapes will have "inward" anti-aliasing.
	public void PathClear() { Path.Shrink(0); }
	public void PathLineTo(ref ImVec2 pos)
	{
		Path.Add(ref pos);
	}
	public void PathLineToMergeDuplicate(ref ImVec2 pos)
	{ 
		if (Path.Size == 0 || Path.GetLast() != pos)
			Path.Add(ref pos);
	}
	public void PathFillConvex(ImU32 col) 
	{
		ImDrawList_AddConvexPolyFilled(ref this, ImGuiHelper.GetPointer(ref Path.Get(0)), Path.Size, col);
		Path.Shrink(0);
	}
	public void PathStroke(ImU32 col, ImDrawFlags flags = 0, float thickness = 1.0f)
	{
		ImDrawList_AddPolyline(ref this, ImGuiHelper.GetPointer(ref Path.Get(0)), Path.Size, col, flags, thickness);
		Path.Shrink(0);
	}
	public void PathArcTo(ref ImVec2 center, float radius, float a_min, float a_max, int num_segments = 0)
		=> ImDrawList_PathArcTo(ref this, ref center, radius, a_min, a_max, num_segments);
	/// <summary>
	/// Use precomputed angles for a 12 steps circle
	/// </summary>
	public void PathArcToFast(ref ImVec2 center, float radius, int a_min_of_12, int a_max_of_12)
		=> ImDrawList_PathArcToFast(ref this, ref center, radius, a_min_of_12, a_max_of_12);
	/// <summary>
	/// Cubic Bezier (4 control points)
	/// </summary>
	void PathBezierCubicCurveTo(ref ImVec2 p2, ref ImVec2 p3, ref ImVec2 p4, int num_segments = 0)
		=> ImDrawList_PathBezierCubicCurveTo(ref this, ref p2, ref p3, ref p4, num_segments);
	/// <summary>
	/// Quadratic Bezier (3 control points)
	/// </summary>
	void PathBezierQuadraticCurveTo(ref ImVec2 p2, ref ImVec2 p3, int num_segments = 0)
		=> ImDrawList_PathBezierQuadraticCurveTo(ref this, ref p2, ref p3, num_segments);
	void PathRect(ref ImVec2 rect_min, ref ImVec2 rect_max, float rounding = 0.0f, ImDrawFlags flags = 0)
		=> ImDrawList_PathRect(ref this, ref rect_min, ref rect_max, rounding, flags);

	// Advanced
	/// <summary>
	/// Your rendering function must check for 'UserCallback' in ImDrawCmd and call the function instead of rendering triangles.
	/// </summary>
	public void AddCallback(ImDrawCallback callback, void* callback_data)
		=> ImDrawList_AddCallback(ref this, callback, callback_data);
	/// <summary>
	/// This is useful if you need to forcefully create a new draw call (to allow for dependent rendering / blending). Otherwise primitives are merged into the same draw-call as much as possible
	/// </summary>
	public void AddDrawCmd()
		=> ImDrawList_AddDrawCmd(ref this);
	/// <summary>
	/// Create a clone of the CmdBuffer/IdxBuffer/VtxBuffer.
	/// </summary>
	/// <returns></returns>
	public ImDrawList* CloneOutput()
		=> ImDrawList_CloneOutput(ref this);

	// Advanced: Channels
	// - Use to split render into layers. By switching channels to can render out-of-order (e.g. submit FG primitives before BG primitives)
	// - Use to minimize draw calls (e.g. if going back-and-forth between multiple clipping rectangles, prefer to append into separate channels then merge at the end)
	// - FIXME-OBSOLETE: This API shouldn't have been in ImDrawList in the first place!
	// Prefer using your own persistent instance of ImDrawListSplitter as you can stack them.
	// Using the ImDrawList::ChannelsXXXX you cannot stack a split over another.
	public void ChannelsSplit(int count) => Splitter.Split(ref this, count);
	public void ChannelsMerge() => Splitter.Merge(ref this);
	public void ChannelsSetCurrent(int n) => Splitter.SetCurrentChannel(ref this, n);

	// Advanced: Primitives allocations
	// - We render triangles (three vertices)
	// - All primitives needs to be reserved via PrimReserve() beforehand.
	public void PrimReserve(int idx_count, int vtx_count)
		=> ImDrawList_PrimReserve(ref this, idx_count, vtx_count);
	public void PrimUnreserve(int idx_count, int vtx_count)
		=> ImDrawList_PrimUnreserve(ref this, idx_count, vtx_count);
	/// <summary>
	/// Axis aligned rectangle (composed of two triangles)
	/// </summary>
	public void PrimRect(ref ImVec2 a, ref ImVec2 b, ImU32 col)
		=> ImDrawList_PrimRect(ref this, ref a, ref b, col);
	public void PrimRectUV(ref ImVec2 a, ref ImVec2 b, ref ImVec2 uv_a, ref ImVec2 uv_b, ImU32 col)
		=> ImDrawList_PrimRectUV(ref this, ref a, ref b, ref uv_a, ref uv_b, col);
	public void PrimQuadUV(ref ImVec2 a, ref ImVec2 b, ref ImVec2 c, ref ImVec2 d, ref ImVec2 uv_a, ref ImVec2 uv_b, ref ImVec2 uv_c, ref ImVec2 uv_d, ImU32 col)
		=> ImDrawList_PrimQuadUV(ref this, ref a, ref b, ref c, ref d, ref uv_a, ref uv_b, ref uv_c, ref uv_d, col);
	public void PrimWriteVtx(ref ImVec2 pos, ref ImVec2 uv, ImU32 col)
	{
		VtxWritePtr.pos = pos;
		VtxWritePtr.uv = uv;
		VtxWritePtr.col = col;
		this._vtxWritePtr++;
		VtxCurrentIdx++;
	}
	public void PrimWriteIdx(ImDrawIdx idx)
	{
		*this._idxWritePtr = idx;
		IdxWritePtr++;
	}
	/// <summary>
	/// Write vertex with unique index
	/// </summary>
	public void PrimVtx(ref ImVec2 pos, ref ImVec2 uv, ImU32 col)
	{ 
		PrimWriteIdx((ImDrawIdx)VtxCurrentIdx); 
		PrimWriteVtx(ref pos, ref uv, col);
	}
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_PushClipRect(ref ImDrawList self, ref ImVec2 clip_rect_min, ref ImVec2 clip_rect_max, [MarshalAs(UnmanagedType.U1)] bool intersect_with_current_clip_rect);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_PushClipRectFullScreen(ref ImDrawList self);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_PopClipRect(ref ImDrawList self);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_PushTextureID(ref ImDrawList self, ImTextureID texture_id);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_PopTextureID(ref ImDrawList self);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_AddLine(ref ImDrawList self, ref ImVec2 p1, ref ImVec2 p2, ImU32 col, float thickness);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_AddRect(ref ImDrawList self, ref ImVec2 p_min, ref ImVec2 p_max, ImU32 col, float rounding, ImDrawFlags flags, float thickness);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_AddRectFilled(ref ImDrawList self, ref ImVec2 p_min, ref ImVec2 p_max, ImU32 col, float rounding, ImDrawFlags flags);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_AddRectFilledMultiColor(ref ImDrawList self, ref ImVec2 p_min, ref ImVec2 p_max, ImU32 col_upr_left, ImU32 col_upr_right, ImU32 col_bot_right, ImU32 col_bot_left);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_AddQuad(ref ImDrawList self, ref ImVec2 p1, ref ImVec2 p2, ref ImVec2 p3, ref ImVec2 p4, ImU32 col, float thickness);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_AddQuadFilled(ref ImDrawList self, ref ImVec2 p1, ref ImVec2 p2, ref ImVec2 p3, ref ImVec2 p4, ImU32 col);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_AddTriangle(ref ImDrawList self, ref ImVec2 p1, ref ImVec2 p2, ref ImVec2 p3, ImU32 col, float thickness);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_AddTriangleFilled(ref ImDrawList self, ref ImVec2 p1, ref ImVec2 p2, ref ImVec2 p3, ImU32 col);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_AddCircle(ref ImDrawList self, ref ImVec2 center, float radius, ImU32 col, int num_segments, float thickness);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_AddCircleFilled(ref ImDrawList self, ref ImVec2 center, float radius, ImU32 col, int num_segments);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_AddNgon(ref ImDrawList self, ref ImVec2 center, float radius, ImU32 col, int num_segments, float thickness);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_AddNgonFilled(ref ImDrawList self, ref ImVec2 center, float radius, ImU32 col, int num_segments);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_AddText_Vec2(ref ImDrawList self, ref ImVec2 pos, ImU32 col, byte* text_begin, byte* text_end);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_AddText_FontPtr(ref ImDrawList self, ref ImFont font, float font_size, ref ImVec2 pos, ImU32 col, byte* text_begin, byte* text_end, float wrap_width, ref ImVec4 cpu_fine_clip_rect);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_AddPolyline(ref ImDrawList self, ImVec2* points, int num_points, ImU32 col, ImDrawFlags flags, float thickness);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_AddConvexPolyFilled(ref ImDrawList self, ImVec2* points, int num_points, ImU32 col);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_AddBezierCubic(ref ImDrawList self, ref ImVec2 p1, ref ImVec2 p2, ref ImVec2 p3, ref ImVec2 p4, ImU32 col, float thickness, int num_segments);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_AddBezierQuadratic(ref ImDrawList self, ref ImVec2 p1, ref ImVec2 p2, ref ImVec2 p3, ImU32 col, float thickness, int num_segments);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_AddImage(ref ImDrawList self, ImTextureID user_texture_id, ref ImVec2 p_min, ref ImVec2 p_max, ref ImVec2 uv_min, ref ImVec2 uv_max, ImU32 col);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_AddImageQuad(ref ImDrawList self, ImTextureID user_texture_id, ref ImVec2 p1, ref ImVec2 p2, ref ImVec2 p3, ref ImVec2 p4, ref ImVec2 uv1, ref ImVec2 uv2, ref ImVec2 uv3, ref ImVec2 uv4, ImU32 col);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_AddImageRounded(ref ImDrawList self, ImTextureID user_texture_id, ref ImVec2 p_min, ref ImVec2 p_max, ref ImVec2 uv_min, ref ImVec2 uv_max, ImU32 col, float rounding, ImDrawFlags flags);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_PathArcTo(ref ImDrawList self, ref ImVec2 center, float radius, float a_min, float a_max, int num_segments);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_PathArcToFast(ref ImDrawList self, ref ImVec2 center, float radius, int a_min_of_12, int a_max_of_12);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_PathBezierCubicCurveTo(ref ImDrawList self, ref ImVec2 p2, ref ImVec2 p3, ref ImVec2 p4, int num_segments);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_PathBezierQuadraticCurveTo(ref ImDrawList self, ref ImVec2 p2, ref ImVec2 p3, int num_segments);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_PathRect(ref ImDrawList self, ref ImVec2 rect_min, ref ImVec2 rect_max, float rounding, ImDrawFlags flags);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_AddCallback(ref ImDrawList self, ImDrawCallback callback, void* callback_data);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern ImDrawList* ImDrawList_CloneOutput(ref ImDrawList self);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_PrimReserve(ref ImDrawList self, int idx_count, int vtx_count);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_AddDrawCmd(ref ImDrawList self);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_PrimUnreserve(ref ImDrawList self, int idx_count, int vtx_count);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_PrimRect(ref ImDrawList self, ref ImVec2 a, ref ImVec2 b, ImU32 col);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_PrimRectUV(ref ImDrawList self, ref ImVec2 a, ref ImVec2 b, ref ImVec2 uv_a, ref ImVec2 uv_b, ImU32 col);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawList_PrimQuadUV(ref ImDrawList self, ref ImVec2 a, ref ImVec2 b, ref ImVec2 c, ref ImVec2 d, ref ImVec2 uv_a, ref ImVec2 uv_b, ref ImVec2 uv_c, ref ImVec2 uv_d, ImU32 col);
}