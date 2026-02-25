namespace Entropy.CodeEditor.UI.TextEditor;

using Identifiers = Dictionary<string, Identifier>;
using Keywords = HashSet<string>;
using TokenRegexStrings = List<TokenRegexString>;

public struct TokenRegexString : IEquatable<TokenRegexString>
{
	public string Pattern { get; set; }
	public PaletteIndex Color { get; set; }

	public override bool Equals(object obj) => obj is TokenRegexString other && this.Equals(other);

	public override int GetHashCode() => HashCode.Combine(this.Pattern, this.Color);

	public static bool operator ==(TokenRegexString left, TokenRegexString right) => left.Equals(right);

	public static bool operator !=(TokenRegexString left, TokenRegexString right) => !(left == right);

	public bool Equals(TokenRegexString other) => this.Pattern == other.Pattern && this.Color == other.Color;
}
public struct LanguageDefinition : IEquatable<LanguageDefinition>
{
	public delegate IEnumerable<(ArraySegment<char> Segment, PaletteIndex ColorIndex)> TokenizeCallback(char[] buffer, int start, int length);
	public delegate bool IsValidIdentifierCallback(char[] buffer, int start, int length);

	private TokenRegexStrings mTokenRegexStrings;
	private Identifiers mIdentifiers;
	private Identifiers mPreprocIdentifiers;

	public string Name { get; set; }
	public Keywords Keywords { get; set; }
	public Identifiers Identifiers
	{
		get => this.mIdentifiers ??= new Identifiers();
		set => this.mIdentifiers = value;
	}

	public Identifiers PreprocIdentifiers
	{
		get => this.mPreprocIdentifiers ??= new Identifiers();
		set => this.mPreprocIdentifiers = value;
	}
	public string? CommentStart { get; set; }
	public string? CommentEnd { get; set; }
	public string? SingleLineComment { get; set; }
	public char PreprocChar { get; set; }
	public bool AutoIndentation { get; set; }

	public TokenizeCallback? Tokenize { get; set; }
	public IsValidIdentifierCallback? IsValidIdentifier { get; set; }
	public TokenRegexStrings TokenRegexStrings
	{
		get => this.mTokenRegexStrings ??= new TokenRegexStrings();
		set => this.mTokenRegexStrings = value;
	}

	public bool CaseSensitive { get; set; }

	public override bool Equals(object obj) => obj is LanguageDefinition other && this.Equals(other);

	public override int GetHashCode() => HashCode.Combine(
		HashCode.Combine(
			this.Name,
			this.Keywords,
			this.Identifiers,
			this.PreprocIdentifiers,
			this.CommentStart),
		HashCode.Combine(
			this.CommentEnd,
			this.SingleLineComment,
			this.PreprocChar,
			this.AutoIndentation,
			this.TokenRegexStrings,
			this.CaseSensitive));

	public static bool operator ==(LanguageDefinition left, LanguageDefinition right) => left.Equals(right);

	public static bool operator !=(LanguageDefinition left, LanguageDefinition right) => !(left == right);

	public bool Equals(LanguageDefinition other) => this.Equals(other);
}