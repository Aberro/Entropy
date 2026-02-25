#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImGuiNET;
using ImGuiKeyRoutingIndex = short;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiKeyRoutingTable
{
	private fixed ImGuiKeyRoutingIndex _index[(int)ImGuiKey.NamedKey_COUNT];
	private ImVector<ImGuiKeyRoutingData> _entries;
	private ImVector<ImGuiKeyRoutingData> _entriesNext;

	/// <summary>
	/// Index of first entry in Entries[]
	/// </summary>
	public Span<ImGuiKeyRoutingIndex> Index
	{
		get
		{
			fixed (ImGuiKeyRoutingIndex* ptr = this._index)
			{
				return new Span<ImGuiKeyRoutingIndex>(ptr, (int)ImGuiKey.NamedKey_COUNT);
			}
		}
	}
	public ref ImVector<ImGuiKeyRoutingData> Entries => ref this._entries;
	/// <summary>
	/// Double-buffer to avoid reallocation (could use a shared buffer)
	/// </summary>
	public ref ImVector<ImGuiKeyRoutingData> EntriesNext => ref this._entriesNext;

	public ImGuiKeyRoutingTable()
	{
		for(var n = 0; n < (int)ImGuiKey.NamedKey_COUNT; n++)
			this._index[n] = -1;
	}
}