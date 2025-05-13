#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImGuiID = uint;
namespace Entropy.UI.ImGUI;

public unsafe struct ImGuiTypingSelectState
{
	public ImGuiTypingSelectRequest Request; // User-facing data
	public fixed char SearchBuffer[64]; // Search buffer: no need to make dynamic as this search is very transient.
	public ImGuiID FocusScope;
	public int LastRequestFrame = 0;
	public float LastRequestTime = 0.0f;

	public bool SingleCharModeLock = false; // After a certain single char repeat count we lock into SingleCharMode. Two benefits: 1) buffer never fill, 2) we can provide an immediate SingleChar mode without timer elapsing.

	public ImGuiTypingSelectState() { }
	public void Clear()
	{
		this.SearchBuffer[0] = '\0';
		this.SingleCharModeLock = false;
	} // We preserve remaining data for easier debugging
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member