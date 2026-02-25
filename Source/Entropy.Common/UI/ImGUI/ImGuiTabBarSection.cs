#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types


namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiTabBarSection
{
	private int _tabCount;
	private float _width;
	private float _spacing;

	/// <summary>
	/// Number of tabs in this section.
	/// </summary>
	public ref int TabCount => ref this._tabCount;
	/// <summary>
	/// Sum of width of tabs in this section (after shrinking down)
	/// </summary>
	public ref float Width => ref this._width;
	/// <summary>
	/// Horizontal spacing at the end of the section.
	/// </summary>
	public ref float Spacing => ref this._spacing;
}