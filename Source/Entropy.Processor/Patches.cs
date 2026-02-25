using Assets.Scripts;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Motherboards;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.Serialization;
using Assets.Scripts.UI;
using Entropy.Common.Utils;
using Entropy.Processor.Types;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace Entropy.Processor;

[HarmonyPatch]
public static class Patches
{
	extension(ProgrammableChip instance)
	{
		public ChipProcessor ChipProcessor
		{
			get => instance.Extensions.GetOrCreate(() => ChipProcessor.Translate(instance));
			set => instance.Extensions.Set(value);
		}
	}
	[HarmonyPatch(typeof(InputSourceCode), nameof(InputSourceCode.Initialize))]
	[HarmonyTranspiler]
	public static IEnumerable<CodeInstruction> InputSourceCodeInitializeTranspiler(IEnumerable<CodeInstruction> instructions)
	{
		ArgumentNullException.ThrowIfNull(instructions);
		ProcessorMod.Instance.Logger.LogDebug("Applying transpilation to Initialize method...");
		foreach (var instruction in instructions)
		{
			if (instruction.operand is 128) instruction.operand = 1024;
			yield return instruction;
		}
		ProcessorMod.Instance.Logger.LogDebug("Initialize method is updated.");
	}

	[HarmonyPatch(typeof(InputSourceCode), "RemoveLine")]
	[HarmonyTranspiler]
	public static IEnumerable<CodeInstruction> InputSourceCodeRemoveLineTranspiler(IEnumerable<CodeInstruction> instructions)
	{
		ProcessorMod.Instance.Logger.LogDebug("Applying transpilation to RemoveLine method...");
		foreach (var instruction in instructions)
		{
			if (instruction.operand is 128) instruction.operand = 1024;
			yield return instruction;
		}
		ProcessorMod.Instance.Logger.LogDebug("RemoveLine method is updated.");
	}

	[HarmonyPatch(typeof(InputSourceCode), "HandleInput")]
	[HarmonyTranspiler]
	public static IEnumerable<CodeInstruction> InputSourceCodeHandleInputTranspiler(IEnumerable<CodeInstruction> instructions)
	{
		ProcessorMod.Instance.Logger.LogDebug("Applying transpilation to HandleInput method...");
		// Here we need to look forward, so...
		CodeInstruction prevInstruction = null;
		var replace = true;
		foreach (var instruction in instructions)
		{
			// We need to be sure that the next instruction after ldc.i4.s is not a call, the static call (to GetKeyDown).
			// If it is, we ignore updating prevInstruction.
			replace = instruction.opcode != OpCodes.Call;
			if (replace && prevInstruction != null && prevInstruction.opcode == OpCodes.Ldc_I4_S && prevInstruction.operand is (sbyte) 127)
			{
				prevInstruction.opcode = OpCodes.Ldc_I4;
				prevInstruction.operand = 1023;
			}
			if (prevInstruction != null)
				yield return prevInstruction;
			prevInstruction = instruction;
		}
		yield return prevInstruction;
		ProcessorMod.Instance.Logger.LogDebug("HandleInput method is updated.");
	}

	[HarmonyPatch(typeof(InputSourceCode), "UpdateFileSize")]
	[HarmonyTranspiler]
	public static IEnumerable<CodeInstruction> InputSourceCodeUpdateFileSizeTranspiler(IEnumerable<CodeInstruction> instructions)
	{

		foreach (var instruction in instructions)
		{
			if (instruction.opcode == OpCodes.Ldc_I4 && instruction.operand is 4096) instruction.operand = 65536;
			yield return instruction;
		}
	}

	[HarmonyPatch(typeof(ProgrammableChip), nameof(ProgrammableChip.SetSourceCode), typeof(string))]
	[HarmonyPostfix]
	public static void ProgrammableChipSetSourceCodePostfix(ProgrammableChip __instance, string sourceCode)
	{
		if(__instance is null)
			return;
		__instance.ChipProcessor = ChipProcessor.Translate(__instance);
	}

#if PROFILE
	public static Stopwatch stopwatch = new Stopwatch();
	public static long elapsedTicksChipProcessor = 0;
	public static long elapsedTicksProgrammableChip = 0;
#endif
	public struct MyData { }
	[HarmonyPatch(typeof(ProgrammableChip), nameof(ProgrammableChip.Execute))]
	[HarmonyPrefix]
	public static bool ProgrammableChipExecutePrefix(ProgrammableChip __instance, int runCount)
	{
		if(__instance is null)
			return true;
		var processor = __instance.ChipProcessor;
#if !PROFILE
		processor.Execute(runCount);
		return false;
#else
		var traverse = Traverse.Create(__instance);
		var nextAddrTraverse = traverse.Field<int>("_NextAddr");
		var nextAddr = nextAddrTraverse.Value;
		var registers = new double[processor.Registers.Length];
		var stack = new double[processor.Stack.Length];
		processor.Registers.CopyTo(registers, 0);
		processor.Stack.CopyTo(stack, 0);
		stopwatch.Restart();
		processor.Execute(runCount);
		stopwatch.Stop();
		elapsedTicksChipProcessor = stopwatch.ElapsedTicks;
		//return false;
		// Restore the state
		nextAddrTraverse.Value = nextAddr;
		registers.CopyTo(processor.Registers, 0);
		stack.CopyTo(processor.Stack, 0);
		stopwatch.Restart();
		return true;
	}
	[HarmonyPatch(typeof(ProgrammableChip), nameof(ProgrammableChip.Execute))]
	[HarmonyPostfix]
	public static void ProgrammableChipExecutePostfix(ProgrammableChip __instance, int runCount)
	{
		stopwatch.Stop();
		elapsedTicksProgrammableChip = stopwatch.ElapsedTicks;
		Plugin.Log($"Elapsed ticks: ChipProcessor: {elapsedTicksChipProcessor}, ProgrammableChip: {elapsedTicksProgrammableChip}");
#endif
	}

	//[HarmonyPatch]
	public static class ProgrammableChipSOperationExecutePatch
	{
		public static MethodBase TargetMethod()
		{
			var privateClassType = typeof(ProgrammableChip).GetNestedType("_S_Operation", BindingFlags.NonPublic);
			if (privateClassType == null)
				throw new ApplicationException("Cannot find _S_Operation class!");
			var result = privateClassType.GetMethod("Execute", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (result == null)
				throw new ApplicationException("Cannot find _S_Operation.Execute method!");
			return result;
		}

		public static void Postfix(object __instance, int index, ProgrammableChip ____Chip, object ____DeviceIndex, object ____Argument1, object ____LogicType, ref int __result)
		{
			var aliasTargetType = typeof(ProgrammableChip).GetNestedType("_AliasTarget", BindingFlags.NonPublic);
			var aliasTargetDevice = (int) aliasTargetType.GetField("Device", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
			var aliasTargetRegister = (int) aliasTargetType.GetField("Register", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

			// Then, call the methods on the traversed objects
			var variableIndex = Traverse.Create(____DeviceIndex).Method("GetVariableIndex", aliasTargetDevice, true).GetValue<int>();
			var variableValue1 = Traverse.Create(____Argument1).Method("GetVariableValue", aliasTargetRegister).GetValue<double>();
			var variableValue2 = Traverse.Create(____LogicType).Method("GetVariableValue", aliasTargetRegister).GetValue<LogicType>();

			var networkIndex = Traverse.Create(____DeviceIndex).Method("GetNetworkIndex").GetValue<int>();
			var logicable = Traverse.Create(____Chip).Property("CircuitHousing").Method("GetLogicable", variableIndex, networkIndex).GetValue<ILogicable>();

#pragma warning disable CS0252 // Possible unintended reference comparison; left hand side needs cast
			if (logicable == ____Chip && variableValue2 == LogicType.On && variableValue1 == 0) __result = -index - 1;
#pragma warning restore CS0252 // Possible unintended reference comparison; left hand side needs cast
			//int variableIndex = ____DeviceIndex.GetVariableIndex(ProgrammableChip._AliasTarget.Device, true);
			//double variableValue1 = ____Argument1.GetVariableValue(ProgrammableChip._AliasTarget.Register);
			//LogicType variableValue2 = ____LogicType.GetVariableValue(ProgrammableChip._AliasTarget.Register);
			//int networkIndex = ____DeviceIndex.GetNetworkIndex();
			//ILogicable logicable = ____Chip.CircuitHousing.GetLogicable(variableIndex, networkIndex);
		}
	}
}