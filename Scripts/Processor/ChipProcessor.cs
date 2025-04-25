#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Assets.Scripts.Objects.Motherboards;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.Objects.Entities;
using HarmonyLib;
using Reagents;
using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects;
using Objects.Rockets;

namespace Entropy.Scripts.Processor
{
	[SuppressMessage("ReSharper", "ForCanBeConvertedToForeach", Justification = "To avoid allocations")]
	[SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery", Justification = "To avoid allocations")]
	public partial class ChipProcessor
	{
		public struct DebugArgument
		{
			public AliasTarget Target;
			public int Index;
		}
		public struct DebugSourceLine
		{
			public readonly string Source;
			public readonly int ArgumentsCount;
			public readonly DebugArgument[] Arguments;

			public DebugSourceLine(string source)
			{
				this.Source = source;
				this.ArgumentsCount = 0;
				this.Arguments = Array.Empty<DebugArgument>();
			}
			public DebugSourceLine(string source, DebugArgument[] arguments)
			{
				this.Source = source;
				this.ArgumentsCount = arguments.Length;
				this.Arguments = arguments;
			}

			public string ToString(ChipProcessor processor)
			{
				var arguments = new object?[this.ArgumentsCount];
				for(var i = 0; i < arguments.Length; i++)
					arguments[i] = FormatArgument(processor, this.Arguments[i]);
				return string.Format(this.Source, arguments);
			}
			private object? FormatArgument(ChipProcessor processor, DebugArgument argument)
			{
				if (argument.Target != AliasTarget.None)
					return argument.Target switch
					{
						AliasTarget.Register => processor.Registers[argument.Index],
						AliasTarget.Device => processor.Aliases[argument.Index].GetValue(processor, -1),
						_ => null,
					};
				return null;
			}
		}
#if DEBUG
		private static bool SaveToAssembly = true;
#endif
		private DebugSourceLine[] _sourceCode;

		public static readonly Dictionary<string, LineGenerator> InstructionCompilers = new();
		public static readonly Dictionary<string, VariableKind[]> InstructionArguments = new();

		public readonly Alias[] Aliases;
		/// <summary>
		/// Registers. 16'th is SP, 17'th is PC
		/// </summary>
		public readonly double[] Registers;
		/// <summary>
		/// Program Counter
		/// </summary>
		public int Pc;
		/// <summary>
		/// Stack memory
		/// </summary>
		public readonly double[] Stack;
		/// <summary>
		/// True when the processor is at a break state, i.e. not executing until commanded to.
		/// </summary>
		public bool IsAtBreakpoint;

		private readonly ProcessorCode? _code;
		private double _sleepDuration;
		private double _sleptAt;
		private CircuitHousing _circuitHousing;
		private bool _nextStep;
		private readonly Traverse<CircuitHousing> _circuitHousingField;
		private readonly Traverse<ushort> _errorLineNumber;
		private readonly Traverse<int> _nextAddr;
		private readonly Traverse<ProgrammableChipException.ICExceptionType> _errorType;

		public double SleepDuration
		{
			get => this._sleepDuration;
			private set => this._sleepDuration = value;
		}
		public double SleptAt
		{
			get => this._sleptAt;
			private set => this._sleptAt = value;
		}
		public ProgrammableChip Chip { get; private set; }

		public ChipProcessor(ProgrammableChip chip, ProcessorCode? code, Alias[] aliases, DebugSourceLine[] sourceCode)
		{
			Chip = chip;
			var traverse = Traverse.Create(chip);
			this.Stack = traverse.Field<double[]>("_Stack").Value;
			this.Registers = traverse.Field<double[]>("_Registers").Value;
			this.Aliases = aliases;
			this._circuitHousingField = traverse.Property<CircuitHousing>("CircuitHousing");
			this._circuitHousing = this._circuitHousingField.Value;
			this._errorLineNumber = traverse.Property<ushort>("CompileErrorLineNumber");
			this._errorType = traverse.Property<ProgrammableChipException.ICExceptionType>("CompileErrorType");
			this._nextAddr = traverse.Field<int>("_NextAddr");
			this._code = code;
			this._sourceCode = sourceCode;
		}

		public static void DefineInstructionHandler(string instruction, VariableKind[] variables, LineGenerator generator)
		{
			if (InstructionCompilers.ContainsKey(instruction))
				throw new ArgumentException($"Instruction `{instruction}' already has a handler defined.");
			InstructionArguments.Add(instruction, variables);
			InstructionCompilers.Add(instruction, generator);
		}

		public static IEnumerable<string> GetInstructions() =>InstructionCompilers.Keys;

		public static IEnumerable<string> GetEnumerationValues()
		{
			return GetEnumerationSimpleValuesInternal<LogicType>()
				.Union(GetEnumerationSimpleValuesInternal<LogicSlotType>())
				.Union(GetEnumerationSimpleValuesInternal<LogicReagentMode>())
				.Union(GetEnumerationSimpleValuesInternal<LogicBatchMethod>())
				//.Union(GetEnumerationTypedValuesInternal<LogicType>())
				//.Union(GetEnumerationTypedValuesInternal<LogicSlotType>())
				//.Union(GetEnumerationTypedValuesInternal<SoundAlert>("Sound"))
				//.Union(GetEnumerationTypedValuesInternal<LogicTransmitterMode>("TransmitterMode"))
				//.Union(GetEnumerationTypedValuesInternal<ElevatorMode>("ElevatorMode"))
				//.Union(GetEnumerationTypedValuesInternal<ColorType>("Color"))
				//.Union(GetEnumerationTypedValuesInternal<EntityState>("EntityState"))
				//.Union(GetEnumerationTypedValuesInternal<AirControlMode>("AirControl"))
				//.Union(GetEnumerationTypedValuesInternal<DaylightSensor.DaylightSensorMode>("DaylightSensorMode"))
				//.Union(GetEnumerationTypedValuesInternal<ConditionOperation>())
				//.Union(GetEnumerationTypedValuesInternal<AirConditioningMode>("AirCon"))
				//.Union(GetEnumerationTypedValuesInternal<VentDirection>("Vent"))
				//.Union(GetEnumerationTypedValuesInternal<PowerMode>("PowerMode"))
				//.Union(GetEnumerationTypedValuesInternal<RobotMode>("RobotMode"))
				//.Union(GetEnumerationTypedValuesInternal<SortingClass>("SortingClass"))
				//.Union(GetEnumerationTypedValuesInternal<Slot.Class>("SlotClass"))
				//.Union(GetEnumerationTypedValuesInternal<Chemistry.GasType>("GasType"))
				//.Union(GetEnumerationTypedValuesInternal<RocketMode>())
				//.Union(GetEnumerationTypedValuesInternal<ReEntryProfile>())
				//.Union(GetEnumerationTypedValuesInternal<SorterInstruction>())
				//.Union(GetEnumerationTypedValuesInternal<PrinterInstruction>())
				;
		}
		private static IEnumerable<string> GetEnumerationTypedValuesInternal<T>(string? typename = null)
		{
			foreach(var value in Enum.GetValues(typeof(T)))
				yield return (typename ?? typeof(T).Name) + @"\." + value;
		}
		private static IEnumerable<string> GetEnumerationSimpleValuesInternal<T>()
		{
			foreach(var value in Enum.GetValues(typeof(T)))
				yield return value.ToString();
		}

		public virtual void Execute(int runCount)
		{
			if (SleepDuration > 0)
			{
				// This is a hack for serialization, because during deserialization the GameTime is incorrect and we can't rely on it.
				if (SleptAt < 0)
				{
					// SleptAt after deserialization would be set as the negative time spent asleep instead, so restore the correct SleptAt value.
					// Trying to calculate SleptAt based on current GameTime might result in negative value, so we adjust SleepDuration instead.
					// Also, we need to account for the last tick time.
					SleepDuration += SleptAt - GameManager.LastTickTimeSeconds;
					SleptAt = GameManager.GameTime;
				}
				if (GameManager.GameTime - SleptAt < SleepDuration)
					return;
				SleepDuration = 0;
				SleptAt = 0;
			}
			if (this._code != null && (!this.IsAtBreakpoint || this._nextStep))
			{
				if (this.IsAtBreakpoint && this._nextStep)
				{
					this._nextStep = false;
					runCount = 1;
				}
				this._circuitHousing = this._circuitHousingField.Value;
				try
				{
					this.Pc = this._code(this.Pc, this, this.Registers, this.Aliases, runCount);
					this._nextAddr.Value = this.Pc;
				}
				catch (ProgrammableChipException e)
				{
					if (this._circuitHousing != null)
					{
						this._circuitHousing.RaiseError(1);
						this._errorLineNumber.Value = e.LineNumber;
						this._errorType.Value = e.ExceptionType;
					}
					this.Pc = e.LineNumber;
					this._nextAddr.Value = e.LineNumber;
					return;
				}
				catch
				{
					if (this._circuitHousing != null)
					{
						this._circuitHousing.RaiseError(1);
						this._errorLineNumber.Value = ushort.MaxValue;
						this._errorType.Value = ProgrammableChipException.ICExceptionType.Unknown;
					}
					this.Pc = ushort.MaxValue;
					this._nextAddr.Value = this.Pc;
					return;
				}
				if (this._circuitHousing != null)
				{
					this._circuitHousing.RaiseError(0);
					this._errorLineNumber.Value = 0;
					this._errorType.Value = ProgrammableChipException.ICExceptionType.None;
				}
				this._nextAddr.Value = this.Pc;
			}
		}

		public string GetSourceLine(int lineNum)
		{
			if (lineNum < 0 || lineNum >= this._sourceCode.Length)
				return string.Empty;
			return this._sourceCode[lineNum].ToString(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ILogicable? GetDevice(int index, int networkIndex, bool throwError, int lineNum)
		{
			if (index == int.MaxValue)
				index = int.MaxValue;
			var logicable = this._circuitHousing.GetLogicableFromIndex(index, networkIndex);
			if (logicable == null && throwError)
				throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.DeviceNotFound, lineNum);
			return logicable;
		}

		public ILogicable? GetDeviceById(int id, bool throwError, int lineNum)
		{
			var logicable = this._circuitHousing.GetLogicableFromId(id);
			if (logicable == null && throwError)
				throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.DeviceNotFound, lineNum);
			return logicable;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double ReadDevice(ILogicable logicable, LogicType logicType, int lineNum)
		{
			return logicable.CanLogicRead(logicType)
				? logicable.GetLogicValue(logicType)
				: throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectLogicType, lineNum);
		}

		public double ReadDeviceBatch(int deviceHash, LogicType logicType, LogicBatchMethod method, int lineNum)
		{
			var devices = this._circuitHousing.GetBatchOutput() ?? throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.DeviceListNull, lineNum);
			for (int i = 0; i < devices.Count; i++)
			{
				var device = devices[i];
				if (device == null || device.GetPrefabHash() != deviceHash) continue;
				if(!device.CanLogicRead(logicType))
					throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectLogicType, lineNum);
			}
			return Device.BatchRead(method, logicType, deviceHash, devices);
		}

		public double BatchReadSlot(int deviceHash, int slotIndex, LogicSlotType logicSlotType, LogicBatchMethod batchMode, int lineNum)
		{
			var devices = this._circuitHousing.GetBatchOutput() ?? throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.DeviceListNull, lineNum);
			for(int i = 0; i < devices.Count; i++)
			{
				var device = devices[i];
				if (device == null || device.GetPrefabHash() != deviceHash) continue;
				if(!device.CanLogicRead(logicSlotType, slotIndex))
					throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectLogicType, lineNum);
			}
			return Device.BatchRead(batchMode, logicSlotType, slotIndex, deviceHash, devices);
		}

		public double BatchReadName(int deviceHash, int nameHash, LogicType logicType, LogicBatchMethod batchMode, int lineNum)
		{
			var devices = this._circuitHousing.GetBatchOutput() ?? throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.DeviceListNull, lineNum);
			for (int i = 0; i < devices.Count; i++)
			{
				var device = devices[i];
				if (device == null || device.GetPrefabHash() != deviceHash || device.GetNameHash() != nameHash) continue;
				if(!device.CanLogicRead(logicType))
					throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectLogicType, lineNum);
			}
			return Device.BatchRead(batchMode, logicType, deviceHash, nameHash, devices);
		}

		public double BatchReadNameSlot(int deviceHash, int nameHash, int slotIndex, LogicSlotType logicSlotType, LogicBatchMethod batchMode, int lineNum)
		{
			var devices = this._circuitHousing.GetBatchOutput() ?? throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.DeviceListNull, lineNum);
			for (int i = 0; i < devices.Count; i++)
			{
				var device = devices[i];
				if (device == null || device.GetPrefabHash() != deviceHash || device.GetNameHash() != nameHash) continue;
				if(!device.CanLogicRead(logicSlotType, slotIndex))
					throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectLogicType, lineNum);
			}
			return Device.BatchRead(batchMode, logicSlotType, slotIndex, nameHash, deviceHash, devices);
		}

		public void BatchWriteName(int deviceHash, int nameHash, LogicType logicType, double value, int lineNum)
		{
			var devices = this._circuitHousing.GetBatchOutput() ?? throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.DeviceListNull, lineNum);
			for (int i = 0; i < devices.Count; i++)
			{
				var device = devices[i];
				if (device == null || device.GetPrefabHash() != deviceHash || device.GetNameHash() != nameHash) continue;
				if(!device.CanLogicWrite(logicType))
					throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectLogicType, lineNum);
			}
			devices.ForEach(x => x.SetLogicValue(logicType, value));
		}

		public void BatchWriteSlot(int deviceHash, int slotIndex, LogicSlotType logicSlotType, double value, int lineNum)
		{
			var devices = this._circuitHousing.GetBatchOutput() ?? throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.DeviceListNull, lineNum);
			for (int i = 0; i < devices.Count; i++)
			{
				var device = devices[i];
				if (device == null || device.GetPrefabHash() != deviceHash) continue;
				if(device is ISlotWriteable slotWriteable && !slotWriteable.CanLogicWrite(logicSlotType, slotIndex))
					throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectLogicType, lineNum);
			}
			devices.ForEach(x => ((ISlotWriteable)x).SetLogicValue(logicSlotType, slotIndex, value));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double ReadDeviceSlot(ILogicable logicable, LogicSlotType slotType, int slotId, int lineNum)
		{
			return logicable.CanLogicRead(slotType, slotId)
				? logicable.GetLogicValue(slotType, slotId)
				: throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectLogicType, lineNum);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteDeviceSlot(ILogicable logicable, LogicSlotType slotType, int slotId, double value, int lineNum)
		{
			if (logicable is ISlotWriteable slotWriteable && slotWriteable.CanLogicWrite(slotType, slotId))
				slotWriteable.SetLogicValue(slotType, slotId, value);
			else
				throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectLogicType, lineNum);
		}

		public double ReadDeviceReagent(ILogicable logicable, LogicReagentMode mode, int reagent, int lineNum)
		{
			switch (mode)
			{
				case LogicReagentMode.Contents:
					if (logicable is Device device)
						return device.ReagentMixture.Get(Reagent.Find(reagent));
					break;
				case LogicReagentMode.Required:
					if (logicable is IRequireReagent requireReagent)
						return requireReagent.RequiredReagents.Get(Reagent.Find(reagent));
					break;
				case LogicReagentMode.Recipe:
					if (logicable is IRequireReagent recipeReagent)
						return recipeReagent.CurrentRecipe.Get(Reagent.Find(reagent));
					break;
				case LogicReagentMode.TotalContents:
					if (logicable is Device totalContentsDevice)
						return totalContentsDevice.ReagentMixture.TotalReagents;
					break;
			}
			throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.UnhandledReagentMode, lineNum);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteDevice(ILogicable logicable, LogicType logicType, double value, int lineNum)
		{
			if (logicable.CanLogicWrite(logicType))
				logicable.SetLogicValue(logicType, value);
			else
				throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectLogicType, lineNum);
		}

		public void WriteDeviceBatch(int deviceHash, LogicType logicType, double value, int lineNum)
		{
			var devices = this._circuitHousing.GetBatchOutput() ?? throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.DeviceListNull, lineNum);
			for(int i = 0; i < devices.Count; i++)
			{
				var device = devices[i];
				if (device == null || device.GetPrefabHash() != deviceHash) continue;
				if(!device.CanLogicWrite(logicType))
					throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectLogicType, lineNum);
				device.SetLogicValue(logicType, value);
			}
		}

		public void Sleep(double durationSeconds)
		{
			SleepDuration = durationSeconds;
			SleptAt = GameManager.GameTime;
		}

		public void Break(string sourceLine, int lineNum)
		{
			this.IsAtBreakpoint = true;
		}

		public void DoNextStep() =>
			this._nextStep = true;

		public static ChipProcessor Translate(ProgrammableChip chip)
		{
			try
			{
				var lines = Parser.Parse(chip.SourceCode, out var defines, out var labels, out var aliases, out var sourceLines);
				var count = lines.Length;
				if (count > 0)
				{
					var aliasesArray = new Alias[aliases.Count];
					// Then generate the code.
					var code = Compile(chip, lines, aliases);
					var traverse = Traverse.Create(chip);
					traverse.Property<ushort>("CompileErrorLineNumber").Value = 0;
					traverse.Property<ProgrammableChipException.ICExceptionType>("CompileErrorType").Value = ProgrammableChipException.ICExceptionType.None;
					return new ChipProcessor(chip, code, aliasesArray, sourceLines.ToArray());
				}
			}
			catch(ProgrammableChipException e)
			{
#if DEBUG && !SEGI
				Plugin.LogWarning("Error while compiling the script: \n" + e);
#endif
				return new InvalidProcessor(chip, e);
			}
			return new InvalidProcessor(chip, new ProgrammableChipException(ProgrammableChipException.ICExceptionType.None, 0));
		}

		private static ProcessorCode Compile(
			ProgrammableChip chip,
			Span<LineOfCode> lines,
			List<string> aliases)
		{
			var lineParamName = "line";
			var processorParamName = "processor";
			var registersParamName = "registers";
			var aliasesParamName = "aliases";
			var yieldCounterName = "yield";
#if DEBUG
			var sourceLineName = "source";
#endif

			// Declare parameters:
			var lineParam = Expression.Parameter(typeof(int), lineParamName);
			var processorParam = Expression.Parameter(typeof(ChipProcessor), processorParamName);
			var registersParam = Expression.Parameter(typeof(double[]), registersParamName);
			var aliasesParam = Expression.Parameter(typeof(Alias[]), aliasesParamName);
			// This variable would be used as a counter to force execution yield.
			var yieldParam = Expression.Variable(typeof(int), yieldCounterName);
			var parameters = new[] { lineParam, processorParam, registersParam, aliasesParam, yieldParam };

			// Declare variables:
			var variables = new Dictionary<string, ParameterExpression>();
			var aliasesIndexed = new Dictionary<int, Expression>(aliases.Count); // we add 3 more aliases for "db", "sp" and "ra"
			for (var i = 0; i < aliases.Count; i++)
				aliasesIndexed.Add(i, Expression.ArrayAccess(aliasesParam, Expression.Constant(i)));
			variables.Add(lineParamName, lineParam);
			variables.Add(processorParamName, processorParam);
			variables.Add(registersParamName, registersParam);
			variables.Add(aliasesParamName, aliasesParam);
			variables.Add(yieldCounterName, yieldParam);
#if DEBUG
			var sourceLine = Expression.Variable(typeof(string), sourceLineName);
			// This variable would be used to store the source line for debugging purposes.
			variables.Add(sourceLineName, sourceLine);
#endif

			// Declare line labels for each line
			var labels = new LabelTarget[lines.Length];
			for (var i = 0; i < lines.Length; i++)
				labels[i] = Expression.Label("Line_" + i);
			var endLabel = Expression.Label("End");
			var bodyExpressions = new List<Expression>();
			// Initialize alias values
			// The resulting code should look like this:
			// alias_x = aliases[idx];
			//aliases.ForEach(x =>
			//    bodyExpressions.Add(
			//        Expression.Assign(variables[x], Expression.ArrayIndex(aliasesParam, Expression.Constant(aliases.IndexOf(x))))));


			// Declare control block switch (used to transition between lines of code)
			var controlSwitchLabel = Expression.Label("ControlSwitch");
			bodyExpressions.Add(Expression.Label(controlSwitchLabel)); // Place the label before the control switch
			bodyExpressions.Add(Expression.Switch( // Define the control switch
					lineParam, // switch based on value of 'line' variable
					Expression.Goto(endLabel), // default case: jump to the end of code
					labels.Select((x, i) => Expression.SwitchCase(Expression.Goto(x), Expression.Constant(i))).ToArray()) // jumps to lines of code
			);

			// Write the lines of code, preceded by label.
			// The resulting base variant of the code should look like this:
			// Line_x:
			// // We need to check if we need to yield the execution, either when yield counter reaches 0 or when we reach the end of the code.
			// if(yield-- || line >= source_lines)
			// {
			//     debug = string.Format(
			//         // The string comes from the line.Source.
			//         "instr r1{0} alias{1} d0 Literal",
			//         // registers are passed as value
			//         $"({registers[1]})",
			//         // alias displayed only when it's a register
			//         $aliases[i].Target == AliasTarget.Register
			//             ? $"({aliases[i].GetValue()})"
			//             : "");
			//     goto Return;
			// }
			// (the effective code of the line)
			var index = 0;
			var data = new CodeGeneratorData(bodyExpressions, variables, aliasesIndexed, controlSwitchLabel, labels, endLabel);

			foreach (var line in lines)
			{
				// Place the line label
				bodyExpressions.Add(Expression.Label(labels[index]));
				// Instead of incrementing the line value to track currently executing line and return it on yield, we just assign it for each line.
				bodyExpressions.Add(Expression.Assign(lineParam, Expression.Constant(index)));
#if DEBUG
				bodyExpressions.Add(Expression.Assign(sourceLine, Expression.Constant(line.Source)));
#endif

				// Place the yield check
				bodyExpressions.Add(
					Expression.IfThen(
						Expression.LessThan(
							Expression.PreDecrementAssign(yieldParam), Expression.Constant(0)),
						Expression.Block(Expression.Goto(endLabel))));
				if (line.Command is not null && !line.Command.Equals("nop", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(line.Command))
					GenerateLineCode(line, data, index);
				index++;
			}

			bodyExpressions.Add(Expression.Label(endLabel));
			var returnLabel = Expression.Label(typeof(int));
			bodyExpressions.Add(Expression.Label(returnLabel, lineParam));
			//bodyExpressions.Add(Expression.Label(returnLabel, Expression.Constant(lines.Length-1)));
			var body = Expression.Block(
				new[]
				{
#if DEBUG
					sourceLine,
#endif
				}, bodyExpressions);
			var lambda = Expression.Lambda<ProcessorCode>(body, "IC_" + chip.ReferenceId, parameters);
			var result = lambda.Compile();
#if DEBUG
			if (SaveToAssembly)
			{
				SaveExpressionTreeToAssembly(lambda, "IC_" + chip.ReferenceId);
				//SaveToAssembly = false;
			}
#endif

			return result;
		}
#if DEBUG
		private static void SaveExpressionTreeToAssembly(LambdaExpression lambda, string chipName)
		{
			var dynamicAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(chipName), AssemblyBuilderAccess.Save);
			var dynamicModule = dynamicAssembly.DefineDynamicModule(chipName, chipName + ".dll");
			var dynamicType = dynamicModule.DefineType(chipName, TypeAttributes.Public | TypeAttributes.Class);
			lambda.CompileToMethod(dynamicType.DefineMethod("Execute", MethodAttributes.Public | MethodAttributes.Static));
			dynamicType.CreateType();
			dynamicAssembly.Save(chipName + ".dll");
		}
#endif

		private static void GenerateLineCode(LineOfCode line, CodeGeneratorData data, int lineNumber)
		{
			if (!InstructionCompilers.TryGetValue(line.Command, out var generator)
				|| !InstructionArguments.TryGetValue(line.Command, out var arguments))
				throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.UnrecognisedInstruction, lineNumber);

			bool isCorrectNumberOfArguments = arguments.Length switch
			{
				0 => line.Argument1.Kind == VariableKind.None,
				1 => line.Argument2.Kind == VariableKind.None,
				2 => line.Argument3.Kind == VariableKind.None,
				3 => line.Argument4.Kind == VariableKind.None,
				4 => line.Argument5.Kind == VariableKind.None,
				5 => line.Argument6.Kind == VariableKind.None,
				6 => true,
				_ => throw new ApplicationException()
			};
			if(!isCorrectNumberOfArguments) throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectArgumentCount, lineNumber);
			bool isValid = arguments.Length switch
			{
				1 => CheckArgument(line.Argument1.Kind, arguments[0]),
				2 => CheckArgument(line.Argument1.Kind, arguments[0])
				  && CheckArgument(line.Argument2.Kind, arguments[1]),
				3 => CheckArgument(line.Argument1.Kind, arguments[0])
				  && CheckArgument(line.Argument2.Kind, arguments[1])
				  && CheckArgument(line.Argument3.Kind, arguments[2]),
				4 => CheckArgument(line.Argument1.Kind, arguments[0])
				  && CheckArgument(line.Argument2.Kind, arguments[1])
				  && CheckArgument(line.Argument3.Kind, arguments[2])
				  && CheckArgument(line.Argument4.Kind, arguments[3]),
				5 => CheckArgument(line.Argument1.Kind, arguments[0])
				  && CheckArgument(line.Argument2.Kind, arguments[1])
				  && CheckArgument(line.Argument3.Kind, arguments[2])
				  && CheckArgument(line.Argument4.Kind, arguments[3])
				  && CheckArgument(line.Argument5.Kind, arguments[4]),
				6 => CheckArgument(line.Argument1.Kind, arguments[0])
				  && CheckArgument(line.Argument2.Kind, arguments[1])
				  && CheckArgument(line.Argument3.Kind, arguments[2])
				  && CheckArgument(line.Argument4.Kind, arguments[3])
				  && CheckArgument(line.Argument5.Kind, arguments[4])
				  && CheckArgument(line.Argument6.Kind, arguments[5]),
				_ => line.Argument1.Kind == VariableKind.None,
			};
			if (!isValid)
				throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectVariableType, lineNumber);
			generator(line, data, lineNumber);
		}

		private static bool CheckArgument(VariableKind kind, VariableKind expected) => kind != VariableKind.None && expected.HasFlag(kind);
	}

	public class InvalidProcessor : ChipProcessor
	{
		private readonly Traverse<ushort> _errorLineNumber;
		private readonly Traverse<ProgrammableChipException.ICExceptionType> _errorType;
		private readonly ProgrammableChipException? _exception;
		public InvalidProcessor(ProgrammableChip chip)
			: base(chip, null, Array.Empty<Alias>(), Array.Empty<DebugSourceLine>())
		{
			var traverse = Traverse.Create(chip);
			this._errorLineNumber = traverse.Property<ushort>("_ErrorLineNumber");
			this._errorType = traverse.Property<ProgrammableChipException.ICExceptionType>("_ErrorType");
		}
		public InvalidProcessor(ProgrammableChip chip, ProgrammableChipException exception)
			: this(chip) => this._exception = exception;

		public override void Execute(int runCount)
		{
			if(this._exception is not null)
			{
				this._errorLineNumber.Value = this._exception.LineNumber;
				this._errorType.Value = this._exception.ExceptionType;
			}
		}
	}
}
