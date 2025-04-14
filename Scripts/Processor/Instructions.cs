﻿#nullable enable
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Pipes;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Entropy.Assets.Scripts.Processor
{
	public partial class ChipProcessor
	{
		private static readonly Random Random = new Random();
		private static readonly MethodInfo MathSqrtMethodInfo = typeof(Math).GetMethod(nameof(Math.Sqrt), new[] { typeof(double) })!;
		private static readonly MethodInfo MathRoundMethodInfo = typeof(Math).GetMethod(nameof(Math.Round), new[] { typeof(double) })!;
		private static readonly MethodInfo MathTruncateMethodInfo = typeof(Math).GetMethod(nameof(Math.Truncate), new[] { typeof(double) })!;
		private static readonly MethodInfo MathCeilingMethodInfo = typeof(Math).GetMethod(nameof(Math.Ceiling), new[] { typeof(double) })!;
		private static readonly MethodInfo MathFloorMethodInfo = typeof(Math).GetMethod(nameof(Math.Floor), new[] { typeof(double) })!;
		private static readonly MethodInfo MathMaxMethodInfo = typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(double), typeof(double) })!;
		private static readonly MethodInfo MathMinMethodInfo = typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(double), typeof(double) })!;
		private static readonly MethodInfo MathAbsMethodInfo = typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(double) })!;
		private static readonly MethodInfo MathLogMethodInfo = typeof(Math).GetMethod(nameof(Math.Log), new[] { typeof(double) })!;
		private static readonly MethodInfo MathExpMethodInfo = typeof(Math).GetMethod(nameof(Math.Exp), new[] { typeof(double) })!;
		private static readonly MethodInfo MathSinMethodInfo = typeof(Math).GetMethod(nameof(Math.Sin), new[] { typeof(double) })!;
		private static readonly MethodInfo MathASinMethodInfo = typeof(Math).GetMethod(nameof(Math.Asin), new[] { typeof(double) })!;
		private static readonly MethodInfo MathTanMethodInfo = typeof(Math).GetMethod(nameof(Math.Tan), new[] { typeof(double) })!;
		private static readonly MethodInfo MathAtanMethodInfo = typeof(Math).GetMethod(nameof(Math.Atan), new[] { typeof(double) })!;
		private static readonly MethodInfo MathCosMethodInfo = typeof(Math).GetMethod(nameof(Math.Cos), new[] { typeof(double) })!;
		private static readonly MethodInfo MathACosMethodInfo = typeof(Math).GetMethod(nameof(Math.Acos), new[] { typeof(double) })!;
		private static readonly MethodInfo MathAtan2MethodInfo = typeof(Math).GetMethod(nameof(Math.Atan2), new[] { typeof(double), typeof(double) })!;
		private static readonly MethodInfo DoubleIsNanMethodInfo = typeof(double).GetMethod(nameof(double.IsNaN))!;
		private static readonly MethodInfo RandomNextDoubleMethodInfo = typeof(Random).GetMethod(nameof(Random.NextDouble))!;
		private static readonly MethodInfo IMemoryWritableWriteMemoryMethodInfo = typeof(IMemoryWritable).GetMethod(nameof(IMemoryWritable.WriteMemory))!;
		private static readonly MethodInfo IMemoryReadableReadMemoryMethodInfo = typeof(IMemoryReadable).GetMethod(nameof(IMemoryReadable.ReadMemory))!;

		static ChipProcessor()
		{
			var none = new VariableKind[] { };
			LineGenerator noneGen = (line, data, lineNum) => { };
			var operation10 = new VariableKind[] { VariableKind.Store };
			var operation11 = new VariableKind[] { VariableKind.Store, VariableKind.DoubleValue };
			var operation12 = new VariableKind[] { VariableKind.Store, VariableKind.DoubleValue, VariableKind.DoubleValue };
			var operation13 = new VariableKind[] { VariableKind.Store, VariableKind.DoubleValue, VariableKind.DoubleValue, VariableKind.DoubleValue };
			var operationj0 = new VariableKind[] { VariableKind.IntValue };
			var operationj1 = new VariableKind[] { VariableKind.DoubleValue, VariableKind.IntValue };
			var operationj2 = new VariableKind[] { VariableKind.DoubleValue, VariableKind.DoubleValue, VariableKind.IntValue };
			var operationj3 = new VariableKind[] { VariableKind.DoubleValue, VariableKind.DoubleValue, VariableKind.DoubleValue, VariableKind.IntValue };

			//break
			DefineInstructionHandler("break", Array.Empty<VariableKind>(),
				(line, data, lineNum) =>
					data.Add(data.Break(line, lineNum)));
			//l,
			DefineInstructionHandler("l",
				new VariableKind[] { VariableKind.Store, VariableKind.Device, VariableKind.Enum },
				(line, data, lineNum) =>
					data.Add(data.Assign(line.Argument1, data.ReadDevice(line.Argument2, line.Argument3, lineNum), lineNum)));
			//s,
			DefineInstructionHandler("s",
				new VariableKind[] { VariableKind.Device, VariableKind.Enum, VariableKind.DoubleValue },
				(line, data, lineNum) =>
					data.Add(data.WriteDevice(line.Argument1, line.Argument2, line.Argument3, lineNum)));
			//ls,
			DefineInstructionHandler("ls",
				new VariableKind[] { VariableKind.Store, VariableKind.Device, VariableKind.IntValue, VariableKind.Enum },
				(line, data, lineNum) =>
					data.Add(data.Assign(line.Argument1, data.ReadDeviceSlot(line.Argument2, line.Argument3, line.Argument4, lineNum), lineNum)));
			//ss,
			DefineInstructionHandler("ss",
				new VariableKind[] { VariableKind.Device, VariableKind.IntValue, VariableKind.Enum, VariableKind.DoubleValue },
				(line, data, lineNum) =>
					data.Add(data.WriteDeviceSlot(line.Argument1, line.Argument2, line.Argument3, line.Argument4, lineNum)));
			//lr,
			DefineInstructionHandler("lr",
				new VariableKind[] { VariableKind.Store, VariableKind.Device, VariableKind.Enum, VariableKind.IntValue },
				(line, data, lineNum) =>
					data.Add(data.Assign(line.Argument1, data.ReadDeviceReagent(line.Argument2, line.Argument3, line.Argument4, lineNum), lineNum)));
			//sb,
			DefineInstructionHandler("sb",
				new VariableKind[] { VariableKind.IntValue, VariableKind.DoubleValue, VariableKind.Enum },
				(line, data, lineNum) =>
					data.Add(data.WriteDeviceBatch(line.Argument1, line.Argument3, line.Argument2, lineNum)));
			//lb,
			DefineInstructionHandler("lb",
				new VariableKind[] { VariableKind.Store, VariableKind.IntValue, VariableKind.Enum, VariableKind.Enum },
				(line, data, lineNum) =>
					data.Add(data.Assign(line.Argument1, data.ReadDeviceBatch(line.Argument2, line.Argument3, line.Argument4, lineNum), lineNum)));
			//alias,
			DefineInstructionHandler("alias",
				new VariableKind[] { VariableKind.Alias | VariableKind.Register | VariableKind.DeviceRegister },
				(line, data, lineNum) =>
				{
					// alias_x = new Alias { Kind = AliasTarget.Register, Index = index, Recurse = recurse };
					// - OR -
					// alias_x = new Alias { Kind = AliasTarget.Device, Index = index, NetworkIndex = networkIndex };
					// - OR -
					// alias_x = new Alias { Kind = AliasTarget.Alias, Index = index };
					var alias = data.GetAlias(line.Argument1.AliasIndex); // The alias variable
					(AliasTarget target, int index, int recurse, int network) = line.Argument1.Kind switch
					{
						VariableKind.Register => (AliasTarget.Register, line.Argument1.RegisterIndex, line.Argument1.RegisterRecurse, int.MinValue),
						VariableKind.DeviceRegister => (AliasTarget.Device, line.Argument1.RegisterIndex, line.Argument1.RegisterRecurse, line.Argument1.NetworkIndex),
						VariableKind.Alias => (AliasTarget.Alias, (int)line.Argument1.Constant, 0, int.MinValue),
						_ => throw new InvalidOperationException("Unexpected type of variable in alias instruction."),
					};

					data.Add(Expression.Assign(Expression.PropertyOrField(alias, nameof(Alias.Kind)), Expression.Constant(target)));
					data.Add(Expression.Assign(Expression.PropertyOrField(alias, nameof(Alias.Index)), Expression.Constant(index)));
					data.Add(Expression.Assign(Expression.PropertyOrField(alias, nameof(Alias.Recurse)), Expression.Constant(recurse)));
					data.Add(Expression.Assign(Expression.PropertyOrField(alias, nameof(Alias.NetworkIndex)), Expression.Constant(network)));
				});
			//move,
			DefineInstructionHandler("move", operation11,
				// store = argument1
				(line, data, lineNum) =>
					data.Add(data.Assign(line.Argument1, data.DoubleValue(line.Argument2, lineNum), lineNum)));

			//add,
			DefineInstructionHandler("add", operation12,
				// store = argument1 + argument2
				(line, data, lineNum) =>
					data.Add(data.Assign(line.Argument1, Expression.Add(data.DoubleValue(line.Argument2, lineNum), data.DoubleValue(line.Argument3, lineNum)), lineNum)));
			//sub,
			DefineInstructionHandler("sub", operation12,
				// store = argument1 - argument2
				(line, data, lineNum) =>
					data.Add(data.Assign(line.Argument1, Expression.Subtract(data.DoubleValue(line.Argument2, lineNum), data.DoubleValue(line.Argument3, lineNum)), lineNum)));
			//and,
			DefineInstructionHandler("and", operation12,
				// store = LongToDouble(DoubleToLong(argument1) - DoubleToLong(argument2))
				(line, data, lineNum) =>
					data.Add(
						data.Assign(
							line.Argument1,
							data.LongToDoubleExpression(
								Expression.And(
									data.LongValue(line.Argument2, lineNum),
									data.LongValue(line.Argument3, lineNum)),
								lineNum),
							lineNum)));
			//or,
			DefineInstructionHandler("or", operation12,
				// store = LongToDouble(DoubleToLong(argument1) - DoubleToLong(argument2))
				(line, data, lineNum) =>
					data.Add(
						data.Assign(
							line.Argument1,
							data.LongToDoubleExpression(
								Expression.Or(
									data.LongValue(line.Argument2, lineNum),
									data.LongValue(line.Argument3, lineNum)),
								lineNum),
							lineNum)));
			//xor,
			DefineInstructionHandler("xor", operation12,
				// store = LongToDouble(DoubleToLong(argument1) - DoubleToLong(argument2))
				(line, data, lineNum) =>
					data.Add(
						data.Assign(
							line.Argument1,
							data.LongToDoubleExpression(
								Expression.ExclusiveOr(
									data.LongValue(line.Argument2, lineNum),
									data.LongValue(line.Argument3, lineNum)),
								lineNum),
							lineNum)));
			//nor,
			DefineInstructionHandler("nor", operation12,
				// store = LongToDouble(DoubleToLong(argument1) - DoubleToLong(argument2))
				(line, data, lineNum) =>
					data.Add(
						data.Assign(
							line.Argument1,
							data.LongToDoubleExpression(
								Expression.Not(
									Expression.Or(
										data.LongValue(line.Argument2, lineNum),
										data.LongValue(line.Argument3, lineNum))),
								lineNum),
							lineNum)));
			//mul,
			DefineInstructionHandler("mul", operation12,
				// store = argument1 - argument2
				(line, data, lineNum) =>
					data.Add(data.Assign(line.Argument1, Expression.Multiply(data.DoubleValue(line.Argument2, lineNum), data.DoubleValue(line.Argument3, lineNum)), lineNum)));
			//div,
			DefineInstructionHandler("div", operation12,
				// store = argument1 - argument2
				(line, data, lineNum) =>
					data.Add(data.Assign(line.Argument1, Expression.Multiply(data.DoubleValue(line.Argument2, lineNum), data.DoubleValue(line.Argument3, lineNum)), lineNum)));
			//mod,
			DefineInstructionHandler("mod", operation12,

				// store = argument1 - argument2
				(line, data, lineNum) =>
				{
					var dividend = data.DoubleValue(line.Argument2, lineNum);
					var divisor = data.DoubleValue(line.Argument3, lineNum);
					var resultVar = Expression.Variable(typeof(double), "result");
					var value = Expression.Condition(
						Expression.LessThan(resultVar, Expression.Constant(0.0)),
						Expression.Add(resultVar, divisor),
						resultVar);
					data.Add(Expression.Block(
						new[] { resultVar },
						Expression.Assign(resultVar, Expression.Modulo(dividend, divisor)), // result = dividend % divisor
						(line.Argument1.Kind switch
						{
							VariableKind.Register => Expression.Assign(data.RegisterSet(line.Argument1, lineNum), value),
							VariableKind.Alias => Expression.Call(data.GetAlias(line.Argument1.AliasIndex), AliasSetValueMethodInfo, value, Expression.Constant(lineNum)),
							_ => throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectVariableType, lineNum)
						})));
				});
			//sqrt
			DefineInstructionHandler("sqrt", operation11,
				// store = Math.Sqrt(argument1)
				(line, data, lineNum) =>
					data.Add(data.Assign(line.Argument1, Expression.Call(MathSqrtMethodInfo, data.DoubleValue(line.Argument2, lineNum)), lineNum)));
			//round,
			DefineInstructionHandler("round", operation11,
				// store = Math.Sqrt(argument1)
				(line, data, lineNum) =>
					data.Add(data.Assign(line.Argument1, Expression.Call(MathRoundMethodInfo, data.DoubleValue(line.Argument2, lineNum)), lineNum)));
			//trunc,
			DefineInstructionHandler("trunc", operation11,
				// store = Math.Sqrt(argument1)
				(line, data, lineNum) =>
					data.Add(data.Assign(line.Argument1, Expression.Call(MathTruncateMethodInfo, data.DoubleValue(line.Argument2, lineNum)), lineNum)));
			//ceil,
			DefineInstructionHandler("ceil", operation11,
				// store = Math.Sqrt(argument1)
				(line, data, lineNum) =>
					data.Add(data.Assign(line.Argument1, Expression.Call(MathCeilingMethodInfo, data.DoubleValue(line.Argument2, lineNum)), lineNum)));
			//floor,
			DefineInstructionHandler("floor", operation11,
				// store = Math.Sqrt(argument1)
				(line, data, lineNum) =>
					data.Add(data.Assign(line.Argument1, Expression.Call(MathFloorMethodInfo, data.DoubleValue(line.Argument2, lineNum)), lineNum)));
			//max,
			DefineInstructionHandler("max", operation12,
				// store = Math.Sqrt(argument1)
				(line, data, lineNum) =>
					data.Add(data.Assign(line.Argument1, Expression.Call(MathMaxMethodInfo, data.DoubleValue(line.Argument2, lineNum), data.DoubleValue(line.Argument3, lineNum)), lineNum)));
			//min,
			DefineInstructionHandler("min", operation12,
				// store = Math.Sqrt(argument1)
				(line, data, lineNum) =>
					data.Add(data.Assign(line.Argument1, Expression.Call(MathMinMethodInfo, data.DoubleValue(line.Argument2, lineNum), data.DoubleValue(line.Argument3, lineNum)), lineNum)));
			//abs,
			DefineInstructionHandler("abs", operation11,
				// store = Math.Sqrt(argument1)
				(line, data, lineNum) =>
					data.Add(data.Assign(line.Argument1, Expression.Call(MathAbsMethodInfo, data.DoubleValue(line.Argument2, lineNum)), lineNum)));
			//log,
			DefineInstructionHandler("log", operation11,
				// store = Math.Sqrt(argument1)
				(line, data, lineNum) =>
					data.Add(data.Assign(line.Argument1, Expression.Call(MathLogMethodInfo, data.DoubleValue(line.Argument2, lineNum)), lineNum)));
			//exp,
			DefineInstructionHandler("exp", operation11,
				// store = Math.Sqrt(argument1)
				(line, data, lineNum) =>
					data.Add(data.Assign(line.Argument1, Expression.Call(MathExpMethodInfo, data.DoubleValue(line.Argument2, lineNum)), lineNum)));
			//rand,
			DefineInstructionHandler("rand", operation10,
				(line, data, lineNum) =>
					data.Add(data.Assign(line.Argument1, Expression.Call(Expression.Constant(Random), RandomNextDoubleMethodInfo), lineNum)));
			//yield,
			DefineInstructionHandler("yield", none,
				(line, data, lineNum) =>
				{
					data.Add(Expression.Assign(data.LineVariable(), Expression.Constant(lineNum + 1)));
					data.Add(Expression.Goto(data.EndLabel()));
				});
			//sleep,
			DefineInstructionHandler("sleep",
				new VariableKind[] { VariableKind.DoubleValue },
				(line, data, lineNum) =>
				{
					data.Add(Expression.Assign(data.LineVariable(), Expression.Constant(lineNum + 1)));
					data.Add(data.Sleep(line.Argument1, lineNum));
					data.Add(Expression.Goto(data.EndLabel()));
				});
			//label,
			DefineInstructionHandler("label", none, noneGen);
			//peek,
			DefineInstructionHandler("peek", operation10,
				(line, data, lineNum) =>
					data.Add(
						Expression.IfThenElse(Expression.LessThan(data.StackPointer(), Expression.Constant(0.0)),
							Expression.Throw(Expression.New(ProgrammableChipExceptionConstructorInfo,
								Expression.Constant(ProgrammableChipException.ICExceptionType.StackUnderFlow), Expression.Constant(lineNum))),
							Expression.IfThenElse(Expression.GreaterThanOrEqual(data.StackPointer(), Expression.Convert(data.StackSize(), typeof(double))),
								Expression.Throw(Expression.New(ProgrammableChipExceptionConstructorInfo,
									Expression.Constant(ProgrammableChipException.ICExceptionType.StackOverFlow), Expression.Constant(lineNum))),
								data.Assign(line.Argument1, data.Stack(data.StackPointer()), lineNum)))));
			//push,
			DefineInstructionHandler("push",
				new VariableKind[] { VariableKind.DoubleValue },
				(line, data, lineNum) =>
				{
					data.Add(
						Expression.IfThenElse(Expression.LessThan(data.StackPointer(), Expression.Constant(0.0)),
							Expression.Throw(Expression.New(ProgrammableChipExceptionConstructorInfo,
								Expression.Constant(ProgrammableChipException.ICExceptionType.StackUnderFlow), Expression.Constant(lineNum))),
							Expression.IfThenElse(Expression.GreaterThanOrEqual(data.StackPointer(), Expression.Convert(data.StackSize(), typeof(double))),
								Expression.Throw(Expression.New(ProgrammableChipExceptionConstructorInfo,
									Expression.Constant(ProgrammableChipException.ICExceptionType.StackOverFlow), Expression.Constant(lineNum))),
							Expression.Assign(data.Stack(data.StackPointer()), data.DoubleValue(line.Argument1, lineNum)))));
					data.Add(Expression.PostIncrementAssign(data.StackPointer()));
				});
			//pop,
			DefineInstructionHandler("pop", operation10,
				(line, data, lineNum) =>
				{
					data.Add(Expression.PostDecrementAssign(data.StackPointer()));
					data.Add(
						Expression.IfThenElse(Expression.LessThan(data.StackPointer(), Expression.Constant(0.0)),
							Expression.Throw(Expression.New(ProgrammableChipExceptionConstructorInfo,
								Expression.Constant(ProgrammableChipException.ICExceptionType.StackUnderFlow), Expression.Constant(lineNum))),
							Expression.IfThenElse(Expression.GreaterThanOrEqual(data.StackPointer(), Expression.Convert(data.StackSize(), typeof(double))),
								Expression.Throw(Expression.New(ProgrammableChipExceptionConstructorInfo,
									Expression.Constant(ProgrammableChipException.ICExceptionType.StackOverFlow), Expression.Constant(lineNum))),
						data.Assign(line.Argument1, data.Stack(data.StackPointer()), lineNum))));
				});
			//hcf,
			DefineInstructionHandler("hcf", none,
				(line, data, lineNum) => data.Add(
					Expression.Throw(
						Expression.New(ProgrammableChipExceptionConstructorInfo,
							Expression.Constant(ProgrammableChipException.ICExceptionType.ChipCatchingFire),
							Expression.Constant(lineNum)))));
			//select,
			DefineInstructionHandler("select", operation13,
				(line, data, lineNum) =>
				{
					var condition = Expression.NotEqual(data.DoubleValue(line.Argument2, lineNum), Expression.Constant(0.0));
					var trueValue = data.DoubleValue(line.Argument3, lineNum);
					var falseValue = data.DoubleValue(line.Argument4, lineNum);
					var result = Expression.Condition(condition, trueValue, falseValue);
					data.Add(data.Assign(line.Argument1, result, lineNum));
				});
			//j,
			DefineInstructionHandler("j", operationj0,
				(line, data, lineNum) =>
				{
					switch (line.Argument1.Kind)
					{
						case VariableKind.Constant:
							data.Add(Expression.Goto(data.LineLabel((int)line.Argument1.Constant)));
							break;
						default:
							data.Add(Expression.Assign(data.LineVariable(), data.IntValue(line.Argument1, lineNum)));
							data.Add(Expression.Goto(data.ControlSwitchLabel()));
							break;
					}
				});
			//jr,
			DefineInstructionHandler("jr", operationj0,
				(line, data, lineNum) =>
				{
					switch (line.Argument1.Kind)
					{
						case VariableKind.Constant:
							data.Add(Expression.Goto(data.LineLabel(lineNum + (int)line.Argument1.Constant)));
							break;
						default:
							data.Add(Expression.Assign(data.LineVariable(), Expression.Add(Expression.Constant(lineNum), data.IntValue(line.Argument1, lineNum))));
							data.Add(Expression.Goto(data.ControlSwitchLabel()));
							break;
					}
				});
			//jal,
			DefineInstructionHandler("jal", operationj0,
				(line, data, lineNum) =>
				{
					data.Add(
						Expression.IfThenElse(Expression.LessThan(data.StackPointer(), Expression.Constant(0.0)),
							Expression.Throw(Expression.New(ProgrammableChipExceptionConstructorInfo,
								Expression.Constant(ProgrammableChipException.ICExceptionType.StackUnderFlow), Expression.Constant(lineNum))),
							Expression.IfThenElse(Expression.GreaterThanOrEqual(data.StackPointer(), Expression.Convert(data.StackSize(), typeof(double))),
								Expression.Throw(Expression.New(ProgrammableChipExceptionConstructorInfo,
									Expression.Constant(ProgrammableChipException.ICExceptionType.StackOverFlow), Expression.Constant(lineNum))),
								Expression.Assign(data.Stack(data.StackPointer()), Expression.Constant(lineNum)))));
					data.Add(Expression.PostIncrementAssign(data.StackPointer()));
					switch (line.Argument1.Kind)
					{
						case VariableKind.Constant:
							data.Add(Expression.Goto(data.LineLabel((int)line.Argument1.Constant)));
							break;
						default:
							data.Add(Expression.Assign(data.LineVariable(), data.IntValue(line.Argument1, lineNum)));
							data.Add(Expression.Goto(data.ControlSwitchLabel()));
							break;
					}
				});
			//beq,
			DefineInstructionHandler("beq", operationj2, CompareBranchGenerator(false, false, false, Expression.Equal));
			//bne,
			DefineInstructionHandler("bne", operationj2, CompareBranchGenerator(false, false, false, Expression.NotEqual));
			//blt,
			DefineInstructionHandler("blt", operationj2, CompareBranchGenerator(false, false, false, Expression.LessThan));
			//ble,
			DefineInstructionHandler("ble", operationj2, CompareBranchGenerator(false, false, false, Expression.LessThanOrEqual));
			//bgt,
			DefineInstructionHandler("bgt", operationj2, CompareBranchGenerator(false, false, false, Expression.GreaterThan));
			//bge,
			DefineInstructionHandler("bge", operationj2, CompareBranchGenerator(false, false, false, Expression.GreaterThanOrEqual));
			//beqz,
			DefineInstructionHandler("beqz", operationj1, CompareBranchGenerator(false, false, true, Expression.Equal));
			//bnez,
			DefineInstructionHandler("bnez", operationj1, CompareBranchGenerator(false, false, true, Expression.NotEqual));
			//bltz,
			DefineInstructionHandler("bltz", operationj1, CompareBranchGenerator(false, false, true, Expression.LessThan));
			//blez,
			DefineInstructionHandler("blez", operationj1, CompareBranchGenerator(false, false, true, Expression.LessThanOrEqual));
			//bgtz,
			DefineInstructionHandler("bgtz", operationj1, CompareBranchGenerator(false, false, true, Expression.GreaterThan));
			//bgez,
			DefineInstructionHandler("bgez", operationj1, CompareBranchGenerator(false, false, true, Expression.GreaterThanOrEqual));
			//beqal,
			DefineInstructionHandler("beqal", operationj2, CompareBranchGenerator(false, true, false, Expression.Equal));
			//bneal,
			DefineInstructionHandler("bneal", operationj2, CompareBranchGenerator(false, true, false, Expression.NotEqual));
			//bltal,
			DefineInstructionHandler("bltal", operationj2, CompareBranchGenerator(false, true, false, Expression.LessThan));
			//bleal,
			DefineInstructionHandler("bleal", operationj2, CompareBranchGenerator(false, true, false, Expression.LessThanOrEqual));
			//bgtal,
			DefineInstructionHandler("bgtal", operationj2, CompareBranchGenerator(false, true, false, Expression.GreaterThan));
			//bgeal,
			DefineInstructionHandler("bgeal", operationj2, CompareBranchGenerator(false, true, false, Expression.GreaterThanOrEqual));
			//beqzal,
			DefineInstructionHandler("beqzal", operationj1, CompareBranchGenerator(false, true, true, Expression.Equal));
			//bnezal,
			DefineInstructionHandler("bnezal", operationj1, CompareBranchGenerator(false, true, true, Expression.NotEqual));
			//bltzal,
			DefineInstructionHandler("bltzal", operationj1, CompareBranchGenerator(false, true, true, Expression.LessThan));
			//blezal,
			DefineInstructionHandler("blezal", operationj1, CompareBranchGenerator(false, true, true, Expression.LessThanOrEqual));
			//bgtzal,
			DefineInstructionHandler("bgtzal", operationj1, CompareBranchGenerator(false, true, true, Expression.GreaterThan));
			//bgezal,
			DefineInstructionHandler("bgezal", operationj1, CompareBranchGenerator(false, true, true, Expression.GreaterThanOrEqual));
			//breq,
			DefineInstructionHandler("breq", operationj2, CompareBranchGenerator(true, false, false, Expression.Equal));
			//brne,
			DefineInstructionHandler("brne", operationj2, CompareBranchGenerator(true, false, false, Expression.NotEqual));
			//brlt,
			DefineInstructionHandler("brlt", operationj2, CompareBranchGenerator(true, false, false, Expression.LessThan));
			//brle,
			DefineInstructionHandler("brle", operationj2, CompareBranchGenerator(true, false, false, Expression.LessThanOrEqual));
			//brgt,
			DefineInstructionHandler("brgt", operationj2, CompareBranchGenerator(true, false, false, Expression.GreaterThan));
			//brge,
			DefineInstructionHandler("brge", operationj2, CompareBranchGenerator(true, false, false, Expression.GreaterThanOrEqual));
			//breqz,
			DefineInstructionHandler("breqz", operationj1, CompareBranchGenerator(true, false, true, Expression.Equal));
			//brnez,
			DefineInstructionHandler("brnez", operationj1, CompareBranchGenerator(true, false, true, Expression.NotEqual));
			//brltz,
			DefineInstructionHandler("brltz", operationj1, CompareBranchGenerator(true, false, true, Expression.LessThan));
			//brlez,
			DefineInstructionHandler("brlez", operationj1, CompareBranchGenerator(true, false, true, Expression.LessThanOrEqual));
			//brgtz,
			DefineInstructionHandler("brgtz", operationj1, CompareBranchGenerator(true, false, true, Expression.GreaterThan));
			//brgez,
			DefineInstructionHandler("brgez", operationj1, CompareBranchGenerator(true, false, true, Expression.GreaterThanOrEqual));
			//breqal,
			DefineInstructionHandler("breqal", operationj2, CompareBranchGenerator(true, true, false, Expression.Equal));
			//brneal,
			DefineInstructionHandler("brneal", operationj2, CompareBranchGenerator(true, true, false, Expression.NotEqual));
			//brltal,
			DefineInstructionHandler("brltal", operationj2, CompareBranchGenerator(true, true, false, Expression.LessThan));
			//brleal,
			DefineInstructionHandler("brleal", operationj2, CompareBranchGenerator(true, true, false, Expression.LessThanOrEqual));
			//brgtal,
			DefineInstructionHandler("brgtal", operationj2, CompareBranchGenerator(true, true, false, Expression.GreaterThan));
			//brgeal,
			DefineInstructionHandler("brgeal", operationj2, CompareBranchGenerator(true, true, false, Expression.GreaterThanOrEqual));
			//breqzal,
			DefineInstructionHandler("breqzal", operationj1, CompareBranchGenerator(true, true, true, Expression.Equal));
			//brnezal,
			DefineInstructionHandler("brnezal", operationj1, CompareBranchGenerator(true, true, true, Expression.NotEqual));
			//brltzal,
			DefineInstructionHandler("brltzal", operationj1, CompareBranchGenerator(true, true, true, Expression.LessThan));
			//brlezal,
			DefineInstructionHandler("brlezal", operationj1, CompareBranchGenerator(true, true, true, Expression.LessThanOrEqual));
			//brgtzal,
			DefineInstructionHandler("brgtzal", operationj1, CompareBranchGenerator(true, true, true, Expression.GreaterThan));
			//brgezal,
			DefineInstructionHandler("brgezal", operationj1, CompareBranchGenerator(true, true, true, Expression.GreaterThanOrEqual));
			//bap,
			DefineInstructionHandler("bap", operationj3, ApproxBranchGenerator(false, false, false, true));
			//bna,
			DefineInstructionHandler("bna", operationj3, ApproxBranchGenerator(false, false, false, false));
			//bapz,
			DefineInstructionHandler("bapz", operationj2, ApproxBranchGenerator(false, false, true, true));
			//bnaz,
			DefineInstructionHandler("bnaz", operationj2, ApproxBranchGenerator(false, false, true, false));
			//bapal,
			DefineInstructionHandler("bapal", operationj3, ApproxBranchGenerator(false, true, false, true));
			//bnaal,
			DefineInstructionHandler("bnaal", operationj3, ApproxBranchGenerator(false, true, false, false));
			//bapzal,
			DefineInstructionHandler("bapzal", operationj2, ApproxBranchGenerator(false, true, true, true));
			//bnazal,
			DefineInstructionHandler("bnazal", operationj2, ApproxBranchGenerator(false, true, true, false));
			//brap,
			DefineInstructionHandler("brap", operationj3, ApproxBranchGenerator(true, false, false, true));
			//brna,
			DefineInstructionHandler("brna", operationj3, ApproxBranchGenerator(true, false, false, false));
			//brapz,
			DefineInstructionHandler("brapz", operationj2, ApproxBranchGenerator(true, false, true, true));
			//brnaz,
			DefineInstructionHandler("brnaz", operationj2, ApproxBranchGenerator(true, false, true, false));
			//bnan,
			DefineInstructionHandler("bnan", operationj1, NanBranchGenerator(false, false));
			//brnan,
			DefineInstructionHandler("brnan", operationj1, NanBranchGenerator(true, false));
			//bnanal,
			DefineInstructionHandler("bnanal", operationj1, NanBranchGenerator(false, true));
			//brnanal,
			DefineInstructionHandler("brnanal", operationj1, NanBranchGenerator(true, true));
			//bdse,
			DefineInstructionHandler("bdse",
				new VariableKind[] { VariableKind.Device, VariableKind.IntValue },
				DeviceBranchGenerator(false, false, true));
			//bdns,
			DefineInstructionHandler("bdns",
				new VariableKind[] { VariableKind.Device, VariableKind.IntValue },
				DeviceBranchGenerator(false, false, false));
			//bdseal,
			DefineInstructionHandler("bdseal",
				new VariableKind[] { VariableKind.Device, VariableKind.IntValue },
				DeviceBranchGenerator(false, true, true));
			//bdnsal,
			DefineInstructionHandler("bdnsal",
				new VariableKind[] { VariableKind.Device, VariableKind.IntValue },
				DeviceBranchGenerator(false, true, false));
			//brdse,
			DefineInstructionHandler("brdse",
				new VariableKind[] { VariableKind.Device, VariableKind.IntValue },
				DeviceBranchGenerator(true, false, true));
			//brdns,
			DefineInstructionHandler("brdns",
				new VariableKind[] { VariableKind.Device, VariableKind.IntValue },
				DeviceBranchGenerator(true, false, false));
			//seq,
			DefineInstructionHandler("seq", operation12, ConditionalSetGenerator(false, Expression.Equal));
			//sne,
			DefineInstructionHandler("sne", operation12, ConditionalSetGenerator(false, Expression.NotEqual));
			//sgt,
			DefineInstructionHandler("sgt", operation12, ConditionalSetGenerator(false, Expression.GreaterThan));
			//sge,
			DefineInstructionHandler("sge", operation12, ConditionalSetGenerator(false, Expression.GreaterThanOrEqual));
			//slt,
			DefineInstructionHandler("slt", operation12, ConditionalSetGenerator(false, Expression.LessThan));
			//sle,
			DefineInstructionHandler("sle", operation12, ConditionalSetGenerator(false, Expression.LessThanOrEqual));
			//seqz,
			DefineInstructionHandler("seqz", operation11, ConditionalSetGenerator(true, Expression.Equal));
			//snez,
			DefineInstructionHandler("snez", operation11, ConditionalSetGenerator(true, Expression.NotEqual));
			//sgtz,
			DefineInstructionHandler("sgtz", operation11, ConditionalSetGenerator(true, Expression.GreaterThan));
			//sgez,
			DefineInstructionHandler("sgez", operation11, ConditionalSetGenerator(true, Expression.GreaterThanOrEqual));
			//sltz,
			DefineInstructionHandler("sltz", operation11, ConditionalSetGenerator(true, Expression.LessThan));
			//slez,
			DefineInstructionHandler("slez", operation11, ConditionalSetGenerator(true, Expression.LessThanOrEqual));
			//sdse,
			DefineInstructionHandler("sdse",
				new VariableKind[] { VariableKind.Store, VariableKind.Device },
				(line, data, lineNum) =>
					data.Add(
						data.Assign(
							line.Argument1,
							Expression.Condition(
								Expression.NotEqual(data.Device(line.Argument1, false, lineNum), Expression.Constant(null)),
								Expression.Constant(1.0),
								Expression.Constant(0.0)),
							lineNum)));
			//sdns,
			DefineInstructionHandler("sdns",
				new VariableKind[] { VariableKind.Store, VariableKind.Device },
				(line, data, lineNum) =>
					data.Add(
						data.Assign(
							line.Argument1,
							Expression.Condition(
								Expression.Equal(data.Device(line.Argument1, false, lineNum), Expression.Constant(null)),
								Expression.Constant(1.0),
								Expression.Constant(0.0)),
							lineNum)));
			//sap,
			DefineInstructionHandler("sap", operation12, ApproxConditionalSetGenerator(false, true));
			//sna,
			DefineInstructionHandler("sna", operation12, ApproxConditionalSetGenerator(false, false));
			//sapz,
			DefineInstructionHandler("sapz", operation11, ApproxConditionalSetGenerator(true, true));
			//snaz,
			DefineInstructionHandler("snaz", operation11, ApproxConditionalSetGenerator(true, false));
			//snan,
			DefineInstructionHandler("snan", operation11,
				(line, data, lineNum) => data.Add(data.Assign(
					line.Argument1,
					Expression.Condition(
						Expression.Call(DoubleIsNanMethodInfo, data.DoubleValue(line.Argument2, lineNum)),
						Expression.Constant(1.0),
						Expression.Constant(0.0)),
					lineNum)));
			//snanz,
			DefineInstructionHandler("snanz", operation11,
				(line, data, lineNum) => data.Add(data.Assign(
					line.Argument1,
					Expression.Condition(
						Expression.Call(DoubleIsNanMethodInfo, data.DoubleValue(line.Argument2, lineNum)),
						Expression.Constant(0.0),
						Expression.Constant(1.0)),
					lineNum)));
			//define,
			DefineInstructionHandler("define", none, noneGen);
			//sin,
			DefineInstructionHandler("sin", operation11,
				// store = Math.Sqrt(argument1)
				(line, data, lineNum) => data.Add(data.Assign(line.Argument1,
					Expression.Call(MathSinMethodInfo, data.DoubleValue(line.Argument2, lineNum)), lineNum)));
			//asin,
			DefineInstructionHandler("asin", operation11,
				// store = Math.Sqrt(argument1)
				(line, data, lineNum) => data.Add(data.Assign(line.Argument1,
					Expression.Call(MathASinMethodInfo, data.DoubleValue(line.Argument2, lineNum)), lineNum)));
			//tan,
			DefineInstructionHandler("tan", operation11,
				// store = Math.Sqrt(argument1)
				(line, data, lineNum) => data.Add(data.Assign(line.Argument1,
					Expression.Call(MathTanMethodInfo, data.DoubleValue(line.Argument2, lineNum)), lineNum)));
			//atan,
			DefineInstructionHandler("atan", operation11,
				// store = Math.Sqrt(argument1)
				(line, data, lineNum) => data.Add(data.Assign(line.Argument1,
					Expression.Call(MathAtanMethodInfo, data.DoubleValue(line.Argument2, lineNum)), lineNum)));
			//cos,
			DefineInstructionHandler("cos", operation11,
				// store = Math.Sqrt(argument1)
				(line, data, lineNum) => data.Add(data.Assign(line.Argument1,
					Expression.Call(MathCosMethodInfo, data.DoubleValue(line.Argument2, lineNum)), lineNum)));
			//acos,
			DefineInstructionHandler("acos", operation11,
				// store = Math.Sqrt(argument1)
				(line, data, lineNum) => data.Add(data.Assign(line.Argument1,
					Expression.Call(MathACosMethodInfo, data.DoubleValue(line.Argument2, lineNum)), lineNum)));
			//atan2,
			DefineInstructionHandler("atan2", operation11,
				// store = Math.Sqrt(argument1)
				(line, data, lineNum) => data.Add(data.Assign(line.Argument1,
					Expression.Call(MathAtan2MethodInfo, data.DoubleValue(line.Argument2, lineNum)), lineNum)));
			//lbs,
			DefineInstructionHandler("lbs",
				new VariableKind[] { VariableKind.Store, VariableKind.IntValue, VariableKind.IntValue, VariableKind.Enum, VariableKind.Enum },
				(line, data, lineNum) => data.Add(data.Assign(line.Argument1,
					data.BatchReadSlot(line.Argument2, line.Argument3, line.Argument4, line.Argument5, lineNum), lineNum)));
			//lbn,
			DefineInstructionHandler("lbn",
				new VariableKind[] { VariableKind.Store, VariableKind.IntValue, VariableKind.IntValue, VariableKind.Enum, VariableKind.Enum },
				(line, data, lineNum) => data.Add(data.Assign(line.Argument1,
					data.BatchReadName(line.Argument2, line.Argument3, line.Argument4, line.Argument5, lineNum), lineNum)));
			//lbns,
			DefineInstructionHandler("lbns",
				new VariableKind[] { VariableKind.Store, VariableKind.IntValue, VariableKind.IntValue, VariableKind.IntValue, VariableKind.Enum, VariableKind.Enum },
				(line, data, lineNum) =>
					data.Add(data.Assign(line.Argument1,
						data.BatchReadNameSlot(line.Argument2, line.Argument3, line.Argument4, line.Argument5, line.Argument6, lineNum), lineNum)));
			//sbn,
			DefineInstructionHandler("sbn",
				new VariableKind[] { VariableKind.IntValue, VariableKind.IntValue, VariableKind.Enum, VariableKind.DoubleValue },
				(line, data, lineNum) => data.Add(data.BatchWriteName(line.Argument1, line.Argument2, line.Argument3, line.Argument4, lineNum)));
			//sbs,
			DefineInstructionHandler("sbs",
				new VariableKind[] { VariableKind.IntValue, VariableKind.IntValue, VariableKind.Enum, VariableKind.DoubleValue },
				(line, data, lineNum) => data.Add(data.BatchWriteSlot(line.Argument1, line.Argument2, line.Argument3, line.Argument4, lineNum)));
			//srl,
			DefineInstructionHandler("srl", operation12,
				(line, data, lineNum) =>
					data.Add(
						data.Assign(
							line.Argument1,
							data.LongToDoubleExpression(
								Expression.RightShift(
									data.LongValue(line.Argument2, false, lineNum),
									data.IntValue(line.Argument3, lineNum)),
								lineNum),
							lineNum)));
			//sra,
			DefineInstructionHandler("sra", operation12,
				(line, data, lineNum) =>
					data.Add(
						data.Assign(
							line.Argument1,
							data.LongToDoubleExpression(
								Expression.RightShift(
									data.LongValue(line.Argument2, lineNum),
									data.IntValue(line.Argument3, lineNum)),
								lineNum),
							lineNum)));
			//sll,
			DefineInstructionHandler("sll", operation12,
				(line, data, lineNum) =>
					data.Add(
						data.Assign(
							line.Argument1,
							data.LongToDoubleExpression(
								Expression.LeftShift(
									data.LongValue(line.Argument2, lineNum),
									data.IntValue(line.Argument3, lineNum)),
								lineNum),
							lineNum)));
			//sla,
			DefineInstructionHandler("sla", operation12,
				(line, data, lineNum) =>
					data.Add(
						data.Assign(
							line.Argument1,
							data.LongToDoubleExpression(
								Expression.LeftShift(
									data.LongValue(line.Argument2, lineNum),
									data.IntValue(line.Argument3, lineNum)),
								lineNum),
							lineNum)));
			//not,
			DefineInstructionHandler("not", operation11,
				// store = Math.Sqrt(argument1)
				(line, data, lineNum) =>
					data.Add(data.Assign(line.Argument1, Expression.Not(data.DoubleValue(line.Argument2, lineNum)), lineNum)));
			//ld,
			DefineInstructionHandler("ld",
				new VariableKind[] { VariableKind.Store, VariableKind.IntValue, VariableKind.Enum },
				(line, data, lineNum) =>
					data.Add(data.Assign(line.Argument1, data.ReadDevice(data.GetDeviceById(line.Argument2, true, lineNum), line.Argument3, lineNum), lineNum)));
			//sd,
			DefineInstructionHandler("sd",
				new VariableKind[] { VariableKind.IntValue, VariableKind.Enum, VariableKind.DoubleValue },
				(line, data, lineNum) =>
					data.Add(data.WriteDevice(data.GetDeviceById(line.Argument1, true, lineNum), line.Argument2, line.Argument3, lineNum)));
			//poke,
			DefineInstructionHandler("poke",
				new VariableKind[] { VariableKind.IntValue, VariableKind.DoubleValue },
				(line, data, lineNum) =>
					data.Add(
						Expression.IfThenElse(Expression.LessThan(data.IntValue(line.Argument1, lineNum), Expression.Constant(0.0)),
							Expression.Throw(Expression.New(ProgrammableChipExceptionConstructorInfo,
								Expression.Constant(ProgrammableChipException.ICExceptionType.StackUnderFlow), Expression.Constant(lineNum))),
							Expression.IfThenElse(Expression.GreaterThanOrEqual(data.IntValue(line.Argument1, lineNum), data.StackSize()),
								Expression.Throw(Expression.New(ProgrammableChipExceptionConstructorInfo,
									Expression.Constant(ProgrammableChipException.ICExceptionType.StackOverFlow), Expression.Constant(lineNum))),
								Expression.Assign(data.Stack(data.IntValue(line.Argument1, lineNum)), data.DoubleValue(line.Argument2, lineNum))
							)
						)
					)
			);
			//fetch,
			DefineInstructionHandler("fetch",
				new VariableKind[] { VariableKind.Store, VariableKind.IntValue },
				(line, data, lineNum) =>
					data.Add(
						Expression.IfThenElse(Expression.LessThan(data.IntValue(line.Argument2, lineNum), Expression.Constant(0.0)),
							Expression.Throw(Expression.New(ProgrammableChipExceptionConstructorInfo,
								Expression.Constant(ProgrammableChipException.ICExceptionType.StackUnderFlow), Expression.Constant(lineNum))),
							Expression.IfThenElse(Expression.GreaterThanOrEqual(data.IntValue(line.Argument2, lineNum), data.StackSize()),
								Expression.Throw(Expression.New(ProgrammableChipExceptionConstructorInfo,
									Expression.Constant(ProgrammableChipException.ICExceptionType.StackOverFlow), Expression.Constant(lineNum))),
								data.Assign(line.Argument1, data.Stack(data.IntValue(line.Argument2, lineNum)), lineNum)
							)
						)
					)
			);
			//getd,
			DefineInstructionHandler("getd",
				new VariableKind[] { VariableKind.Store, VariableKind.IntValue, VariableKind.IntValue },
				(line, data, lineNum) =>
					data.Add(
						Expression.TryCatch(
							data.Assign(
								line.Argument1,
								Expression.Call(
									Expression.Convert(data.GetDeviceById(line.Argument2, true, lineNum), typeof(IMemoryReadable)),
									IMemoryReadableReadMemoryMethodInfo,
									data.IntValue(line.Argument3, lineNum)),
								lineNum),
							Expression.Catch(typeof(InvalidCastException),
								Expression.Throw(Expression.New(
									ProgrammableChipExceptionConstructorInfo,
									Expression.Constant(ProgrammableChipException.ICExceptionType.MemoryNotWriteable),
									Expression.Constant(lineNum)
								))
							)
						)
					)
			);
			//putd,
			DefineInstructionHandler("putd",
				new VariableKind[] { VariableKind.DoubleValue, VariableKind.IntValue, VariableKind.IntValue },
				(line, data, lineNum) =>
					data.Add(
						Expression.TryCatch(
							Expression.Call(
								Expression.Convert(data.GetDeviceById(line.Argument2, true, lineNum), typeof(IMemoryWritable)),
								IMemoryWritableWriteMemoryMethodInfo,
								data.IntValue(line.Argument3, lineNum),
								data.DoubleValue(line.Argument1, lineNum)),
							Expression.Catch(typeof(InvalidCastException),
								Expression.Throw(Expression.New(
									ProgrammableChipExceptionConstructorInfo,
									Expression.Constant(ProgrammableChipException.ICExceptionType.MemoryNotWriteable),
									Expression.Constant(lineNum)))),
							Expression.Catch(typeof(NullReferenceException),
								Expression.Throw(Expression.New(
									ProgrammableChipExceptionConstructorInfo,
									Expression.Constant(ProgrammableChipException.ICExceptionType.DeviceNotFound),
									Expression.Constant(lineNum)))),
							Expression.Catch(typeof(StackUnderflowException),
								Expression.Throw(Expression.New(
									ProgrammableChipExceptionConstructorInfo,
									Expression.Constant(ProgrammableChipException.ICExceptionType.StackUnderFlow),
									Expression.Constant(lineNum)))),
							Expression.Catch(typeof(StackOverflowException),
								Expression.Throw(Expression.New(
									ProgrammableChipExceptionConstructorInfo,
									Expression.Constant(ProgrammableChipException.ICExceptionType.StackOverFlow),
									Expression.Constant(lineNum)))),
							Expression.Catch(typeof(Exception),
								Expression.Throw(Expression.New(
									ProgrammableChipExceptionConstructorInfo,
									Expression.Constant(ProgrammableChipException.ICExceptionType.Unknown),
									Expression.Constant(lineNum)))))));
			//get,
			DefineInstructionHandler("get",
				new VariableKind[] { VariableKind.Store, VariableKind.Device, VariableKind.IntValue },
				(line, data, lineNum) =>
					data.Add(
						Expression.TryCatch(
							data.Assign(
								line.Argument1,
								Expression.Call(
									Expression.Convert(data.Device(line.Argument2, true, lineNum), typeof(IMemoryReadable)),
									IMemoryReadableReadMemoryMethodInfo,
									data.IntValue(line.Argument3, lineNum)),
								lineNum),
							Expression.Catch(typeof(InvalidCastException),
								Expression.Throw(Expression.New(
									ProgrammableChipExceptionConstructorInfo,
									Expression.Constant(ProgrammableChipException.ICExceptionType.MemoryNotWriteable),
									Expression.Constant(lineNum)
								))
							)
						)
					)
				);
			//put,
			DefineInstructionHandler("put",
				new VariableKind[] { VariableKind.DoubleValue, VariableKind.Device, VariableKind.IntValue },
				(line, data, lineNum) =>
					data.Add(
						Expression.TryCatch(
							Expression.Call(
								Expression.Convert(data.Device(line.Argument2, true, lineNum), typeof(IMemoryWritable)),
								IMemoryWritableWriteMemoryMethodInfo,
								data.IntValue(line.Argument3, lineNum),
								data.DoubleValue(line.Argument1, lineNum)),
							Expression.Catch(typeof(InvalidCastException),
								Expression.Throw(Expression.New(
									ProgrammableChipExceptionConstructorInfo,
									Expression.Constant(ProgrammableChipException.ICExceptionType.MemoryNotWriteable),
									Expression.Constant(lineNum)))),
							Expression.Catch(typeof(NullReferenceException),
								Expression.Throw(Expression.New(
									ProgrammableChipExceptionConstructorInfo,
									Expression.Constant(ProgrammableChipException.ICExceptionType.DeviceNotFound),
									Expression.Constant(lineNum)))),
							Expression.Catch(typeof(StackUnderflowException),
								Expression.Throw(Expression.New(
									ProgrammableChipExceptionConstructorInfo,
									Expression.Constant(ProgrammableChipException.ICExceptionType.StackUnderFlow),
									Expression.Constant(lineNum)))),
							Expression.Catch(typeof(StackOverflowException),
								Expression.Throw(Expression.New(
									ProgrammableChipExceptionConstructorInfo,
									Expression.Constant(ProgrammableChipException.ICExceptionType.StackOverFlow),
									Expression.Constant(lineNum)))),
							Expression.Catch(typeof(Exception),
								Expression.Throw(Expression.New(
									ProgrammableChipExceptionConstructorInfo,
									Expression.Constant(ProgrammableChipException.ICExceptionType.Unknown),
									Expression.Constant(lineNum)))))));
		}


		private static LineGenerator ConditionalSetGenerator(bool compareToZero, Func<Expression, Expression, BinaryExpression> comparison) =>
			(line, data, lineNum) =>
			{
				var value1 = data.DoubleValue(line.Argument2, lineNum);
				var value2 = compareToZero ? Expression.Constant(0.0) : data.DoubleValue(line.Argument3, lineNum);
				data.Add(
					data.Assign(
						line.Argument1,
						Expression.Condition(comparison(value1, value2), Expression.Constant(1.0), Expression.Constant(0.0)),
						lineNum));
			};

		private static LineGenerator ApproxConditionalSetGenerator(bool compareToZero, bool equals) =>
			(line, data, lineNum) =>
			{
				var value1 = data.DoubleValue(line.Argument2, lineNum);
				var value2 = compareToZero ? Expression.Constant(0.0) : data.DoubleValue(line.Argument3, lineNum);
				BinaryExpression comparison;
				if (equals)

					//Math.Abs(variableValue1 - variableValue2) <= Math.Max(variableValue3 * Math.Max(Math.Abs(variableValue1), Math.Abs(variableValue2)), 1.1210387714598537E-44)
					comparison =
						Expression.LessThanOrEqual(
							Expression.Call(MathAbsMethodInfo,
								Expression.Subtract(value1, value2)),
							Expression.Call(MathMaxMethodInfo,
								Expression.Multiply(data.DoubleValue(line.Argument3, lineNum),
									Expression.Call(MathMaxMethodInfo,
										Expression.Call(MathAbsMethodInfo, value1),
										Expression.Call(MathAbsMethodInfo, value2))),
								Expression.Constant(1.1210387714598537E-44)));
				else

					//Math.Abs(variableValue1 - variableValue2) > Math.Max(variableValue3 * Math.Max(Math.Abs(variableValue1), Math.Abs(variableValue2)), 1.1210387714598537E-44)
					comparison =
						Expression.GreaterThan(
							Expression.Call(MathAbsMethodInfo,
								Expression.Subtract(value1, value2)),
							Expression.Call(MathMaxMethodInfo,
								Expression.Multiply(data.DoubleValue(line.Argument3, lineNum),
									Expression.Call(MathMaxMethodInfo,
										Expression.Call(MathAbsMethodInfo, value1),
										Expression.Call(MathAbsMethodInfo, value2))),
								Expression.Constant(1.1210387714598537E-44)));
				data.Add(data.Assign(line.Argument1, Expression.Condition(comparison, Expression.Constant(1.0), Expression.Constant(0.0)), lineNum));
			};
		private static LineGenerator CompareBranchGenerator(bool isRelative, bool storeAddress, bool compareToZero, Func<Expression, Expression, BinaryExpression> comparison) =>
			(line, data, lineNum) =>
			{
				var then = MakeGotoExpression(isRelative, storeAddress, compareToZero ? line.Argument2 : line.Argument3, data, lineNum);
				var value1 = data.DoubleValue(line.Argument1, lineNum);
				var value2 = compareToZero ? Expression.Constant(0.0) : data.DoubleValue(line.Argument2, lineNum);
				data.Add(Expression.IfThen(comparison(value1, value2), then));
			};

		private static LineGenerator ApproxBranchGenerator(bool isRelative, bool storeAddress, bool compareToZero, bool equals) =>
			(line, data, lineNum) =>
			{
				var then = MakeGotoExpression(isRelative, storeAddress, compareToZero ? line.Argument3 : line.Argument4, data, lineNum);
				var value1 = data.DoubleValue(line.Argument1, lineNum);
				var value2 = compareToZero ? Expression.Constant(0.0) : data.DoubleValue(line.Argument2, lineNum);
				BinaryExpression comparison;
				if (equals)

					//Math.Abs(variableValue1 - variableValue2) <= Math.Max(variableValue3 * Math.Max(Math.Abs(variableValue1), Math.Abs(variableValue2)), 1.1210387714598537E-44)
					comparison =
						Expression.LessThanOrEqual(
							Expression.Call(MathAbsMethodInfo,
								Expression.Subtract(value1, value2)),
							Expression.Call(MathMaxMethodInfo,
								Expression.Multiply(data.DoubleValue(compareToZero ? line.Argument2 : line.Argument3, lineNum),
									Expression.Call(MathMaxMethodInfo,
										Expression.Call(MathAbsMethodInfo, value1),
										Expression.Call(MathAbsMethodInfo, value2))),
								Expression.Constant(1.1210387714598537E-44)));
				else

					//Math.Abs(variableValue1 - variableValue2) > Math.Max(variableValue3 * Math.Max(Math.Abs(variableValue1), Math.Abs(variableValue2)), 1.1210387714598537E-44)
					comparison =
						Expression.GreaterThan(
							Expression.Call(MathAbsMethodInfo,
								Expression.Subtract(value1, value2)),
							Expression.Call(MathMaxMethodInfo,
								Expression.Multiply(data.DoubleValue(compareToZero ? line.Argument2 : line.Argument3, lineNum),
									Expression.Call(MathMaxMethodInfo,
										Expression.Call(MathAbsMethodInfo, value1),
										Expression.Call(MathAbsMethodInfo, value2))),
								Expression.Constant(1.1210387714598537E-44)));
				data.Add(Expression.IfThen(comparison, then));
			};

		private static LineGenerator NanBranchGenerator(bool isRelative, bool storeAddress) =>
			(line, data, lineNum) =>
			{
				var then = MakeGotoExpression(isRelative, storeAddress, line.Argument2, data, lineNum);
				var value = data.DoubleValue(line.Argument1, lineNum);
				data.Add(Expression.IfThen(Expression.Call(DoubleIsNanMethodInfo, value), then));
			};

		private static LineGenerator DeviceBranchGenerator(bool isRelative, bool storeAddress, bool deviceExists) =>
			(line, data, lineNum) =>
			{
				var then = MakeGotoExpression(isRelative, storeAddress, line.Argument2, data, lineNum);
				var value = data.Device(line.Argument1, false, lineNum);
				data.Add(Expression.IfThen(deviceExists
					? Expression.NotEqual(value, Expression.Constant(null))
					: Expression.Equal(value, Expression.Constant(null)),
					then));
			};
		private static Expression MakeGotoExpression(bool isRelative, bool storeAddress, Variable target, CodeGeneratorData data, int lineNum)
		{
			Expression then;
			Expression gotoExpr;
			int value;
			switch (target.Kind)
			{
				case VariableKind.Constant:
					value = (int)target.Constant;
					gotoExpr = Expression.Goto(data.LineLabel(isRelative ? lineNum + value : value));
					then = storeAddress
						? Expression.Block(
							Expression.IfThenElse(Expression.LessThan(data.StackPointer(), Expression.Constant(0.0)),
								Expression.Throw(Expression.New(ProgrammableChipExceptionConstructorInfo,
									Expression.Constant(ProgrammableChipException.ICExceptionType.StackUnderFlow), Expression.Constant(lineNum))),
								Expression.IfThenElse(Expression.GreaterThanOrEqual(data.StackPointer(), Expression.Convert(data.StackSize(), typeof(double))),
									Expression.Throw(Expression.New(ProgrammableChipExceptionConstructorInfo,
										Expression.Constant(ProgrammableChipException.ICExceptionType.StackOverFlow), Expression.Constant(lineNum))),
									Expression.Assign(data.Stack(data.StackPointer()), data.LineVariable()))),
							Expression.PostIncrementAssign(data.StackPointer()),
							gotoExpr)
						: gotoExpr;
					break;
				default:
					if (storeAddress)
						then = Expression.Block(
							Expression.IfThenElse(Expression.LessThan(data.StackPointer(), Expression.Constant(0.0)),
								Expression.Throw(Expression.New(ProgrammableChipExceptionConstructorInfo,
									Expression.Constant(ProgrammableChipException.ICExceptionType.StackUnderFlow), Expression.Constant(lineNum))),
								Expression.IfThenElse(Expression.GreaterThanOrEqual(data.StackPointer(), Expression.Convert(data.StackSize(), typeof(double))),
									Expression.Throw(Expression.New(ProgrammableChipExceptionConstructorInfo,
										Expression.Constant(ProgrammableChipException.ICExceptionType.StackOverFlow), Expression.Constant(lineNum))),
									Expression.Assign(data.Stack(data.StackPointer()), data.LineVariable()))),
							Expression.PostIncrementAssign(data.StackPointer()),
							Expression.Assign(data.LineVariable(), isRelative
								? Expression.Add(Expression.Constant(lineNum), data.IntValue(target, lineNum))
								: data.IntValue(target, lineNum)),
							Expression.Goto(data.ControlSwitchLabel()));
					else
						then = Expression.Block(
							Expression.Assign(data.LineVariable(), isRelative
								? Expression.Add(Expression.Constant(lineNum), data.IntValue(target, lineNum))
								: data.IntValue(target, lineNum)),
							Expression.Goto(data.ControlSwitchLabel()));
					break;
			}
			return then;
		}
	}
}
