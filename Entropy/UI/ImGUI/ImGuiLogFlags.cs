#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Entropy.UI.ImGUI;

public enum ImGuiLogFlags
{
	None = 0,
	OutputTTY = 1 << 0,
	OutputFile = 1 << 1,
	OutputBuffer = 1 << 2,
	OutputClipboard = 1 << 3,
	OutputMask = OutputTTY | OutputFile | OutputBuffer | OutputClipboard,
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member