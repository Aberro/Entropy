using System.Diagnostics;

namespace Entropy.CodeEditor.UI.TextEditor;

public struct Coordinates : IEquatable<Coordinates>
{
	public int Line { get; set; }
	public int Column { get; set; }
	public Coordinates(int aLine, int aColumn)
	{
		this.Line = aLine;
		this.Column = aColumn;
		Debug.Assert(aLine >= 0);
		Debug.Assert(aColumn >= 0);
	}
	public static readonly Coordinates Invalid = new(-1, -1);

	public static bool operator ==(Coordinates left, Coordinates right) => left.Line == right.Line && left.Column == right.Column;

	public static bool operator !=(Coordinates left, Coordinates right) => left.Line != right.Line || left.Column != right.Column;

	public static bool operator <(Coordinates left, Coordinates right) =>
		left.Line != right.Line
			? left.Line < right.Line
			: left.Column < right.Column;

	public static bool operator >(Coordinates left, Coordinates right) =>
		left.Line != right.Line
			? left.Line > right.Line
			: left.Column > right.Column;

	public static bool operator <=(Coordinates left, Coordinates right) =>
		left.Line != right.Line
			? left.Line < right.Line
			: left.Column <= right.Column;

	public static bool operator >=(Coordinates left, Coordinates right) =>
		left.Line != right.Line
			? left.Line > right.Line
			: left.Column >= right.Column;
	public bool Equals(Coordinates other) => this.Line == other.Line && this.Column == other.Column;

	public override bool Equals(object? obj) => obj is Coordinates other && Equals(other);
	public override int GetHashCode() => HashCode.Combine(this.Line, this.Column);

	public int CompareTo(Coordinates other)
	{
		if (this.Line != other.Line)
			return this.Line.CompareTo(other.Line);
		return this.Column.CompareTo(other.Column);
	}
}