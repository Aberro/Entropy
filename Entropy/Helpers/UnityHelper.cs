using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Object = UnityEngine.Object;

namespace Entropy.Helpers;

/// <summary>
/// Helper class for Unity-related operations.
/// </summary>
public static class UnityHelper
{
	/// <summary>
	/// Checks if the object is valid (not null and not destroyed).
	/// </summary>
	/// <param name="obj"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsValid([NotNullWhen(true)]this Object? obj) => obj != null && (bool)obj;
}