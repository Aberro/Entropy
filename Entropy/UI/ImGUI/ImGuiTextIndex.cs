#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Entropy.UI.ImGUI;

public unsafe struct ImGuiTextIndex
{
	public ImVector<int> LineOffsets;
	public int EndOffset = 0; // Because we don't own text buffer we need to maintain EndOffset (may bake in LineOffsets?)

	public ImGuiTextIndex() { }
}