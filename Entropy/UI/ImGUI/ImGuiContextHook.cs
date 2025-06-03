#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImGuiNET;
using ImGuiID = uint;

namespace Entropy.UI.ImGUI;

public struct ImGuiContextHook
{
	public ImGuiID HookId; // A unique ID assigned by AddContextHook()
	public ImGuiContextHookType Type;
	public ImGuiID Owner;
	public IntPtr Callback;
	public IntPtr UserData;
}

public delegate void ImGuiContextHookCallback(ImGuiContextPtr ctx, ImGuiContextHookPtr hook);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member