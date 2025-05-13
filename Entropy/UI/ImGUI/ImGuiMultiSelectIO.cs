#pragma warning disable CS1591, CS0169 // Missing XML comment for publicly visible type or member
namespace Entropy.UI.ImGUI;

public struct ImGuiMultiSelectIO
{
	public ImVector<ImGuiSelectionRequest> Requests;
	public ImGuiSelectionUserData RangeSrcItem;
	public ImGuiSelectionUserData NavIdItem;
	public bool NavIdSelected;
	public bool RangeSrcReset;
	public int ItemsCount;
}
#pragma warning restore CS1591, CS0169 // Missing XML comment for publicly visible type or member