#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiTextIndex
{
	private ImVector<int> _lineOffsets;
	private int _endOffset;
	public ref ImVector<int> LineOffsets => ref this._lineOffsets;
	public ref int EndOffset => ref this._endOffset;
}