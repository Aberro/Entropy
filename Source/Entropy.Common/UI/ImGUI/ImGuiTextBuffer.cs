#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiTextBuffer
{
	private ImVector<byte> _buf;
	public ref ImVector<byte> Buf => ref this._buf;

	public readonly ref byte this[int i]
	{
		get
		{
			if(this._buf.IsEmpty)
				throw new InvalidOperationException();
			return ref this._buf.Get(i);
		}
	}
	//const char*         begin() const           { return Buf.Data ? &Buf.front() : string.Empty; }
	//const char*         end() const             { return Buf.Data ? &Buf.back() : string.Empty; }   // Buf is zero-terminated, so end() will point on the zero-terminator
	public readonly int Size => this._buf.IsEmpty ? 0 : this._buf.Size - 1;
	public readonly bool IsEmpty => this._buf.IsEmpty;
	public void Clear() => Buf.Clear();
	public void Reserve(int capacity) => Buf.Reserve(capacity);
	public override string ToString() => ImGuiHelper.GetString(ImGuiHelper.GetPointer(ref this._buf.Get(0)));
	public void Append(string str)
	{
		var strPtr = ImGuiHelper.GetStringPointer(str);
		ImGuiTextBuffer_append(ref this, (nint)strPtr, IntPtr.Zero);
	}
	public void AppendFormatted(string fmt, params object[] args)
	{
		var strPtr = ImGuiHelper.GetStringPointer(string.Format(CultureInfo.InvariantCulture, fmt, args));
		ImGuiTextBuffer_append(ref this, (nint)strPtr, IntPtr.Zero);
	}

	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes", Justification = "<Pending>")]
	private static extern void ImGuiTextBuffer_append(ref ImGuiTextBuffer self, IntPtr str, IntPtr str_end);
}