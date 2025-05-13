
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImGuiNET;

namespace Entropy.UI.ImGUI;

public struct ImGuiWindowStackData
{
	public ImGuiWindowPtr Window;
	public ImGuiLastItemData ParentLastItemDataBackup;
	public ImGuiErrorRecoveryState StackSizesInBegin;          // Store size of various stacks for asserting
	public bool DisabledOverrideReenable;   // Non-child window override disabled flag
	public float DisabledOverrideReenableAlphaBackup;
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member