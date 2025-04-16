namespace Entropy.Scripts.Processor
{
    public partial class ChipProcessor
    {
        /// <summary>
        /// A delegate for generated processor code.
        /// </summary>
        /// <param name="line">The line number at which the execution should start.</param>
        /// <param name="processor">The processor that executes the code in the game.</param>
        /// <param name="registers">The registers array of the processor.</param>
        /// <param name="aliases">The array of aliases used by the code.</param>
        /// <param name="operations">The number of operations to execute.</param>
        /// <returns></returns>
        public delegate int ProcessorCode(int line, ChipProcessor processor, double[] registers, Alias[] aliases, int operations);

        public delegate void LineGenerator(LineOfCode line, CodeGeneratorData data, int lineNumber);
    }
}
