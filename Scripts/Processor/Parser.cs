#nullable enable
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Assets.Scripts.Objects.Electrical;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entropy.Scripts.Processor
{
	public class Parser
	{
		public static ChipProcessor.LineOfCode[] Parse(string code, out Dictionary<string, double> defines, out Dictionary<string, int> labels, out List<string> aliases, out List<ChipProcessor.DebugSourceLine> sourceLines)
		{
			var inputStream = new AntlrInputStream(code);
			var lexer = new MIPSLexer(inputStream);
			var tokenStream = new CommonTokenStream(lexer);
			var parser = new MIPSParser(tokenStream);
			sourceLines = new List<ChipProcessor.DebugSourceLine>();
			(defines, aliases, labels) = SymbolsCollector.Collect(parser);
			inputStream.Reset();
			parser.Reset();
			return LineParser.Parse(parser, defines, aliases, labels, sourceLines);
		}

		private class LineParser : MIPSBaseListener
		{
			internal static ChipProcessor.LineOfCode[] Parse(MIPSParser parser, Dictionary<string, double> defines, List<string> aliases, Dictionary<string, int> labels, List<ChipProcessor.DebugSourceLine> sourceLines)
			{
				var tree = parser.program();
				var lineParser = new LineParser(defines, aliases, labels, sourceLines);
				ParseTreeWalker.Default.Walk(lineParser, tree);
				return lineParser._lines.ToArray();
			}
			private readonly Dictionary<string, double> _defines;
			private readonly List<string> _aliases;
			private readonly Dictionary<string, int> _labels;
			private readonly List<ChipProcessor.DebugSourceLine> _sourceLines;
			private List<ChipProcessor.LineOfCode> _lines = new();
			private int _currentLine = -1;
			private LineParser(Dictionary<string, double> defines, List<string> aliases, Dictionary<string, int> labels, List<ChipProcessor.DebugSourceLine> sourceLines)
			{
				this._defines = defines;
				this._aliases = aliases;
				this._labels = labels;
				this._sourceLines = sourceLines;
			}

			public override void ExitLine([NotNull] MIPSParser.LineContext context)
			{
				base.ExitLine(context);
				this._currentLine++;
				var source = context.GetText();

				var instruction = context.instruction();

				if(instruction is null || instruction.define() != null)
				{
					this._sourceLines.Add(new ChipProcessor.DebugSourceLine(source));
					this._lines.Add(new ChipProcessor.LineOfCode(this._currentLine, source));
					return;
				}
				ChipProcessor.LineOfCode line;
				var alias = instruction.alias();
				var define = instruction.define();
				if (alias != null)
				{
					line = ParseAlias(alias, source);
				}
				else if(define != null)
				{
					line = new ChipProcessor.LineOfCode(this._currentLine, source, "define", new ChipProcessor.Variable
					{
						Kind = ChipProcessor.VariableKind.Constant,
						Constant =  this._defines[define.name().GetText()]
					});
					this._sourceLines.Add(new ChipProcessor.DebugSourceLine(source));
				}
				else
				{
					var sourceFormatted = new StringBuilder(source);
					var formattedArgs = new List<ChipProcessor.DebugArgument>();
					var command = instruction.operation().GetText();
					var operands = instruction.operand();
					if (operands.Length > 6)
						throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectArgumentCount, this._currentLine);
					var variables = new ChipProcessor.Variable[6];
					int formatIndex = 0;
					for(int i = 0; i < operands.Length; i++)
					{
						var operand = operands[i];
						variables[i] = ParseOperand(operand);
						if ((variables[i].Kind &
							(ChipProcessor.VariableKind.Register
							| ChipProcessor.VariableKind.Alias)) != ChipProcessor.VariableKind.None)
						{
							sourceFormatted.Insert(operand.Start.Column + operand.GetText().Length, $"{{{formatIndex}}}");
							if (variables[i].Kind == ChipProcessor.VariableKind.Register)
								formattedArgs.Add(new ChipProcessor.DebugArgument {  Target = ChipProcessor.AliasTarget.Register, Index = variables[i].RegisterIndex });
							else if (variables[i].Kind == ChipProcessor.VariableKind.Alias) formattedArgs.Add(new ChipProcessor.DebugArgument { Target = ChipProcessor.AliasTarget.Alias, Index = variables[i].AliasIndex });
						}
					}
					for(int i = operands.Length; i < 6; i++) variables[i] = ChipProcessor.Variable.None;
					line = new ChipProcessor.LineOfCode(this._currentLine, sourceFormatted.ToString(), command, variables[0], variables[1], variables[2], variables[3], variables[4], variables[5]);
					this._sourceLines.Add(new ChipProcessor.DebugSourceLine(source, formattedArgs.ToArray()));
				}
				this._lines.Add(line);
			}

			private ChipProcessor.Variable ParseOperand(MIPSParser.OperandContext context)
			{
				var register = context.register();
				var device = context.device();
				var hash = context.hash();
				var number = context.NUMBER();
				var identifier = context.IDENTIFIER();
				if(register != null)
					return ParseRegister(register);
				if(device != null)
					return ParseDevice(device);
				if (hash != null)
					return new ChipProcessor.Variable
					{
						Kind = ChipProcessor.VariableKind.Constant,
						Constant = UnityEngine.Animator.StringToHash(hash.IDENTIFIER().GetText()),
					};
				if (number != null)
				{
					if(!TryParseNumber(number.GetText(), out var value)) throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectVariable, this._currentLine);
					return new ChipProcessor.Variable
					{
						Kind = ChipProcessor.VariableKind.Constant,
						Constant = value,
					};
				}
				if(identifier != null)
				{
					var name = identifier.GetText();
					var aliasIndex = this._aliases.IndexOf(name);
					var isLabel = this._labels.TryGetValue(name, out int label);
					var isDefine = this._defines.TryGetValue(name, out double define);
					if(aliasIndex >= 0)
						return new ChipProcessor.Variable
						{
							Kind = ChipProcessor.VariableKind.Alias,
							AliasIndex = aliasIndex,
						};
					else if(isLabel)
						return new ChipProcessor.Variable
						{
							Kind = ChipProcessor.VariableKind.Constant,
							Constant = label,
						};
					else if(isDefine)
						return new ChipProcessor.Variable
						{
							Kind = ChipProcessor.VariableKind.Constant,
							Constant = define,
						};
					else

						// This is either an enum or undefined value
						return new ChipProcessor.Variable
						{
							Kind = ChipProcessor.VariableKind.Literal,
							Literal = name,
						};
				}
				throw new ApplicationException("Unexpected token kind!");
			}
			private ChipProcessor.LineOfCode ParseAlias(MIPSParser.AliasContext alias, string source)
			{
				var name = alias.name().GetText();
				var aliasIndex = this._aliases.IndexOf(name);
				var target = alias.alias_target();
				ChipProcessor.Variable variable;
				var identifier = target.IDENTIFIER();
				if (identifier != null)
				{
					var identifierText = identifier.GetText();
					var targetIndex = this._aliases.IndexOf(identifierText);
					if (aliasIndex < 0) throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectVariable, this._currentLine);
					variable = new ChipProcessor.Variable
					{
						Kind = ChipProcessor.VariableKind.Alias,
						AliasIndex = aliasIndex,
						Constant = targetIndex
					};
				}
				else
				{
					var register = target.register();
					var device = target.device();
					if (register != null)
					{
						variable = ParseRegister(register);
						variable.AliasIndex = aliasIndex;
					}
					else if (device != null)
					{
						variable = ParseDevice(device);
						variable.AliasIndex = aliasIndex;
					}
					else
					{
						throw new ApplicationException("Unexpected alias target kind!");
					}
				}
				return new ChipProcessor.LineOfCode(this._currentLine, source, "alias", variable);
			}

			private ChipProcessor.Variable ParseRegister(MIPSParser.RegisterContext context)
			{
				// r0 - index = 0, recurse = 0
				// rr0 - index = 0, recurse = 1
				// rrr1 - index = 1, recurse = 2
				var text = context.GetText();

				// Calculate index and recurse
				var recurse = text.LastIndexOf("r", StringComparison.InvariantCultureIgnoreCase);
				var lastRegister = text.Substring(recurse + 1);
				int index;
				if(lastRegister.StartsWith("sp"))
					index = 16;
				else if (lastRegister.StartsWith("a"))
					index = 17;
				else
					index = int.Parse(lastRegister);

				return new ChipProcessor.Variable
				{
					Kind = ChipProcessor.VariableKind.Register,
					RegisterIndex = index,
					RegisterRecurse = recurse,
				};
			}
			private ChipProcessor.Variable ParseDevice(MIPSParser.DeviceContext context)
			{
				// db - index = int.MaxValue, recurse = 0, network = int.MinValue
				// db:1 - index = int.MaxValue, recurse = 0, network = 1
				// d0 - index = 0, recurse = 0, network = int.MinValue
				// dr0 - index = 0, recurse = 1, network = int.MinValue
				// drr1 - index = 1, recurse = 2, network = int.MinValue
				// d0:0 - index = 0, recurse = 0, network = 0
				// d0:1 - index = 0, recurse = 0, network = 1
				// dr1:2 - index = 1, recurse = 1, network = 2
				var text = context.GetText();

				// Calculate index, recurse and network
				var recurse = text.LastIndexOf('r');
				recurse = recurse != -1 ? recurse : 0;
				var colonIndex = text.IndexOf(':');
				int index;
				int network = int.MinValue;
				if (text.StartsWith("db"))
				{
					if (colonIndex >= 0)
						network = int.Parse(text.Substring(colonIndex + 1));
					index = int.MaxValue;
				}
				else if (text.StartsWith("d"))
				{
					if(colonIndex >= 0)
						network = int.Parse(text.Substring(colonIndex + 1));
					index = int.Parse(text.Substring(recurse + 1));
				}
				else
				{
					throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectVariable, this._currentLine);
				}

				return new ChipProcessor.Variable
				{
					Kind = ChipProcessor.VariableKind.DeviceRegister,
					RegisterIndex = index,
					RegisterRecurse = recurse,
					NetworkIndex = network
				};
			}
		}

		private class SymbolsCollector : MIPSBaseListener
		{
			public static (Dictionary<string, double> defines, List<string> aliases, Dictionary<string, int> labels) Collect(MIPSParser parser)
			{
				var tree = parser.program();
				var collector = new SymbolsCollector();
				ParseTreeWalker.Default.Walk(collector, tree);
				return (collector._defines, collector._aliases, collector._labels);
			}
			private SymbolsCollector() { }
			private readonly Dictionary<string, double> _defines = new();
			private readonly List<string> _aliases = new();
			private readonly Dictionary<string, int> _labels = new();
			private int _currentLine = -1;

			public override void ExitLine([NotNull] MIPSParser.LineContext context)
			{
				base.ExitLine(context);
				this._currentLine++;

				if (context.label() != null)
				{
					var label = context.label().name().GetText();
					if (this._labels.ContainsKey(label) || this._defines.ContainsKey(label) || this._aliases.Contains(label))
						throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.JumpTagDuplicate, this._currentLine);
					this._labels.Add(label, this._currentLine);
				}

				var instruction = context.instruction();
				if (instruction is null) return;

				var define = instruction.define();
				if (define != null)
				{
					var name = define.name().GetText();
					var identifier = define.define_target().hash()?.IDENTIFIER()?.GetText();
					var value = define.define_target().GetText();
					double number = double.NaN;

					if (identifier is not null)
					{
						UnityEngine.Animator.StringToHash(identifier);
					}
					else
					{
						if (!TryParseNumber(value, out number))
							throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectVariable, this._currentLine);
					}
					if (this._labels.ContainsKey(name) || this._defines.ContainsKey(name) || this._aliases.Contains(name))
						throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.ExtraDefine, this._currentLine);
					this._defines.Add(name, number);
				}

				var alias = instruction.alias();
				if(alias != null)
				{
					var name = alias.name().GetText();
					if (this._labels.ContainsKey(name) || this._defines.ContainsKey(name) || this._aliases.Contains(name))
						throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectVariable, this._currentLine);
					this._aliases.Add(name);
				}
			}
		}

		private static bool TryParseNumber(string text, out double result)
		{
			if (text.StartsWith("0x"))
				try
				{
					result = Convert.ToInt32(text.Substring(2), 16);
					return true;
				}
				catch (FormatException)
				{
					result = 0;
					return false;
				}
			if (text.StartsWith("$"))
				try
				{
					result = Convert.ToInt32(text.Substring(1), 16);
					return true;
				}
				catch (FormatException)
				{
					result = 0;
					return false;
				}
			if (text.StartsWith("0b"))
				try
				{
					result = Convert.ToInt32(text.Substring(2), 2);
					return true;
				}
				catch (FormatException)
				{
					result = 0;
					return false;
				}
			return double.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result);
		}
	}
}
