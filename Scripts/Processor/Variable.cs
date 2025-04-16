namespace Entropy.Scripts.Processor
{
    public partial class ChipProcessor
    {
        public struct Variable
        {
            public VariableKind Kind;
            public int RegisterIndex;
            public int RegisterRecurse;
            public int AliasIndex;
            //public short DefineIndex;
            public int NetworkIndex;
            public double Constant;
            public string Literal;
            public Variable(int _)
            {
	            this.Kind = VariableKind.None;
	            this.RegisterIndex = -1;
	            this.RegisterRecurse = -1;
	            this.AliasIndex = -1;
                //DefineIndex = -1;
                this.NetworkIndex = int.MinValue;
                this.Constant = double.NaN;
                this.Literal = null;
            }
            public static Variable None => new(0);

            public static bool operator ==(Variable a, Variable b)
            {
                return a.Kind == b.Kind
                       && a.RegisterIndex == b.RegisterIndex
                       && a.RegisterRecurse == b.RegisterRecurse
                       && a.AliasIndex == b.AliasIndex
                       //&& a.DefineIndex == b.DefineIndex
                       && a.NetworkIndex == b.NetworkIndex
                       && (double.IsNaN(a.Constant) && double.IsNaN(b.Constant) || a.Constant == b.Constant)
                       && a.Literal == b.Literal;
            }

            public static bool operator !=(Variable a, Variable b)
            {
                return a.Kind != b.Kind
                       || a.RegisterIndex != b.RegisterIndex
                       || a.RegisterRecurse != b.RegisterRecurse
                       || a.AliasIndex != b.AliasIndex
                       //|| a.DefineIndex != b.DefineIndex
                       || a.NetworkIndex != b.NetworkIndex
                       || (!(double.IsNaN(a.Constant) && double.IsNaN(b.Constant)) && a.Constant != b.Constant)
                       || a.Literal != b.Literal;
            }

            public override bool Equals(object obj)
            {
                return obj is Variable variable && this == variable;
            }
            public override int GetHashCode()
            {
                return this.Kind.GetHashCode()
                    ^ this.RegisterIndex.GetHashCode()
                    ^ this.RegisterRecurse.GetHashCode()
                    ^ this.AliasIndex.GetHashCode()
                    //^ DefineIndex.GetHashCode()
                    ^ this.NetworkIndex.GetHashCode()
                    ^ this.Constant.GetHashCode()
                    ^ this.Literal.GetHashCode();
            }
        }
    }
}
