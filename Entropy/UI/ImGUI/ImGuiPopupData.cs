#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImGuiNET;
using UnityEngine;
using ImGuiID = uint;

namespace Entropy.UI.ImGUI;

public struct ImGuiPopupData
{
	public ImGuiID PopupId; // Set on OpenPopup()
	public ImGuiWindowPtr Window; // Resolved on BeginPopup() - may stay unresolved if user never calls OpenPopup()
	public ImGuiWindowPtr RestoreNavWindow; // Set on OpenPopup(), a NavWindow that will be restored on popup close

	public int ParentNavLayer; // Resolved on BeginPopup(). Actually a ImGuiNavLayer type (declared down below), initialized to -1 which is not part of an enum, but serves well-enough as "not any of layers" value

	public int OpenFrameCount; // Set on OpenPopup()

	public ImGuiID OpenParentId; // Set on OpenPopup(), we need this to differentiate multiple menu sets from each others (e.g. inside menu bar vs loose menu items)

	public ImVec2 OpenPopupPos; // Set on OpenPopup(), preferred popup position (typically == OpenMousePos when using mouse)
	public ImVec2 OpenMousePos; // Set on OpenPopup(), copy of mouse position at the time of opening popup

	public ImGuiPopupData() => this.ParentNavLayer = this.OpenFrameCount = -1;
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member