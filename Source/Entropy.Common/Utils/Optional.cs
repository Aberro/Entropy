namespace Entropy.Common.Utils;

/// <summary>
/// This is basically same as <see cref="Nullable{T}"/>, but it can be used for reference types as well.
/// </summary>
/// <typeparam name="T"></typeparam>
[Serializable]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
public struct Optional<T>(T value) : IEquatable<Optional<T>>
{
	private readonly bool hasValue = true;
	private T value = value;
	public readonly bool HasValue => hasValue;
	public readonly T Value => hasValue ? value : throw new InvalidOperationException("No value!");
	public Optional() : this(default!) => hasValue = false;
	public readonly T GetValueOrDefault() => value;
	public readonly T GetValueOrDefault(T defaultValue) => hasValue ? value : defaultValue;
	public readonly override bool Equals(object? obj) => obj is Optional<T> opt ? Equals(opt) : (hasValue ? (obj is not null ? value!.Equals(obj) : false) : obj is null);
	public readonly override int GetHashCode() => hasValue ? value!.GetHashCode() : 0;
	public readonly override string ToString() => hasValue ? value!.ToString() : "";
	public readonly bool Equals(Optional<T> other) => hasValue ? (other.hasValue ? value!.Equals(other.Value) : false) : !other.hasValue;

	public static implicit operator Optional<T>(T value) => new(value);
	//public static implicit operator Optional<T>(object? nullValue) => nullValue is T t ? new Optional<T>(t) : default;
	public static explicit operator T(Optional<T> value) => value.Value;
	public static bool operator ==(Optional<T> left, object? right) => left.Equals(right);
	public static bool operator !=(Optional<T> left, object? right) => !left.Equals(right);
}