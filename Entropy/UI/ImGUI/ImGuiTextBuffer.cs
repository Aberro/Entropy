#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Runtime.InteropServices;

namespace Entropy.UI.ImGUI;
public unsafe readonly partial struct ImGuiTextBufferPtr
{
	public char this[int i]
	{
		get
		{
			ref ImGuiTextBuffer nativeRef = ref *NativePtr;
			return nativeRef[i];
		}
	}
}
public unsafe struct ImGuiTextBuffer
{
	public ImVector<char> Buf;
	public static string EmptyString = string.Empty;

	public readonly char this[int i]
	{
		get
		{
			if(this.Buf.IsEmpty)
				throw new InvalidOperationException();
			return this.Buf.Get(i);
		}
	}
	//const char*         begin() const           { return Buf.Data ? &Buf.front() : EmptyString; }
	//const char*         end() const             { return Buf.Data ? &Buf.back() : EmptyString; }   // Buf is zero-terminated, so end() will point on the zero-terminator
	public readonly int Size => this.Buf.IsEmpty ? 0 : this.Buf.Size - 1;
	public readonly bool IsEmpty => this.Buf.IsEmpty;
	public void Clear() => this.Buf.Clear();
	public void Reserve(int capacity) => this.Buf.Reserve(capacity);
	public override string ToString() => Marshal.PtrToStringUTF8((IntPtr)this.Buf.GetPtr(0));
	public void Append(string str)
	{
		var strPtr = Marshal.StringToCoTaskMemUTF8(str);
		try
		{
			ImGuiTextBuffer_append(ref this, strPtr, IntPtr.Zero);
		}
		finally
		{
			Marshal.FreeCoTaskMem(strPtr);
		}
	}
	public void AppendFormatted(string fmt, params object[] args)
	{
		var strPtr = Marshal.StringToCoTaskMemUTF8(string.Format(fmt, args));
		try
		{
			ImGuiTextBuffer_append(ref this, strPtr, IntPtr.Zero);
		}
		finally
		{
			Marshal.FreeCoTaskMem(strPtr);
		}
	}

	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImGuiTextBuffer_append(ref ImGuiTextBuffer self, IntPtr str, IntPtr str_end);
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member