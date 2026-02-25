using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Entropy.Common.Utils;

// Based on https://weblogs.asp.net/pwelter34/444961
/// <summary>
/// A serializable dictionary that can be serialized by XmlSerializer.
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
[XmlRoot("Dictionary")]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
{
	private static readonly Dictionary<Type, XmlSerializer> _serializersCache = [];


	public SerializableDictionary() : base() { }
	public SerializableDictionary(int capacity) : base(capacity) { }
	public SerializableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> copyFrom) : base(copyFrom) { }
	public SerializableDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }
	public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
	public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }
	public SerializableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> copyFrom, IEqualityComparer<TKey> comparer) : base(copyFrom, comparer) { }
	public SerializableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }
	public SerializableDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }

	public XmlSchema? GetSchema() => null;
	public void ReadXml(XmlReader reader)
	{
		ArgumentNullException.ThrowIfNull(reader);
		if (reader.IsEmptyElement)
			return;

		XmlSerializer? keySerializer = null;
		if (!typeof(TKey).IsPrimitive && Type.GetTypeCode(typeof(TKey)) != TypeCode.String)
			keySerializer = new XmlSerializer(typeof(TKey));

		reader.ReadStartElement();

		while (reader.IsStartElement("Item"))
		{
			TKey key;
			TValue value = default!;
			var valueDeserialized = false;
			reader.ReadStartElement("Item");
			// I FUCKING HATE XML!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			// From here on we're in samurai mode. Samurai do not think how and why it works nor supposed to work, samurai only sees his goal.
			if (keySerializer is not null)
			{
				reader.ReadStartElement("Key");
				key = (TKey) keySerializer.Deserialize(reader);
				reader.ReadEndElement();
			}
			else
			{
				reader.Read();
				var attr = reader.GetAttribute("key");
				key = (TKey) Convert.ChangeType(attr, typeof(TKey), CultureInfo.InvariantCulture);
				reader.Read();
			}
			//reader.ReadEndElement();

			reader.Read();
			try
			{
				var typeFullName = reader.GetAttribute("type");
				var valueType = typeFullName != null ? Type.GetType(typeFullName, false) : null;
				reader.Read();
				if (valueType is not null && valueType != typeof(void))
				{
					var valueSerializer = GetSerializer(valueType);
					value = (TValue) valueSerializer.Deserialize(reader);
					valueDeserialized = true;
				}
				else
					CommonMod.Instance.Logger.LogWarning($"Could not deserialize {typeFullName}");
			}
			catch(Exception e)
			{
				CommonMod.Instance.Logger.LogException(e);
			}
			reader.ReadEndElement();

			reader.ReadEndElement();

			if (valueDeserialized)
				this.Add(key, value!);
		}
		reader.ReadEndElement();
	}

	public void WriteXml(XmlWriter writer)
	{
		ArgumentNullException.ThrowIfNull(writer);
		XmlSerializer? keySerializer = null;
		if (!typeof(TKey).IsPrimitive && Type.GetTypeCode(typeof(TKey)) != TypeCode.String)
			keySerializer = new XmlSerializer(typeof(TKey));

		foreach (var kvp in this)
		{
			writer.WriteStartElement("Item");
			writer.WriteStartElement("Key");
			if (keySerializer is not null)
				keySerializer.Serialize(writer, kvp.Key);
			else
				writer.WriteAttributeString("key", Convert.ToString(kvp.Key!, CultureInfo.InvariantCulture));
			writer.WriteEndElement();

			writer.WriteStartElement("Value");
			var valueType = kvp.Value?.GetType() ?? typeof(void);
			writer.WriteAttributeString("type", valueType.AssemblyQualifiedName);
			if (valueType != typeof(void))
			{
				var valueSerializer = GetSerializer(valueType);
				valueSerializer.Serialize(writer, kvp.Value);
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
	}
	private static XmlSerializer GetSerializer(Type type) =>
		_serializersCache.TryGetValue(type, out var result) ? result : _serializersCache[type] = new XmlSerializer(type);
}