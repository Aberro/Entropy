#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using STB_TEXTEDIT_POSITIONTYPE = int;
using STB_TEXTEDIT_CHARTYPE = char;
using System.Runtime.InteropServices;

namespace Entropy.Common.UI.ImGUI;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct STB_TexteditState
{
	private int _cursor;
	private int _selectStart;
	private int _selectEnd;
	private byte _insertMode;
	private int _rowCountPerPage;
	private byte _cursorAtEndOfLine;
	private byte _initialized;
	private byte _hasPreferredX;
	private byte _singleLine;
	private byte _padding1;
	private byte _padding2;
	private byte _padding3;
	private float _preferredX;
	private StbUndoState _undoState;

	/////////////////////
	//
	// public data
	//

	public ref int Cursor => ref this._cursor;
	// position of the text cursor within the string
	/// <summary>
	/// selection start point
	/// </summary>
	public ref int SelectStart => ref this._selectStart;

	public ref int SelectEnd => ref this._selectEnd;
	// selection start and end point in characters; if equal, no selection.
	// note that start may be less than or greater than end (e.g. when
	// dragging the mouse, start is where the initial click was, and you
	// can drag in either direction)

	public ref byte InsertMode => ref this._insertMode;
	// each textfield keeps its own insert mode state. to keep an app-wide
	// insert mode, copy this value in/out of the app state

	public ref int RowCountPerPage => ref this._rowCountPerPage;
	// page size in number of row.
	// this value MUST be set to >0 for pageup or pagedown in multilines documents.

	/////////////////////
	//
	// private data
	//
	public ref byte CursorAtEndOfLine => ref this._cursorAtEndOfLine;
	public ref byte Initialized => ref this._initialized;
	public ref byte HasPreferredX => ref this._hasPreferredX;
	public ref byte SingleLine => ref this._singleLine;
	/// <summary>
	///  this determines where the cursor up/down tries to seek to along x
	/// </summary>
	public ref float PreferredX => ref this._preferredX;
	public ref StbUndoState UndoState => ref this._undoState;
}

[StructLayout(LayoutKind.Sequential)]
public struct StbUndoRecord
{
	// private data
	public STB_TEXTEDIT_POSITIONTYPE Where;
	public STB_TEXTEDIT_POSITIONTYPE InsertLength;
	public STB_TEXTEDIT_POSITIONTYPE DeleteLength;
	public int CharStorage;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct StbUndoState
{
	public const int SizeOfStbUndoRecord = (sizeof(STB_TEXTEDIT_POSITIONTYPE) * 3) + sizeof(int);
	public const int STB_TEXTEDIT_UNDOSTATECOUNT = 99;
	public const int STB_TEXTEDIT_UNDOCHARCOUNT = 999;

	private fixed int _undoRec[4 * STB_TEXTEDIT_UNDOSTATECOUNT];
	private fixed STB_TEXTEDIT_CHARTYPE _undoChar[STB_TEXTEDIT_UNDOCHARCOUNT];
	private short _undoPoint;
	private short _redoPoint;
	private int _undoCharPoint;
	private int _redoCharPoint;

	public Span<int> UndoRec
	{
		get
		{
			fixed (int* ptr = this._undoRec)
			{
				return new Span<int>(ptr, 4 * STB_TEXTEDIT_UNDOSTATECOUNT);
			}
		}
	}
	public Span<STB_TEXTEDIT_CHARTYPE> UndoChar
	{
		get
		{
			fixed (STB_TEXTEDIT_CHARTYPE* ptr = this._undoChar)
			{
				return new Span<STB_TEXTEDIT_CHARTYPE>(ptr, STB_TEXTEDIT_UNDOCHARCOUNT);
			}
		}
	}
	public ref short UndoPoint => ref this._undoPoint;
	public ref short RedoPoint => ref this._redoPoint;
	public ref int UndoCharPoint => ref this._undoCharPoint;
	public ref int RedoCharPoint => ref this._redoCharPoint;
}