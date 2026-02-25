using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Motherboards;
using Assets.Scripts.Objects.Pipes;
using Entropy.Common.Utils;
using Objects.Rockets;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Entropy.Common.Services;

public static class ProcessorProvider
{
	private static IDictionary<string, string> _identifiers = GetEnumerationSimpleValuesInternal<LogicType>()
		.Union(GetEnumerationSimpleValuesInternal<LogicSlotType>())
		.Union(GetEnumerationSimpleValuesInternal<LogicReagentMode>())
		.Union(GetEnumerationSimpleValuesInternal<LogicBatchMethod>())
		.Union(GetEnumerationTypedValuesInternal<LogicType>())
		.Union(GetEnumerationTypedValuesInternal<LogicSlotType>())
		.Union(GetEnumerationTypedValuesInternal<ConditionOperation>())
		.Union(GetEnumerationTypedValuesInternal<RocketMode>())
		.Union(GetEnumerationTypedValuesInternal<ReEntryProfile>())
		.Union(GetEnumerationTypedValuesInternal<SorterInstruction>())
		.Union(GetEnumerationTypedValuesInternal<PrinterInstruction>())
		.Union(GetEnumerationTypedValuesInternal<ElevatorMode>())
		.Union(GetEnumerationTypedValuesInternal<EntityState>())
		.Union(GetEnumerationTypedValuesInternal<PowerMode>())
		.Union(GetEnumerationTypedValuesInternal<RobotMode>())
		.Union(GetEnumerationTypedValuesInternal<SortingClass>())
		.Union(GetEnumerationTypedValuesInternal<SoundAlert>("Sound"))
		.Union(GetEnumerationTypedValuesInternal<LogicTransmitterMode>("TransmitterMode"))
		.Union(GetEnumerationTypedValuesInternal<ColorType>("Color"))
		.Union(GetEnumerationTypedValuesInternal<AirControlMode>("AirControl"))
		.Union(GetEnumerationTypedValuesInternal<DaylightSensor.DaylightSensorMode>("DaylightSensorMode"))
		.Union(GetEnumerationTypedValuesInternal<AirConditioningMode>("AirCon"))
		.Union(GetEnumerationTypedValuesInternal<VentDirection>("Vent"))
		.Union(GetEnumerationTypedValuesInternal<Slot.Class>("SlotClass"))
		.Union(GetEnumerationTypedValuesInternal<Chemistry.GasType>("GasType"))
		.GroupBy(x => x.Key)
		.ToDictionary(x => x.Key, x => x.First().Value);
	private static Func<IEnumerable<string>> _listOfCommandsProvider = () => Enum.GetValues(typeof(ScriptCommand)).OfType<ScriptCommand>().Select(c => c.ToString());
	private static Func<IDictionary<string, string>> _listOfIdentifiersProvider = () => _identifiers;

	public static IEnumerable<string> ListOfCommands() => _listOfCommandsProvider();
	public static IDictionary<string, string> ListOfIdentifiers() => _listOfIdentifiersProvider();
	public static void SetListOfCommandsProvider(Func<IEnumerable<string>> provider)
	{
		ArgumentNullException.ThrowIfNull(provider);
		_listOfCommandsProvider = provider;
	}
	public static void SetListOfIdentifiersProvider(Func<IDictionary<string, string>> provider)
	{
		ArgumentNullException.ThrowIfNull(provider);
		_listOfIdentifiersProvider = provider;
	}
	private static IDictionary<string, string> GetEnumerationSimpleValuesInternal<T>() where T : Enum =>
		Enum.GetValues(typeof(T)).OfType<T>().ToDictionary(val => val.ToString(), val =>
			Convert.ToInt64(val, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture));
	private static IDictionary<string, string> GetEnumerationTypedValuesInternal<T>(string? typename = null) where T : Enum =>
		Enum.GetValues(typeof(T)).OfType<object>().ToDictionary(val => (typename ?? typeof(T).Name) + @"\." + val.ToString(), val => 
			Convert.ToInt64(val, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture));
}