#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImGuiNET;
using ImGuiID = uint;

namespace Entropy.UI.ImGUI;

public struct ImGuiContextHookPtr
{
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
	public unsafe ImGuiContextHook* NativePtr { get; }
	public unsafe ImGuiContextHookPtr(ImGuiContextHook* nativePtr) => NativePtr = nativePtr;
	public unsafe ImGuiContextHookPtr(IntPtr nativePtr) => NativePtr = (ImGuiContextHook*)(void*)nativePtr;
	public static unsafe implicit operator ImGuiContextHookPtr(ImGuiContextHook* nativePtr) => new(nativePtr);
	public static unsafe implicit operator ImGuiContextHook*(ImGuiContextHookPtr wrappedPtr) => wrappedPtr.NativePtr;
	public static implicit operator ImGuiContextHookPtr(IntPtr nativePtr) => new(nativePtr);
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
}
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