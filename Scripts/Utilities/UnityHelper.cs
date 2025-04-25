using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Object = UnityEngine.Object;

namespace Entropy.Scripts.Utilities;

public static class UnityHelper
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsValid([NotNullWhen(true)]this Object? obj) => obj != null && (bool)obj;
}