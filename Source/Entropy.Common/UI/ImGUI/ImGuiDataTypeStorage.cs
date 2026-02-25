#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImU8 = byte;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiDataTypeStorage
{

	private fixed ImU8 _data[8];

	/// <summary>
	/// Opaque storage to fit any data up to ImGuiDataType_COUNT
	/// </summary>
	public Span<ImU8> Data
	{
		get
		{
			fixed (ImU8* ptr = this._data)
			{
				return new Span<ImU8>(ptr, 8);
			}
		}
	}
};