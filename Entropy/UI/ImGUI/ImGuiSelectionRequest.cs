#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Entropy.UI.ImGUI;
using ImS8 = sbyte;

public struct ImGuiSelectionRequest
{
	public ImGuiSelectionRequestType Type;
	public bool Selected;
	public ImS8 RangeDirection;
	public ImGuiSelectionUserData RangeFirstItem;
	public ImGuiSelectionUserData RangeLastItem;
}

public enum ImGuiSelectionRequestType
{
	None = 0,
	/// <summary>
	/// Request app to clear selection (if Selected==false) or select all items (if Selected==true). We cannot set RangeFirstItem/RangeLastItem as its contents is entirely up to user (not necessarily an index)
	/// </summary>
	SetAll,
	/// <summary>
	/// Request app to select/unselect [RangeFirstItem..RangeLastItem] items (inclusive) based on value of Selected. Only EndMultiSelect() request this, app code can read after BeginMultiSelect() and it will always be false.
	/// </summary>
	SetRange,
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member