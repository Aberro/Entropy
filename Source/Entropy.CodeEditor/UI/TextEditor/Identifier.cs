namespace Entropy.CodeEditor.UI.TextEditor;

public struct Identifier : IEquatable<Identifier>
{
	public Coordinates Location { get; set; }
	public string Declaration { get; set; }

	public override bool Equals(object obj) => obj is Identifier other && Equals(other);

	public override int GetHashCode() => HashCode.Combine(Location, Declaration);

	public static bool operator ==(Identifier left, Identifier right) => left.Equals(right);

	public static bool operator !=(Identifier left, Identifier right) => !(left == right);

	public bool Equals(Identifier other) => Location.Equals(other.Location) && Declaration == other.Declaration;
}