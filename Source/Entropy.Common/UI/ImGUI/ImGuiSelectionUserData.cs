#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA1815 // Override equals and operator equals on value types
namespace Entropy.Common.UI.ImGUI;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1028:Enum Storage should be Int32")]
public enum ImGuiSelectionUserData : long
{
	None = 0,
	Invalid = -1,
}