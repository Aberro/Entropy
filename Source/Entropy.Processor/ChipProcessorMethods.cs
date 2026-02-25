using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Pipes;
using Entropy.Processor.Types;
using System.Reflection;

namespace Entropy.Processor;

public partial class ChipProcessor
{
	internal static readonly MethodInfo GetDeviceMethodInfo = typeof(ChipProcessor).GetMethod(nameof(GetDevice))!;
	internal static readonly MethodInfo GetDeviceByIdMethodInfo = typeof(ChipProcessor).GetMethod(nameof(GetDeviceById))!;
	internal static readonly MethodInfo ReadDeviceMethodInfo = typeof(ChipProcessor).GetMethod(nameof(ReadDevice))!;
	internal static readonly MethodInfo ReadDeviceSlotMethodInfo = typeof(ChipProcessor).GetMethod(nameof(ReadDeviceSlot))!;
	internal static readonly MethodInfo WriteDeviceSlotMethodInfo = typeof(ChipProcessor).GetMethod(nameof(WriteDeviceSlot))!;
	internal static readonly MethodInfo ReadDeviceReagentMethodInfo = typeof(ChipProcessor).GetMethod(nameof(ReadDeviceReagent))!;
	internal static readonly MethodInfo WriteDeviceMethodInfo = typeof(ChipProcessor).GetMethod(nameof(WriteDevice))!;
	internal static readonly MethodInfo WriteDeviceBatchMethodInfo = typeof(ChipProcessor).GetMethod(nameof(WriteDeviceBatch))!;
	internal static readonly MethodInfo ReadDeviceBatchMethodInfo = typeof(ChipProcessor).GetMethod(nameof(ReadDeviceBatch))!;
	internal static readonly MethodInfo BatchReadSlotMethodInfo = typeof(ChipProcessor).GetMethod(nameof(BatchReadSlot))!;
	internal static readonly MethodInfo BatchReadNameMethodInfo = typeof(ChipProcessor).GetMethod(nameof(BatchReadName))!;
	internal static readonly MethodInfo BatchReadNameSlotMethodInfo = typeof(ChipProcessor).GetMethod(nameof(BatchReadNameSlot))!;
	internal static readonly MethodInfo BatchWriteNameMethodInfo = typeof(ChipProcessor).GetMethod(nameof(BatchWriteName))!;
	internal static readonly MethodInfo BatchWriteSlotMethodInfo = typeof(ChipProcessor).GetMethod(nameof(BatchWriteSlot))!;
	internal static readonly MethodInfo SleepMethodInfo = typeof(ChipProcessor).GetMethod(nameof(Sleep))!;
	internal static readonly ConstructorInfo ProgrammableChipExceptionConstructorInfo =
		typeof(ProgrammableChipException).GetConstructor(new[] { typeof(ProgrammableChipException.ICExceptionType), typeof(int) })!;
	internal static readonly MethodInfo BreakMethodInfo = typeof(ChipProcessor).GetMethod(nameof(Break))!;
	internal static readonly Random Random = new();
	internal static readonly MethodInfo MathSqrtMethodInfo = typeof(Math).GetMethod(nameof(Math.Sqrt), new[] { typeof(double) })!;
	internal static readonly MethodInfo MathRoundMethodInfo = typeof(Math).GetMethod(nameof(Math.Round), new[] { typeof(double) })!;
	internal static readonly MethodInfo MathTruncateMethodInfo = typeof(Math).GetMethod(nameof(Math.Truncate), new[] { typeof(double) })!;
	internal static readonly MethodInfo MathCeilingMethodInfo = typeof(Math).GetMethod(nameof(Math.Ceiling), new[] { typeof(double) })!;
	internal static readonly MethodInfo MathFloorMethodInfo = typeof(Math).GetMethod(nameof(Math.Floor), new[] { typeof(double) })!;
	internal static readonly MethodInfo MathMaxMethodInfo = typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(double), typeof(double) })!;
	internal static readonly MethodInfo MathMinMethodInfo = typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(double), typeof(double) })!;
	internal static readonly MethodInfo MathAbsMethodInfo = typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(double) })!;
	internal static readonly MethodInfo MathLogMethodInfo = typeof(Math).GetMethod(nameof(Math.Log), new[] { typeof(double) })!;
	internal static readonly MethodInfo MathExpMethodInfo = typeof(Math).GetMethod(nameof(Math.Exp), new[] { typeof(double) })!;
	internal static readonly MethodInfo MathSinMethodInfo = typeof(Math).GetMethod(nameof(Math.Sin), new[] { typeof(double) })!;
	internal static readonly MethodInfo MathASinMethodInfo = typeof(Math).GetMethod(nameof(Math.Asin), new[] { typeof(double) })!;
	internal static readonly MethodInfo MathTanMethodInfo = typeof(Math).GetMethod(nameof(Math.Tan), new[] { typeof(double) })!;
	internal static readonly MethodInfo MathAtanMethodInfo = typeof(Math).GetMethod(nameof(Math.Atan), new[] { typeof(double) })!;
	internal static readonly MethodInfo MathCosMethodInfo = typeof(Math).GetMethod(nameof(Math.Cos), new[] { typeof(double) })!;
	internal static readonly MethodInfo MathACosMethodInfo = typeof(Math).GetMethod(nameof(Math.Acos), new[] { typeof(double) })!;
	internal static readonly MethodInfo MathAtan2MethodInfo = typeof(Math).GetMethod(nameof(Math.Atan2), new[] { typeof(double), typeof(double) })!;
	internal static readonly MethodInfo DoubleIsNanMethodInfo = typeof(double).GetMethod(nameof(double.IsNaN))!;
	internal static readonly MethodInfo RandomNextDoubleMethodInfo = typeof(Random).GetMethod(nameof(Random.NextDouble))!;
	internal static readonly MethodInfo IMemoryWritableWriteMemoryMethodInfo = typeof(IMemoryWritable).GetMethod(nameof(IMemoryWritable.WriteMemory))!;
	internal static readonly MethodInfo IMemoryReadableReadMemoryMethodInfo = typeof(IMemoryReadable).GetMethod(nameof(IMemoryReadable.ReadMemory))!;
	internal static readonly MethodInfo AliasGetValueMethodInfo = typeof(Alias).GetMethod(nameof(Alias.GetValue))!;
	internal static readonly MethodInfo AliasSetValueMethodInfo = typeof(Alias).GetMethod(nameof(Alias.SetValue))!;
	internal static readonly MethodInfo AliasGetDeviceMethodInfo = typeof(Alias).GetMethod(nameof(Alias.GetDevice))!;
}
