using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Windows.Forms;
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

		ConsumeWhitespace(reader);
		var tableName = reader.Name;
		var tableDepth = reader.Depth;
		reader.Read(); // Consume the start element.
		//reader.ReadStartElement(); 

		while (reader.IsStartElement("Item"))
		{
			reader.ReadStartElement("Item"); // Read the item start element. It should have no attributes.

			var key = ReadKeyElement(reader, keySerializer);
			var value = ReadValueElement(reader, tableDepth);

			if (value.HasValue)
				this.Add(key, value.Value);
			if (reader.NodeType != XmlNodeType.EndElement || reader.Name != "Item")
				throw new ApplicationException("Error during deserialization: invalid element name!");
			reader.ReadEndElement();
			ConsumeWhitespace(reader);
		}
		if (reader.NodeType != XmlNodeType.EndElement || reader.Name != tableName)
			throw new ApplicationException("Error during deserialization: unexpected node!");
		reader.ReadEndElement();

		static TKey ReadKeyElement(XmlReader reader, XmlSerializer? keySerializer)
		{
			TKey key;
			ConsumeWhitespace(reader);
			if (keySerializer is not null)
			{
				reader.ReadStartElement("Key"); // First element in an item should be the key, so read it.
				key = (TKey) keySerializer.Deserialize(reader); // Then deserialize the key's value.
				reader.ReadEndElement(); // Consume closing </Key> tag.
			}
			else
			{
				if (reader.Name != "Key")
					throw new ApplicationException("Error during deserialization: invalid element name!");
				var attr = reader.GetAttribute("key"); // The attribute that stores key's primitive value.
				key = (TKey) Convert.ChangeType(attr, typeof(TKey), CultureInfo.InvariantCulture); // Convert the string key value into the target primitive value.
				reader.Read(); // Consume the <Key ... /> element.
			}
			ConsumeWhitespace(reader);
			return key;
		}
		static Optional<TValue> ReadValueElement(XmlReader reader, int tableDepth)
		{
			Optional<TValue> value = default;
			ConsumeWhitespace(reader);
			try
			{
				if (reader.Name != "Value")
					throw new ApplicationException("Error during deserialization: invalid element name!");
				var typeFullName = reader.GetAttribute("type");
				var valueType = typeFullName != null ? Type.GetType(typeFullName, false) : null;
				reader.Read(); // consume the <Value ...> element.
				value = ReadValue(reader, valueType, typeFullName, tableDepth);
			}
			catch (Exception e)
			{
				CommonMod.Instance.Logger.LogException(e);
			}

			if (reader.NodeType != XmlNodeType.EndElement && reader.Name != "Value")
				throw new ApplicationException("Error during deserialization: unexpected node!");
			reader.ReadEndElement(); // Consume the </Value> tag.
			ConsumeWhitespace(reader);
			return value;
		}
		static Optional<TValue> ReadValue(XmlReader reader, Type? valueType, string? typeFullName, int tableDepth)
		{
			Optional<TValue> value = default;
			ConsumeWhitespace(reader);
			if (valueType is not null && valueType != typeof(void))
			{
				var valueSerializer = GetSerializer(valueType);
				value = (TValue) valueSerializer.Deserialize(reader);
			}
			else
			{
				CommonMod.Instance.Logger.LogWarning($"Could not deserialize {typeFullName}");
				// Consume elements until we're at next element or the end 
				while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Value")
						break;
					if (reader.Depth <= tableDepth + 1)
						throw new ApplicationException("Error during deserialization: failed to read value!");
				}
			}
			ConsumeWhitespace(reader);
			return value;
		}
		static void ConsumeWhitespace(XmlReader reader)
		{
			while (reader.NodeType == XmlNodeType.Whitespace)
				reader.Read();
		}
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