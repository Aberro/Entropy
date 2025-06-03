#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entropy.UI.ImGUI;

public struct ImGuiTabBarSection
{
	/// <summary>
	/// Number of tabs in this section.
	/// </summary>
	public int TabCount;
	/// <summary>
	/// Sum of width of tabs in this section (after shrinking down)
	/// </summary>
	public float Width;
	/// <summary>
	/// Horizontal spacing at the end of the section.
	/// </summary>
	public float Spacing;
}