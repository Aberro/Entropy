using Assets.Scripts;
using Assets.Scripts.Objects;
using Assets.Scripts.Serialization;
using Assets.Scripts.Util;
using HarmonyLib;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using static Assets.Scripts.Networking.NetworkUpdateType.Thing.LogicUnit;
using static Assets.Scripts.Serialization.XmlSaveLoad;
using ExtensionsTable = Entropy.Common.Utils.SerializableDictionary<string, object>;

namespace Entropy.Common.Utils;

internal static class ExtensionsStorage
{
	private static readonly ConditionalWeakTable<object, ExtensionsTable> _storage = new();
	public static bool HasExtension<TExtension>(object instance) =>
		_storage.TryGetValue(instance, out var table) && table.ContainsKey(typeof(TExtension).FullName);

	public static void ClearExtension<TExtension>(object instance)
	{
		if (!_storage.TryGetValue(instance, out var table))
			return;
		table.Remove(typeof(TExtension).FullName);
		if (table.Count <= 0)
			_storage.Remove(instance);
	}

	public static Optional<TExtension> GetExtension<TExtension>(object instance) =>
		_storage.TryGetValue(instance, out var table) && table.TryGetValue(typeof(TExtension).FullName, out var result)
			? (Optional<TExtension>)result
			: default;

	public static TExtension GetOrCreateExtension<TExtension>(object instance, Func<TExtension> factory)
	{
		ArgumentNullException.ThrowIfNull(factory);
		if (_storage.TryGetValue(instance, out var table) && table.TryGetValue(typeof(TExtension).FullName, out var result))
			return (TExtension)result;
		result = factory() ?? throw new NullReferenceException($"{nameof(factory)} callback returned null value!");
		if (table is null)
			_storage.Add(instance, table = new ExtensionsTable());
		table.Add(typeof(TExtension).FullName, result);
		return (TExtension)result;
	}

	public static void SetExtension<TExtension>(object instance, TExtension value)
	{
		if(value is null)
			throw new ArgumentNullException(nameof(value));
		if (!_storage.TryGetValue(instance, out var table))
		{
			_storage.Add(instance, new ExtensionsTable());
		} else
		{
			table[typeof(TExtension).FullName] = value;
		}
	}

	internal static IEnumerable<ThingSaveData> GetThingData()
	{
#pragma warning disable CS0618 // Type or member is obsolete
		return _storage.Where(entry =>
			{
				if (entry.Key is Thing thing && entry.Value.Values.Any(ex => ex is IXmlSerializable || ex.GetType().GetCustomAttributes<XmlRootAttribute>().Any()))
				{
					if (thing.IgnoreSave)
					{
						foreach (var ex in entry.Value.Values.Where(ex => ex is IXmlSerializable || ex.GetType().GetCustomAttributes<XmlRootAttribute>().Any()))
							CommonMod.Instance.Logger.LogWarning($"{ex.GetType().FullName} is serializable and is extending an instance of {typeof(Thing).FullName}, but the instance has IgnoreSave property set to true.");
					}
					return true;
				}
				return false;
			})
			.Select(entry => new ExtensionThingData((Thing) entry.Key, entry.Value));
#pragma warning restore CS0618 // Type or member is obsolete
	}
	internal static void RestoreData(Thing thing, ExtensionsTable table)
	{
		if(!_storage.TryGetValue(thing, out var existingTable))
			_storage.Add(thing, existingTable = new ExtensionsTable());
		foreach(var entry in table)
			existingTable[entry.Key] = entry.Value;
	}
	internal static void ResetStorage()
	{
		_storage.Clear();
	}
}

/// <summary>
/// A helper struct to access an instance extensions.
/// </summary>
/// <typeparam name="TClass"></typeparam>
/// <remarks>
/// Initializes a new <see cref="Extension{TClass}"/> instance for the given instance.
/// </remarks>
/// <param name="instance">The instance to associate with extensions.</param>
[Obsolete("Do not use directly! Access through Extensions extension property instead.")]
public struct Extension<TClass>(TClass instance) where TClass : class
{
	private TClass _instance = instance;
	/// <summary>
	/// Initializes a new <see cref="Extension{TClass}"/> instance for the given instance.
	/// </summary>
	/// <param name="instance"></param>
	/// <returns></returns>
	public static Extension<TClass> Get(TClass instance) => new(instance);

	/// <summary>
	/// Gets the extension data of type <typeparamref name="TExtension"/> associated with the instance, if it exists.
	/// </summary>
	/// <typeparam name="TExtension">The type of the extension to retrieve.</typeparam>
	/// <returns></returns>
	public readonly Optional<TExtension> Get<TExtension>() => ExtensionsStorage.GetExtension<TExtension>(_instance);
	/// <summary>
	/// Gets the existing extension of the specified type, or creates and adds it using the provided factory if it does not
	/// exist.
	/// </summary>
	/// <typeparam name="TExtension">The type of the extension to retrieve or create.</typeparam>
	/// <param name="factory">A function that creates an instance of the extension if one does not already exist. Cannot be null.</param>
	/// <returns>The existing or newly created extension of type TExtension.</returns>
	[SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
	public readonly TExtension GetOrCreate<TExtension>(Func<TExtension> factory) => ExtensionsStorage.GetOrCreateExtension(_instance, factory!);
	/// <summary>
	/// Sets the extension data of type <typeparamref name="TExtension"/> for the instance.
	/// If the provided data has a value, it will be set as the extension; otherwise, any existing extension of that type will be cleared.
	/// </summary>
	/// <typeparam name="TExtension">The type of the extension to set.</typeparam>
	/// <param name="data"></param>
	public readonly void Set<TExtension>(TExtension data)
	{
		if(data is null)
			ExtensionsStorage.ClearExtension<TExtension>(_instance);
		else
			ExtensionsStorage.SetExtension(_instance, data);
	}
	/// <summary>
	/// Sets the extension data of type <typeparamref name="TExtension"/> for the instance using a nullable object (used for setting a null value to clear an extension data).
	/// </summary>
	/// <typeparam name="TExtension">The type of the extension to set.</typeparam>
	/// <param name="nullableData"></param>
	public readonly void Reset<TExtension>() => ExtensionsStorage.ClearExtension<TExtension>(_instance);
	/// <summary>
	/// Determines whether the instance has an extension of type <typeparamref name="TExtension"/> associated with it.
	/// </summary>
	/// <typeparam name="TExtension">The type of the extension to check for.</typeparam>
	/// <returns>true if an extension of type TExtension is present; otherwise, false.</returns>
	public readonly bool Has<TExtension>() => ExtensionsStorage.HasExtension<TExtension>(_instance);
}
public static class ClassExtensionExtensions
{
	extension<TClass>(TClass instance) where TClass : class
	{
#pragma warning disable CS0618 // Type or member is obsolete
		public Extension<TClass> Extensions => Extension<TClass>.Get(instance);
#pragma warning restore CS0618 // Type or member is obsolete
	}
}

[SuppressMessage("Design", "CA1051:Do not declare visible instance fields")]
[Obsolete("INTERNAL! Do not use this type, it has to be public for serialization.")]
[XmlInclude(typeof(ExtensionThingData))]
public class ExtensionThingData : ThingSaveData
{
	/// <summary>
	/// The reference ID of the instance that this extension data is associated with.
	/// This is used to link the extension data to the correct instance during deserialization.
	/// </summary>
	[XmlElement]
	public long InstanceReferenceId = -1;
	[XmlElement]
	public ExtensionsTable ExtensionsTable;

	public ExtensionThingData() { ExtensionsTable = null!; }
	public ExtensionThingData(Thing thing, ExtensionsTable table)
	{
		ArgumentNullException.ThrowIfNull(thing);
		this.InstanceReferenceId = thing.ReferenceId;
		this.ExtensionsTable = new ExtensionsTable(table.Where(entry => entry.Value is IXmlSerializable || entry.Value.GetType().GetCustomAttributes<XmlRootAttribute>().Any()));
	}

}

[HarmonyPatch]
public static class ExtensionsSerializationPatches
{
	private static readonly Dictionary<long, WeakReference<Thing>> _loadedThings = new();
	private static List<Type> _extraTypes = null!;
	[HarmonyPatch(typeof(XmlSaveLoad), nameof(GetWorldData))]
	[HarmonyPostfix]
	public static void XmlSaveLoadGetWorldDataPostfix(WorldData __result)
	{
		if (__result is null)
			return;
		__result.OrderedThings.AddRange(ExtensionsStorage.GetThingData());
	}
	[HarmonyPatch(typeof(XmlSaveLoad), nameof(LoadWorld))]
	[HarmonyPrefix]
	public static void XmlSaveLoadLoadWorldPrefix()
	{
		ExtensionsStorage.ResetStorage();
	}
	[HarmonyPatch(typeof(XmlSaveLoad), nameof(LoadThing))]
	[HarmonyPrefix]
	public static bool XmlSaveLoadLoadThingPrefix(ThingSaveData thingData)
	{
#pragma warning disable CS0618 // Type or member is obsolete
		if (thingData is ExtensionThingData extension)
		{
			if (extension.InstanceReferenceId < 0 || !_loadedThings.TryGetValue(extension.InstanceReferenceId, out var thingRef) || !thingRef.TryGetTarget(out var thing))
			{
				CommonMod.Instance.Logger.LogError("Failed to restore serialized extensions data");
				return false;
			}
			ExtensionsStorage.RestoreData(thing, extension.ExtensionsTable);
			return false;
		}
#pragma warning restore CS0618 // Type or member is obsolete
		return true;
	}
	[HarmonyPatch(typeof(XmlSaveLoad), nameof(LoadThing))]
	[HarmonyPostfix]
	public static void XmlSaveLoadLoadThingPrefix(Thing __result)
	{
		if (__result is null)
			return;
		_loadedThings[__result.ReferenceId] = new WeakReference<Thing>(__result);
	}
	// We simply need a point at which we can clear the dictionary safely after it's been used.
	[HarmonyPatch(typeof(PlanetaryAtmosphereSimulation), nameof(PlanetaryAtmosphereSimulation.Load))]
	[HarmonyPostfix]
	public static void PlanetaryAtmosphereSimulationLoadPostfix()
	{
		_loadedThings.Clear();
	}

	[HarmonyPatch(typeof(XmlSaveLoad), nameof(XmlSaveLoad.AddExtraTypes))]
	[HarmonyPostfix]
	public static void XmlSaveLoadAddExtraTypesPostfix(List<Type> extraTypes)
	{
		if (extraTypes is null)
			return;
		_extraTypes = extraTypes;
#pragma warning disable CS0618 // Type or member is obsolete
		_extraTypes.Add(typeof(ExtensionThingData));
#pragma warning restore CS0618 // Type or member is obsolete
		var worldDataField = new Traverse(typeof(Serializers)).Field<XmlSerializer>("_worldData");
		if(worldDataField.Value is not null)
			worldDataField.Value = new XmlSerializer(typeof(XmlSaveLoad.WorldData), XmlSaveLoad.ExtraTypes);
		CommonMod.Instance.Logger.LogInfo("ExtensionThingData extra type added for serialization.");
	}
}