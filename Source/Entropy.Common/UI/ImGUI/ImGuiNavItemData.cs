#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImGuiNET;
using ImGuiID = uint;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiNavItemData
{
	private ImGuiWindow* _window;
	private ImGuiID _id;
	private ImGuiID _focusScopeId;
	private ImRect _rectRel;
	private ImGuiItemFlags _inFlags;
	private float _distBox;
	private float _distCenter;
	private float _distAxial;

	/// <summary>
	/// (Init,Move) Best candidate window (result->ItemWindow->RootWindowForNav == request->Window)
	/// </summary>
	public ref ImGuiWindow Window => ref *this._window;
	public bool HasWindow => this._window != null;
	public void SetWindow(ref ImGuiWindow window) => this._window = ImGuiHelper.GetPointer(ref window);
	public void ClearWindow() => this._window = null;
	/// <summary>
	/// (Init,Move) Best candidate item ID
	/// </summary>
	public ref ImGuiID ID => ref this._id;
	/// <summary>
	/// (Init,Move) Best candidate focus scope ID
	/// </summary>
	public ref ImGuiID FocusScopeId => ref this._focusScopeId;
	/// <summary>
	/// (Init,Move) Best candidate bounding box in window relative space
	/// </summary>
	public ref ImRect RectRel => ref this._rectRel;
	/// <summary>
	/// (????,Move) Best candidate item flags
	/// </summary>
	public ref ImGuiItemFlags InFlags => ref this._inFlags;
	/// <summary>
	/// (Move) Best candidate box distance to current NavId
	/// </summary>
	public ref float DistBox => ref this._distBox;
	/// <summary>
	/// (Move) Best candidate center distance to current NavId
	/// </summary>
	public ref float DistCenter => ref this._distCenter;
	/// <summary>
	/// (Move) Best candidate axial distance to current NavId
	/// </summary>
	public ref float DistAxial => ref this._distAxial;

	public ImGuiNavItemData() => Clear();

	void Clear()
	{
		this._window = null;
		this._id = this._focusScopeId = 0;
		this._inFlags = 0;
		this._distBox = this._distCenter = this._distAxial = float.MaxValue;
	}
}