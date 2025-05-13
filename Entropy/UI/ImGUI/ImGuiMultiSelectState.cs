#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImGuiNET;
using ImGuiID = uint;
using ImS8 = sbyte;

namespace Entropy.UI.ImGUI;

public struct ImGuiMultiSelectStatePtr
{
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
	public unsafe ImGuiMultiSelectState* NativePtr { get; }
	public unsafe ImGuiMultiSelectStatePtr(ImGuiMultiSelectState* nativePtr) => NativePtr = nativePtr;

	public unsafe ImGuiMultiSelectStatePtr(IntPtr nativePtr) =>
		NativePtr = (ImGuiMultiSelectState*)(void*)nativePtr;

	public static unsafe implicit operator ImGuiMultiSelectStatePtr(ImGuiMultiSelectState* nativePtr) =>
		new(nativePtr);

	public static unsafe implicit operator ImGuiMultiSelectState*(ImGuiMultiSelectStatePtr wrappedPtr) =>
		wrappedPtr.NativePtr;

	public static implicit operator ImGuiMultiSelectStatePtr(IntPtr nativePtr) => new(nativePtr);
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
}
public struct ImGuiMultiSelectState
{
	public ImGuiWindowPtr Window;
	public ImGuiID ID;
	public int LastFrameActive; // Last used frame-count, for GC.
	public int LastSelectionSize; // Set by BeginMultiSelect() based on optional info provided by user. May be -1 if unknown.
	public ImS8 RangeSelected; // -1 (don't have) or true/false
	public ImS8 NavIdSelected; // -1 (don't have) or true/false
	public ImGuiSelectionUserData RangeSrcItem; //
	public ImGuiSelectionUserData NavIdItem; // SetNextItemSelectionUserData() value for NavId (if part of submitted items)

	public ImGuiMultiSelectState()
	{
		this.Window = null;
		this.ID = 0;
		this.LastFrameActive = this.LastSelectionSize = 0;
		this.RangeSelected = this.NavIdSelected = -1;
		this.RangeSrcItem = this.NavIdItem = ImGuiSelectionUserData.Invalid;
	}
}
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member