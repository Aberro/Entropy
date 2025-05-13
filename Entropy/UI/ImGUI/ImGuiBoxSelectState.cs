#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImGuiNET;
using UnityEngine;
using ImGuiID = uint;
using ImGuiKeyChord = short;

namespace Entropy.UI.ImGUI;

public struct ImGuiBoxSelectState
{
	// Active box-selection data (persistent, 1 active at a time)
	public ImGuiID ID;
	public bool IsActive;
	public bool IsStarting;
	public bool IsStartedFromVoid; // Starting click was not from an item.
	public bool IsStartedSetNavIdOnce;
	public bool RequestClear;
	public ImGuiKeyChord KeyMods; // Latched key-mods for box-select logic.
	public ImVec2 StartPosRel; // Start position in window-contents relative space (to support scrolling)
	public ImVec2 EndPosRel; // End position in window-contents relative space
	public ImVec2 ScrollAccum; // Scrolling accumulator (to behave at high-frame spaces)
	public ImGuiWindowPtr Window;

	// Temporary/Transient data
	public bool UnclipMode; // (Temp/Transient, here in hot area). Set/cleared by the BeginMultiSelect()/EndMultiSelect() owning active box-select.
	public ImRect UnclipRect; // Rectangle where ItemAdd() clipping may be temporarily disabled. Need support by multi-select supporting widgets.
	public ImRect BoxSelectRectPrev; // Selection rectangle in absolute coordinates (derived every frame from BoxSelectStartPosRel and MousePos)
	public ImRect BoxSelectRectCurr;
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member