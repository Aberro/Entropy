#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using System.Runtime.InteropServices;
using ImGuiID = uint;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiKeyOwnerData
{
	public const ImGuiID Any = 0;
	public const ImGuiID NoOwner = uint.MaxValue;

	private ImGuiID _ownerCurr;
	private ImGuiID _ownerNext;
	[MarshalAs(UnmanagedType.U1)]
	private bool _lockThisFrame;
	[MarshalAs(UnmanagedType.U1)]
	private bool _lockUntilRelease;

	public ref ImGuiID OwnerCurr => ref this._ownerCurr;
	public ref ImGuiID OwnerNext => ref this._ownerNext;
	public ref bool LockThisFrame => ref this._lockThisFrame;
	/// <summary>
	/// Reading this key requires explicit owner id (until key is released). Set by ImGuiInputFlags_LockUntilRelease. When this is true LockThisFrame is always true as well.
	/// </summary>
	public ref bool LockUntilRelease => ref this._lockUntilRelease;

	public ImGuiKeyOwnerData()
	{
		this._ownerCurr = this._ownerNext = NoOwner;
		this._lockThisFrame = this._lockUntilRelease = false;
	}
}