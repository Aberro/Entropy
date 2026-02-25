#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImGuiNET;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using ImU8 = byte;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImDrawListSharedData
{
	public const int IM_DRAWLIST_ARCFAST_TABLE_SIZE = 48;
	public const int IM_DRAWLIST_TEX_LINES_WIDTH_MAX = 63;

	private ImVec2 _texUvWhitePixel;
	private ImFont* _font;
	private float _fontSize;
	private float _curveTessellationTol;
	private float _circleSegmentMaxError;
	private ImVec4 _clipRectFullscreen;
	private ImDrawListFlags _initialFlags;
	private fixed float _arcFastVtx[IM_DRAWLIST_ARCFAST_TABLE_SIZE * 2];
	private float _arcFastRadiusCutoff;
	private fixed ImU8 _circleSegmentCounts[64];
	private ImVec4* _texUvLines;

	/// <summary>
	/// UV of white pixel in the atlas
	/// </summary>
	public ref ImVec2 TexUvWhitePixel => ref this._texUvWhitePixel;

	/// <summary>
	/// Current/default font (optional, for simplified AddText overload)
	/// </summary>
	public ref ImFont Font => ref *this._font;
	/// <summary>
	/// Current/default font size (optional, for simplified AddText overload)
	/// </summary>
	public ref float FontSize => ref this._fontSize;
	/// <summary>
	/// Tessellation tolerance when using PathBezierCurveTo()
	/// </summary>
	public ref float CurveTessellationTol => ref this._curveTessellationTol;
	/// <summary>
	/// Number of circle segments to use per pixel of radius for AddCircle() etc
	/// </summary>
	public ref float CircleSegmentMaxError => ref this._circleSegmentMaxError;
	/// <summary>
	/// Value for PushClipRectFullscreen()
	/// </summary>
	public ref ImVec4 ClipRectFullscreen => ref this._clipRectFullscreen;
	/// <summary>
	/// Initial flags at the beginning of the frame (it is possible to alter flags on a per-drawlist basis afterwards)
	/// </summary>
	public ref ImDrawListFlags InitialFlags => ref this._initialFlags;

	// [Internal] Lookup tables
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
	public ref float ArcFastRadiusCutoff => ref this._arcFastRadiusCutoff;
	/// <summary>
	/// Precomputed segment count for given radius before we calculate it dynamically (to avoid calculation overhead)
	/// </summary>
	public Span<ImU8> CircleSegmentCounts
	{
		get
		{
			fixed(void* ptr = this._circleSegmentCounts)
			{
				return new Span<ImU8>(ptr, 64);
			}
		}
	}
	/// <summary>
	/// UV of anti-aliased lines in the atlas
	/// </summary>
	public Span<ImVec4> TexUvLines => new(this._texUvLines, IM_DRAWLIST_TEX_LINES_WIDTH_MAX);

	public unsafe void SetCircleTessellationMaxError(float max_error) => ImDrawListSharedData_SetCircleTessellationMaxError(ref this, max_error);

	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawListSharedData_SetCircleTessellationMaxError(ref ImDrawListSharedData self, float max_error);
}