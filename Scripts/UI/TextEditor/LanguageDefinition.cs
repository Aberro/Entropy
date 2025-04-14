#nullable enable
using Entropy.Assets.Scripts.Processor;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Entropy.Assets.Scripts.Assets.Scripts.UI.TextEditor
{
	using Identifiers = Dictionary<string, Identifier>;
	using Keywords = HashSet<string>;
	using TokenRegexStrings = List<TokenRegexString>;

	public struct TokenRegexString
	{
		public string Pattern;
		public PaletteIndex Color;
	}
	public struct LanguageDefinition
	{
		public delegate IEnumerable<(ArraySegment<char> Segment, PaletteIndex ColorIndex)> TokenizeCallback(char[] buffer, int start, int length);

		private TokenRegexStrings mTokenRegexStrings;
		private Identifiers mIdentifiers;
		private Identifiers mPreprocIdentifiers;

		public string mName;
		public Keywords mKeywords;
		public Identifiers Identifiers
		{
			get => this.mIdentifiers ??= new Identifiers();
			set => this.mIdentifiers = value;
		}

		public Identifiers PreprocIdentifiers => this.mPreprocIdentifiers ??= new Identifiers();
		public string? mCommentStart, mCommentEnd, mSingleLineComment;
		public char mPreprocChar;
		public bool mAutoIndentation;

		public TokenizeCallback? mTokenize;
		public TokenRegexStrings TokenRegexStrings
		{
			get => this.mTokenRegexStrings ??= new TokenRegexStrings();
			set => this.mTokenRegexStrings = value;
		}

		public bool mCaseSensitive;
	}
}