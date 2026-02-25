#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types

using ImGuiNET;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImDrawListSplitter
{
	private int _current;
	private int _count;
	private ImVector<ImDrawChannel> _channels;

	/// <summary>
	/// Current channel number (0)
	/// </summary>
	public ref int Current => ref this._current;
	/// <summary>
	/// Number of active channels (1+)
	/// </summary>
	public ref int Count => ref this._count;
	/// <summary>
	/// Draw channels (not resized down so _Count might be < Channels.Size)
	/// </summary>
	public ref ImVector<ImDrawChannel> Channels => ref this._channels;

	/// <summary>
	/// Do not clear Channels[] so our allocations are reused next frame
	/// </summary>
	public void Clear() 
	{
		Current = 0;
		Count = 1;
	}
	public void ClearFreeMemory()
		=> ImDrawListSplitter_ClearFreeMemory(ref this);
	public unsafe void Split(ref ImDrawList draw_list, int count)
	{
		fixed(ImDrawList* draw_list_ptr = &draw_list)
		{
			ImDrawListSplitter_Split(ref this, draw_list_ptr, count);
		}
	}
	public void Merge(ref ImDrawList draw_list)
	{
		fixed(ImDrawList* draw_list_ptr = &draw_list)
		{
			ImDrawListSplitter_Merge(ref this, draw_list_ptr);
		}
	}
	public void SetCurrentChannel(ref ImDrawList draw_list, int channel_idx)
	{
		fixed(ImDrawList* draw_list_ptr = &draw_list)
		{
			ImDrawListSplitter_SetCurrentChannel(ref this, draw_list_ptr, channel_idx);
		}
	}
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawListSplitter_ClearFreeMemory(ref ImDrawListSplitter splitter);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawListSplitter_Split(ref ImDrawListSplitter splitter, ImDrawList* draw_list, int count);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawListSplitter_Merge(ref ImDrawListSplitter splitter, ImDrawList* draw_list);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImDrawListSplitter_SetCurrentChannel(ref ImDrawListSplitter splitter, ImDrawList* draw_list, int channel_idx);
}