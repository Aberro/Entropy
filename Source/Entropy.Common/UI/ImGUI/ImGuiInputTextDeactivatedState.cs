#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImGuiID = uint;

namespace Entropy.Common.UI.ImGUI;


public unsafe struct ImGuiInputTextDeactivatedState
{
	private ImGuiID _id;
	private ImVector<char> _textA;

	/// <summary>
	/// widget id owning the text state (which just got deactivated)
	/// </summary>
	public ref ImGuiID ID => ref this._id;
	/// <summary>
	/// text buffer
	/// </summary>
	public ref ImVector<char> TextA => ref this._textA;

	public void ClearFreeMemory()
	{
		this._id = 0;
		this._textA.Clear();
	}
}