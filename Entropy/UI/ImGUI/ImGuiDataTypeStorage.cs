#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImU8 = byte;

namespace Entropy.UI.ImGUI
{
	public unsafe struct ImGuiDataTypeStorage
	{
		public fixed ImU8 Data[8]; // Opaque storage to fit any data up to ImGuiDataType_COUNT
	};
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member