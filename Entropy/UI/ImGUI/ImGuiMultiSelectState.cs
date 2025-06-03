#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImGuiNET;
using ImGuiID = uint;
using ImS8 = sbyte;
using ImGuiWindowPtr = Entropy.UI.ImGUI.ImGuiWindowPtr;

namespace Entropy.UI.ImGUI;

public struct ImGuiMultiSelectState
{
	public ImGuiWindowPtr Window;
	public ImGuiID ID;
	public int LastFrameActive; // Last used frame-count, for GC.
	public int LastSelectionSize; // Set by BeginMultiSelect() based on optional info provided by user. May be -1 if unknown.
	public ImS8 RangeSelected; // -1 (don't have) or true/false
	public ImS8 NavIdSelected; // -1 (don't have) or true/false
	public ImGuiSelectionUserData RangeSrcItem; //
	public ImGuiSelectionUserData NavIdItem; // SetNextItemSelectionUserData() value for NavId (if part of submitted items)

	public ImGuiMultiSelectState()
	{
		this.Window = null;
		this.ID = 0;
		this.LastFrameActive = this.LastSelectionSize = 0;
		this.RangeSelected = this.NavIdSelected = -1;
		this.RangeSrcItem = this.NavIdItem = ImGuiSelectionUserData.Invalid;
	}
}
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member