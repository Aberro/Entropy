#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImS16 = short;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiDebugAllocInfo
{
	private const int SIZEOF_IMGUI_DEBUG_ALLOC_ENTRY = 8; // 6 shorts
	private int _totalAllocCount; // Number of call to MemAlloc().
	private int _totalFreeCount;
	private ImS16 _lastEntriesIdx; // Current index in buffer
	private fixed byte _lastEntriesBuf[6 * SIZEOF_IMGUI_DEBUG_ALLOC_ENTRY]; // Track last 6 frames that had allocations

	/// <summary>
	/// Number of call to MemAlloc().
	/// </summary>
	public ref int TotalAllocCount => ref this._totalAllocCount;
	public ref int TotalFreeCount => ref this._totalFreeCount;
	/// <summary>
	/// Current index in buffer
	/// </summary>
	public ref ImS16 LastEntriesIdx => ref this._lastEntriesIdx;
	/// <summary>
	///  Track last 6 frames that had allocations
	/// </summary>
	public Span<byte> LastEntriesBuf
	{
		get
		{
			fixed (byte* ptr = this._lastEntriesBuf)
			{
				return new Span<byte>(ptr, 6 * SIZEOF_IMGUI_DEBUG_ALLOC_ENTRY);
			}
		}
	}
}

