#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImGuiNET;
using System.Runtime.InteropServices;

namespace Entropy.UI.ImGUI;

public unsafe struct ImGuiStyle
{
	/// <summary>
	/// Global alpha applies to everything in Dear ImGui.
	/// </summary>
	public float Alpha;
	/// <summary>
	/// Additional alpha multiplier applied by BeginDisabled(). Multiply over current value of Alpha.
	/// </summary>
	public float DisabledAlpha;
	/// <summary>
	/// Padding within a window.
	/// </summary>
	public ImVec2 WindowPadding;
	/// <summary>
	/// Radius of window corners rounding. Set to 0.0f to have rectangular windows. Large values tend to lead to variety of artifacts and are not recommended.
	/// </summary>
	public float WindowRounding;
	/// <summary>
	/// Thickness of border around windows. Generally set to 0.0f or 1.0f. (Other values are not well tested and more CPU/GPU costly).
	/// </summary>
	public float WindowBorderSize;
	/// <summary>
	/// Minimum window size. This is a global setting. If you want to constraint individual windows, use SetNextWindowSizeConstraints().
	/// </summary>
	public ImVec2 WindowMinSize;
	/// <summary>
	/// Alignment for title bar text. Defaults to (0.0f,0.5f) for left-aligned,vertically centered.
	/// </summary>
	public ImVec2 WindowTitleAlign;
	/// <summary>
	/// Side of the collapsing/docking button in the title bar (None/Left/Right). Defaults to ImGuiDir_Left.
	/// </summary>
	public ImGuiDir WindowMenuButtonPosition;
	/// <summary>
	/// Radius of child window corners rounding. Set to 0.0f to have rectangular windows.
	/// </summary>
	public float ChildRounding;
	/// <summary>
	/// Thickness of border around child windows. Generally set to 0.0f or 1.0f. (Other values are not well tested and more CPU/GPU costly).
	/// </summary>
	public float ChildBorderSize;
	/// <summary>
	/// Radius of popup window corners rounding. (Note that tooltip windows use WindowRounding)
	/// </summary>
	public float PopupRounding;
	/// <summary>
	/// Thickness of border around popup/tooltip windows. Generally set to 0.0f or 1.0f. (Other values are not well tested and more CPU/GPU costly).
	/// </summary>
	public float PopupBorderSize;
	/// <summary>
	/// Padding within a framed rectangle (used by most widgets).
	/// </summary>
	public ImVec2 FramePadding;
	/// <summary>
	/// Radius of frame corners rounding. Set to 0.0f to have rectangular frame (used by most widgets).
	/// </summary>
	public float FrameRounding;
	/// <summary>
	/// Thickness of border around frames. Generally set to 0.0f or 1.0f. (Other values are not well tested and more CPU/GPU costly).
	/// </summary>
	public float FrameBorderSize;
	/// <summary>
	/// Horizontal and vertical spacing between widgets/lines.
	/// </summary>
	public ImVec2 ItemSpacing;
	/// <summary>
	/// Horizontal and vertical spacing between within elements of a composed widget (e.g. a slider and its label).
	/// </summary>
	public ImVec2 ItemInnerSpacing;
	/// <summary>
	/// Padding within a table cell
	/// </summary>
	public ImVec2 CellPadding;
	/// <summary>
	/// Expand reactive bounding box for touch-based system where touch position is not accurate enough. Unfortunately we don't sort widgets so priority on overlap will always be given to the first widget. So don't grow this too much!
	/// </summary>
	public ImVec2 TouchExtraPadding;
	/// <summary>
	/// Horizontal indentation when e.g. entering a tree node. Generally == (FontSize + FramePadding.x*2).
	/// </summary>
	public float IndentSpacing;
	/// <summary>
	/// Minimum horizontal spacing between two columns. Preferably > (FramePadding.x + 1).
	/// </summary>
	public float ColumnsMinSpacing;
	/// <summary>
	/// Width of the vertical scrollbar, Height of the horizontal scrollbar.
	/// </summary>
	public float ScrollbarSize;
	/// <summary>
	/// Radius of grab corners for scrollbar.
	/// </summary>
	public float ScrollbarRounding;
	/// <summary>
	/// Minimum width/height of a grab box for slider/scrollbar.
	/// </summary>
	public float GrabMinSize;
	/// <summary>
	/// Radius of grabs corners rounding. Set to 0.0f to have rectangular slider grabs.
	/// </summary>
	public float GrabRounding;
	/// <summary>
	/// The size in pixels of the dead-zone around zero on logarithmic sliders that cross zero.
	/// </summary>
	public float LogSliderDeadzone;
	/// <summary>
	/// Radius of upper corners of a tab. Set to 0.0f to have rectangular tabs.
	/// </summary>
	public float TabRounding;
	/// <summary>
	/// Thickness of border around tabs.
	/// </summary>
	public float TabBorderSize;
	/// <summary>
	/// Minimum width for close button to appears on an unselected tab when hovered. Set to 0.0f to always show when hovering, set to FLT_MAX to never show close button unless selected.
	/// </summary>
	public float TabMinWidthForCloseButton;
	/// <summary>
	/// Side of the color button in the ColorEdit4 widget (left/right). Defaults to ImGuiDir_Right.
	/// </summary>
	public ImGuiDir ColorButtonPosition;
	/// <summary>
	/// Alignment of button text when button is larger than text. Defaults to (0.5f, 0.5f) (centered).
	/// </summary>
	public ImVec2 ButtonTextAlign;
	/// <summary>
	/// Alignment of selectable text. Defaults to (0.0f, 0.0f) (top-left aligned). It's generally important to keep this left-aligned if you want to lay multiple items on a same line.
	/// </summary>
	public ImVec2 SelectableTextAlign;
	/// <summary>
	/// Window position are clamped to be visible within the display area or monitors by at least this amount. Only applies to regular windows.
	/// </summary>
	public ImVec2 DisplayWindowPadding;
	/// <summary>
	/// If you cannot see the edges of your screen (e.g. on a TV) increase the safe area padding. Apply to popups/tooltips as well regular windows. NB: Prefer configuring your TV sets correctly!
	/// </summary>
	public ImVec2 DisplaySafeAreaPadding;
	/// <summary>
	/// Scale software rendered mouse cursor (when io.MouseDrawCursor is enabled). May be removed later.
	/// </summary>
	public float MouseCursorScale;
	/// <summary>
	/// Enable anti-aliased lines/borders. Disable if you are really tight on CPU/GPU. Latched at the beginning of the frame (copied to ImDrawList).
	/// </summary>
	public bool AntiAliasedLines;
	/// <summary>
	/// Enable anti-aliased lines/borders using textures where possible. Require backend to render with bilinear filtering (NOT point/nearest filtering). Latched at the beginning of the frame (copied to ImDrawList).
	/// </summary>
	public bool AntiAliasedLinesUseTex;
	/// <summary>
	/// Enable anti-aliased edges around filled shapes (rounded rectangles, circles, etc.). Disable if you are really tight on CPU/GPU. Latched at the beginning of the frame (copied to ImDrawList).
	/// </summary>
	public bool AntiAliasedFill;
	/// <summary>
	/// Tessellation tolerance when using PathBezierCurveTo() without a specific number of segments. Decrease for highly tessellated curves (higher quality, more polygons), increase to reduce quality.
	/// </summary>
	public float CurveTessellationTol;
	/// <summary>
	/// Maximum error (in pixels) allowed when using AddCircle()/AddCircleFilled() or drawing rounded corner rectangles with no explicit segment count specified. Decrease for higher quality but more geometry.
	/// </summary>
	public float CircleTessellationMaxError;
	private fixed float _colors[(int)ImGuiCol.COUNT * 4];
	public Span<ImVec4> Colors
	{
		get
		{
			fixed(void* ptr = this._colors)
			{
				return new Span<ImVec4>(ptr, (int)ImGuiCol.COUNT);
			}
		}
	}

	public void ScaleAllSizes(float scale_factor) => ImGuiStyle_ScaleAllSizes(ref this, scale_factor);

	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImGuiStyle_ScaleAllSizes(ref ImGuiStyle self, float scale_factor);
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member