#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value 'value'
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImGuiNET;
using System.Runtime.InteropServices;
using UnityEngine;
using ImGuiID = uint;
using ImGuiKeyChord = short;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiBoxSelectState
{
	private ImGuiID _id;
	[MarshalAs(UnmanagedType.U1)]
	private bool _isActive;
	[MarshalAs(UnmanagedType.U1)]
	private bool _isStarting;
	[MarshalAs(UnmanagedType.U1)]
	private bool _isStartedFromVoid;
	[MarshalAs(UnmanagedType.U1)]
	private bool _isStartedSetNavIdOnce;
	[MarshalAs(UnmanagedType.U1)]
	private bool _requestClear;
	private ImGuiKeyChord _keyMods;
	private ImVec2 _startPosRel;
	private ImVec2 _endPosRel;
	private ImVec2 _scrollAccum;
	private ImGuiWindow* _window;
	[MarshalAs(UnmanagedType.U1)]
	private bool _unclipMode;
	private ImRect _unclipRect;
	private ImRect _boxSelectRectPrev;
	private ImRect _boxSelectRectCurr;

	// Active box-selection data (persistent, 1 active at a time)
	public ref ImGuiID ID => ref this._id;

	public ref bool IsActive => ref this._isActive;

	public ref bool IsStarting => ref this._isStarting;

	/// <summary>
	/// Starting click was not from an item.
	/// </summary>
	public ref bool IsStartedFromVoid => ref this._isStartedFromVoid;

	public ref bool IsStartedSetNavIdOnce => ref this._isStartedSetNavIdOnce;

	public ref bool RequestClear => ref this._requestClear;

	/// <summary>
	/// Latched key-mods for box-select logic.
	/// </summary>
	public ref ImGuiKeyChord KeyMods => ref this._keyMods;

	/// <summary>
	/// Start position in window-contents relative space (to support scrolling)
	/// </summary>
	public ref ImVec2 StartPosRel => ref this._startPosRel;

	/// <summary>
	/// End position in window-contents relative space
	/// </summary>
	public ref ImVec2 EndPosRel => ref this._endPosRel;

	/// <summary>
	/// Scrolling accumulator (to behave at high-frame spaces)
	/// </summary>
	public ref ImVec2 ScrollAccum => ref this._scrollAccum;

	public ref ImGuiWindow Window => ref *this._window;

	// Temporary/Transient data
	/// <summary>
	/// (Temp/Transient, here in hot area). Set/cleared by the BeginMultiSelect()/EndMultiSelect() owning active box-select.
	/// </summary>
	public ref bool UnclipMode => ref this._unclipMode;

	/// <summary>
	/// Rectangle where ItemAdd() clipping may be temporarily disabled. Need support by multi-select supporting widgets.
	/// </summary>
	public ref ImRect UnclipRect => ref this._unclipRect;

	/// <summary>
	/// Selection rectangle in absolute coordinates (derived every frame from BoxSelectStartPosRel and MousePos)
	/// </summary>
	public ref ImRect BoxSelectRectPrev => ref this._boxSelectRectPrev;
	
	public ref ImRect BoxSelectRectCurr => ref this._boxSelectRectCurr;
}