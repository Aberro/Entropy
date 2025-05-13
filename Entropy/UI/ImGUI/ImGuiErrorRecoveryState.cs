#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Entropy.UI.ImGUI;

public readonly struct ImGuiErrorRecoveryStatePtr
{
	public unsafe ImGuiErrorRecoveryState* NativePtr { get; }
	public unsafe ImGuiErrorRecoveryStatePtr(ImGuiErrorRecoveryState* nativePtr) => NativePtr = nativePtr;
	public unsafe ImGuiErrorRecoveryStatePtr(IntPtr nativePtr) => NativePtr = (ImGuiErrorRecoveryState*)(void*)nativePtr;
	public static unsafe implicit operator ImGuiErrorRecoveryStatePtr(ImGuiErrorRecoveryState* nativePtr) => new(nativePtr);
	public static unsafe implicit operator ImGuiErrorRecoveryState*(ImGuiErrorRecoveryStatePtr wrappedPtr) => wrappedPtr.NativePtr;
	public static implicit operator ImGuiErrorRecoveryStatePtr(IntPtr nativePtr) => new(nativePtr);
}
public struct ImGuiErrorRecoveryState
{
	public short SizeOfWindowStack;
	public short SizeOfIDStack;
	public short SizeOfTreeStack;
	public short SizeOfColorStack;
	public short SizeOfStyleVarStack;
	public short SizeOfFontStack;
	public short SizeOfFocusScopeStack;
	public short SizeOfGroupStack;
	public short SizeOfItemFlagsStack;
	public short SizeOfBeginPopupStack;
	public short SizeOfDisabledStack;
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member