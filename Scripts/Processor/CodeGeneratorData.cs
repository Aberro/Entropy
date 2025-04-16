﻿#nullable enable
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Motherboards;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Entropy.Scripts.Processor
{
	public partial class ChipProcessor
	{
		private static readonly MethodInfo GetDeviceMethodInfo = typeof(ChipProcessor).GetMethod(nameof(GetDevice))!;
		private static readonly MethodInfo GetDeviceByIdMethodInfo = typeof(ChipProcessor).GetMethod(nameof(GetDeviceById))!;
		private static readonly MethodInfo ReadDeviceMethodInfo = typeof(ChipProcessor).GetMethod(nameof(ReadDevice))!;
		private static readonly MethodInfo ReadDeviceSlotMethodInfo = typeof(ChipProcessor).GetMethod(nameof(ReadDeviceSlot))!;
		private static readonly MethodInfo WriteDeviceSlotMethodInfo = typeof(ChipProcessor).GetMethod(nameof(WriteDeviceSlot))!;
		private static readonly MethodInfo ReadDeviceReagentMethodInfo = typeof(ChipProcessor).GetMethod(nameof(ReadDeviceReagent))!;
		private static readonly MethodInfo WriteDeviceMethodInfo = typeof(ChipProcessor).GetMethod(nameof(WriteDevice))!;
		private static readonly MethodInfo WriteDeviceBatchMethodInfo = typeof(ChipProcessor).GetMethod(nameof(WriteDeviceBatch))!;
		private static readonly MethodInfo ReadDeviceBatchMethodInfo = typeof(ChipProcessor).GetMethod(nameof(ReadDeviceBatch))!;
		private static readonly MethodInfo BatchReadSlotMethodInfo = typeof(ChipProcessor).GetMethod(nameof(BatchReadSlot))!;
		private static readonly MethodInfo BatchReadNameMethodInfo = typeof(ChipProcessor).GetMethod(nameof(BatchReadName))!;
		private static readonly MethodInfo BatchReadNameSlotMethodInfo = typeof(ChipProcessor).GetMethod(nameof(BatchReadNameSlot))!;
		private static readonly MethodInfo BatchWriteNameMethodInfo = typeof(ChipProcessor).GetMethod(nameof(BatchWriteName))!;
		private static readonly MethodInfo BatchWriteSlotMethodInfo = typeof(ChipProcessor).GetMethod(nameof(BatchWriteSlot))!;
		private static readonly MethodInfo SleepMethodInfo = typeof(ChipProcessor).GetMethod(nameof(Sleep))!;
		private static readonly ConstructorInfo ProgrammableChipExceptionConstructorInfo =
			typeof(ProgrammableChipException).GetConstructor(new[] { typeof(ProgrammableChipException.ICExceptionType), typeof(int) })!;
		private static readonly MethodInfo BreakMethodInfo = typeof(ChipProcessor).GetMethod(nameof(Break))!;

		public struct CodeGeneratorData
		{
			private readonly List<Expression> _body;
			private readonly Dictionary<string, ParameterExpression> _variables;
			private readonly Dictionary<int, Expression> _aliases;
			private readonly LabelTarget _controlSwitchLabel;
			private readonly LabelTarget[] _lineLabels;
			private readonly LabelTarget _endLabel;
			public CodeGeneratorData(
				List<Expression> body,
				Dictionary<string, ParameterExpression> variables,
				Dictionary<int, Expression> aliases,
				LabelTarget controlSwitchLabel,
				LabelTarget[] lineLabels,
				LabelTarget endLabel)
			{
				this._body = body;
				this._variables = variables;
				this._aliases = aliases;
				this._controlSwitchLabel = controlSwitchLabel;
				this._lineLabels = lineLabels;
				this._endLabel = endLabel;
			}

			public void Add(Expression expression)
			{
				this._body.Add(expression);
			}

			public LabelTarget ControlSwitchLabel() => this._controlSwitchLabel;
			public LabelTarget EndLabel() => this._endLabel;
			public LabelTarget LineLabel(int index) => index >= this._lineLabels.Length ? this._lineLabels[^1] : this._lineLabels[index];
			public Expression StackPointer() => Expression.ArrayAccess(this._variables["registers"], Expression.Constant(16));
			public ParameterExpression LineVariable() => this._variables["line"];
			public Expression Stack(Expression pointer) => Expression.ArrayAccess(Expression.Field(this._variables["processor"], nameof(ChipProcessor.Stack)), Expression.Convert(pointer, typeof(int)));

			public Expression StackSize() => Expression.Property(Expression.Field(this._variables["processor"], nameof(ChipProcessor.Stack)), nameof(Array.Length));
			public readonly Expression GetAlias(int index) => this._aliases[index]; // we add 3 to account for db alias.

			/// <summary>
			/// Returns an expression that traverses register references and results in final index. If registerRecurse is 0, the result is registerIndex.
			/// </summary>
			public Expression TraverseReferences(int registerIndex, int registerRecurse)
			{
				var registers = this._variables["registers"];
				var index = (Expression)Expression.Constant(registerIndex);
				while (registerRecurse-- > 0) index = Expression.Convert(Expression.ArrayAccess(registers, index), typeof(int));
				return index;
			}

			/// <summary>
			/// Returns an expression that accesses a target register for reading.
			/// </summary>
			public Expression GetRegisterOrAlias(Variable variable, int lineNum) => variable.Kind switch
			{
				// registers[registers[index]]
				VariableKind.Register => Expression.ArrayAccess(this._variables["registers"], TraverseReferences(variable.RegisterIndex, variable.RegisterRecurse)),

				// alias_x.GetValue()
				VariableKind.Alias => Expression.Call(GetAlias(variable.AliasIndex), AliasGetValueMethodInfo, this._variables["processor"], Expression.Constant(lineNum)),
				_ => throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectVariableType, lineNum)
			};

			/// <summary>
			/// Returns an expression that accesses a target register for writing.
			/// </summary>
			public Expression RegisterSet(Variable variable, int lineNum) => variable.Kind switch
			{
				// registers[registers[index]]
				VariableKind.Register => Expression.ArrayAccess(this._variables["registers"], TraverseReferences(variable.RegisterIndex, variable.RegisterRecurse)),

				_ => throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectVariableType, lineNum)
			};

			/// <summary>
			/// Returns an expression that returns an enumeration value (either directly or via a register or an alias).
			/// </summary>
			public Expression Enum<T>(Variable variable, int lineNum) where T : struct, Enum =>
				variable.Kind switch
				{
					// LogicType.value
					VariableKind.Constant => Expression.Convert(Expression.Constant((int)variable.Constant), typeof(T)),
					// (LogicType)((int)registers[registers[index]])
					(VariableKind.Register or VariableKind.Alias) =>
						Expression.Convert(
							Expression.Convert(
								GetRegisterOrAlias(variable, lineNum),
								typeof(int)),
							typeof(T)),
					VariableKind.Literal => System.Enum.TryParse(variable.Literal, out T result) ? Expression.Constant(result) : throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectVariable, lineNum),
					_ => throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectVariableType, lineNum)
				};

			public Expression Enum(Variable variable, int lineNum, bool asInteger)
			{
				double value = 0;
				bool isValueSet = false;
				foreach (var ienum in ProgrammableChip.InternalEnums)
				{
					ienum.Execute(ref isValueSet, ref value, variable.Literal, InstructionInclude.Enum);
					if (isValueSet)
						break;
				}
				if (isValueSet)
					return asInteger ? Expression.Constant((int)value) : Expression.Constant(value);
				throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectVariable, lineNum);
			}

			public Expression DoubleValue(Variable variable, int lineNum) => variable.Kind switch
			{
				VariableKind.Constant => Expression.Constant(variable.Constant),
				(VariableKind.Register or
				VariableKind.Alias) => GetRegisterOrAlias(variable, lineNum),
				VariableKind.Literal => Enum(variable, lineNum, false),
				_ => throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectVariableType, lineNum),
			};

			public Expression LongValue(Variable variable, int lineNum) => LongValue(variable, true, lineNum);
			public Expression LongValue(Variable variable, bool signed, int lineNum)
			{
				switch (variable.Kind)
				{
					case VariableKind.Constant:
						// In case of defined value, we convert it to long value during the compilation.
						var value = variable.Constant;
						return value < long.MinValue
							? throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.ShiftUnderflow, lineNum)
							: value > long.MaxValue
								? throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.ShiftOverflow, lineNum)
								: Expression.Constant(DoubleToLong(value));
					case VariableKind.Alias:
					case VariableKind.Register:
						// And in other cases, we need to do the same in runtime.
						var valueExpr = GetRegisterOrAlias(variable, lineNum);

						// value < long.MinValue
						return Expression.Condition(Expression.LessThan(valueExpr, Expression.Constant(long.MinValue)),
							// ? throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.ShiftUnderflow, lineNum)
							Expression.Throw(Expression.New(ProgrammableChipExceptionConstructorInfo,
								Expression.Constant(ProgrammableChipException.ICExceptionType.ShiftUnderflow), Expression.Constant(lineNum))),
							// : value > long.MaxValue
							Expression.Condition(Expression.GreaterThan(valueExpr, Expression.Constant(long.MaxValue)),
								// ? throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.ShiftOverflow, lineNum)
								Expression.Throw(Expression.New(ProgrammableChipExceptionConstructorInfo,
									Expression.Constant(ProgrammableChipException.ICExceptionType.ShiftOverflow), Expression.Constant(lineNum))),
								// : (long)(d % 9.00719925474099E+15)
								DoubleToLongExpression(valueExpr)));
					default:
						throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectVariableType, lineNum);
				}
			}

			public Expression IntValue(Variable variable, int lineNum) => variable.Kind switch
			{
				VariableKind.Constant => Expression.Constant((int)variable.Constant),
				(VariableKind.Alias or
				VariableKind.Register) => Expression.Convert(GetRegisterOrAlias(variable, lineNum), typeof(int)),
				VariableKind.Literal => Enum(variable, lineNum, true),
				_ => throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectVariableType, lineNum),
			};

			public Expression LongToDoubleExpression(Expression l, int lineNum) =>
				Expression.Convert(
					Expression.Condition(
						Expression.NotEqual(
							Expression.And(l, Expression.Constant(0x20000000000000)),
							Expression.Constant(0)),
						Expression.Or(Expression.And(l, Expression.Constant(0x1FFFFFFFFFFFFF)), Expression.Constant(-0x20000000000000)),
						Expression.And(l, Expression.Constant(0x1FFFFFFFFFFFFF))),
					typeof(double));

			public static double LongToDouble(long l) =>
				((l & 0x20000000000000) != 0 ? (l & 0x1FFFFFFFFFFFFF) | -0x20000000000000 /* FFE0 0000 0000 0000 */ : l & 0x1FFFFFFFFFFFFF);

			public Expression DoubleToLongExpression(Expression d, bool signed = true) =>
				signed
					? Expression.Convert(Expression.Modulo(d, Expression.Constant(9.00719925474099E+15)), typeof(long))
					: Expression.And(
						Expression.Convert(Expression.Modulo(d, Expression.Constant(9.00719925474099E+15)), typeof(long)),
						Expression.Constant(0x3FFFFFFFFFFFFF));
			public long DoubleToLong(double d, bool signed = true) =>
				(long)(d % 9.00719925474099E+15) & (signed ? -1 : 0x3FFFFFFFFFFFFF);

			/// <summary>
			/// Returns an expression that assigns given value expression to a target register (either directly or via an alias).
			/// </summary>
			public Expression Assign(Variable variable, Expression value, int lineNum)
			{
				return variable.Kind switch
				{
					VariableKind.Register => Expression.Assign(RegisterSet(variable, lineNum), value),
					VariableKind.Alias => Expression.Call(GetAlias(variable.AliasIndex), AliasSetValueMethodInfo, this._variables["processor"], value, Expression.Constant(lineNum)),
					_ => throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectVariableType, lineNum)
				};
			}

			/// <summary>
			/// Returns an expressions that returns ILogicable of a device.
			/// </summary>
			public Expression Device(Variable variable, bool throwError, int lineNum) => variable.Kind switch
			{
				// devices[registers[index]]
				VariableKind.DeviceRegister => Expression.Call(this._variables["processor"],
					GetDeviceMethodInfo,
					TraverseReferences(variable.RegisterIndex, variable.RegisterRecurse),
					Expression.Constant(variable.NetworkIndex),
					Expression.Constant(throwError),
					Expression.Constant(lineNum)),
				// alias_x.GetDevice()
				VariableKind.Alias => Expression.Call(GetAlias(variable.AliasIndex), AliasGetDeviceMethodInfo, this._variables["processor"], Expression.Constant(lineNum)),
				_ => throw new InvalidOperationException("Unexpected type of variable in l instruction.")
			};

			public Expression GetDeviceById(Variable variable, bool throwError, int lineNum) =>
				Expression.Call(this._variables["processor"],
					GetDeviceByIdMethodInfo,
					IntValue(variable, lineNum),
					Expression.Constant(throwError),
					Expression.Constant(lineNum));

			public Expression ReadDevice(Variable device, Variable logicType, int lineNum)
			{
				return Expression.Call(this._variables["processor"],
					ReadDeviceMethodInfo,
					Device(device, true, lineNum),
					Enum<LogicType>(logicType, lineNum),
					Expression.Constant(lineNum));
			}
			public Expression ReadDevice(Expression device, Variable logicType, int lineNum)
			{
				return Expression.Call(this._variables["processor"],
					ReadDeviceMethodInfo,
					device,
					Enum<LogicType>(logicType, lineNum),
					Expression.Constant(lineNum));
			}

			public Expression ReadDeviceBatch(Variable deviceHash, Variable logicType, Variable batchMethod, int lineNum)
			{
				return Expression.Call(this._variables["processor"],
					ReadDeviceBatchMethodInfo,
					IntValue(deviceHash, lineNum),
					Enum<LogicType>(logicType, lineNum),
					Enum<LogicBatchMethod>(batchMethod, lineNum),
					Expression.Constant(lineNum));
			}

			public Expression ReadDeviceSlot(Variable device, Variable slotIndex, Variable logicType, int lineNum)
			{
				return Expression.Call(this._variables["processor"],
					ReadDeviceSlotMethodInfo,
					Device(device, true, lineNum),
					Enum<LogicSlotType>(logicType, lineNum),
					IntValue(slotIndex, lineNum),
					Expression.Constant(lineNum));
			}

			public Expression WriteDeviceSlot(Variable device, Variable slotIndex, Variable logicType, Variable value, int lineNum)
			{
				return Expression.Call(this._variables["processor"],
					WriteDeviceSlotMethodInfo,
					Device(device, true, lineNum),
					Enum<LogicSlotType>(logicType, lineNum),
					IntValue(slotIndex, lineNum),
					DoubleValue(value, lineNum),
					Expression.Constant(lineNum));
			}

			public Expression ReadDeviceReagent(Variable device, Variable mode, Variable reagent, int lineNum)
			{
				return Expression.Call(this._variables["processor"],
					ReadDeviceReagentMethodInfo,
					Device(device, true, lineNum),
					Enum<LogicReagentMode>(mode, lineNum),
					IntValue(reagent, lineNum),
					Expression.Constant(lineNum));
			}

			public Expression BatchReadSlot(Variable deviceHash, Variable slotIndex, Variable logicType, Variable batchMethod, int lineNum)
			{
				return Expression.Call(this._variables["processor"],
					BatchReadSlotMethodInfo,
					IntValue(deviceHash, lineNum),
					IntValue(slotIndex, lineNum),
					Enum<LogicSlotType>(logicType, lineNum),
					Enum<LogicBatchMethod>(batchMethod, lineNum),
					Expression.Constant(lineNum));
			}

			public Expression BatchReadName(Variable deviceHash, Variable nameHash, Variable logicType, Variable batchMethod, int lineNum)
			{
				return Expression.Call(this._variables["processor"],
					BatchReadNameMethodInfo,
					IntValue(deviceHash, lineNum),
					IntValue(nameHash, lineNum),
					Enum<LogicType>(logicType, lineNum),
					Enum<LogicBatchMethod>(batchMethod, lineNum),
					Expression.Constant(lineNum));
			}

			public Expression BatchReadNameSlot(Variable deviceHash, Variable nameHash, Variable slotIndex, Variable logicSlotType, Variable batchMethod, int lineNum)
			{
				return Expression.Call(this._variables["processor"],
					BatchReadNameSlotMethodInfo,
					IntValue(deviceHash, lineNum),
					IntValue(nameHash, lineNum),
					IntValue(slotIndex, lineNum),
					Enum<LogicSlotType>(logicSlotType, lineNum),
					Enum<LogicBatchMethod>(batchMethod, lineNum),
					Expression.Constant(lineNum));
			}

			public Expression BatchWriteName(Variable deviceHash, Variable nameHash, Variable logicType, Variable value, int lineNum)
			{
				return Expression.Call(this._variables["processor"],
					BatchWriteNameMethodInfo,
					IntValue(deviceHash, lineNum),
					IntValue(nameHash, lineNum),
					Enum<LogicType>(logicType, lineNum),
					DoubleValue(value, lineNum),
					Expression.Constant(lineNum));
			}

			public Expression BatchWriteSlot(Variable deviceHash, Variable slotIndex, Variable logicSlotType, Variable value, int lineNum)
			{
				return Expression.Call(this._variables["processor"],
					BatchWriteSlotMethodInfo,
					IntValue(deviceHash, lineNum),
					IntValue(slotIndex, lineNum),
					Enum<LogicSlotType>(logicSlotType, lineNum),
					DoubleValue(value, lineNum),
					Expression.Constant(lineNum));
			}

			public Expression WriteDevice(Expression device, Variable logicType, Variable value, int lineNum)
			{
				return Expression.Call(this._variables["processor"],
					WriteDeviceMethodInfo,
					device,
					Enum<LogicType>(logicType, lineNum),
					DoubleValue(value, lineNum),
					Expression.Constant(lineNum));
			}

			public Expression WriteDevice(Variable device, Variable logicType, Variable value, int lineNum)
			{
				return Expression.Call(this._variables["processor"],
					WriteDeviceMethodInfo,
					Device(device, true, lineNum),
					Enum<LogicType>(logicType, lineNum),
					DoubleValue(value, lineNum),
					Expression.Constant(lineNum));
			}

			public Expression WriteDeviceBatch(Variable deviceHash, Variable logicType, Variable value, int lineNum)
			{
				return Expression.Call(this._variables["processor"],
					WriteDeviceBatchMethodInfo,
					IntValue(deviceHash, lineNum),
					Enum<LogicType>(logicType, lineNum),
					DoubleValue(value, lineNum),
					Expression.Constant(lineNum));
			}

			public Expression Sleep(Variable duration, int lineNum)
			{
				return Expression.Call(this._variables["processor"],
					SleepMethodInfo,
					DoubleValue(duration, lineNum));
			}

			public Expression Break(LineOfCode line, int lineNum)
			{
				return Expression.Block(
					Expression.Call(this._variables["processor"], BreakMethodInfo, Expression.Constant(line.Source), Expression.Constant(line.SourceLine)),
					Expression.Assign(this._variables["line"], Expression.Constant(lineNum + 1)),
					Expression.Goto(EndLabel()));
			}
		}
	}
}
