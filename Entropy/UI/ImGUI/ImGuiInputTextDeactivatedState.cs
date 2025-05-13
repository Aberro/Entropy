#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Entropy.UI.ImGUI;
using ImGuiID = uint;

public struct ImGuiInputTextDeactivatedState
{
	public ImGuiID ID; // widget id owning the text state (which just got deactivated)
	public ImVector<char> TextA; // text buffer

	public void ClearFreeMemory()
	{
		this.ID = 0;
		this.TextA.Clear();
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member