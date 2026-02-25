#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImGuiNET;
using UnityEngine;
using ImGuiID = uint;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiPopupData
{
	private ImGuiID _popupId;
	private ImGuiWindow* _window;
	private ImGuiWindow* _restoreNavWindow;
	private int _parentNavLayer;
	private int _openFrameCount;
	private ImGuiID _openParentId;
	private ImVec2 _openPopupPos;
	private ImVec2 _openMousePos;

	/// <summary>
	/// Set on OpenPopup()
	/// </summary>
	public ref ImGuiID PopupId => ref this._popupId;
	/// <summary>
	/// Resolved on BeginPopup() - may stay unresolved if user never calls OpenPopup()
	/// </summary>
	public ref ImGuiWindow Window => ref *this._window;
	public bool HasWindow => this._window != null;
	public void SetWindow(ref ImGuiWindow window) => this._window = ImGuiHelper.GetPointer(ref window);
	public void ClearWindow() => this._window = null;
	/// <summary>
	/// Set on OpenPopup(), a NavWindow that will be restored on popup close
	/// </summary>
	public ref ImGuiWindow RestoreNavWindow => ref *this._restoreNavWindow;
	public bool HasRestoreNavWindow => this._restoreNavWindow != null;
	public void SetRestoreNavWindow(ref ImGuiWindow window) => this._restoreNavWindow = ImGuiHelper.GetPointer(ref window);
	public void ClearRestoreNavWindow() => this._restoreNavWindow = null;
	/// <summary>
	/// Resolved on BeginPopup(). Actually a ImGuiNavLayer type (declared down below), initialized to -1 which is not part of an enum, but serves well-enough as "not any of layers" value
	/// </summary>
	public ref int ParentNavLayer => ref this._parentNavLayer;
	/// <summary>
	/// Set on OpenPopup()
	/// </summary>
	public ref int OpenFrameCount => ref this._openFrameCount;
	/// <summary>
	/// Set on OpenPopup(), we need this to differentiate multiple menu sets from each others (e.g. inside menu bar vs loose menu items)
	/// </summary>
	public ref ImGuiID OpenParentId => ref this._openParentId;
	/// <summary>
	/// Set on OpenPopup(), preferred popup position (typically == OpenMousePos when using mouse)
	/// </summary>
	public ref ImVec2 OpenPopupPos => ref this._openPopupPos;
	/// <summary>
	/// Set on OpenPopup(), copy of mouse position at the time of opening popup
	/// </summary>
	public ref ImVec2 OpenMousePos => ref this._openMousePos;

	public ImGuiPopupData() => this._parentNavLayer = this._openFrameCount = -1;
}