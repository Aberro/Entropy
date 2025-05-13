#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImGuiID = uint;

namespace Entropy.UI.ImGUI;
public struct ImGuiDeactivatedItemData
{
	public ImGuiID ID;
	public int ElapseFrame;
	public bool HasBeenEditedBefore;
	public bool IsAlive;
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member