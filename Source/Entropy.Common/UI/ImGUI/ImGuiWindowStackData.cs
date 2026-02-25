#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImGuiNET;
using System.Runtime.InteropServices;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiWindowStackData
{
	private ImGuiWindow* _window;
	private ImGuiLastItemData _parentLastItemDataBackup;
	private ImGuiErrorRecoveryState _stackSizesInBegin;
	[MarshalAs(UnmanagedType.U1)]
	private bool _disabledOverrideReenable;
	private float _disabledOverrideReenableAlphaBackup;

	public ref ImGuiWindow Window => ref *this._window;
	public bool HasWindow => this._window != null;
	public void SetWindow(ref ImGuiWindow window) => this._window = ImGuiHelper.GetPointer(ref window);
	public void ClearWindow() => this._window = null;
	public ref ImGuiLastItemData ParentLastItemDataBackup => ref this._parentLastItemDataBackup;
	/// <summary>
	/// Store size of various stacks for asserting
	/// </summary>
	public ref ImGuiErrorRecoveryState StackSizesInBegin => ref this._stackSizesInBegin;
	/// <summary>
	/// Non-child window override disabled flag
	/// </summary>
	public ref bool DisabledOverrideReenable => ref this._disabledOverrideReenable;
	public ref float DisabledOverrideReenableAlphaBackup => ref this._disabledOverrideReenableAlphaBackup;
}