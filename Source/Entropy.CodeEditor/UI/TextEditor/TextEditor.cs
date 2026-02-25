using Entropy.Common.Utils;
using ImGuiNET;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace Entropy.CodeEditor.UI.TextEditor;

using Breakpoints = HashSet<int>;
using ErrorMarkers = Dictionary<int, string>;
using Line = List<Glyph>;
using Lines = List<List<Glyph>>;

public partial class TextEditor
{
	private struct RegexItem
	{
		public Regex Regex;
		public PaletteIndex Index;
	}

	private struct EditorState
	{
		public Coordinates mSelectionStart;
		public Coordinates mSelectionEnd;
		public Coordinates mCursorPosition;
	};

	public static readonly Color[] DarkPalette =
	{
		Color.FromUIntReversed(0xff7f7f7f),	// Default
		Color.FromUIntReversed(0xffd69c56),	// Keyword
		Color.FromUIntReversed(0xff00ff00),	// Number
		Color.FromUIntReversed(0xff7070e0),	// String
		Color.FromUIntReversed(0xff70a0e0), // Char literal
		Color.FromUIntReversed(0xffffffff), // Punctuation
		Color.FromUIntReversed(0xff408080),	// Preprocessor
		Color.FromUIntReversed(0xffaaaaaa), // Identifier
		Color.FromUIntReversed(0xff9bc64d), // Known identifier
		Color.FromUIntReversed(0xffc040a0), // Preproc identifier
		Color.FromUIntReversed(0xff206020), // Comment (single line)
		Color.FromUIntReversed(0xff406020), // Comment (multi line)
		Color.FromUIntReversed(0xff101010), // Background
		Color.FromUIntReversed(0xffe0e0e0), // Cursor
		Color.FromUIntReversed(0x80a06020), // Selection
		Color.FromUIntReversed(0x800020ff), // ErrorMarker
		Color.FromUIntReversed(0x40f08000), // Breakpoint
		Color.FromUIntReversed(0xff707000), // Line number
		Color.FromUIntReversed(0x40000000), // Current line fill
		Color.FromUIntReversed(0x40808080), // Current line fill (inactive)
		Color.FromUIntReversed(0x40a0a0a0), // Current line edge
	};
	public static readonly Color[] LightPalette =
	{
		Color.FromUIntReversed(0xff7f7f7f),	// None
		Color.FromUIntReversed(0xffff0c06),	// Keyword
		Color.FromUIntReversed(0xff008000),	// Number
		Color.FromUIntReversed(0xff2020a0),	// String
		Color.FromUIntReversed(0xff304070), // Char literal
		Color.FromUIntReversed(0xff000000), // Punctuation
		Color.FromUIntReversed(0xff406060),	// Preprocessor
		Color.FromUIntReversed(0xff404040), // Identifier
		Color.FromUIntReversed(0xff606010), // Known identifier
		Color.FromUIntReversed(0xffc040a0), // Preproc identifier
		Color.FromUIntReversed(0xff205020), // Comment (single line)
		Color.FromUIntReversed(0xff405020), // Comment (multi line)
		Color.FromUIntReversed(0xffffffff), // Background
		Color.FromUIntReversed(0xff000000), // Cursor
		Color.FromUIntReversed(0x80600000), // Selection
		Color.FromUIntReversed(0xa00010ff), // ErrorMarker
		Color.FromUIntReversed(0x80f08000), // Breakpoint
		Color.FromUIntReversed(0xff505000), // Line number
		Color.FromUIntReversed(0x40000000), // Current line fill
		Color.FromUIntReversed(0x40808080), // Current line fill (inactive)
		Color.FromUIntReversed(0x40000000), // Current line edge
	};
	public static readonly Color[] RetroBluePalette =
	{
		Color.FromUIntReversed(0xff00ffff),	// None
		Color.FromUIntReversed(0xffffff00),	// Keyword
		Color.FromUIntReversed(0xff00ff00),	// Number
		Color.FromUIntReversed(0xff808000),	// String
		Color.FromUIntReversed(0xff808000), // Char literal
		Color.FromUIntReversed(0xffffffff), // Punctuation
		Color.FromUIntReversed(0xff008000),	// Preprocessor
		Color.FromUIntReversed(0xff00ffff), // Identifier
		Color.FromUIntReversed(0xffffffff), // Known identifier
		Color.FromUIntReversed(0xffff00ff), // Preproc identifier
		Color.FromUIntReversed(0xff808080), // Comment (single line)
		Color.FromUIntReversed(0xff404040), // Comment (multi line)
		Color.FromUIntReversed(0xff800000), // Background
		Color.FromUIntReversed(0xff0080ff), // Cursor
		Color.FromUIntReversed(0xffffff00), // Selection
		Color.FromUIntReversed(0xa00000ff), // ErrorMarker
		Color.FromUIntReversed(0x80ff8000), // Breakpoint
		Color.FromUIntReversed(0xff808000), // Line number
		Color.FromUIntReversed(0xff808080), // Current line fill
		Color.FromUIntReversed(0xff808080), // Current line fill (inactive)
		Color.FromUIntReversed(0xff000000), // Current line edge
	};

	private float mLineSpacing;
	private Lines mLines = new();
	private EditorState mState;
	private Stack<UndoRecord> mUndoBuffer = new();
	private Stack<UndoRecord> mRedoBuffer = new();

	private int mTabSize;
	private bool mOverwrite;
	private bool mReadOnly;
	private bool mWithinRender;
	private bool mScrollToCursor;
	private bool mScrollToTop;
	private bool mTextChanged;
	private bool mColorizerEnabled;
	private float mTextStart;                   // position (in pixels) where a code line starts relative to the left of the TextEditor.
	private int mLeftMargin;
	private bool mCursorPositionChanged;
	private int mColorRangeMin, mColorRangeMax;
	private SelectionMode mSelectionMode;
	private bool mHandleKeyboardInputs;
	private bool mHandleMouseInputs;
	private bool mIgnoreImGuiChild;
	private bool mShowWhitespaces;
	private bool mUnsavedChanges;

	private Color[] mPaletteBase;
	private Color[] mPalette;
	private LanguageDefinition mLanguageDefinition;
	private List<RegexItem> mRegexList = new();

	private bool mCheckComments;
	private Breakpoints mBreakpoints = new();
	private ErrorMarkers mErrorMarkers = new();
	private Vector2 mCharAdvance;
	private Coordinates mInteractiveStart, mInteractiveEnd;
	private StringBuilder mLineBuffer = new();
	private DateTime mStartTime;

	private float mLastClick;

	public LanguageDefinition LanguageDefinition
	{
		get => this.mLanguageDefinition;
		set
		{
			this.mLanguageDefinition = value;
			this.mLanguageDefinition.IsValidIdentifier ??= IsValidIdentifierDefault;
			this.mRegexList.Clear();
			foreach (var r in this.mLanguageDefinition.TokenRegexStrings)
				this.mRegexList.Add(new RegexItem { Regex = new Regex(r.Pattern, RegexOptions.Compiled), Index = r.Color });
			Colorize();
		}
	}

	public Color[] Palette
	{
		get => this.mPaletteBase;
		set
		{
			ArgumentNullException.ThrowIfNull(value);
			this.mPaletteBase = value;
		}
	}

	public string Text
	{
		get => GetText();
		set
		{
			ArgumentNullException.ThrowIfNull(value);
			SetText(value ?? "");
		}
	}

	public bool ReadOnly
	{
		get => this.mReadOnly;
		set => this.mReadOnly = value;
	}

	public bool ColorizerEnabled
	{
		get => this.mColorizerEnabled;
		set => this.mColorizerEnabled = value;
	}

	public Coordinates CursorPosition
	{
		get => GetActualCursorCoordinates();
		set
		{
			if (this.mState.mCursorPosition == value)
				return;
			this.mState.mCursorPosition = value;
			this.mCursorPositionChanged = true;
			EnsureCursorVisible();
		}
	}

	public int TabSize
	{
		get => this.mTabSize;
		set => this.mTabSize = Math.Clamp(value, 0, 32);
	}

	public bool UnsavedChanges
	{
		get => this.mUnsavedChanges;
		set => this.mUnsavedChanges = value;
	}

	public int TotalLines => this.mLines.Count;

	public TextEditor(LanguageDefinition language)
	{
		this.mLineSpacing = 1.0f;
		this.mTabSize = 4;
		this.mOverwrite = false;
		this.mReadOnly = false;
		this.mWithinRender = false;
		this.mScrollToCursor = false;
		this.mScrollToTop = false;
		this.mTextChanged = false;
		this.mColorizerEnabled = true;
		this.mTextStart = 20.0f;
		this.mLeftMargin = 10;
		this.mCursorPositionChanged = false;
		this.mColorRangeMin = 0;
		this.mColorRangeMax = 0;
		this.mSelectionMode = SelectionMode.Normal;
		this.mCheckComments = true;
		this.mLastClick = -1.0f;
		this.mHandleKeyboardInputs = true;
		this.mHandleMouseInputs = true;
		this.mIgnoreImGuiChild = false;
		this.mShowWhitespaces = true;
		this.mStartTime = DateTime.Now;
		this.mPalette = DarkPalette.ToArray();
		this.mPaletteBase = DarkPalette;
		LanguageDefinition = language;
		this.mLines = new Lines { new() };
		this.mLineBuffer = new StringBuilder();
		this.mLanguageDefinition.IsValidIdentifier = IsValidIdentifierDefault;
	}

	public void SetErrorMarkers(ErrorMarkers aMarkers) => this.mErrorMarkers = aMarkers;
	public void SetBreakpoints(Breakpoints aMarkers) => this.mBreakpoints = aMarkers;

	public void ResetUnsavedChanges() => this.mUnsavedChanges = false;

	public void Render(string aTitle, Vector2 aSize = new Vector2(), bool aBorder = false)
	{
		this.mWithinRender = true;
		this.mTextChanged = false;
		this.mCursorPositionChanged = false;

		if (this.mPalette is null || this.mPalette.Length < (int)PaletteIndex.Max)
		{
			this.mPalette = new Color[(int)PaletteIndex.Max];
			this.mPaletteBase.CopyTo(this.mPalette, 0);
		}
		ImGui.PushStyleColor(ImGuiCol.ChildBg, this.mPalette[(int)PaletteIndex.Background]);
		ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0.0f, 0.0f));
		if (!this.mIgnoreImGuiChild)
			ImGui.BeginChild(aTitle, aSize, aBorder, ImGuiWindowFlags.HorizontalScrollbar | ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.NoMove);

		if (this.mHandleKeyboardInputs)
		{
			HandleKeyboardInputs();
			ImGui.PushAllowKeyboardFocus(true);
		}

		if (this.mHandleMouseInputs)
			HandleMouseInputs();

		ColorizeInternal();
		Render();

		if (this.mHandleKeyboardInputs)
			ImGui.PopAllowKeyboardFocus();

		if (!this.mIgnoreImGuiChild)
			ImGui.EndChild();

		ImGui.PopStyleVar();
		ImGui.PopStyleColor();

		this.mWithinRender = false;
	}

	public void SetTextLines(List<string> aLines)
	{
		aLines ??= [];
		this.mLines.Clear();

		if (aLines.Count <= 0)
			this.mLines.Add(new Line());
		else
			this.mLines.AddRange(aLines.Select(aLine => aLine.Select(c => new Glyph(c, PaletteIndex.Default)).ToList()));

		this.mTextChanged = true;
		this.mUnsavedChanges = true;
		this.mScrollToTop = true;

		this.mUndoBuffer.Clear();
		this.mRedoBuffer.Clear();

		Colorize();
	}
	public List<string> GetTextLines() => this.mLines.Select(line => new string(line.Select(g => g.Char).ToArray())).ToList();

	public string GetSelectedText() => GetText(this.mState.mSelectionStart, this.mState.mSelectionEnd);
	public string GetCurrentLineText()
	{
		var lineLength = GetLineMaxColumn(this.mState.mCursorPosition.Line);
		return GetText(
			new Coordinates(this.mState.mCursorPosition.Line, 0),
			new Coordinates(this.mState.mCursorPosition.Line, lineLength));
	}
	public bool IsOverwrite() => this.mOverwrite;
	public bool IsTextChanged() => this.mTextChanged;
	public bool IsCursorPositionChanged() => this.mCursorPositionChanged;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetHandleMouseInputs(bool aValue) => this.mHandleMouseInputs = aValue;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsHandleMouseInputsEnabled() => this.mHandleKeyboardInputs;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetHandleKeyboardInputs(bool aValue) => this.mHandleKeyboardInputs = aValue;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsHandleKeyboardInputsEnabled() => this.mHandleKeyboardInputs;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetImGuiChildIgnored(bool aValue) => this.mIgnoreImGuiChild = aValue;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsImGuiChildIgnored() => this.mIgnoreImGuiChild;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetShowWhitespaces(bool aValue) => this.mShowWhitespaces = aValue;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsShowingWhitespaces() => this.mShowWhitespaces;

	public void InsertText(string aValue)
	{
		if (string.IsNullOrEmpty(aValue))
			return;

		var pos = GetActualCursorCoordinates();
		var start = pos < this.mState.mSelectionStart ? pos : this.mState.mSelectionStart;
		var totalLines = pos.Line - start.Line;

		totalLines += InsertTextAt(ref pos, aValue);

		SetSelection(pos, pos);
		CursorPosition = pos;
		Colorize(start.Line - 1, totalLines + 2);
	}

	public void MoveUp(int aAmount = 1, bool aSelect = false)
	{
		var oldPos = this.mState.mCursorPosition;
		this.mState.mCursorPosition.Line = Math.Max(0, this.mState.mCursorPosition.Line - aAmount);
		if (oldPos == this.mState.mCursorPosition)
			return;
		if (aSelect)
		{
			if (oldPos == this.mInteractiveStart)
			{
				this.mInteractiveStart = this.mState.mCursorPosition;
			}
			else if (oldPos == this.mInteractiveEnd)
			{
				this.mInteractiveEnd = this.mState.mCursorPosition;
			}
			else
			{
				this.mInteractiveStart = this.mState.mCursorPosition;
				this.mInteractiveEnd = oldPos;
			}
		}
		else
		{
			this.mInteractiveStart = this.mInteractiveEnd = this.mState.mCursorPosition;
		}
		SetSelection(this.mInteractiveStart, this.mInteractiveEnd);

		EnsureCursorVisible();
	}
	public void MoveDown(int aAmount = 1, bool aSelect = false)
	{
		Debug.Assert(this.mState.mCursorPosition.Column >= 0);
		var oldPos = this.mState.mCursorPosition;
		this.mState.mCursorPosition.Line = Math.Clamp(this.mState.mCursorPosition.Line + aAmount, 0, this.mLines.Count - 1);

		if (this.mState.mCursorPosition != oldPos)
		{
			if (aSelect)
			{
				if (oldPos == this.mInteractiveEnd)
				{
					this.mInteractiveEnd = this.mState.mCursorPosition;
				}
				else if (oldPos == this.mInteractiveStart)
				{
					this.mInteractiveStart = this.mState.mCursorPosition;
				}
				else
				{
					this.mInteractiveStart = oldPos;
					this.mInteractiveEnd = this.mState.mCursorPosition;
				}
			}
			else
			{
				this.mInteractiveStart = this.mInteractiveEnd = this.mState.mCursorPosition;
			}
			SetSelection(this.mInteractiveStart, this.mInteractiveEnd);

			EnsureCursorVisible();
		}
	}
	public void MoveLeft(int aAmount = 1, bool aSelect = false, bool aWordMode = false)
	{
		if (this.mLines.Count <= 0)
			return;

		var oldPos = this.mState.mCursorPosition;
		this.mState.mCursorPosition = GetActualCursorCoordinates();
		var line = this.mState.mCursorPosition.Line;
		var cIndex = GetCharacterIndex(this.mState.mCursorPosition);

		while (aAmount-- > 0)
		{
			if (cIndex == 0)
			{
				if (line > 0)
				{
					--line;
					if (this.mLines.Count > line)
						cIndex = this.mLines[line].Count;
					else
						cIndex = 0;
				}
			}
			else
			{
				--cIndex;
			}

			this.mState.mCursorPosition = new Coordinates(line, GetCharacterColumn(line, cIndex));
			if (aWordMode)
			{
				this.mState.mCursorPosition = FindWordStart(this.mState.mCursorPosition);
				cIndex = GetCharacterIndex(this.mState.mCursorPosition);
			}
		}

		this.mState.mCursorPosition = new Coordinates(line, GetCharacterColumn(line, cIndex));

		Debug.Assert(this.mState.mCursorPosition.Column >= 0);
		if (aSelect)
		{
			if (oldPos == this.mInteractiveStart)
			{
				this.mInteractiveStart = this.mState.mCursorPosition;
			}
			else if (oldPos == this.mInteractiveEnd)
			{
				this.mInteractiveEnd = this.mState.mCursorPosition;
			}
			else
			{
				this.mInteractiveStart = this.mState.mCursorPosition;
				this.mInteractiveEnd = oldPos;
			}
		}
		else
		{
			this.mInteractiveStart = this.mInteractiveEnd = this.mState.mCursorPosition;
		}
		SetSelection(this.mInteractiveStart, this.mInteractiveEnd, aSelect && aWordMode ? SelectionMode.Word : SelectionMode.Normal);

		EnsureCursorVisible();
	}
	public void MoveRight(int aAmount = 1, bool aSelect = false, bool aWordMode = false)
	{
		var oldPos = this.mState.mCursorPosition;

		if (this.mLines.Count <= 0 || oldPos.Line >= this.mLines.Count)
			return;

		var cIndex = GetCharacterIndex(this.mState.mCursorPosition);
		while (aAmount-- > 0)
		{
			var lIndex = this.mState.mCursorPosition.Line;
			var line = this.mLines[lIndex];

			if (cIndex >= line.Count)
			{
				if (this.mState.mCursorPosition.Line >= this.mLines.Count - 1)
				{
					return;
				}
				else
				{
					this.mState.mCursorPosition.Line = Math.Clamp(this.mState.mCursorPosition.Line + 1, 0, this.mLines.Count - 1);
					this.mState.mCursorPosition.Column = 0;
				}
			}
			else
			{
				cIndex++;
				this.mState.mCursorPosition = new Coordinates(lIndex, GetCharacterColumn(lIndex, cIndex));
				if (aWordMode)
					this.mState.mCursorPosition = FindNextWord(this.mState.mCursorPosition);
			}
		}

		if (aSelect)
		{
			if (oldPos == this.mInteractiveEnd)
			{
				this.mInteractiveEnd = SanitizeCoordinates(this.mState.mCursorPosition);
			}
			else if (oldPos == this.mInteractiveStart)
			{
				this.mInteractiveStart = this.mState.mCursorPosition;
			}
			else
			{
				this.mInteractiveStart = oldPos;
				this.mInteractiveEnd = this.mState.mCursorPosition;
			}
		}
		else
		{
			this.mInteractiveStart = this.mInteractiveEnd = this.mState.mCursorPosition;
		}
		SetSelection(this.mInteractiveStart, this.mInteractiveEnd, aSelect && aWordMode ? SelectionMode.Word : SelectionMode.Normal);

		EnsureCursorVisible();
	}
	public void MoveTop(bool aSelect = false)
	{
		var oldPos = this.mState.mCursorPosition;
		CursorPosition = new Coordinates(0, 0);

		if (this.mState.mCursorPosition == oldPos)
			return;
		if (aSelect)
		{
			this.mInteractiveEnd = oldPos;
			this.mInteractiveStart = this.mState.mCursorPosition;
		}
		else
		{
			this.mInteractiveStart = this.mInteractiveEnd = this.mState.mCursorPosition;
		}
		SetSelection(this.mInteractiveStart, this.mInteractiveEnd);
	}
	public void MoveBottom(bool aSelect = false)
	{
		var oldPos = CursorPosition;
		var newPos = new Coordinates(this.mLines.Count - 1, 0);
		CursorPosition = newPos;
		if (aSelect)
		{
			this.mInteractiveStart = oldPos;
			this.mInteractiveEnd = newPos;
		}
		else
		{
			this.mInteractiveStart = this.mInteractiveEnd = newPos;
		}
		SetSelection(this.mInteractiveStart, this.mInteractiveEnd);
	}
	public void MoveHome(bool aSelect = false)
	{
		var oldPos = this.mState.mCursorPosition;
		CursorPosition = new Coordinates(this.mState.mCursorPosition.Line, 0);

		if (this.mState.mCursorPosition == oldPos)
			return;
		if (aSelect)
		{
			if (oldPos == this.mInteractiveStart)
			{
				this.mInteractiveStart = this.mState.mCursorPosition;
			}
			else if (oldPos == this.mInteractiveEnd)
			{
				this.mInteractiveEnd = this.mState.mCursorPosition;
			}
			else
			{
				this.mInteractiveStart = this.mState.mCursorPosition;
				this.mInteractiveEnd = oldPos;
			}
		}
		else
		{
			this.mInteractiveStart = this.mInteractiveEnd = this.mState.mCursorPosition;
		}
		SetSelection(this.mInteractiveStart, this.mInteractiveEnd);
	}
	public void MoveEnd(bool aSelect = false)
	{
		var oldPos = this.mState.mCursorPosition;
		CursorPosition = new Coordinates(this.mState.mCursorPosition.Line, GetLineMaxColumn(oldPos.Line));

		if (this.mState.mCursorPosition == oldPos)
			return;
		if (aSelect)
		{
			if (oldPos == this.mInteractiveEnd)
			{
				this.mInteractiveEnd = this.mState.mCursorPosition;
			}
			else if (oldPos == this.mInteractiveStart)
			{
				this.mInteractiveStart = this.mState.mCursorPosition;
			}
			else
			{
				this.mInteractiveStart = oldPos;
				this.mInteractiveEnd = this.mState.mCursorPosition;
			}
		}
		else
		{
			this.mInteractiveStart = this.mInteractiveEnd = this.mState.mCursorPosition;
		}
		SetSelection(this.mInteractiveStart, this.mInteractiveEnd);
	}

	public void SetSelectionStart(Coordinates aPosition)
	{
		this.mState.mSelectionStart = SanitizeCoordinates(aPosition);
		if (this.mState.mSelectionStart > this.mState.mSelectionEnd)
			(this.mState.mSelectionStart, this.mState.mSelectionEnd) = (this.mState.mSelectionEnd, this.mState.mSelectionStart);
	}
	public void SetSelectionEnd(Coordinates aPosition)
	{
		this.mState.mSelectionEnd = SanitizeCoordinates(aPosition);
		if (this.mState.mSelectionStart > this.mState.mSelectionEnd)
			(this.mState.mSelectionStart, this.mState.mSelectionEnd) = (this.mState.mSelectionEnd, this.mState.mSelectionStart);
	}
	public void SetSelection(Coordinates aStart, Coordinates aEnd, SelectionMode aMode = SelectionMode.Normal)
	{
		var oldSelStart = this.mState.mSelectionStart;
		var oldSelEnd = this.mState.mSelectionEnd;

		this.mState.mSelectionStart = SanitizeCoordinates(aStart);
		this.mState.mSelectionEnd = SanitizeCoordinates(aEnd);
		if (this.mState.mSelectionStart > this.mState.mSelectionEnd)
			(this.mState.mSelectionStart, this.mState.mSelectionEnd) = (this.mState.mSelectionEnd, this.mState.mSelectionStart);

		switch (aMode)
		{
			case SelectionMode.Normal:
				break;
			case SelectionMode.Word:
				this.mState.mSelectionStart = FindWordStart(this.mState.mSelectionStart);
				if (!IsOnWordBoundary(this.mState.mSelectionEnd))
					this.mState.mSelectionEnd = FindWordEnd(FindWordStart(this.mState.mSelectionEnd));
				break;
			case SelectionMode.Line:
				var lineNo = this.mState.mSelectionEnd.Line;
				this.mState.mSelectionStart = new Coordinates(this.mState.mSelectionStart.Line, 0);
				this.mState.mSelectionEnd = new Coordinates(lineNo, GetLineMaxColumn(lineNo));
				break;
		}

		if (this.mState.mSelectionStart != oldSelStart || this.mState.mSelectionEnd != oldSelEnd)
			this.mCursorPositionChanged = true;
	}
	public void SelectWordUnderCursor()
	{
		var c = CursorPosition;
		SetSelection(FindWordStart(c), FindWordEnd(c));
	}
	public void SelectAll() => SetSelection(new Coordinates(0, 0), new Coordinates(this.mLines.Count, 0));
	public bool HasSelection() => this.mState.mSelectionEnd > this.mState.mSelectionStart;

	public void Copy()
	{
		if (HasSelection())
			ImGui.SetClipboardText(GetSelectedText());
		else
			if (this.mLines.Count > 0)
				ImGui.SetClipboardText(new string(this.mLines[GetActualCursorCoordinates().Line].Select(c => c.Char).ToArray()));
	}
	public void Cut()
	{
		if (ReadOnly)
		{
			Copy();
		}
		else
		{
			if (HasSelection())
			{
				var u = new UndoRecord
				{
					mBefore = this.mState,
					mRemoved = GetSelectedText(),
					mRemovedStart = this.mState.mSelectionStart,
					mRemovedEnd = this.mState.mSelectionEnd,
				};

				Copy();
				DeleteSelection();

				u.mAfter = this.mState;
				AddUndo(u);
			}
		}
	}
	public void Paste()
	{
		if (ReadOnly)
			return;

		var clipText = ImGui.GetClipboardText();
		if (string.IsNullOrEmpty(clipText))
			return;
		var u = new UndoRecord
		{
			mBefore = this.mState,
		};

		if (HasSelection())
		{
			u.mRemoved = GetSelectedText();
			u.mRemovedStart = this.mState.mSelectionStart;
			u.mRemovedEnd = this.mState.mSelectionEnd;
			DeleteSelection();
		}

		u.mAdded = clipText;
		u.mAddedStart = GetActualCursorCoordinates();

		InsertText(clipText);

		u.mAddedEnd = GetActualCursorCoordinates();
		u.mAfter = this.mState;
		AddUndo(u);
	}
	public void Delete(bool entireLine, bool wordMode)
	{
		Debug.Assert(!ReadOnly);

		if (this.mLines.Count <= 0)
			return;

		var u = new UndoRecord
		{
			mBefore = this.mState,
		};

		if (HasSelection())
		{
			u.mRemoved = GetSelectedText();
			u.mRemovedStart = this.mState.mSelectionStart;
			u.mRemovedEnd = this.mState.mSelectionEnd;

			DeleteSelection();
		}
		else
		{
			var pos = GetActualCursorCoordinates();
			CursorPosition = pos;
			var line = this.mLines[pos.Line];
			var lineWidth = GetLineMaxColumn(pos.Line);
			if (entireLine)
			{
				RemoveLine(pos.Line);
			} else
			{
				if (pos.Column == lineWidth)
				{
					if (pos.Line == this.mLines.Count - 1)
						return;

					u.mRemoved = "\n";
					u.mRemovedStart = u.mRemovedEnd = GetActualCursorCoordinates();
					Advance(u.mRemovedEnd);

					var nextLine = this.mLines[pos.Line + 1];
					line.AddRange(nextLine);
					RemoveLine(pos.Line + 1);
				} else
				{
					u.mRemovedStart = pos;
					// Here we can't be at the end of the line, because of previous if branch condition. So, we can safely increment.
					u.mRemovedEnd = wordMode ? FindWordEnd(pos) : pos with { Column = pos.Column + 1 };
					u.mRemoved = GetText(u.mRemovedStart, u.mRemovedEnd);
					line.RemoveRange(u.mRemovedStart.Column, u.mRemovedEnd.Column - u.mRemovedStart.Column);
				}
				Colorize(pos.Line, 1);
			}

			this.mTextChanged = true;
			this.mUnsavedChanges = true;
		}

		u.mAfter = this.mState;
		AddUndo(u);
	}

	public bool CanUndo() => !ReadOnly && this.mUndoBuffer.Count > 0;
	public bool CanRedo() => !ReadOnly && this.mRedoBuffer.Count > 0;
	public void Undo() => Undo(1);
	public void Undo(int aSteps)
	{
		while (CanUndo() && aSteps-- > 0)
		{
			var undo = this.mUndoBuffer.Pop();
			undo.Undo(this);
			this.mRedoBuffer.Push(undo);
		}
	}
	public void Redo() => Redo(1);
	public void Redo(int aSteps)
	{
		while (CanRedo() && aSteps-- > 0)
		{
			var redo = this.mRedoBuffer.Pop();
			redo.Redo(this);
			this.mUndoBuffer.Push(redo);
		}
	}
	private void SetText(string aText)
	{
		this.mLines.Clear();
		this.mLines.Add(new Line());
		foreach (var chr in aText)
		{
			if (chr == '\r')
				// ignore the carriage return character
				continue;
			if (chr == '\n')
				this.mLines.Add(new Line());
			else
				this.mLines[^1].Add(new Glyph(chr, PaletteIndex.Default));
		}

		this.mTextChanged = true;
		this.mUnsavedChanges = true;
		this.mScrollToTop = true;

		this.mUndoBuffer.Clear();
		this.mRedoBuffer.Clear();

		Colorize();
	}
	private string GetText() => GetText(default, new Coordinates(this.mLines.Count, 0));

	private void Colorize(int aFromLine = 0, int aLines = -1)
	{
		var toLine = aLines == -1 ? this.mLines.Count : Math.Min(this.mLines.Count, aFromLine + aLines);
		this.mColorRangeMin = Math.Min(this.mColorRangeMin, aFromLine);
		this.mColorRangeMax = Math.Max(this.mColorRangeMax, toLine);
		this.mColorRangeMin = Math.Max(0, this.mColorRangeMin);
		this.mColorRangeMax = Math.Max(this.mColorRangeMin, this.mColorRangeMax);
		this.mCheckComments = true;
	}

	private void ColorizeRange(int aFromLine = 0, int aToLine = 0)
	{
		if (this.mLines.Count <= 0 || aFromLine >= aToLine)
			return;
		var buffer = Array.Empty<char>();

		string id;

		var endLine = Math.Clamp(aToLine, 0, this.mLines.Count);
		for (var i = aFromLine; i < endLine; ++i)
		{
			var line = this.mLines[i];
			if (line.Count <= 0)
				continue;

			if (buffer.Length < line.Count)
				Array.Resize(ref buffer, line.Count);
			var bufferSize = line.Count;
			for (var j = 0; j < line.Count; ++j)
			{
				var col = line[j];
				buffer[j] = col.Char;
				col.ColorIndex = PaletteIndex.Default;
				line[j] = col;
			}

			if(this.mLanguageDefinition.Tokenize != null)
			{
				foreach(var (Segment, ColorIndex) in this.mLanguageDefinition.Tokenize(buffer, 0, bufferSize))
				{
					var colorIndex = ColorIndex;
					if(colorIndex == PaletteIndex.Identifier)
					{
						id = new string(buffer, Segment.Offset, Segment.Count);

						if (this.mLanguageDefinition.Keywords.Contains(id))
							colorIndex = PaletteIndex.Keyword;
						else if (this.mLanguageDefinition.Identifiers.ContainsKey(id))
							colorIndex = PaletteIndex.KnownIdentifier;
						else if (this.mLanguageDefinition.PreprocIdentifiers.ContainsKey(id))
							colorIndex = PaletteIndex.PreprocIdentifier;
					}
					for (var j = Segment.Offset; j < Segment.Offset + Segment.Count && j < bufferSize; ++j)
					{
						line[j] = new Glyph(line[j].Char, colorIndex);
					}
				}
				continue;
			}

			var last = bufferSize - 1;

			for (var chIdx = 0; chIdx <= last;)
			{
				var span = new ReadOnlySpan<char>(buffer, chIdx, bufferSize - chIdx);
				var tokenBegin = 0;
				var tokenLength = 0;
				var tokenColor = PaletteIndex.Default;

				var hasTokenizeResult = false;

				foreach (var p in this.mRegexList)
				{
					var match = p.Regex.Match(new string(span));
					if (match.Success)
					{
						hasTokenizeResult = true;
						tokenBegin = match.Index;
						tokenLength = match.Length;
						tokenColor = p.Index;
						break;
					}
				}

				if (hasTokenizeResult)
				{
					if (tokenColor == PaletteIndex.Identifier)
					{
						id = new string(buffer, chIdx + tokenBegin, tokenLength);

						// todo : almost all language definitions use lower case to specify keywords, so shouldn't this use .tolower ?
						if (!this.mLanguageDefinition.CaseSensitive)
							id = id.ToUpperInvariant();

						if (!line[chIdx].Preprocessor)
						{
							if (this.mLanguageDefinition.Keywords.Contains(id))
								tokenColor = PaletteIndex.Keyword;
							else if (this.mLanguageDefinition.Identifiers.ContainsKey(id))
								tokenColor = PaletteIndex.KnownIdentifier;
							else if (this.mLanguageDefinition.PreprocIdentifiers.ContainsKey(id))
								tokenColor = PaletteIndex.PreprocIdentifier;
						}
						else
						{
							if (this.mLanguageDefinition.PreprocIdentifiers.ContainsKey(id))
								tokenColor = PaletteIndex.PreprocIdentifier;
						}
					}

					for (var j = 0; j < tokenLength; ++j)
					{
						var col = line[chIdx + tokenBegin + j];
						col.ColorIndex = tokenColor;
						line[chIdx + tokenBegin + j] = col;
					}
					chIdx += tokenBegin + tokenLength;
				}
				else
				{
					chIdx++;
				}
			}
		}
	}
	private void ColorizeInternal()
	{
		if (this.mLines.Count <= 0 || !this.mColorizerEnabled)
			return;

		if (this.mCheckComments)
		{
			var endLine = this.mLines.Count;
			var endIndex = 0;
			var commentStartLine = endLine;
			var commentStartIndex = endIndex;
			var withinString = false;
			var withinSingleLineComment = false;
			var withinPreproc = false;
			var firstChar = true;          // there is no other non-whitespace characters in the line before
			var concatenate = false;       // '\' on the very end of the line
			var currentLine = 0;
			var currentIndex = 0;
			while (currentLine < endLine || currentIndex < endIndex)
			{
				var line = this.mLines[currentLine];

				if (currentIndex == 0 && !concatenate)
				{
					withinSingleLineComment = false;
					withinPreproc = false;
					firstChar = true;
				}

				concatenate = false;

				if (line.Count > 0)
				{
					var g = line[currentIndex];
					var c = g.Char;

					if (c != this.mLanguageDefinition.PreprocChar && !char.IsWhiteSpace(c))
						firstChar = false;

					if ((currentIndex == line.Count - 1) && line[^1].Char == '\\')
						concatenate = true;

					var inComment = commentStartLine < currentLine || (commentStartLine == currentLine && commentStartIndex <= currentIndex);

					if (withinString)
					{
						g.MultiLineComment = inComment;
						line[currentIndex] = g;

						if (c == '\"')
						{
							if (currentIndex + 1 < line.Count && line[currentIndex + 1].Char == '\"')
							{
								currentIndex += 1;
								if (currentIndex < line.Count)
								{
									g.MultiLineComment = inComment;
									line[currentIndex - 1] = g;
								}
							}
							else
							{
								withinString = false;
							}
						}
						else if (c == '\\')
						{
							currentIndex += 1;
							if (currentIndex < line.Count)
							{
								g.MultiLineComment = inComment;
								line[currentIndex - 1] = g;
							}
						}
					}
					else
					{
						if (firstChar && c == this.mLanguageDefinition.PreprocChar)
							withinPreproc = true;

						if (c == '\"')
						{
							withinString = true;
							g.MultiLineComment = inComment;
							line[currentIndex] = g;
						}
						else
						{
							var from = currentIndex;
							var startStr = this.mLanguageDefinition.CommentStart ?? "";
							var singleStartStr = this.mLanguageDefinition.SingleLineComment ?? "";

							if (singleStartStr.Length > 0 &&
								currentIndex + singleStartStr.Length <= line.Count &&
								StartsWith(line, from, singleStartStr.Length, singleStartStr))
							{
								withinSingleLineComment = true;
							}
							else if (!withinSingleLineComment && currentIndex + startStr.Length <= line.Count &&
								StartsWith(line, from, startStr.Length, startStr))
							{
								commentStartLine = currentLine;
								commentStartIndex = currentIndex;
							}

							inComment = commentStartLine < currentLine || (commentStartLine == currentLine && commentStartIndex <= currentIndex);

							g = line[currentIndex];
							g.MultiLineComment = inComment;
							g.Comment = withinSingleLineComment;
							line[currentIndex] = g;

							var endStr = this.mLanguageDefinition.CommentEnd ?? "";
							if (currentIndex + 1 >= endStr.Length &&
								StartsWith(line, from + 1 - endStr.Length, endStr.Length, endStr))
							{
								commentStartIndex = endIndex;
								commentStartLine = endLine;
							}
						}
					}
					g = line[currentIndex];
					g.Preprocessor = withinPreproc;
					line[currentIndex] = g;
					currentIndex++;
					if (currentIndex >= line.Count)
					{
						currentIndex = 0;
						++currentLine;
					}
				}
				else
				{
					currentIndex = 0;
					++currentLine;
				}
			}
			this.mCheckComments = false;
		}

		if (this.mColorRangeMin < this.mColorRangeMax)
		{
			var increment = (this.mLanguageDefinition.Tokenize == null) ? 10 : 10000;
			var to = Math.Min(this.mColorRangeMin + increment, this.mColorRangeMax);
			ColorizeRange(this.mColorRangeMin, to);
			this.mColorRangeMin = to;

			if (this.mColorRangeMax == this.mColorRangeMin)
			{
				this.mColorRangeMin = int.MaxValue;
				this.mColorRangeMax = 0;
			}
		}
	}
	private float TextDistanceToLineStart(Coordinates aFrom)
	{
		var line = this.mLines[aFrom.Line];
		var distance = 0.0f;
		var spaceSize = ImGui.GetFont().CalcTextSizeA(ImGui.GetFontSize(), float.MaxValue, -1.0f, " ").x;
		var colIndex = GetCharacterIndex(aFrom);
		for (var it = 0; it < line.Count && it < colIndex; it++)
		{
			distance = line[it].Char == '\t'
				? (1.0f + (float) Math.Floor((1.0f + distance) / (this.mTabSize * spaceSize))) * (this.mTabSize * spaceSize)
				: distance + ImGui.GetFont().CalcTextSizeA(ImGui.GetFontSize(), float.MaxValue, -1.0f, line[it].Char).x;
		}

		return distance;
	}
	private void EnsureCursorVisible()
	{
		if (!this.mWithinRender)
		{
			this.mScrollToCursor = true;
			return;
		}

		var scrollX = ImGui.GetScrollX();
		var scrollY = ImGui.GetScrollY();

		var height = ImGui.GetWindowHeight();
		var width = ImGui.GetWindowWidth();

		var top = 1 + (int)Math.Ceiling(scrollY / this.mCharAdvance.y);
		var bottom = (int)Math.Ceiling((scrollY + height) / this.mCharAdvance.y);

		var left = (int)Math.Ceiling(scrollX / this.mCharAdvance.x);
		var right = (int)Math.Ceiling((scrollX + width) / this.mCharAdvance.x);

		var pos = GetActualCursorCoordinates();
		var len = TextDistanceToLineStart(pos);

		if (pos.Line < top)
			ImGui.SetScrollY(Math.Max(0.0f, (pos.Line - 1) * this.mCharAdvance.y));
		if (pos.Line > bottom - 4)
			ImGui.SetScrollY(Math.Max(0.0f, ((pos.Line + 4) * this.mCharAdvance.y) - height));
		if (len + this.mTextStart < left + 4)
			ImGui.SetScrollX(Math.Max(0.0f, len + this.mTextStart - 4));
		if (len + this.mTextStart > right - 4)
			ImGui.SetScrollX(Math.Max(0.0f, len + this.mTextStart + 4 - width));
	}
	private int GetPageSize()
	{
		var height = ImGui.GetWindowHeight() - 20.0f;
		return (int)Math.Floor(height / this.mCharAdvance.y);
	}
	private string GetText(Coordinates aStart, Coordinates aEnd)
	{
		var result = new StringBuilder();

		var lstart = aStart.Line;
		var lend = aEnd.Line;
		var istart = GetCharacterIndex(aStart);
		var iend = GetCharacterIndex(aEnd);
		var s = 0;

		for (var i = lstart; i < lend; i++)
			s += this.mLines[i].Count;

		result.Capacity = s + (s / 8);

		while (istart < iend || lstart < lend)
		{
			if (lstart >= this.mLines.Count)
				break;

			var line = this.mLines[lstart];
			if (istart < line.Count)
			{
				result.Append(line[istart].Char);
				istart++;
			}
			else
			{
				istart = 0;
				++lstart;
				result.Append('\n');
			}
		}

		return result.ToString();
	}
	private Coordinates GetActualCursorCoordinates() => SanitizeCoordinates(this.mState.mCursorPosition);
	private Coordinates SanitizeCoordinates(Coordinates aValue)
	{
		var line = aValue.Line;
		var column = aValue.Column;
		if (line >= this.mLines.Count)
		{
			if (this.mLines.Count <= 0)
			{
				line = 0;
				column = 0;
			}
			else
			{
				line = this.mLines.Count - 1;
				column = GetLineMaxColumn(line);
			}
			return new Coordinates(line, column);
		}
		else
		{
			column = this.mLines.Count > 0 ? Math.Min(column, GetLineMaxColumn(line)) : 0;
			return new Coordinates(line, column);
		}
	}
	private void Advance(Coordinates aCoordinates)
	{
		if (aCoordinates.Line < this.mLines.Count)
		{
			var line = this.mLines[aCoordinates.Line];
			var cindex = GetCharacterIndex(aCoordinates);

			if (cindex + 1 < line.Count)
			{
				var delta = 1;
				cindex = Math.Min(cindex + delta, line.Count - 1);
			}
			else
			{
				++aCoordinates.Line;
				cindex = 0;
			}
			aCoordinates.Column = GetCharacterColumn(aCoordinates.Line, cindex);
		}
	}
	private void DeleteRange(Coordinates aStart, Coordinates aEnd)
	{
		Debug.Assert(aEnd >= aStart);
		Debug.Assert(!ReadOnly);

		//printf("D(%d.%d)-(%d.%d)\n", aStart.mLine, aStart.mColumn, aEnd.mLine, aEnd.mColumn);

		if (aEnd == aStart)
			return;

		var start = GetCharacterIndex(aStart);
		var end = GetCharacterIndex(aEnd);

		if (aStart.Line == aEnd.Line)
		{
			var line = this.mLines[aStart.Line];
			var n = GetLineMaxColumn(aStart.Line);
			if (aEnd.Column >= n)
				line.RemoveRange(start, line.Count - start);
			else
				line.RemoveRange(start, end - start);
		}
		else
		{
			var firstLine = this.mLines[aStart.Line];
			var lastLine = this.mLines[aEnd.Line];

			firstLine.RemoveRange(start, firstLine.Count - start);
			lastLine.RemoveRange(0, end);

			if (aStart.Line < aEnd.Line)
				firstLine.AddRange(lastLine);

			if (aStart.Line < aEnd.Line)
				RemoveLine(aStart.Line + 1, aEnd.Line + 1);
		}

		this.mTextChanged = true;
		this.mUnsavedChanges = true;
	}
	private int InsertTextAt(ref Coordinates aWhere, string aValue)
	{
		Debug.Assert(!ReadOnly);

		var cindex = GetCharacterIndex(aWhere);
		var totalLines = 0;
		var idx = 0;
		while (idx < aValue.Length)
		{
			Debug.Assert(this.mLines.Count > 0);

			if (aValue[idx] == '\r')
			{
				// skip
				++idx;
			}
			else if (aValue[idx] == '\n')
			{
				if (cindex < this.mLines[aWhere.Line].Count)
				{
					var newLine = InsertLine(aWhere.Line + 1);
					var line = this.mLines[aWhere.Line];
					newLine.InsertRange(0, line.Skip(cindex));
					line.RemoveRange(cindex, line.Count - cindex);
				}
				else
				{
					InsertLine(aWhere.Line + 1);
				}
				++aWhere.Line;
				aWhere.Column = 0;
				cindex = 0;
				++totalLines;
				++idx;
			}
			else
			{
				var line = this.mLines[aWhere.Line];
				line.Insert(cindex++, new Glyph(aValue[idx++], PaletteIndex.Default));
				++aWhere.Column;
			}

			this.mTextChanged = true;
			this.mUnsavedChanges = true;
		}

		return totalLines;
	}
	private void AddUndo(UndoRecord aValue)
	{
		Debug.Assert(!ReadOnly);
		//printf("AddUndo: (@%d.%d) +\'%s' [%d.%d .. %d.%d], -\'%s', [%d.%d .. %d.%d] (@%d.%d)\n",
		//	aValue.mBefore.mCursorPosition.mLine, aValue.mBefore.mCursorPosition.mColumn,
		//	aValue.mAdded(), aValue.mAddedStart.mLine, aValue.mAddedStart.mColumn, aValue.mAddedEnd.mLine, aValue.mAddedEnd.mColumn,
		//	aValue.mRemoved(), aValue.mRemovedStart.mLine, aValue.mRemovedStart.mColumn, aValue.mRemovedEnd.mLine, aValue.mRemovedEnd.mColumn,
		//	aValue.mAfter.mCursorPosition.mLine, aValue.mAfter.mCursorPosition.mColumn
		//	);

		this.mUndoBuffer.Push(aValue);
	}
	private Coordinates ScreenPosToCoordinates(Vector2 aPosition)
	{
		var origin = ImGui.GetCursorScreenPos();
		var local = new Vector2(aPosition.x - origin.x, aPosition.y - origin.y);

		var lineNo = Math.Max(0, (int)Math.Floor(local.y / this.mCharAdvance.y));

		var columnCoord = 0;

		if (lineNo < this.mLines.Count)
		{
			var line = this.mLines[lineNo];

			var columnIndex = 0;
			var columnX = 0.0f;

			while (columnIndex < line.Count)
			{
				float columnWidth;

				if (line[columnIndex].Char == '\t')
				{
					var spaceSize = ImGui.GetFont().CalcTextSizeA(ImGui.GetFontSize(), float.MaxValue, -1.0f, " ").x;
					var oldX = columnX;
					var newColumnX = (1.0f + (float)Math.Floor((1.0f + columnX) / (this.mTabSize * spaceSize))) * (this.mTabSize * spaceSize);
					columnWidth = newColumnX - oldX;
					if (this.mTextStart + columnX + (columnWidth * 0.5f) > local.x)
						break;
					columnX = newColumnX;
					columnCoord = (columnCoord / this.mTabSize * this.mTabSize) + this.mTabSize;
					columnIndex++;
				}
				else
				{
					columnWidth = ImGui.GetFont().CalcTextSizeA(ImGui.GetFontSize(), float.MaxValue, -1.0f, line[columnIndex++].Char).x;
					if (this.mTextStart + columnX + (columnWidth * 0.5f) > local.x)
						break;
					columnX += columnWidth;
					columnCoord++;
				}
			}
		}

		return SanitizeCoordinates(new Coordinates(lineNo, columnCoord));
	}
	private Coordinates FindWordStart(Coordinates at)
	{
		if (at.Line >= this.mLines.Count)
			return at;

		var line = this.mLines[at.Line];
		var cindex = GetCharacterIndex(at) - 1;

		if (cindex >= line.Count)
		{
			if (line.Count == 0)
				return at;
			cindex = line.Count - 1;
		}
		if (cindex <= 0)
			return at;

		if (char.IsWhiteSpace(line[cindex].Char))
		{
			while (cindex > 0 && char.IsWhiteSpace(line[cindex].Char))
				--cindex;
			cindex++;
			return new Coordinates(at.Line, GetCharacterColumn(at.Line, cindex));
		}

		var cstart = line[cindex].ColorIndex;
		while (cindex > 0)
		{
			var c = line[cindex].Char;
			if (c <= 32 && char.IsWhiteSpace(c))
			{
				cindex++;
				break;
			}
			if (cstart != line[cindex - 1].ColorIndex)
				break;
			--cindex;
		}
		return new Coordinates(at.Line, GetCharacterColumn(at.Line, cindex));
	}
	private Coordinates FindWordEnd(Coordinates at)
	{
		if (at.Line >= this.mLines.Count)
			return at;

		var line = this.mLines[at.Line];
		var cindex = GetCharacterIndex(at);

		if (cindex >= line.Count)
			return at;

		var prevspace = char.IsWhiteSpace(line[cindex].Char);
		var cstart = line[cindex].ColorIndex;
		while (cindex < line.Count)
		{
			var c = line[cindex].Char;
			if (cstart != line[cindex].ColorIndex || prevspace != char.IsWhiteSpace(c))
				break;
			cindex++;
		}
		return new Coordinates(at.Line, GetCharacterColumn(at.Line, cindex));
	}
	private Coordinates FindNextWord(Coordinates at)
	{
		if (at.Line >= this.mLines.Count)
			return at;

		// skip to the next non-word character
		var cindex = GetCharacterIndex(at);
		var isword = false;
		var skip = false;
		if (cindex < this.mLines[at.Line].Count)
		{
			var line = this.mLines[at.Line];
			isword = char.IsLetterOrDigit(line[cindex].Char);
			skip = isword;
		}

		while (!isword || skip)
		{
			if (at.Line >= this.mLines.Count)
			{
				var l = Math.Max(0, this.mLines.Count - 1);
				return new Coordinates(l, GetLineMaxColumn(l));
			}

			var line = this.mLines[at.Line];
			if (cindex < line.Count)
			{
				isword = char.IsLetterOrDigit(line[cindex].Char);

				if (isword && !skip)
					return new Coordinates(at.Line, GetCharacterColumn(at.Line, cindex));

				if (!isword)
					skip = false;

				cindex++;
			}
			else
			{
				cindex = 0;
				++at.Line;
				skip = false;
				isword = false;
			}
		}

		return at;
	}
	private int GetCharacterIndex(Coordinates aCoordinates)
	{
		if (aCoordinates.Line >= this.mLines.Count)
			return -1;
		var line = this.mLines[aCoordinates.Line];
		var c = 0;
		var i = 0;
		for (; i < line.Count && c < aCoordinates.Column;)
		{
			if (line[i].Char == '\t')
				c = (c / this.mTabSize * this.mTabSize) + this.mTabSize;
			else
				++c;
			i += 1;
		}
		return i;
	}
	private int GetCharacterColumn(int aLine, int aIndex)
	{
		if (aLine >= this.mLines.Count)
			return 0;
		var line = this.mLines[aLine];
		var col = 0;
		var i = 0;
		while (i < aIndex && i < line.Count)
		{
			var c = line[i].Char;
			i += 1;
			if (c == '\t')
				col = (col / this.mTabSize * this.mTabSize) + this.mTabSize;
			else
				col++;
		}
		return col;
	}
	private int GetLineCharacterCount(int aLine)
	{
		if (aLine >= this.mLines.Count)
			return 0;
		var line = this.mLines[aLine];
		var c = 0;
		for (var i = 0; i < line.Count; c++)
			i += 1;
		return c;
	}
	private int GetLineMaxColumn(int aLine)
	{
		if (aLine >= this.mLines.Count)
			return 0;
		var line = this.mLines[aLine];
		var col = 0;
		for (var i = 0; i < line.Count;)
		{
			var c = line[i].Char;
			if (c == '\t')
				col = (col / this.mTabSize * this.mTabSize) + this.mTabSize;
			else
				col++;
			i += 1;
		}
		return col;
	}
	private bool IsOnWordBoundary(Coordinates aAt)
	{
		if (aAt.Line >= this.mLines.Count || aAt.Column == 0)
			return true;

		var line = this.mLines[aAt.Line];
		var cindex = GetCharacterIndex(aAt);
		if (cindex >= line.Count)
			return true;

		if (this.mColorizerEnabled)
			return line[cindex].ColorIndex != line[cindex - 1].ColorIndex;

		return char.IsWhiteSpace(line[cindex].Char) != char.IsWhiteSpace(line[cindex - 1].Char);
	}
	private void RemoveLine(int aStart, int aEnd)
	{
		Debug.Assert(!ReadOnly);
		Debug.Assert(aEnd >= aStart);
		Debug.Assert(this.mLines.Count > aEnd - aStart);

		ErrorMarkers etmp = new();
		foreach (var i in this.mErrorMarkers)
		{
			KeyValuePair<int, string> e = new(i.Key >= aStart ? i.Key - 1 : i.Key, i.Value);
			if (e.Key >= aStart && e.Key <= aEnd)
				continue;
			etmp.Add(e.Key, e.Value);
		}
		this.mErrorMarkers = etmp;

		Breakpoints btmp = new();
		foreach (var i in this.mBreakpoints)
		{
			if (i >= aStart && i <= aEnd)
				continue;
			btmp.Add(i >= aStart ? i - 1 : i);
		}
		this.mBreakpoints = btmp;

		this.mLines.RemoveRange(aStart, aEnd - aStart);
		Debug.Assert(this.mLines.Count > 0);

		this.mTextChanged = true;
		this.mUnsavedChanges = true;
	}
	private void RemoveLine(int aIndex)
	{

		Debug.Assert(!ReadOnly);
		Debug.Assert(this.mLines.Count > 1);

		var etmp = new ErrorMarkers();
		foreach (var i in this.mErrorMarkers)
		{
			KeyValuePair<int, string> e = new(i.Key > aIndex ? i.Key - 1 : i.Key, i.Value);
			if (e.Key - 1 == aIndex)
				continue;
			etmp.Add(e.Key, e.Value);
		}
		this.mErrorMarkers = etmp;

		Breakpoints btmp = new();
		foreach (var i in this.mBreakpoints)
		{
			if (i == aIndex)
				continue;
			btmp.Add(i >= aIndex ? i - 1 : i);
		}
		this.mBreakpoints = btmp;

		this.mLines.RemoveAt(aIndex);
		Debug.Assert(this.mLines.Count > 0);

		this.mTextChanged = true;
		this.mUnsavedChanges = true;
	}
	private Line InsertLine(int aIndex)
	{
		Debug.Assert(!ReadOnly);

		var result = new Line();
		this.mLines.Insert(aIndex, result);

		ErrorMarkers etmp = new();
		foreach (var i in this.mErrorMarkers)
			etmp.Add(i.Key >= aIndex ? i.Key + 1 : i.Key, i.Value);
		this.mErrorMarkers = etmp;

		Breakpoints btmp = new();
		foreach (var i in this.mBreakpoints)
			btmp.Add(i >= aIndex ? i + 1 : i);
		this.mBreakpoints = btmp;

		return result;
	}
	private void EnterCharacter(char aChar, bool aShift)
	{
		Debug.Assert(!ReadOnly);

		UndoRecord u = new();

		u.mBefore = this.mState;

		if (HasSelection())
		{
			if (aChar == '\t' && this.mState.mSelectionStart.Line != this.mState.mSelectionEnd.Line)
			{

				var start = this.mState.mSelectionStart;
				var end = this.mState.mSelectionEnd;
				var originalEnd = end;

				if (start > end)
					(start, end) = (end, start);
				start.Column = 0;
				//			end.mColumn = end.mLine < mLines.Count ? mLines[end.mLine].Count : 0;
				if (end.Column == 0 && end.Line > 0)
					--end.Line;
				if (end.Line >= this.mLines.Count)
					end.Line = this.mLines.Count > 0 ? this.mLines.Count - 1 : 0;
				end.Column = GetLineMaxColumn(end.Line);

				//if (end.mColumn >= GetLineMaxColumn(end.mLine))
				//	end.mColumn = GetLineMaxColumn(end.mLine) - 1;

				u.mRemovedStart = start;
				u.mRemovedEnd = end;
				u.mRemoved = GetText(start, end);

				var modified = false;

				for (var i = start.Line; i <= end.Line; i++)
				{
					var line = this.mLines[i];
					if (aShift)
					{
						if (line.Count > 0)
						{
							if (line[0].Char == '\t')
							{
								line.RemoveAt(0);
								modified = true;
							}
							else
							{
								for (var j = 0; j < this.mTabSize && line.Count > 0 && line[0].Char == ' '; j++)
								{
									line.RemoveAt(0);
									modified = true;
								}
							}
						}
					}
					else
					{
						line.Insert(0, new Glyph('\t', PaletteIndex.Background));
						modified = true;
					}
				}

				if (modified)
				{
					start = new Coordinates(start.Line, GetCharacterColumn(start.Line, 0));
					Coordinates rangeEnd;
					if (originalEnd.Column != 0)
					{
						end = new Coordinates(end.Line, GetLineMaxColumn(end.Line));
						rangeEnd = end;
						u.mAdded = GetText(start, end);
					}
					else
					{
						end = new Coordinates(originalEnd.Line, 0);
						rangeEnd = new Coordinates(end.Line - 1, GetLineMaxColumn(end.Line - 1));
						u.mAdded = GetText(start, rangeEnd);
					}

					u.mAddedStart = start;
					u.mAddedEnd = rangeEnd;
					u.mAfter = this.mState;

					this.mState.mSelectionStart = start;
					this.mState.mSelectionEnd = end;
					AddUndo(u);

					this.mTextChanged = true;
					this.mUnsavedChanges = true;

					EnsureCursorVisible();
				}

				return;
			} // c == '\t'
			else
			{
				u.mRemoved = GetSelectedText();
				u.mRemovedStart = this.mState.mSelectionStart;
				u.mRemovedEnd = this.mState.mSelectionEnd;
				DeleteSelection();
			}
		} // HasSelection

		var coord = GetActualCursorCoordinates();
		u.mAddedStart = coord;

		Debug.Assert(this.mLines.Count > 0);

		if (aChar == '\n')
		{
			InsertLine(coord.Line + 1);
			var line = this.mLines[coord.Line];
			var newLine = this.mLines[coord.Line + 1];

			if (this.mLanguageDefinition.AutoIndentation)
			{
				for (var it = 0; it < line.Count && IsAscii(line[it].Char) && char.IsWhiteSpace(line[it].Char); ++it)
					newLine.Add(line[it]);
			}

			var whitespaceSize = newLine.Count;
			var cindex = GetCharacterIndex(coord);
			newLine.AddRange(line.Skip(cindex));
			line.RemoveRange(cindex, line.Count - cindex);
			CursorPosition = new Coordinates(coord.Line + 1, GetCharacterColumn(coord.Line + 1, whitespaceSize));
			u.mAdded = aChar.ToString();
		}
		else
		{
			var line = this.mLines[coord.Line];
			var cindex = GetCharacterIndex(coord);

			if (this.mOverwrite && cindex < line.Count)
			{
				var d = 1;

				u.mRemovedStart = this.mState.mCursorPosition;
				u.mRemovedEnd = new Coordinates(coord.Line, GetCharacterColumn(coord.Line, cindex + d));

				while (d-- > 0 && cindex < line.Count)
				{
					u.mRemoved += line[cindex].Char;
					line.RemoveAt(cindex);
				}
			}

			line.Insert(cindex, new Glyph(aChar, PaletteIndex.Default));
			u.mAdded = aChar.ToString();

			CursorPosition = new Coordinates(coord.Line, GetCharacterColumn(coord.Line, cindex)+1);
		}

		this.mTextChanged = true;
		this.mUnsavedChanges = true;

		u.mAddedEnd = GetActualCursorCoordinates();
		u.mAfter = this.mState;

		AddUndo(u);

		Colorize(coord.Line - 1, 3);
		EnsureCursorVisible();
	}
	private void Backspace(bool wordMode)
	{
		Debug.Assert(!ReadOnly);

		if (this.mLines.Count <= 0)
			return;

		UndoRecord u = new();
		u.mBefore = this.mState;

		if (HasSelection())
		{
			u.mRemoved = GetSelectedText();
			u.mRemovedStart = this.mState.mSelectionStart;
			u.mRemovedEnd = this.mState.mSelectionEnd;

			DeleteSelection();
		}
		else
		{
			var pos = GetActualCursorCoordinates();
			CursorPosition = pos;

			if (this.mState.mCursorPosition.Column == 0)
			{
				if (this.mState.mCursorPosition.Line == 0)
					return;

				u.mRemoved = "\n";
				u.mRemovedStart = u.mRemovedEnd = new Coordinates(pos.Line - 1, GetLineMaxColumn(pos.Line - 1));
				Advance(u.mRemovedEnd);

				var line = this.mLines[this.mState.mCursorPosition.Line];
				var prevLine = this.mLines[this.mState.mCursorPosition.Line - 1];
				var prevSize = GetLineMaxColumn(this.mState.mCursorPosition.Line - 1);
				prevLine.AddRange(line);

				ErrorMarkers etmp = new();
				foreach (var i in this.mErrorMarkers)
					etmp.Add(i.Key - 1 == this.mState.mCursorPosition.Line ? i.Key - 1 : i.Key, i.Value);
				this.mErrorMarkers = etmp;

				RemoveLine(this.mState.mCursorPosition.Line);
				--this.mState.mCursorPosition.Line;
				this.mState.mCursorPosition.Column = prevSize;
			}
			else
			{
				var line = this.mLines[this.mState.mCursorPosition.Line];
				u.mRemovedStart = wordMode ? FindWordStart(pos) : pos with { Column = pos.Column - 1 };
				u.mRemovedEnd = pos;
				u.mRemoved = GetText(u.mRemovedStart, u.mRemovedEnd);
				line.RemoveRange(u.mRemovedStart.Column, u.mRemovedEnd.Column - u.mRemovedStart.Column);
				this.mState.mCursorPosition = u.mRemovedStart;
			}

			this.mTextChanged = true;
			this.mUnsavedChanges = true;

			EnsureCursorVisible();
			Colorize(this.mState.mCursorPosition.Line, 1);
		}

		u.mAfter = this.mState;
		AddUndo(u);
	}
	private void DeleteSelection()
	{
		Debug.Assert(this.mState.mSelectionEnd >= this.mState.mSelectionStart);

		if (this.mState.mSelectionEnd == this.mState.mSelectionStart)
			return;

		DeleteRange(this.mState.mSelectionStart, this.mState.mSelectionEnd);

		SetSelection(this.mState.mSelectionStart, this.mState.mSelectionStart);
		CursorPosition = this.mState.mSelectionStart;
		Colorize(this.mState.mSelectionStart.Line, 1);
	}
	private string GetWordUnderCursor() => GetWordAt(CursorPosition);
	private string GetWordAt(Coordinates aCoords)
	{
		var start = FindWordStart(aCoords);
		var end = FindWordEnd(aCoords);

		var istart = GetCharacterIndex(start);
		var iend = GetCharacterIndex(end);

		return new string(this.mLines[aCoords.Line].Skip(istart).Take(iend - istart).Select(x => x.Char).ToArray());
	}
	private Color GetGlyphColor(Glyph aGlyph)
	{
		if (!this.mColorizerEnabled)
			return this.mPalette[(int)PaletteIndex.Default];
		if (aGlyph.Comment)
			return this.mPalette[(int)PaletteIndex.Comment];
		if (aGlyph.MultiLineComment)
			return this.mPalette[(int)PaletteIndex.MultiLineComment];
		var color = this.mPalette[(int)aGlyph.ColorIndex];
		if (aGlyph.Preprocessor)
		{
			var ppcolor = this.mPalette[(int)PaletteIndex.Preprocessor];
			return (ppcolor + color) / 2;
		}
		return color;
	}

	private void HandleKeyboardInputs()
	{
		var io = ImGui.GetIO();
		var shift = io.KeyShift;
		var ctrl = io.ConfigMacOSXBehaviors ? io.KeySuper : io.KeyCtrl;
		var alt = io.ConfigMacOSXBehaviors ? io.KeyCtrl : io.KeyAlt;

		if (ImGui.IsWindowFocused())
		{
			if (ImGui.IsWindowHovered())
				ImGui.SetMouseCursor(ImGuiMouseCursor.TextInput);
			//ImGui.CaptureKeyboardFromApp(true);

			io.WantCaptureKeyboard = true;
			io.WantTextInput = true;

			if (!ReadOnly && ctrl && !shift && !alt && ImGui.IsKeyPressed(ImGuiKey.Z))
				Undo();
			else if (!ReadOnly && !ctrl && !shift && alt && ImGui.IsKeyPressed(ImGuiKey.Backspace))
				Undo();
			else if (!ReadOnly && ctrl && !shift && !alt && ImGui.IsKeyPressed(ImGuiKey.Y))
				Redo();
			else if (!ctrl && !alt && ImGui.IsKeyPressed(ImGuiKey.UpArrow))
				MoveUp(1, shift);
			else if (!ctrl && !alt && ImGui.IsKeyPressed(ImGuiKey.DownArrow))
				MoveDown(1, shift);
			else if (!alt && ImGui.IsKeyPressed(ImGuiKey.LeftArrow))
				MoveLeft(1, shift, ctrl);
			else if (!alt && ImGui.IsKeyPressed(ImGuiKey.RightArrow))
				MoveRight(1, shift, ctrl);
			else if (!alt && ImGui.IsKeyPressed(ImGuiKey.PageUp))
				MoveUp(GetPageSize() - 4, shift);
			else if (!alt && ImGui.IsKeyPressed(ImGuiKey.PageDown))
				MoveDown(GetPageSize() - 4, shift);
			else if (!alt && ctrl && ImGui.IsKeyPressed(ImGuiKey.Home))
				MoveTop(shift);
			else if (ctrl && !alt && ImGui.IsKeyPressed(ImGuiKey.End))
				MoveBottom(shift);
			else if (!ctrl && !alt && ImGui.IsKeyPressed(ImGuiKey.Home))
				MoveHome(shift);
			else if (!ctrl && !alt && ImGui.IsKeyPressed(ImGuiKey.End))
				MoveEnd(shift);
			else if (!ReadOnly && !(ctrl && shift) && !(alt && shift) && !(alt && ctrl) && ImGui.IsKeyPressed(ImGuiKey.Delete))
				Delete(shift, ctrl);
			else if (!ReadOnly && !(ctrl && shift) && !(alt && shift) && !(alt && ctrl) && ImGui.IsKeyPressed(ImGuiKey.Backspace))
			{
				if (alt && CanRedo())
					Redo();
				else
					Backspace(ctrl);
				}
			else if (!ctrl && !shift && !alt && ImGui.IsKeyPressed(ImGuiKey.Insert))
				this.mOverwrite ^= true;
			else if (ctrl && !shift && !alt && ImGui.IsKeyPressed(ImGuiKey.Insert))
				Copy();
			else if (ctrl && !shift && !alt && ImGui.IsKeyPressed(ImGuiKey.C))
				Copy();
			else if (!ReadOnly && !ctrl && shift && !alt && ImGui.IsKeyPressed(ImGuiKey.Insert))
				Paste();
			else if (!ReadOnly && ctrl && !shift && !alt && ImGui.IsKeyPressed(ImGuiKey.V))
				Paste();
			else if (ctrl && !shift && !alt && ImGui.IsKeyPressed(ImGuiKey.X))
				Cut();
			else if (!ctrl && shift && !alt && ImGui.IsKeyPressed(ImGuiKey.Delete))
				Cut();
			else if (ctrl && !shift && !alt && ImGui.IsKeyPressed(ImGuiKey.A))
				SelectAll();
			else if (!ReadOnly && !ctrl && !shift && !alt && ImGui.IsKeyPressed(ImGuiKey.Enter))
				EnterCharacter('\n', false);
			else if (!ReadOnly && !ctrl && !alt && ImGui.IsKeyPressed(ImGuiKey.Tab))
				EnterCharacter('\t', shift);

			if (!ReadOnly && io.InputQueueCharacters.Size > 0)
			{
				for (var i = 0; i < io.InputQueueCharacters.Size; i++)
				{
					var c = io.InputQueueCharacters[i];
					if (c != 0 && (c == '\n' || c >= 32))
						EnterCharacter((char)c, shift);
				}
				io.ClearInputCharacters();
			}
		}
	}
	private void HandleMouseInputs()
	{
		var io = ImGui.GetIO();
		var shift = io.KeyShift;
		var ctrl = io.ConfigMacOSXBehaviors ? io.KeySuper : io.KeyCtrl;
		var alt = io.ConfigMacOSXBehaviors ? io.KeyCtrl : io.KeyAlt;

		if (ImGui.IsWindowHovered())
		{
			if (!shift && !alt)
			{
				var click = ImGui.IsMouseClicked(0);
				var doubleClick = ImGui.IsMouseDoubleClicked(0);
				var t = ImGui.GetTime();
				var tripleClick = click && !doubleClick && this.mLastClick != -1.0f && (t - this.mLastClick) < io.MouseDoubleClickTime;

				/*
				Left mouse button triple click
				*/
				if (tripleClick)
				{
					if (!ctrl)
					{
						this.mState.mCursorPosition = this.mInteractiveStart = this.mInteractiveEnd = ScreenPosToCoordinates(ImGui.GetMousePos());
						this.mSelectionMode = SelectionMode.Line;
						SetSelection(this.mInteractiveStart, this.mInteractiveEnd, this.mSelectionMode);
					}

					this.mLastClick = -1.0f;
				}
				/*
				Left mouse button double click
				*/
				else if (doubleClick)
				{
					if (!ctrl)
					{
						this.mState.mCursorPosition = this.mInteractiveStart = this.mInteractiveEnd = ScreenPosToCoordinates(ImGui.GetMousePos());
						if (this.mSelectionMode == SelectionMode.Line)
							this.mSelectionMode = SelectionMode.Normal;
						else
							this.mSelectionMode = SelectionMode.Word;
						SetSelection(this.mInteractiveStart, this.mInteractiveEnd, this.mSelectionMode);
					}

					this.mLastClick = (float) ImGui.GetTime();
				}
				/*
				Left mouse button click
				*/
				else if (click)
				{
					this.mState.mCursorPosition = this.mInteractiveStart = this.mInteractiveEnd = ScreenPosToCoordinates(ImGui.GetMousePos());
					if (ctrl)
						this.mSelectionMode = SelectionMode.Word;
					else
						this.mSelectionMode = SelectionMode.Normal;
					SetSelection(this.mInteractiveStart, this.mInteractiveEnd, this.mSelectionMode);

					this.mLastClick = (float) ImGui.GetTime();
				}

				// Mouse left button dragging (=> update selection)
				else if (ImGui.IsMouseDragging(0) && ImGui.IsMouseDown(0))
				{
					io.WantCaptureMouse = true;
					this.mState.mCursorPosition = this.mInteractiveEnd = ScreenPosToCoordinates(ImGui.GetMousePos());
					SetSelection(this.mInteractiveStart, this.mInteractiveEnd, this.mSelectionMode);
				}
			}
		}
	}
	private void Render()
	{
		/* Compute mCharAdvance regarding to scaled font size (Ctrl + mouse wheel)*/
		var fontSize = ImGui.GetFont().CalcTextSizeA(ImGui.GetFontSize(), float.MaxValue, -1.0f, "#").x;
		this.mCharAdvance = new Vector2(fontSize, ImGui.GetTextLineHeightWithSpacing() * this.mLineSpacing);

		/* Update palette with the current alpha from style */
		for (var i = 0; i < (int)PaletteIndex.Max; ++i)
		{
			var color = this.mPaletteBase[i];
			color.a *= ImGui.GetStyle().Alpha;
			if (this.mPalette is null || this.mPalette.Length < (int)PaletteIndex.Max)
			{
				this.mPalette = new Color[(int)PaletteIndex.Max];
			}
			this.mPalette[i] = color;
		}

		Debug.Assert(this.mLineBuffer.Length == 0);

		var contentSize = ImGui.GetWindowContentRegionMax();
		var drawList = ImGui.GetWindowDrawList();
		var longest = this.mTextStart;

		if (this.mScrollToTop)
		{
			this.mScrollToTop = false;
			ImGui.SetScrollY(0f);
		}

		var cursorScreenPos = ImGui.GetCursorScreenPos();
		var scrollX = ImGui.GetScrollX();
		var scrollY = ImGui.GetScrollY();

		var lineNo = (int)Math.Floor(scrollY / this.mCharAdvance.y);
		var globalLineMax = this.mLines.Count;
		var lineMax = Math.Max(0, Math.Min(this.mLines.Count - 1, lineNo + (int)Math.Floor((scrollY + contentSize.y) / this.mCharAdvance.y)));

		// Deduce mTextStart by evaluating mLines size (global lineMax) plus two spaces as text width
		var buf = $" {globalLineMax} ";
		this.mTextStart = ImGui.GetFont().CalcTextSizeA(ImGui.GetFontSize(), float.MaxValue, -1.0f, buf).x + this.mLeftMargin;
		buf = $"{lineNo}  ";
		var lineNoWidth = ImGui.GetFont().CalcTextSizeA(ImGui.GetFontSize(), float.MaxValue, -1.0f, buf).x;

		if (this.mLines.Count > 0)
		{
			var spaceSize = ImGui.GetFont().CalcTextSizeA(ImGui.GetFontSize(), float.MaxValue, -1.0f, " ").x;

			while (lineNo <= lineMax)
			{
				var lineStartScreenPos = new Vector2(cursorScreenPos.x, cursorScreenPos.y + (lineNo * this.mCharAdvance.y));
				var textScreenPos = new Vector2(lineStartScreenPos.x + this.mTextStart, lineStartScreenPos.y);

				var line = this.mLines[lineNo];
				longest = Math.Max(this.mTextStart + TextDistanceToLineStart(new Coordinates(lineNo, GetLineMaxColumn(lineNo))), longest);
				var columnNo = 0;
				Coordinates lineStartCoord = new(lineNo, 0);
				Coordinates lineEndCoord = new(lineNo, GetLineMaxColumn(lineNo));

				// Draw selection for the current line
				var sstart = -1.0f;
				var ssend = -1.0f;

				Debug.Assert(this.mState.mSelectionStart <= this.mState.mSelectionEnd);
				if (this.mState.mSelectionStart <= lineEndCoord)
					sstart = this.mState.mSelectionStart > lineStartCoord ? TextDistanceToLineStart(this.mState.mSelectionStart) : 0.0f;
				if (this.mState.mSelectionEnd > lineStartCoord)
					ssend = TextDistanceToLineStart(this.mState.mSelectionEnd < lineEndCoord ? this.mState.mSelectionEnd : lineEndCoord);

				if (this.mState.mSelectionEnd.Line > lineNo)
					ssend += this.mCharAdvance.x;

				if (sstart != -1 && ssend != -1 && sstart < ssend)
				{
					Vector2 vstart = new(lineStartScreenPos.x + this.mTextStart + sstart, lineStartScreenPos.y);
					Vector2 vend = new(lineStartScreenPos.x + this.mTextStart + ssend, lineStartScreenPos.y + this.mCharAdvance.y);
					drawList.AddRectFilled(vstart, vend, this.mPalette[(int)PaletteIndex.Selection].ToUint());
				}

				// Draw breakpoints
				var start = new Vector2(lineStartScreenPos.x + scrollX, lineStartScreenPos.y);

				if (this.mBreakpoints.Contains(lineNo + 1))
				{
					var end = new Vector2(lineStartScreenPos.x + contentSize.x + (2.0f * scrollX), lineStartScreenPos.y + this.mCharAdvance.y);
					drawList.AddRectFilled(start, end, this.mPalette[(int)PaletteIndex.Breakpoint].ToUint());
				}

				// Draw error markers
				if (this.mErrorMarkers.TryGetValue(lineNo + 1, out var value))
				{
					var end = new Vector2(lineStartScreenPos.x + contentSize.x + (2.0f * scrollX), lineStartScreenPos.y + this.mCharAdvance.y);
					drawList.AddRectFilled(start, end, this.mPalette[(int)PaletteIndex.ErrorMarker].ToUint());

					if (ImGui.IsMouseHoveringRect(lineStartScreenPos, end))
					{
						ImGui.BeginTooltip();
						ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.2f, 0.2f, 1.0f));
						ImGui.Text("Error at line {lineNo + 1}:");
						ImGui.PopStyleColor();
						ImGui.Separator();
						ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 1.0f, 0.2f, 1.0f));
						ImGui.Text(value);
						ImGui.PopStyleColor();
						ImGui.EndTooltip();
					}
				}

				if (this.mState.mCursorPosition.Line == lineNo)
				{
					var focused = ImGui.IsWindowFocused();

					// Highlight the current line (where the cursor is)
					if (!HasSelection())
					{
						var end = new Vector2(start.x + contentSize.x + scrollX, start.y + this.mCharAdvance.y);
						drawList.AddRectFilled(start, end, this.mPalette[(int)(focused ? PaletteIndex.CurrentLineFill : PaletteIndex.CurrentLineFillInactive)].ToUint());
						drawList.AddRect(start, end, this.mPalette[(int)PaletteIndex.CurrentLineEdge].ToUint(), 1.0f);
					}

					// Render the cursor
					if (focused)
					{
						var timeEnd = DateTime.Now;
						var elapsed = timeEnd - this.mStartTime;
						if (elapsed.TotalMilliseconds > 400)
						{
							var width = 1.0f;
							var cindex = GetCharacterIndex(this.mState.mCursorPosition);
							var cx = TextDistanceToLineStart(this.mState.mCursorPosition);

							if (this.mOverwrite && cindex < line.Count)
							{
								var c = line[cindex].Char;
								if (c == '\t')
								{
									var x = (1.0f + (float)Math.Floor((1.0f + cx) / (this.mTabSize * spaceSize))) * (this.mTabSize * spaceSize);
									width = x - cx;
								}
								else
								{
									width = ImGui.GetFont().CalcTextSizeA(ImGui.GetFontSize(), float.MaxValue, -1.0f, line[cindex].Char).x;
								}
							}
							Vector2 cstart = new(textScreenPos.x + cx, lineStartScreenPos.y);
							Vector2 cend = new(textScreenPos.x + cx + width, lineStartScreenPos.y + this.mCharAdvance.y);
							drawList.AddRectFilled(cstart, cend, this.mPalette[(int)PaletteIndex.Cursor].ToUint());
							if (elapsed.TotalMilliseconds > 800)
								this.mStartTime = timeEnd;
						}
					}
				}

				// Render colorized text
				var prevColor = line.Count > 0 ? GetGlyphColor(line[0]) : this.mPalette[(int)PaletteIndex.Default];
				Vector2 bufferOffset = default;

				for (var i = 0; i < line.Count;)
				{
					var glyph = line[i];
					var color = GetGlyphColor(glyph);

					if ((color != prevColor || glyph.Char == '\t' || glyph.Char == ' ') && this.mLineBuffer.Length > 0)
					{
						Vector2 newOffset = new(textScreenPos.x + bufferOffset.x, textScreenPos.y + bufferOffset.y);
						drawList.AddText(newOffset, prevColor.ToUint(), this.mLineBuffer.ToString());
						var textSize = ImGui.GetFont().CalcTextSizeA(ImGui.GetFontSize(), float.MaxValue, -1.0f, this.mLineBuffer.ToString());
						bufferOffset.x += textSize.x;
						this.mLineBuffer.Clear();
					}
					prevColor = color;

					if (glyph.Char == '\t')
					{
						var oldX = bufferOffset.x;
						bufferOffset.x = (1.0f + (float)Math.Floor((1.0f + bufferOffset.x) / (this.mTabSize * spaceSize))) * (this.mTabSize * spaceSize);
						++i;

						if (this.mShowWhitespaces)
						{
							var s = ImGui.GetFontSize();
							var x1 = textScreenPos.x + oldX + 1.0f;
							var x2 = textScreenPos.x + bufferOffset.x - 1.0f;
							var y = textScreenPos.y + bufferOffset.y + (s * 0.5f);
							Vector2 p1 = new(x1, y);
							Vector2 p2 = new(x2, y);
							Vector2 p3 = new(x2 - (s * 0.2f), y - (s * 0.2f));
							Vector2 p4 = new(x2 - (s * 0.2f), y + (s * 0.2f));
							drawList.AddLine(p1, p2, 0x90909090);
							drawList.AddLine(p2, p3, 0x90909090);
							drawList.AddLine(p2, p4, 0x90909090);
						}
					}
					else if (glyph.Char == ' ')
					{
						if (this.mShowWhitespaces)
						{
							var s = ImGui.GetFontSize();
							var x = textScreenPos.x + bufferOffset.x + (spaceSize * 0.5f);
							var y = textScreenPos.y + bufferOffset.y + (s * 0.5f);
							drawList.AddCircleFilled(new Vector2(x, y), 1.5f, 0x80808080, 4);
						}
						bufferOffset.x += spaceSize;
						i++;
					}
					else
					{
							this.mLineBuffer.Append(line[i++].Char);
					}
					++columnNo;
				}

				if (this.mLineBuffer.Length > 0)
				{
					Vector2 newOffset = new(textScreenPos.x + bufferOffset.x, textScreenPos.y + bufferOffset.y);
					drawList.AddText(newOffset, prevColor.ToUint(), this.mLineBuffer.ToString());
					this.mLineBuffer.Clear();
				}

				// Draw line number (right aligned)
				drawList.AddText(new Vector2(lineStartScreenPos.x + this.mTextStart - lineNoWidth, lineStartScreenPos.y), this.mPalette[(int) PaletteIndex.LineNumber].ToUint(), buf);

				++lineNo;
			}

			// Draw a tooltip on known identifiers/preprocessor symbols
			if (ImGui.IsMousePosValid())
			{
				var id = GetWordAt(ScreenPosToCoordinates(ImGui.GetMousePos()));
				if (id.Length > 0)
				{
					if (this.mLanguageDefinition.Identifiers.TryGetValue(id, out var value))
					{
						ImGui.BeginTooltip();
						ImGui.TextUnformatted(value.Declaration);
						ImGui.EndTooltip();
					}
					else
					{
						if (this.mLanguageDefinition.PreprocIdentifiers.TryGetValue(id, out value))
						{
							ImGui.BeginTooltip();
							ImGui.TextUnformatted(value.Declaration);
							ImGui.EndTooltip();
						}
					}
				}
			}
		}


		ImGui.Dummy(new Vector2(longest + 2, this.mLines.Count * this.mCharAdvance.y));

		if (this.mScrollToCursor)
		{
			EnsureCursorVisible();
			ImGui.SetWindowFocus();
			this.mScrollToCursor = false;
		}
	}

	private static bool IsValidIdentifierDefault(char[] buffer, int start, int length)
	{
		var end = start + length;
		var ch = buffer[start];
		for (var i = start; i < end; i++, ch = buffer[i])
		{
			if ((i == start && char.IsDigit(ch)) || !(char.IsLetter(ch) || ch == '_'))
				return false;
		}
		return true;
	}

	private static bool StartsWith(List<Glyph> line, int from, int length, string str)
	{
		Debug.Assert(line.Count >= from + length);
		if (string.IsNullOrEmpty(str) || length < str.Length)
			return false;
		for (var i = 0; i < str.Length; i++)
		{
			if (line[i].Char != str[i])
				return false;
		}
		return true;
	}
	private static bool IsAscii(char c)
	{
		// For earlier versions or if char.IsAscii is not available
		return c >= 0 && c <= 127;
	}
}