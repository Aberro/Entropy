#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImGuiNET;
using ImGuiID = uint;

namespace Entropy.UI.ImGUI;

public struct ImGuiWindowSettings
{
	public ImGuiID ID;
	public Vector2ih Pos;
	public Vector2ih Size;
	public bool Collapsed;
	public bool IsChild;
	public bool WantApply; // Set when loaded from .ini data (to enable merging/loading .ini data into an already running context)
	public bool WantDelete; // Set to invalidate/delete the settings entry
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member