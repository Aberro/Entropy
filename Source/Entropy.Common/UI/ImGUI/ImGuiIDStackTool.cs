#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImGuiNET;
using System.Runtime.InteropServices;
using ImGuiID = uint;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiIDStackTool()
{
	private int _lastActiveFrame;
	private int _stackLevel;
	private ImGuiID _queryId;
	private ImVector<ImGuiStackLevelInfo> _results;
	[MarshalAs(UnmanagedType.U1)]
	private bool _copyToClipboardOnCtrlC;
	private float _copyToClipboardLastTime = -float.MinValue;
	private ImGuiTextBuffer _resultPathBuf;

	public ref int LastActiveFrame => ref this._lastActiveFrame;
	/// <summary>
	/// -1: query stack and resize Results, >= 0: individual stack level
	/// </summary>
	public ref int StackLevel => ref this._stackLevel;
	/// <summary>
	/// ID to query details for
	/// </summary>
	public ref ImGuiID QueryId => ref this._queryId;
	public ref ImVector<ImGuiStackLevelInfo> Results => ref this._results;
	public ref bool CopyToClipboardOnCtrlC => ref this._copyToClipboardOnCtrlC;
	public ref float CopyToClipboardLastTime => ref this._copyToClipboardLastTime;
	public ref ImGuiTextBuffer ResultPathBuf => ref this._resultPathBuf;
}