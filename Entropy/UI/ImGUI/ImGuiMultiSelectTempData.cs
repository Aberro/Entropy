#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using UnityEngine;
using ImGuiID = uint;
using ImGuiKeyChord = int;
using ImS8 = sbyte;

namespace Entropy.UI.ImGUI;

public struct ImGuiMultiSelectTempData
{
	public ImGuiMultiSelectIO IO;                 // MUST BE FIRST FIELD. Requests are set and returned by BeginMultiSelect()/EndMultiSelect() + written to by user during the loop.
	public ImGuiMultiSelectStatePtr Storage;
	public ImGuiID FocusScopeId;       // Copied from g.CurrentFocusScopeId (unless another selection scope was pushed manually)
	public ImGuiMultiSelectFlags Flags;
	public ImVec2 ScopeRectMin;
	public ImVec2 BackupCursorMaxPos;
	public ImGuiSelectionUserData LastSubmittedItem;  // Copy of last submitted item data, used to merge output ranges.
	public ImGuiID BoxSelectId;
	public ImGuiKeyChord KeyMods;
	public ImS8 LoopRequestSetAll;  // -1: no operation, 0: clear all, 1: select all.
	public bool IsEndIO;            // Set when switching IO from BeginMultiSelect() to EndMultiSelect() state.
	public bool IsFocused;          // Set if currently focusing the selection scope (any item of the selection). May be used if you have custom shortcut associated to selection.
	public bool IsKeyboardSetRange; // Set by BeginMultiSelect() when using Shift+Navigation. Because scrolling may be affected we can't afford a frame of lag with Shift+Navigation.
	public bool NavIdPassedBy;
	public bool RangeSrcPassedBy;   // Set by the item that matches RangeSrcItem.
	public bool RangeDstPassedBy;   // Set by the item that matches NavJustMovedToId when IsSetRange is set.

	public ImGuiMultiSelectTempData() => Clear();

	public void Clear() => ClearIO();

	public void ClearIO()
	{
		this.IO.Requests.Resize(0);
		this.IO.RangeSrcItem = this.IO.NavIdItem = ImGuiSelectionUserData.Invalid;
		this.IO.NavIdSelected = this.IO.RangeSrcReset = false;
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member