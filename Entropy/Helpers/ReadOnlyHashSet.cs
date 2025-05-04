using System.Collections;

/// <summary>Represents hash set which don't allow for items addition.</summary>
/// <typeparam name="T">Type of items int he set.</typeparam>
public interface IReadonlyHashSet<T> : IReadOnlyCollection<T>
{
	/// <summary>Returns true if the set contains given item.</summary>
	public bool Contains(T i);
}

/// <summary>Wrapper for a <see cref="HashSet{T}"/> which allows only for lookup.</summary>
/// <typeparam name="T">Type of items in the set.</typeparam>
/// <remarks>Creates new wrapper instance for given hash set.</remarks>
public class ReadonlyHashSet<T>(HashSet<T> set) : IReadonlyHashSet<T>
{
	private readonly HashSet<T> set = set;

	/// <inheritdoc/>
	public int Count => this.set.Count;

	/// <inheritdoc/>
	public bool Contains(T i) => this.set.Contains(i);

	/// <inheritdoc/>
	public IEnumerator<T> GetEnumerator() => this.set.GetEnumerator();
	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => this.set.GetEnumerator();
}

/// <summary>Extension methods for the <see cref="HashSet{T}"/> class.</summary>
public static class HasSetExtensions
{
	/// <summary>Returns read-only wrapper for the set.</summary>
	public static ReadonlyHashSet<T> AsReadOnly<T>(this HashSet<T> s)
		=> new(s);
}