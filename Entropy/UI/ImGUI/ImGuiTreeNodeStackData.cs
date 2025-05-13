#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImGuiNET;
using ImGuiID = uint;
using ImGuiTableColumnIdx = short;

namespace Entropy.UI.ImGUI;

public struct ImGuiTreeNodeStackData
{
	public ImGuiID ID;
	public ImGuiTreeNodeFlags TreeFlags;
	public ImGuiItemFlags ItemFlags; // Used for nav landing
	public ImRect NavRect; // Used for nav landing
	public float DrawLinesX1;
	public float DrawLinesToNodesY2;
	public ImGuiTableColumnIdx DrawLinesTableColumn;
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member