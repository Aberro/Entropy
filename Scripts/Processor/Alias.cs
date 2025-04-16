#nullable enable
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Pipes;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Entropy.Scripts.Processor
{
    public partial class ChipProcessor
    {
        public struct Alias
		{
			public AliasTarget Kind;
			public int Index;
			//public short DefineIndex;
			public int Recurse;
            public int NetworkIndex;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
            public double GetValue(ChipProcessor processor, int lineNum) =>
	            this.Kind switch
                {
	                AliasTarget.Register => processor.Registers[TraverseReferences(processor, this.Index, this.Recurse)],
	                AliasTarget.Alias => processor.Aliases[this.Index].GetValue(processor, lineNum),
	                _ => throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectVariableType, lineNum),
                };

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetValue(ChipProcessor processor, double value, int lineNum)
            {
                switch (this.Kind)
                {
                    case AliasTarget.Register:
                        processor.Registers[TraverseReferences(processor, this.Index, this.Recurse)] = value;
                	    break;
                    case AliasTarget.Alias:
                        processor.Aliases[this.Index].SetValue(processor, value, lineNum);
					    break;
                    default:
                	    throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectVariableType, lineNum);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ILogicable GetDevice(ChipProcessor processor, int lineNum) =>
                this.Kind switch
                {
	                AliasTarget.Device => processor.GetDevice(TraverseReferences(processor, this.Index, this.Recurse), this.NetworkIndex, true, lineNum)
                        ?? throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.DeviceNotFound, lineNum),
	                AliasTarget.Alias => processor.Aliases[this.Index].GetDevice(processor, lineNum),
                    _ => throw new ProgrammableChipException(ProgrammableChipException.ICExceptionType.IncorrectVariableType, lineNum),
                };

			private int TraverseReferences(ChipProcessor processor, int registerIndex, int registerRecurse)
			{
				while (registerRecurse-- > 0)
					registerIndex = (int)processor.Registers[registerIndex];
				return registerIndex;
			}
		}

		private static readonly MethodInfo AliasGetValueMethodInfo = typeof(Alias).GetMethod(nameof(Alias.GetValue))!;
        private static readonly MethodInfo AliasSetValueMethodInfo = typeof(Alias).GetMethod(nameof(Alias.SetValue))!;
        private static readonly MethodInfo AliasGetDeviceMethodInfo = typeof(Alias).GetMethod(nameof(Alias.GetDevice))!;
    }
}
