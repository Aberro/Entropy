
namespace Entropy.Common.Configs;

public class PropertyChangingEventArgs<T> : EventArgs
{
	public ConfigEntryBase Entry { get; }
	public T Value { get; }
	public bool Cancel
	{
		get;
		set
		{
			field |= value; // Only allow setting Cancel to true. Either of the handlers can cancel the event, but cannot reset cancellation.
		}
	}
	public PropertyChangingEventArgs(ConfigEntry<T> entry, T value)
	{
		Entry = entry;
		Value = value;
	}
}
public class PropertyChangedEventArgs<T> : EventArgs
{
	public ConfigEntryBase Entry { get; }
	public PropertyChangedEventArgs(ConfigEntry<T> entry)
	{
		Entry = entry;
	}
}
public delegate void PropertyChangingEventHandler<T>(object sender, PropertyChangingEventArgs<T> args);
public delegate void PropertyChangedEventHandler<T>(object sender, PropertyChangedEventArgs<T> args);