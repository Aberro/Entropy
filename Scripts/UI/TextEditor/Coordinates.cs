#nullable enable
using System;
using System.Diagnostics;

namespace Entropy.Assets.Scripts.Assets.Scripts.UI.TextEditor
{
	public struct Coordinates
	{
		public int mLine;
		public int mColumn;
		public Coordinates(int aLine, int aColumn)
		{
			this.mLine = aLine;
			this.mColumn = aColumn;
			Debug.Assert(aLine >= 0);
			Debug.Assert(aColumn >= 0);
		}
		public static readonly Coordinates Invalid = new Coordinates(-1, -1);

		public static bool operator ==(Coordinates left, Coordinates right) => left.mLine == right.mLine && left.mColumn == right.mColumn;

		public static bool operator !=(Coordinates left, Coordinates right) => left.mLine != right.mLine || left.mColumn != right.mColumn;

		public static bool operator <(Coordinates left, Coordinates right) =>
			left.mLine != right.mLine
				? left.mLine < right.mLine
				: left.mColumn < right.mColumn;

		public static bool operator >(Coordinates left, Coordinates right) =>
			left.mLine != right.mLine
				? left.mLine > right.mLine
				: left.mColumn > right.mColumn;

		public static bool operator <=(Coordinates left, Coordinates right) =>
			left.mLine != right.mLine
				? left.mLine < right.mLine
				: left.mColumn <= right.mColumn;

		public static bool operator >=(Coordinates left, Coordinates right) =>
			left.mLine != right.mLine
				? left.mLine > right.mLine
				: left.mColumn >= right.mColumn;
		public bool Equals(Coordinates other) => this.mLine == other.mLine && this.mColumn == other.mColumn;

		public override bool Equals(object? obj) => obj is Coordinates other && Equals(other);
		public override int GetHashCode()
		{
			return HashCode.Combine(this.mLine, this.mColumn);
		}
	}
}