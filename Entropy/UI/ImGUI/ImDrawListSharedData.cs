#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImGuiNET;
using System.Runtime.InteropServices;
using ImU8 = byte;

namespace Entropy.UI.ImGUI;


public unsafe struct ImDrawListSharedData
{
	public const int IM_DRAWLIST_ARCFAST_TABLE_SIZE = 48;
	/// <summary>
	/// UV of white pixel in the atlas
	/// </summary>
	public ImVec2 TexUvWhitePixel;
	/// <summary>
	/// Current/default font (optional, for simplified AddText overload)
	/// </summary>
	public ImFont* Font;
	/// <summary>
	/// Current/default font size (optional, for simplified AddText overload)
	/// </summary>
	public float FontSize;
	/// <summary>
	/// Tessellation tolerance when using PathBezierCurveTo()
	/// </summary>
	public float CurveTessellationTol;
	/// <summary>
	/// Number of circle segments to use per pixel of radius for AddCircle() etc
	/// </summary>
	public float CircleSegmentMaxError;
	/// <summary>
	/// Value for PushClipRectFullscreen()
	/// </summary>
	public ImVec4 ClipRectFullscreen;
	/// <summary>
	/// Initial flags at the beginning of the frame (it is possible to alter flags on a per-drawlist basis afterwards)
	/// </summary>
	public ImDrawListFlags InitialFlags;

	// [Internal] Lookup tables
	private fixed float _arcFastVtx[IM_DRAWLIST_ARCFAST_TABLE_SIZE * 2];
	/// <summary>
	/// Sample points on the quarter of the circle.
	/// </summary>
	public Span<ImVec2> ArcFastVtx
	{
		get
		{
			fixed(void* ptr = this._arcFastVtx)
			{
				return new Span<ImVec2>(ptr, IM_DRAWLIST_ARCFAST_TABLE_SIZE);
			}
		}
	}
	/// <summary>
	/// Cutoff radius after which arc drawing will fallback to slower PathArcTo()
	/// </summary>
	public float ArcFastRadiusCutoff;
	/// <summary>
	/// Precomputed segment count for given radius before we calculate it dynamically (to avoid calculation overhead)
	/// </summary>
	public fixed ImU8 CircleSegmentCounts[64];
	/// <summary>
	/// UV of anti-aliased lines in the atlas
	/// </summary>
	public ImVec4* TexUvLines;

	public unsafe void SetCircleTessellationMaxError(float max_error) => ImDrawListSharedData_SetCircleTessellationMaxError(ref this, max_error);

	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawListSharedData_SetCircleTessellationMaxError(ref ImDrawListSharedData self, float max_error);
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member