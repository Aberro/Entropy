#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value 'value'
#pragma warning disable CA1815 // Override equals and operator equals on value types
using System.Runtime.InteropServices;
using UnityEngine;
using ImGuiID = uint;
using ImGuiKeyChord = int;
using ImS8 = sbyte;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiMultiSelectTempData
{
	private ImGuiMultiSelectIO _io;
	private ImGuiMultiSelectState* _storage;
	private ImGuiID _focusScopeId;
	private ImGuiMultiSelectFlags _flags;
	private ImVec2 _scopeRectMin;
	private ImVec2 _backupCursorMaxPos;

	private ImGuiSelectionUserData _lastSubmittedItem;
	private ImGuiID _boxSelectId;
	private ImGuiKeyChord _keyMods;
	private ImS8 _loopRequestSetAll;
	[MarshalAs(UnmanagedType.U1)]
	private bool _isEndIO;
	[MarshalAs(UnmanagedType.U1)]
	private bool _isFocused;
	[MarshalAs(UnmanagedType.U1)]
	private bool _isKeyboardSetRange;
	[MarshalAs(UnmanagedType.U1)]
	private bool _navIdPassedBy;
	[MarshalAs(UnmanagedType.U1)]
	private bool _rangeSrcPassedBy;
	[MarshalAs(UnmanagedType.U1)]
	private bool _rangeDstPassedBy;

	/// <summary>
	/// Requests are set and returned by BeginMultiSelect()/EndMultiSelect() + written to by user during the loop.
	/// </summary>
	public ref ImGuiMultiSelectIO IO => ref this._io;
	public ref ImGuiMultiSelectState Storage => ref *this._storage;
	/// <summary>
	/// Copied from g.CurrentFocusScopeId (unless another selection scope was pushed manually)
	/// </summary>
	public ref ImGuiID FocusScopeId => ref this._focusScopeId;
	public ref ImGuiMultiSelectFlags Flags => ref this._flags;
	public ref ImVec2 ScopeRectMin => ref this._scopeRectMin;
	public ref ImVec2 BackupCursorMaxPos => ref this._backupCursorMaxPos;
	/// <summary>
	/// Copy of last submitted item data, used to merge output ranges.
	/// </summary>
	public ref ImGuiSelectionUserData LastSubmittedItem => ref this._lastSubmittedItem;
	public ref ImGuiID BoxSelectId => ref this._boxSelectId;
	public ref ImGuiKeyChord KeyMods => ref this._keyMods;
	/// <summary>
	/// -1: no operation, 0: clear all, 1: select all.
	/// </summary>
	public ref ImS8 LoopRequestSetAll => ref this._loopRequestSetAll;
	/// <summary>
	/// Set when switching IO from BeginMultiSelect() to EndMultiSelect() state.
	/// </summary>
	public ref bool IsEndIO => ref this._isEndIO;
	/// <summary>
	/// Set if currently focusing the selection scope (any item of the selection). May be used if you have custom shortcut associated to selection.
	/// </summary>
	public ref bool IsFocused => ref this._isFocused;
	/// <summary>
	/// Set by BeginMultiSelect() when using Shift+Navigation. Because scrolling may be affected we can't afford a frame of lag with Shift+Navigation.
	/// </summary>
	public ref bool IsKeyboardSetRange => ref this._isKeyboardSetRange;
	public ref bool NavIdPassedBy => ref this._navIdPassedBy;
	/// <summary>
	/// Set by the item that matches RangeSrcItem.
	/// </summary>
	public ref bool RangeSrcPassedBy => ref this._rangeSrcPassedBy;
	/// <summary>
	/// Set by the item that matches NavJustMovedToId when IsSetRange is set.
	/// </summary>
	public ref bool RangeDstPassedBy => ref this._rangeDstPassedBy;

	public ImGuiMultiSelectTempData() => Clear();

	public void Clear() => ClearIO();

	public void ClearIO()
	{
		this._io.Requests.Resize(0);
		this._io.RangeSrcItem = this._io.NavIdItem = ImGuiSelectionUserData.Invalid;
		this._io.NavIdSelected = this._io.RangeSrcReset = false;
	}
}