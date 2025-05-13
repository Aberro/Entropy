#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImGuiNET;
using ImGuiKeyRoutingIndex = short;
using ImGuiID = uint;
using ImU16 = ushort;
using ImU8 = byte;

namespace Entropy.UI.ImGUI;

public unsafe struct ImGuiKeyRoutingTable
{
	public fixed ImGuiKeyRoutingIndex Index[(int)ImGuiKey.NamedKey_COUNT]; // Index of first entry in Entries[]
	public ImVector<ImGuiKeyRoutingData> Entries;
	public ImVector<ImGuiKeyRoutingData> EntriesNext; // Double-buffer to avoid reallocation (could use a shared buffer)

	public ImGuiKeyRoutingTable()
	{
		for(var n = 0; n < (int)ImGuiKey.NamedKey_COUNT; n++)
			this.Index[n] = -1;
	}
}

public struct ImGuiKeyRoutingData
{
	public ImGuiKeyRoutingIndex NextEntryIndex;
	public ImU16 Mods; // Technically we'd only need 4-bits but for simplify we store ImGuiMod_ values which need 16-bits.
	public ImU8 RoutingCurrScore; // [DEBUG] For debug display
	public ImU8 RoutingNextScore; // Lower is better (0: perfect score)
	public ImGuiID RoutingCurr;
	public ImGuiID RoutingNext;

	public ImGuiKeyRoutingData()
	{
		this.NextEntryIndex = -1;
		this.Mods = 0;
		this.RoutingCurrScore = this.RoutingNextScore = 255;
		this.RoutingCurr = this.RoutingNext = ImGuiKeyOwnerData.NoOwner;
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member