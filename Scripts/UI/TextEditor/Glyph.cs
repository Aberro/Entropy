#nullable enable

namespace Entropy.Scripts.UI.TextEditor
{
	public struct Glyph
	{
		public char mChar;
		public PaletteIndex mColorIndex;
		public bool mComment;
		public bool mMultiLineComment;
		public bool mPreprocessor;

		public Glyph(char aChar, PaletteIndex aColorIndex)
		{
			this.mChar = aChar;
			this.mColorIndex = aColorIndex;
			this.mComment = false;
			this.mMultiLineComment = false;
			this.mPreprocessor = false;
		}
	}
}