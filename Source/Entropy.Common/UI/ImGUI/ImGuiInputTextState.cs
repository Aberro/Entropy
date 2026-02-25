#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImGuiNET;
using System.Runtime.InteropServices;
using UnityEngine;
using ImGuiID = uint;

namespace Entropy.Common.UI.ImGUI;


public unsafe struct ImGuiInputTextState
{
	private ImGuiID _id;
	private int _curLenW;
	private int _curLenA;
	private ImVector<char> _textW;
	private ImVector<byte> _textA;
	private ImVector<byte> _initialTextA;
	[MarshalAs(UnmanagedType.U1)]
	private bool _textAIsValid;
	private int _bufCapacityA;
	private float _scrollX;
	private STB_TexteditState _stb;
	private float _cursorAnim;
	[MarshalAs(UnmanagedType.U1)]
	private bool _cursorFollow;
	[MarshalAs(UnmanagedType.U1)]
	private bool _selectedAllMouseLock;
	[MarshalAs(UnmanagedType.U1)]
	private bool _edited;
	private ImGuiInputTextFlags _flags;

	/// <summary>
	/// widget id owning the text state
	/// </summary>
	public ref ImGuiID ID => ref this._id;
	/// <summary>
	/// we need to maintain our buffer length in both UTF-8 and wchar format. UTF-8 length is valid even if TextA is not.
	/// </summary>
	public ref int CurLenW => ref this._curLenW;
	/// <summary>
	/// we need to maintain our buffer length in both UTF-8 and wchar format. UTF-8 length is valid even if TextA is not.
	/// </summary>
	public ref int CurLenA => ref this._curLenA;
	/// <summary>
	/// edit buffer, we need to persist but can't guarantee the persistence of the user-provided buffer. so we copy into own buffer.
	/// </summary>
	public ref ImVector<char> TextW => ref this._textW;
	/// <summary>
	/// temporary UTF8 buffer for callbacks and other operations. this is not updated in every code-path! size=capacity.
	/// </summary>
	public ref ImVector<byte> TextA => ref this._textA;
	/// <summary>
	/// backup of end-user buffer at the time of focus (in UTF-8, unaltered)
	/// </summary>
	public ref ImVector<byte> InitialTextA => ref this._initialTextA;
	/// <summary>
	/// temporary UTF8 buffer is not initially valid before we make the widget active (until then we pull the data from user argument)
	/// </summary>
	public ref bool TextAIsValid => ref this._textAIsValid;
	/// <summary>
	/// end-user buffer capacity
	/// </summary>
	public ref int BufCapacityA => ref this._bufCapacityA;
	/// <summary>
	/// horizontal scrolling/offset
	/// </summary>
	public ref float ScrollX => ref this._scrollX;
	/// <summary>
	/// state for stb_textedit.h
	/// </summary>
	public ref STB_TexteditState Stb => ref this._stb;
	/// <summary>
	/// timer for cursor blink, reset on every user action so the cursor reappears immediately
	/// </summary>
	public ref float CursorAnim => ref this._cursorAnim;
	/// <summary>
	/// set when we want scrolling to follow the current cursor position (not always!)
	/// </summary>
	public ref bool CursorFollow => ref this._cursorFollow;
	/// <summary>
	/// after a double-click to select all, we ignore further mouse drags to update selection
	/// </summary>
	public ref bool SelectedAllMouseLock => ref this._selectedAllMouseLock;
	/// <summary>
	/// edited this frame
	/// </summary>
	public ref bool Edited => ref this._edited;
	/// <summary>
	/// copy of InputText() flags
	/// </summary>
	public ref ImGuiInputTextFlags Flags => ref this._flags;
}