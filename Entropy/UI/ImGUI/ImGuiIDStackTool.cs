#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImGuiNET;
using ImGuiID = uint;

namespace Entropy.UI.ImGUI;

public struct ImGuiIDStackTool()
{
	public int LastActiveFrame;
	public int StackLevel; // -1: query stack and resize Results, >= 0: individual stack level
	public ImGuiID QueryId; // ID to query details for
	public ImVector<ImGuiStackLevelInfo> Results;
	public bool CopyToClipboardOnCtrlC;
	public float CopyToClipboardLastTime = -float.MinValue;
	public ImGuiTextBuffer ResultPathBuf;
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member