#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImS8 = sbyte;

namespace Entropy.UI.ImGUI;

public struct ImGuiTypingSelectRequest
{
	public ImGuiTypingSelectFlags Flags; // Flags passed to GetTypingSelectRequest()
	public int SearchBufferLen;
	public string SearchBuffer; // Search buffer contents (use full string. unless SingleCharMode is set, in which case use SingleCharSize).
	public bool SelectRequest; // Set when buffer was modified this frame, requesting a selection.
	public bool SingleCharMode; // Notify when buffer contains same character repeated, to implement special mode. In this situation it preferred to not display any on-screen search indication.
	public ImS8 SingleCharSize; // Length in bytes of first letter codepoint (1 for ascii, 2-4 for UTF-8). If (SearchBufferLen==RepeatCharSize) only 1 letter has been input.
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member