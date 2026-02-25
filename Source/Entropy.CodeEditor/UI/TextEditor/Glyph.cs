namespace Entropy.CodeEditor.UI.TextEditor;

public struct Glyph : IEquatable<Glyph>
{
	public char Char { get; set; }
	public PaletteIndex ColorIndex { get; set; }
	public bool Comment { get; set; }
	public bool MultiLineComment { get; set; }
	public bool Preprocessor { get; set; }

	public Glyph(char aChar, PaletteIndex aColorIndex)
	{
		this.Char = aChar;
		this.ColorIndex = aColorIndex;
		this.Comment = false;
		this.MultiLineComment = false;
		this.Preprocessor = false;
	}

	public override bool Equals(object obj) => obj is Glyph other && Equals(other);

	public override int GetHashCode() => HashCode.Combine(Char, ColorIndex, Comment, MultiLineComment, Preprocessor);

	public static bool operator ==(Glyph left, Glyph right) => left.Equals(right);

	public static bool operator !=(Glyph left, Glyph right) => !(left == right);

	public bool Equals(Glyph other) =>
		Char == other.Char &&
		ColorIndex == other.ColorIndex &&
		Comment == other.Comment &&
		MultiLineComment == other.MultiLineComment &&
		Preprocessor == other.Preprocessor;
}