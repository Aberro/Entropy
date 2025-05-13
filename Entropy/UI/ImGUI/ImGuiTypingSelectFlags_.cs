#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Entropy.UI.ImGUI;

public enum ImGuiTypingSelectFlags
{
	None = 0,
	/// <summary>
	/// Backspace to delete character inputs. If using: ensure GetTypingSelectRequest() is not called more than once per frame (filter by e.g. focus state)
	/// </summary>
	AllowBackspace = 1 << 0,
	/// <summary>
	/// Allow "single char" search mode which is activated when pressing the same character multiple times.
	/// </summary>
	AllowSingleCharMode = 1 << 1,
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member