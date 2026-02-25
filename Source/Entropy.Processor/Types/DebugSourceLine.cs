namespace Entropy.Processor.Types;

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
		for (var i = 0; i < arguments.Length; i++)
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