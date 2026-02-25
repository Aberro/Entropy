#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImGuiNET;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiStyle
{
	private float _alpha;
	private float _disabledAlpha;
	private ImVec2 _windowPadding;
	private float _windowRounding;
	private float _windowBorderSize;
	private ImVec2 _windowMinSize;
	private ImVec2 _windowTitleAlign;
	private ImGuiDir _windowMenuButtonPosition;
	private float _childRounding;
	private float _childBorderSize;
	private float _popupRounding;
	private float _popupBorderSize;
	private ImVec2 _framePadding;
	private float _frameRounding;
	private float _frameBorderSize;
	private ImVec2 _itemSpacing;
	private ImVec2 _itemInnerSpacing;
	private ImVec2 _cellPadding;
	private ImVec2 _touchExtraPadding;
	private float _indentSpacing;
	private float _columnsMinSpacing;
	private float _scrollbarSize;
	private float _scrollbarRounding;
	private float _grabMinSize;
	private float _grabRounding;
	private float _logSliderDeadzone;
	private float _tabRounding;
	private float _tabBorderSize;
	private float _tabMinWidthForCloseButton;
	private ImGuiDir _colorButtonPosition;
	private ImVec2 _buttonTextAlign;
	private ImVec2 _selectableTextAlign;
	private ImVec2 _displayWindowPadding;
	private ImVec2 _displaySafeAreaPadding;
	private float _mouseCursorScale;
	[MarshalAs(UnmanagedType.U1)]
	private bool _antiAliasedLines;
	[MarshalAs(UnmanagedType.U1)]
	private bool _antiAliasedLinesUseTex;
	[MarshalAs(UnmanagedType.U1)]
	private bool _antiAliasedFill;
	private float _curveTessellationTol;
	private float _circleTessellationMaxError;
	private fixed float _colors[(int)ImGuiCol.COUNT * 4];

	/// <summary>
	/// Global alpha applies to everything in Dear ImGui.
	/// </summary>
	public ref float Alpha => ref this._alpha;
	/// <summary>
	/// Additional alpha multiplier applied by BeginDisabled(). Multiply over current value of Alpha.
	/// </summary>
	public ref float DisabledAlpha => ref this._disabledAlpha;
	/// <summary>
	/// Padding within a window.
	/// </summary>
	public ref ImVec2 WindowPadding => ref this._windowPadding;
	/// <summary>
	/// Radius of window corners rounding. Set to 0.0f to have rectangular windows. Large values tend to lead to variety of artifacts and are not recommended.
	/// </summary>
	public ref float WindowRounding => ref this._windowRounding;
	/// <summary>
	/// Thickness of border around windows. Generally set to 0.0f or 1.0f. (Other values are not well tested and more CPU/GPU costly).
	/// </summary>
	public ref float WindowBorderSize => ref this._windowBorderSize;
	/// <summary>
	/// Minimum window size. This is a global setting. If you want to constraint individual windows, use SetNextWindowSizeConstraints().
	/// </summary>
	public ref ImVec2 WindowMinSize => ref this._windowMinSize;
	/// <summary>
	/// Alignment for title bar text. Defaults to (0.0f,0.5f) for left-aligned,vertically centered.
	/// </summary>
	public ref ImVec2 WindowTitleAlign => ref this._windowTitleAlign;
	/// <summary>
	/// Side of the collapsing/docking button in the title bar (None/Left/Right). Defaults to ImGuiDir_Left.
	/// </summary>
	public ref ImGuiDir WindowMenuButtonPosition => ref this._windowMenuButtonPosition;
	/// <summary>
	/// Radius of child window corners rounding. Set to 0.0f to have rectangular windows.
	/// </summary>
	public ref float ChildRounding => ref this._childRounding;
	/// <summary>
	/// Thickness of border around child windows. Generally set to 0.0f or 1.0f. (Other values are not well tested and more CPU/GPU costly).
	/// </summary>
	public ref float ChildBorderSize => ref this._childBorderSize;
	/// <summary>
	/// Radius of popup window corners rounding. (Note that tooltip windows use WindowRounding)
	/// </summary>
	public ref float PopupRounding => ref this._popupRounding;
	/// <summary>
	/// Thickness of border around popup/tooltip windows. Generally set to 0.0f or 1.0f. (Other values are not well tested and more CPU/GPU costly).
	/// </summary>
	public ref float PopupBorderSize => ref this._popupBorderSize;
	/// <summary>
	/// Padding within a framed rectangle (used by most widgets).
	/// </summary>
	public ref ImVec2 FramePadding => ref this._framePadding;
	/// <summary>
	/// Radius of frame corners rounding. Set to 0.0f to have rectangular frame (used by most widgets).
	/// </summary>
	public ref float FrameRounding => ref this._frameRounding;
	/// <summary>
	/// Thickness of border around frames. Generally set to 0.0f or 1.0f. (Other values are not well tested and more CPU/GPU costly).
	/// </summary>
	public ref float FrameBorderSize => ref this._frameBorderSize;
	/// <summary>
	/// Horizontal and vertical spacing between widgets/lines.
	/// </summary>
	public ref ImVec2 ItemSpacing => ref this._itemSpacing;
	/// <summary>
	/// Horizontal and vertical spacing between within elements of a composed widget (e.g. a slider and its label).
	/// </summary>
	public ref ImVec2 ItemInnerSpacing => ref this._itemInnerSpacing;
	/// <summary>
	/// Padding within a table cell
	/// </summary>
	public ref ImVec2 CellPadding => ref this._cellPadding;
	/// <summary>
	/// Expand reactive bounding box for touch-based system where touch position is not accurate enough. Unfortunately we don't sort widgets so priority on overlap will always be given to the first widget. So don't grow this too much!
	/// </summary>
	public ref ImVec2 TouchExtraPadding => ref this._touchExtraPadding;
	/// <summary>
	/// Horizontal indentation when e.g. entering a tree node. Generally == (FontSize + FramePadding.x*2).
	/// </summary>
	public ref float IndentSpacing => ref this._indentSpacing;
	/// <summary>
	/// Minimum horizontal spacing between two columns. Preferably > (FramePadding.x + 1).
	/// </summary>
	public ref float ColumnsMinSpacing => ref this._columnsMinSpacing;
	/// <summary>
	/// Width of the vertical scrollbar, Height of the horizontal scrollbar.
	/// </summary>
	public ref float ScrollbarSize => ref this._scrollbarSize;
	/// <summary>
	/// Radius of grab corners for scrollbar.
	/// </summary>
	public ref float ScrollbarRounding => ref this._scrollbarRounding;
	/// <summary>
	/// Minimum width/height of a grab box for slider/scrollbar.
	/// </summary>
	public ref float GrabMinSize => ref this._grabMinSize;
	/// <summary>
	/// Radius of grabs corners rounding. Set to 0.0f to have rectangular slider grabs.
	/// </summary>
	public ref float GrabRounding => ref this._grabRounding;
	/// <summary>
	/// The size in pixels of the dead-zone around zero on logarithmic sliders that cross zero.
	/// </summary>
	public ref float LogSliderDeadzone => ref this._logSliderDeadzone;
	/// <summary>
	/// Radius of upper corners of a tab. Set to 0.0f to have rectangular tabs.
	/// </summary>
	public ref float TabRounding => ref this._tabRounding;
	/// <summary>
	/// Thickness of border around tabs.
	/// </summary>
	public ref float TabBorderSize => ref this._tabBorderSize;
	/// <summary>
	/// Minimum width for close button to appears on an unselected tab when hovered. Set to 0.0f to always show when hovering, set to FLT_MAX to never show close button unless selected.
	/// </summary>
	public ref float TabMinWidthForCloseButton => ref this._tabMinWidthForCloseButton;
	/// <summary>
	/// Side of the color button in the ColorEdit4 widget (left/right). Defaults to ImGuiDir_Right.
	/// </summary>
	public ref ImGuiDir ColorButtonPosition => ref this._colorButtonPosition;
	/// <summary>
	/// Alignment of button text when button is larger than text. Defaults to (0.5f, 0.5f) (centered).
	/// </summary>
	public ref ImVec2 ButtonTextAlign => ref this._buttonTextAlign;
	/// <summary>
	/// Alignment of selectable text. Defaults to (0.0f, 0.0f) (top-left aligned). It's generally important to keep this left-aligned if you want to lay multiple items on a same line.
	/// </summary>
	public ref ImVec2 SelectableTextAlign => ref this._selectableTextAlign;
	/// <summary>
	/// Window position are clamped to be visible within the display area or monitors by at least this amount. Only applies to regular windows.
	/// </summary>
	public ref ImVec2 DisplayWindowPadding => ref this._displayWindowPadding;
	/// <summary>
	/// If you cannot see the edges of your screen (e.g. on a TV) increase the safe area padding. Apply to popups/tooltips as well regular windows. NB: Prefer configuring your TV sets correctly!
	/// </summary>
	public ref ImVec2 DisplaySafeAreaPadding => ref this._displaySafeAreaPadding;
	/// <summary>
	/// Scale software rendered mouse cursor (when io.MouseDrawCursor is enabled). May be removed later.
	/// </summary>
	public ref float MouseCursorScale => ref this._mouseCursorScale;
	/// <summary>
	/// Enable anti-aliased lines/borders. Disable if you are really tight on CPU/GPU. Latched at the beginning of the frame (copied to ImDrawList).
	/// </summary>
	public ref bool AntiAliasedLines => ref this._antiAliasedLines;
	/// <summary>
	/// Enable anti-aliased lines/borders using textures where possible. Require backend to render with bilinear filtering (NOT point/nearest filtering). Latched at the beginning of the frame (copied to ImDrawList).
	/// </summary>
	public ref bool AntiAliasedLinesUseTex => ref this._antiAliasedLinesUseTex;
	/// <summary>
	/// Enable anti-aliased edges around filled shapes (rounded rectangles, circles, etc.). Disable if you are really tight on CPU/GPU. Latched at the beginning of the frame (copied to ImDrawList).
	/// </summary>
	public ref bool AntiAliasedFill => ref this._antiAliasedFill;
	/// <summary>
	/// Tessellation tolerance when using PathBezierCurveTo() without a specific number of segments. Decrease for highly tessellated curves (higher quality, more polygons), increase to reduce quality.
	/// </summary>
	public ref float CurveTessellationTol => ref this._curveTessellationTol;
	/// <summary>
	/// Maximum error (in pixels) allowed when using AddCircle()/AddCircleFilled() or drawing rounded corner rectangles with no explicit segment count specified. Decrease for higher quality but more geometry.
	/// </summary>
	public ref float CircleTessellationMaxError => ref this._circleTessellationMaxError;
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

	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImGuiStyle_ScaleAllSizes(ref ImGuiStyle self, float scale_factor);
}