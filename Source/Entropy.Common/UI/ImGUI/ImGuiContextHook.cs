#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImGuiNET;
using ImGuiID = uint;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiContextHook
{
	private ImGuiID _hookId;
	private ImGuiContextHookType _type;
	private ImGuiID _owner;
	private IntPtr _callback;
	private IntPtr _userData;

	/// <summary>
	/// A unique ID assigned by AddContextHook()
	/// </summary>
	public ref ImGuiID HookId => ref this._hookId;
	public ref ImGuiContextHookType Type => ref this._type;
	public ref ImGuiID Owner => ref this._owner;
	public ref IntPtr Callback => ref this._callback;
	public ref IntPtr UserData => ref this._userData;
}

public delegate void ImGuiContextHookCallback(ref ImGuiContext ctx, ref ImGuiContextHook hook);