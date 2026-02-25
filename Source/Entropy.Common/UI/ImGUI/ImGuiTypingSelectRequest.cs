#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using System.Runtime.InteropServices;
using ImS8 = sbyte;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiTypingSelectRequest
{
	private ImGuiTypingSelectFlags _flags;
	private int _searchBufferLen;
	private string _searchBuffer;
	[MarshalAs(UnmanagedType.U1)]
	private bool _selectRequest;
	[MarshalAs(UnmanagedType.U1)]
	private bool _singleCharMode;
	private ImS8 _singleCharSize;

	/// <summary>
	/// Flags passed to GetTypingSelectRequest()
	/// </summary>
	public ref ImGuiTypingSelectFlags Flags => ref this._flags;
	public ref int SearchBufferLen => ref this._searchBufferLen;
	/// <summary>
	/// Search buffer contents (use full string. unless SingleCharMode is set, in which case use SingleCharSize).
	/// </summary>
	public ref string SearchBuffer => ref this._searchBuffer;
	/// <summary>
	/// Set when buffer was modified this frame, requesting a selection.
	/// </summary>
	public ref bool SelectRequest => ref this._selectRequest;
	/// <summary>
	/// Notify when buffer contains same character repeated, to implement special mode. In this situation it preferred to not display any on-screen search indication.
	/// </summary>
	public ref bool SingleCharMode => ref this._singleCharMode;
	/// <summary>
	/// Length in bytes of first letter codepoint (1 for ascii, 2-4 for UTF-8). If (SearchBufferLen==RepeatCharSize) only 1 letter has been input.
	/// </summary>
	public ref ImS8 SingleCharSize => ref this._singleCharSize;
}