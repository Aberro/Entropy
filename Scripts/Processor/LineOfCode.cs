namespace Entropy.Scripts.Processor
{
    public partial class ChipProcessor
    {
        public struct LineOfCode
        {
            public int SourceLine;
            public string Source;
            public readonly string Command;
            public readonly Variable Argument1;
            public readonly Variable Argument2;
            public readonly Variable Argument3;
            public readonly Variable Argument4;
            public readonly Variable Argument5;
            public readonly Variable Argument6;

            public static LineOfCode Empty = new(0, null);
            public LineOfCode(int line, string source)
            {
	            this.SourceLine = line;
	            this.Source  = source;
	            this.Command = null;
	            this.Argument1 = Variable.None;
	            this.Argument2 = Variable.None;
	            this.Argument3 = Variable.None;
	            this.Argument4 = Variable.None;
	            this.Argument5 = Variable.None;
	            this.Argument6 = Variable.None;
            }
            public LineOfCode(int line, string source, string command) : this(line, source, command, Variable.None, Variable.None, Variable.None, Variable.None, Variable.None, Variable.None) { }
            public LineOfCode(int line, string source, string command, Variable var1) : this(line, source, command, var1, Variable.None, Variable.None, Variable.None, Variable.None, Variable.None) { }
            public LineOfCode(int line, string source, string command, Variable var1, Variable var2) : this(line, source, command, var1, var2, Variable.None, Variable.None, Variable.None, Variable.None) { }
            public LineOfCode(int line, string source, string command, Variable var1, Variable var2, Variable var3) : this(line, source, command, var1, var2, var3, Variable.None, Variable.None, Variable.None) { }
            public LineOfCode(int line, string source, string command, Variable var1, Variable var2, Variable var3, Variable var4) : this(line, source, command, var1, var2, var3, var4, Variable.None, Variable.None) { }
            public LineOfCode(int line, string source, string command, Variable var1, Variable var2, Variable var3, Variable var4, Variable var5) : this(line, source, command, var1, var2, var3, var4, var5, Variable.None) { }
            public LineOfCode(int line, string source, string command, Variable var1, Variable var2, Variable var3, Variable var4, Variable var5, Variable var6)
            {
	            this.Command = command;
	            this.Argument1 = var1;
	            this.Argument2 = var2;
	            this.Argument3 = var3;
	            this.Argument4 = var4;
	            this.Argument5 = var5;
	            this.Argument6 = var6;
	            this.SourceLine = line;
	            this.Source = source;
            }

            public static bool operator ==(LineOfCode a, LineOfCode b)
            {
                return a.Command == b.Command && a.Argument1 == b.Argument1 && a.Argument2 == b.Argument2 && a.Argument3 == b.Argument3 && a.Argument4 == b.Argument4 && a.Argument5 == b.Argument5 && a.Argument6 == b.Argument6;
            }

            public static bool operator !=(LineOfCode a, LineOfCode b)
            {
                return a.Command != b.Command || a.Argument1 != b.Argument1 || a.Argument2 != b.Argument2 || a.Argument3 != b.Argument3 || a.Argument4 != b.Argument4 || a.Argument5 != b.Argument5 || a.Argument6 != b.Argument6;
            }

            public override bool Equals(object obj)
            {
                return obj is LineOfCode code && code == this;
            }
            public override int GetHashCode()
            {
                return this.Command.GetHashCode()
                    ^ this.Argument1.GetHashCode()
                    ^ this.Argument2.GetHashCode()
                    ^ this.Argument3.GetHashCode()
                    ^ this.Argument4.GetHashCode()
                    ^ this.Argument5.GetHashCode()
                    ^ this.Argument6.GetHashCode();
            }
        }
    }
}
