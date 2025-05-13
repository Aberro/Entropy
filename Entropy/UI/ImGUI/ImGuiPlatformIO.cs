#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Runtime.InteropServices;

namespace Entropy.UI.ImGUI;

[StructLayout(LayoutKind.Sequential)]
public struct ImGuiPlatformIO
{
	// Function pointers
	public IntPtr Platform_GetClipboardTextFn; // delegate: const char* (*)(ImGuiContext*)
	public IntPtr Platform_SetClipboardTextFn; // delegate: void (*)(ImGuiContext*, const char*)
	public IntPtr Platform_ClipboardUserData;

	public IntPtr Platform_OpenInShellFn; // delegate: bool (*)(ImGuiContext*, const char*)
	public IntPtr Platform_OpenInShellUserData;

	public IntPtr Platform_SetImeDataFn; // delegate: void (*)(ImGuiContext*, ImGuiViewport*, ImGuiPlatformImeData*)
	public IntPtr Platform_ImeUserData;

	public ushort Platform_LocaleDecimalPoint; // ImWchar is typically 2 bytes (UTF-16)

	public IntPtr Renderer_RenderState;
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member