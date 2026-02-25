#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImGuiID = uint;
using ImS8 = sbyte;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiMultiSelectState
{
	private ImGuiWindow* _window;
	private ImGuiID _id;
	private int _lastFrameActive;
	private int _lastSelectionSize;
	private ImS8 _rangeSelected;
	private ImS8 _navIdSelected;
	private ImGuiSelectionUserData _rangeSrcItem;
	private ImGuiSelectionUserData _navIdItem;

	public ref ImGuiWindow Window => ref *this._window;
	public bool HasWindow => this._window != null;
	public void SetWindow(ref ImGuiWindow window) => this._window = ImGuiHelper.GetPointer(ref window);
	public void ClearWindow() => this._window = null;

	public ref ImGuiID ID => ref this._id;
	/// <summary>
	/// Last used frame-count, for GC.
	/// </summary>
	public ref int LastFrameActive => ref this._lastFrameActive;
	/// <summary>
	/// Set by BeginMultiSelect() based on optional info provided by user. May be -1 if unknown.
	/// </summary>
	public ref int LastSelectionSize => ref this._lastSelectionSize;
	/// <summary>
	/// -1 (don't have) or true/false
	/// </summary>
	public ref ImS8 RangeSelected => ref this._rangeSelected;
	/// <summary>
	/// -1 (don't have) or true/false
	/// </summary>
	public ref ImS8 NavIdSelected => ref this._navIdSelected;
	public ref ImGuiSelectionUserData RangeSrcItem => ref this._rangeSrcItem;
	/// <summary>
	/// SetNextItemSelectionUserData() value for NavId (if part of submitted items)
	/// </summary>
	public ref ImGuiSelectionUserData NavIdItem => ref this._navIdItem;

	public ImGuiMultiSelectState()
	{
		this._window = null;
		this._id = 0;
		this._lastFrameActive = this._lastSelectionSize = 0;
		this._rangeSelected = this._navIdSelected = -1;
		this._rangeSrcItem = this._navIdItem = ImGuiSelectionUserData.Invalid;
	}
}