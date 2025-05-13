#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using STB_TEXTEDIT_POSITIONTYPE = int;
using STB_TEXTEDIT_CHARTYPE = char;
using System.Runtime.InteropServices;

namespace Entropy.UI.ImGUI;

[StructLayout(LayoutKind.Sequential)]
public struct STB_TexteditState
{
	/////////////////////
	//
	// public data
	//

	public int Cursor;
	// position of the text cursor within the string

	public int SelectStart;          // selection start point

	public int SelectEnd;
	// selection start and end point in characters; if equal, no selection.
	// note that start may be less than or greater than end (e.g. when
	// dragging the mouse, start is where the initial click was, and you
	// can drag in either direction)

	public byte InsertMode;
	// each textfield keeps its own insert mode state. to keep an app-wide
	// insert mode, copy this value in/out of the app state

	public int RowCountPerPage;
	// page size in number of row.
	// this value MUST be set to >0 for pageup or pagedown in multilines documents.

	/////////////////////
	//
	// private data
	//
	public byte CursorAtEndOfLine; // not implemented yet
	public byte Initialized;
	public byte HasPreferredX;
	public byte SingleLine;
	public byte Padding1, Padding2, Padding3;
	public float PreferredX; // this determines where the cursor up/down tries to seek to along x
	public StbUndoState UndoState;
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
	// private data
	public fixed int UndoRec[4 * STB_TEXTEDIT_UNDOSTATECOUNT];
	public fixed STB_TEXTEDIT_CHARTYPE UndoChar[STB_TEXTEDIT_UNDOCHARCOUNT];
	public short UndoPoint, RedoPoint;
	public int UndoCharPoint, RedoCharPoint;
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member