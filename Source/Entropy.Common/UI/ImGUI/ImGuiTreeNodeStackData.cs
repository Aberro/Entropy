#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImGuiNET;
using ImGuiID = uint;
using ImGuiTableColumnIdx = short;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiTreeNodeStackData
{
	private ImGuiID _id;
	private ImGuiTreeNodeFlags _treeFlags;
	private ImGuiItemFlags _itemFlags; // Used for nav landing
	private ImRect _navRect; // Used for nav landing
	private float _drawLinesX1;
	private float _drawLinesToNodesY2;
	private ImGuiTableColumnIdx _drawLinesTableColumn;

	public ref ImGuiID ID => ref this._id;
	public ref ImGuiTreeNodeFlags TreeFlags => ref this._treeFlags;
	/// <summary>
	/// Used for nav landing
	/// </summary>
	public ref ImGuiItemFlags ItemFlags => ref this._itemFlags;
	/// <summary>
	/// Used for nav landing
	/// </summary>
	public ref ImRect NavRect => ref this._navRect;
	public ref float DrawLinesX1 => ref this._drawLinesX1;
	public ref float DrawLinesToNodesY2 => ref this._drawLinesToNodesY2;
	public ref ImGuiTableColumnIdx DrawLinesTableColumn => ref this._drawLinesTableColumn;
}