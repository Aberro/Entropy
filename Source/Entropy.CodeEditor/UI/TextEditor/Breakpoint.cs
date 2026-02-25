namespace Entropy.CodeEditor.UI.TextEditor;

public struct Breakpoint : IEquatable<Breakpoint>
{
	public int Line { get; set; }
	public bool Enabled { get; set; }
	public string? Condition { get; set; }

	public static Breakpoint Create() => new(0);
	private Breakpoint(int _)
	{
		this.Line = -1;
		this.Enabled = false;
		this.Condition = null;
	}

	public override bool Equals(object obj)
	{
		if (obj is not Breakpoint other)
			return false;
		return this.Line == other.Line
			&& this.Enabled == other.Enabled
			&& this.Condition == other.Condition;
	}

	public override int GetHashCode() => HashCode.Combine(this.Line, this.Enabled, this.Condition);

	public static bool operator ==(Breakpoint left, Breakpoint right) => left.Equals(right);

	public static bool operator !=(Breakpoint left, Breakpoint right) => !(left == right);

	public readonly bool Equals(Breakpoint other) =>
		this.Line == other.Line
			&& this.Enabled == other.Enabled
			&& this.Condition == other.Condition;
}