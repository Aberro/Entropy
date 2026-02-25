#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImS16 = short;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiDebugAllocEntry
{
	private int _frameCount;
	private ImS16 _allocCount;
	private ImS16 _freeCount;

	public ref int FrameCount => ref this._frameCount;
	public ref ImS16 AllocCount => ref this._allocCount;
	public ref ImS16 FreeCount => ref this._freeCount;
}