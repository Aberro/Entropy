#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImGuiNET;
using System.Runtime.InteropServices;
using ImU32 = uint;

namespace Entropy.Common.UI.ImGUI;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct ImGuiInputEvent
{
	[FieldOffset(0)] private ImGuiInputEventType _type;
	[FieldOffset(4)] private ImGuiInputSource _source;
	[FieldOffset(8)] private ImU32 _eventId;
	[FieldOffset(12)] private ImGuiInputEventMousePos _mousePos;
	[FieldOffset(12)] private ImGuiInputEventMouseWheel _mouseWheel;
	[FieldOffset(12)] private ImGuiInputEventMouseButton _mouseButton;
	[FieldOffset(12)] private ImGuiInputEventKey _key;
	[FieldOffset(12)] private ImGuiInputEventText _text;
	[FieldOffset(12)] private ImGuiInputEventAppFocused _appFocused;
	// Next field after union
	// Adjust offset to be after the largest union member
	// actual offset depends on union size and alignment
	[FieldOffset(24)] private bool _addedByTestEngine;

	public ref ImGuiInputEventType Type => ref this._type;
	public ref ImGuiInputSource Source => ref this._source;
	public ref ImU32 EventId => ref this._eventId;

	public ref ImGuiInputEventMousePos MousePos => ref this._mousePos;
	public ref ImGuiInputEventMouseWheel MouseWheel => ref this._mouseWheel;
	public ref ImGuiInputEventMouseButton MouseButton => ref this._mouseButton;
	public ref ImGuiInputEventKey Key => ref this._key;
	public ref ImGuiInputEventText Text => ref this._text;
	public ref ImGuiInputEventAppFocused AppFocused => ref this._appFocused;
	public ref bool AddedByTestEngine => ref this._addedByTestEngine;
}