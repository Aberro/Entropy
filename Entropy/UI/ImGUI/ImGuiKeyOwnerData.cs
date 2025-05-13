#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImGuiID = uint;

namespace Entropy.UI.ImGUI;

public struct ImGuiKeyOwnerData
{
	public const ImGuiID Any = 0;
	public const ImGuiID NoOwner = uint.MaxValue;
	public ImGuiID OwnerCurr;
	public ImGuiID OwnerNext;
	public bool LockThisFrame; // Reading this key requires explicit owner id (until Last of frame). Set by ImGuiInputFlags_LockThisFrame.
	public bool LockUntilRelease; // Reading this key requires explicit owner id (until key is released). Set by ImGuiInputFlags_LockUntilRelease. When this is true LockThisFrame is always true as well.

	public ImGuiKeyOwnerData()
	{
		this.OwnerCurr = this.OwnerNext = NoOwner;
		this.LockThisFrame = this.LockUntilRelease = false;
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member