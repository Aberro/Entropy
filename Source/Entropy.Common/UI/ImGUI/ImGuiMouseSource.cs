#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Entropy.Common.UI.ImGUI;

public enum ImGuiMouseSource : int
{
	/// <summary>
	/// Input is coming from an actual mouse.
	/// </summary>
	Mouse = 0,
	/// <summary>
	/// Input is coming from a touch screen (no hovering prior to initial press, less precise initial press aiming, dual-axis wheeling possible).
	/// </summary>
	TouchScreen,
	/// <summary>
	/// Input is coming from a pressure/magnetic pen (often used in conjunction with high-sampling rates).
	/// </summary>
	Pen,
	COUNT
}