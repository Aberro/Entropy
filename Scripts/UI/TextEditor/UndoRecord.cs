#nullable enable
using Entropy.Scripts.UI.TextEditor;
using System.Diagnostics;

namespace Entropy.Scripts.UI.TextEditor
{
	public partial class TextEditor
	{
		private class UndoRecord
		{
			public UndoRecord() { }

			public UndoRecord(
				string? aAdded,
				Coordinates aAddedStart,
				Coordinates aAddedEnd,
				string? aRemoved,
				Coordinates aRemovedStart,
				Coordinates aRemovedEnd,
				EditorState aBefore,
				EditorState aAfter)
			{
				this.mAdded = aAdded;
				this.mAddedStart = aAddedStart;
				this.mAddedEnd = aAddedEnd;
				this.mRemoved = aRemoved;
				this.mRemovedStart = aRemovedStart;
				this.mRemovedEnd = aRemovedEnd;
				this.mBefore = aBefore;
				this.mAfter = aAfter;
				Debug.Assert(this.mAddedStart <= this.mAddedEnd);
				Debug.Assert(this.mRemovedStart <= this.mRemovedEnd);
			}

			public void Undo(TextEditor editor)
			{
				if (!string.IsNullOrEmpty(this.mAdded))
				{
					editor.DeleteRange(this.mAddedStart, this.mAddedEnd);
					editor.Colorize(this.mAddedStart.mLine - 1, this.mAddedEnd.mLine - this.mAddedStart.mLine + 2);
				}

				if (!string.IsNullOrEmpty(this.mRemoved))
				{
					var start = this.mRemovedStart;
					editor.InsertTextAt(start, this.mRemoved!);
					editor.Colorize(this.mRemovedStart.mLine - 1, this.mRemovedEnd.mLine - this.mRemovedStart.mLine + 2);
				}

				editor.mState = this.mBefore;
				editor.EnsureCursorVisible();
			}
			public void Redo(TextEditor aEditor)
			{
				if (!string.IsNullOrEmpty(this.mRemoved))
				{
					aEditor.DeleteRange(this.mRemovedStart, this.mRemovedEnd);
					aEditor.Colorize(this.mRemovedStart.mLine - 1, this.mRemovedEnd.mLine - this.mRemovedStart.mLine + 1);
				}

				if (!string.IsNullOrEmpty(this.mAdded))
				{
					var start = this.mAddedStart;
					aEditor.InsertTextAt(start, this.mAdded!);
					aEditor.Colorize(this.mAddedStart.mLine - 1, this.mAddedEnd.mLine - this.mAddedStart.mLine + 1);
				}

				aEditor.mState = this.mAfter;
				aEditor.EnsureCursorVisible();
			}

			public string? mAdded;
			public Coordinates mAddedStart;
			public Coordinates mAddedEnd;

			public string? mRemoved;
			public Coordinates mRemovedStart;
			public Coordinates mRemovedEnd;

			public EditorState mBefore;
			public EditorState mAfter;
		};
	}
}
