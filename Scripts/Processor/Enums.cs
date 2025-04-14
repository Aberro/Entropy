using System;

namespace Entropy.Assets.Scripts.Processor
{
    public partial class ChipProcessor
    {
        [Flags]
        public enum VariableKind : byte
        {
            None = 0,
            Alias = 1,
            Register = 2,
            DeviceRegister = 4,
            Constant = 8,
            Literal = 16,
            Store = Register | Alias,
            Device = DeviceRegister | Alias,
            DoubleValue = Register | Alias | Constant | Literal,
            IntValue = Register | Alias | Constant | Literal,
            Enum = Register | Alias | Constant | Literal,
        }
        [Flags]
        public enum AliasTarget
        {
            None = 0,
            Register = 1,
            Device = 2,
            Alias = 3,
        }
    }
}
