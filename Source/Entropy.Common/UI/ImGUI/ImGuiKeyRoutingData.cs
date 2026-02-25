#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImGuiKeyRoutingIndex = short;
using ImGuiID = uint;
using ImU16 = ushort;
using ImU8 = byte;

namespace Entropy.Common.UI.ImGUI;



public unsafe struct ImGuiKeyRoutingData
{
	private ImGuiKeyRoutingIndex _nextEntryIndex;
	private ImU16 _mods;
	private ImU8 _routingCurrScore;
	private ImU8 _routingNextScore;
	private ImGuiID _routingCurr;
	private ImGuiID _routingNext;

	public ref ImGuiKeyRoutingIndex NextEntryIndex => ref this._nextEntryIndex;
	/// <summary>
	/// Technically we'd only need 4-bits but for simplify we store ImGuiMod_ values which need 16-bits.
	/// </summary>
	public ref ImU16 Mods => ref this._mods;
	/// <summary>
	/// [DEBUG] For debug display
	/// </summary>
	public ref ImU8 RoutingCurrScore => ref this._routingCurrScore;
	/// <summary>
	/// Lower is better (0: perfect score)
	/// </summary>
	public ref ImU8 RoutingNextScore => ref this._routingNextScore;
	public ref ImGuiID RoutingCurr => ref this._routingCurr;
	public ref ImGuiID RoutingNext => ref this._routingNext;

	public ImGuiKeyRoutingData()
	{
		this._nextEntryIndex = -1;
		this._mods = 0;
		this._routingCurrScore = this._routingNextScore = 255;
		this._routingCurr = this._routingNext = ImGuiKeyOwnerData.NoOwner;
	}
}