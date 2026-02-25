using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;

namespace Entropy.Common.Utils;

public static class ByteHelper
{
	public static unsafe byte[] CopyPtrToBuffer<T>(ref byte[]? buffer, void* ptr) where T : unmanaged
	{
		var size = Marshal.SizeOf<T>();
		if (buffer is null || buffer.Length < size)
			buffer = new byte[size];
		fixed (byte* bufferPtr = buffer)
			UnsafeUtility.MemCpy(bufferPtr, ptr, size);
		return buffer;
	}

	public static string ToHexString(this byte[] bytes)
	{
		ArgumentNullException.ThrowIfNull(bytes);
		var sb = new StringBuilder("\r\n   | _0 _1 _2 _3 _4 _5 _6 _7 _8 _9 _A _B _C _D _E _F\r\n");
		sb.AppendLine("====================================================");
		var idx = 0;
		var line = 0;
		while (idx < bytes.Length)
		{
			sb.Append(line.ToString("X2", CultureInfo.InvariantCulture));
			sb.Append(" | ");
			for (var i = 0; i < 16; i++)
			{
				if (idx >= bytes.Length)
					break;
				sb.Append(bytes[idx].ToString("X2", CultureInfo.InvariantCulture));
				sb.Append(' ');
				idx++;
			}

			sb.AppendLine();
			line++;
		}

		return sb.ToString();
	}
}