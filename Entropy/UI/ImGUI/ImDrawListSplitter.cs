#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
using ImGuiNET;
using System.Runtime.InteropServices;

namespace Entropy.UI.ImGUI;

public unsafe struct ImDrawListSplitter
{
	public int _Current;    // Current channel number (0)
	public int _Count;      // Number of active channels (1+)
	public ImVector<ImDrawChannel> _Channels;   // Draw channels (not resized down so _Count might be < Channels.Size)

	/// <summary>
	/// Do not clear Channels[] so our allocations are reused next frame
	/// </summary>
	public void Clear() 
	{
		this._Current = 0;
		this._Count = 1;
	}
	public void ClearFreeMemory()
		=> ImDrawListSplitter_ClearFreeMemory(this);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawListSplitter_ClearFreeMemory(in ImDrawListSplitter splitter);
	public unsafe void Split(in ImDrawList draw_list, int count)
	{
		fixed(ImDrawList* draw_list_ptr = &draw_list)
		{
			ImDrawListSplitter_Split(this, draw_list_ptr, count);
		}
	}
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawListSplitter_Split(in ImDrawListSplitter splitter, ImDrawList* draw_list, int count);
	public void Merge(in ImDrawList draw_list)
	{
		fixed(ImDrawList* draw_list_ptr = &draw_list)
		{
			ImDrawListSplitter_Merge(this, draw_list_ptr);
		}
	}
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawListSplitter_Merge(in ImDrawListSplitter splitter, ImDrawList* draw_list);
	public void SetCurrentChannel(in ImDrawList draw_list, int channel_idx)
	{
		fixed(ImDrawList* draw_list_ptr = &draw_list)
		{
			ImDrawListSplitter_SetCurrentChannel(this, draw_list_ptr, channel_idx);
		}
	}
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImDrawListSplitter_SetCurrentChannel(in ImDrawListSplitter splitter, ImDrawList* draw_list, int channel_idx);
}