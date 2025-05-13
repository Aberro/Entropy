#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImS16 = short;

namespace Entropy.UI.ImGUI;

public unsafe struct ImGuiDebugAllocInfo
{
	const int SIZEOF_IMGUI_DEBUG_ALLOC_ENTRY = 8; // 6 shorts
	public int TotalAllocCount; // Number of call to MemAlloc().
	public int TotalFreeCount;
	public ImS16 LastEntriesIdx; // Current index in buffer
	public fixed byte LastEntriesBuf[6 * SIZEOF_IMGUI_DEBUG_ALLOC_ENTRY]; // Track last 6 frames that had allocations
}

public struct ImGuiDebugAllocEntry
{
	public int FrameCount;
	public ImS16 AllocCount;
	public ImS16 FreeCount;
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member