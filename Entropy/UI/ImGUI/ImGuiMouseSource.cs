﻿
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Entropy.UI.ImGUI;

public enum ImGuiMouseSource : int
{
	ImGuiMouseSource_Mouse = 0, // Input is coming from an actual mouse.
	ImGuiMouseSource_TouchScreen, // Input is coming from a touch screen (no hovering prior to initial press, less precise initial press aiming, dual-axis wheeling possible).
	ImGuiMouseSource_Pen, // Input is coming from a pressure/magnetic pen (often used in conjunction with high-sampling rates).
	ImGuiMouseSource_COUNT
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member