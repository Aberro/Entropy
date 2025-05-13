#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImGuiNET;
using ImGuiID = uint;

namespace Entropy.UI.ImGUI;

public unsafe struct ImGuiNavItemData
{
	public ImGuiWindow* Window; // Init,Move    // Best candidate window (result->ItemWindow->RootWindowForNav == request->Window)

	public ImGuiID ID; // Init,Move    // Best candidate item ID
	public ImGuiID FocusScopeId; // Init,Move    // Best candidate focus scope ID
	public ImRect RectRel; // Init,Move    // Best candidate bounding box in window relative space
	public ImGuiItemFlags InFlags; // ????,Move    // Best candidate item flags
	public float DistBox; //      Move    // Best candidate box distance to current NavId
	public float DistCenter; //      Move    // Best candidate center distance to current NavId
	public float DistAxial; //      Move    // Best candidate axial distance to current NavId

	public ImGuiNavItemData() => Clear();

	void Clear()
	{
		this.Window = null;
		this.ID = this.FocusScopeId = 0;
		this.InFlags = 0;
		this.DistBox = this.DistCenter = this.DistAxial = float.MaxValue;
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member