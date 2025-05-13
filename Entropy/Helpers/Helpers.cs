using System.ComponentModel;
using System.Reflection;
using Entropy.Attributes;
using Entropy.Mods;
using Entropy.Patches;
using HarmonyLib;
using System.Text;
using Entropy.UI.ImGUI;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Entropy.Helpers;

/// <summary>
/// Helper class for working with enumerations.
/// </summary>
public static class EnumHelper
{
	/// <summary>
	/// Gets a <see cref="DescriptionAttribute"/> of an enumeration value
	/// </summary>
	/// <param name="value">The enumeration value.</param>
	/// <returns>A <see cref="DescriptionAttribute"/> of the enumeration value or value's name if not found.</returns>
	public static string GetDescription(this Enum value)
	{
		var fieldInfo = value.GetType().GetField(value.ToString());
		var attribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
		return attribute == null ? value.ToString() : attribute.Description;
	}

	/// <summary>
	/// Gets a <see cref="DisplayNameAttribute"/> of an enumeration value
	/// </summary>
	/// <param name="value">The enumeration value.</param>
	/// <returns>A <see cref="DisplayNameAttribute"/> of the enumeration value or the value's name if not found.</returns>
	public static string GetDisplayName(this Enum value)
	{
		var fieldInfo = value.GetType().GetField(value.ToString());
#pragma warning disable CS0436 // Type conflicts with imported type
		var attribute = fieldInfo.GetCustomAttribute<DisplayNameAttribute>();
#pragma warning restore CS0436 // Type conflicts with imported type
		return attribute == null ? value.ToString() : attribute.DisplayName;
	}

	/// <summary>
	/// Gets a <see cref="PatchCategory"/> of an enumeration value from <see cref="HarmonyPatchCategoryAttribute"/> attribute of the given type.
	/// </summary>
	/// <param name="type">Type of the class to get <see cref="PatchCategory"/> from.</param>
	/// <returns>A <see cref="PatchCategory"/> of the give type or a default mod category if <see cref="HarmonyPatchCategoryAttribute"/> nor <see cref="PatchCategoryDefinitionAttribute"/> can be found.</returns>
	public static PatchCategory GetCategory(this Type type)
	{
		var mod = ModsCollector.GetByAssembly(type.Assembly) ?? throw new ApplicationException($"Failed to get mod for {type.Name} type!");
		var attribute = type.GetCustomAttribute<HarmonyPatchCategoryAttribute>();
		if(attribute is not null)
			return PatchCategory.Get(mod, attribute.Category);
		var definition = type.GetCustomAttribute<PatchCategoryDefinitionAttribute>();
		if(definition is not null)
			return PatchCategory.Get(mod, definition.Name);
		return PatchCategory.GetDefault(mod);
	}

	/// <summary>
	/// Executes the specified action on each element of the enumerable.
	/// </summary>
	/// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
	/// <param name="source">The enumerable source.</param>
	/// <param name="action">The action to execute on each element.</param>
	public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
	{
		foreach(var item in source)
			action(item);
	}
}

public static class ByteHelper
{

	public static unsafe byte[] CopyPtrToBuffer<T>(ref byte[]? buffer, void* ptr) where T : unmanaged
	{
		var size = Marshal.SizeOf<T>();
		if(buffer is null || buffer.Length < size)
			buffer = new byte[size];
		fixed(byte* bufferPtr = buffer)
			UnsafeUtility.MemCpy(bufferPtr, ptr, size);
		return buffer;
	}

	public static string ToHexString(this byte[] bytes)
	{
		var sb = new StringBuilder("\r\n   | _0 _1 _2 _3 _4 _5 _6 _7 _8 _9 _A _B _C _D _E _F\r\n");
		sb.AppendLine("====================================================");
		var idx = 0;
		var line = 0;
		while(idx < bytes.Length)
		{
			sb.Append(line.ToString("X2"));
			sb.Append(" | ");
			for(var i = 0; i < 16; i++)
			{
				if(idx >= bytes.Length)
					break;
				sb.Append(bytes[idx].ToString("X2"));
				sb.Append(" ");
				idx++;
			}

			sb.AppendLine();
			line++;
		}

		return sb.ToString();
	}
}