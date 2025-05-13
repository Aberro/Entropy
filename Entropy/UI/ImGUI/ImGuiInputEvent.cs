#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImGuiNET;
using System.Runtime.InteropServices;
using ImU32 = uint;

namespace Entropy.UI.ImGUI;

[StructLayout(LayoutKind.Explicit)]
public struct ImGuiInputEvent
{
	[FieldOffset(0)] public ImGuiInputEventType Type;
	[FieldOffset(4)] public ImGuiInputSource Source;
	[FieldOffset(8)] public ImU32 EventId;

	[FieldOffset(12)] public ImGuiInputEventMousePos MousePos;
	[FieldOffset(12)] public ImGuiInputEventMouseWheel MouseWheel;
	[FieldOffset(12)] public ImGuiInputEventMouseButton MouseButton;
	[FieldOffset(12)] public ImGuiInputEventKey Key;
	[FieldOffset(12)] public ImGuiInputEventText Text;
	[FieldOffset(12)] public ImGuiInputEventAppFocused AppFocused;

	// Next field after union
	// Adjust offset to be after the largest union member
	[FieldOffset(24)] public bool AddedByTestEngine; // actual offset depends on union size and alignment
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member