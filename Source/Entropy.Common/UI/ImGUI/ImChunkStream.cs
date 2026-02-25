#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using Unity.Collections.LowLevel.Unsafe;

namespace Entropy.Common.UI.ImGUI;

/// <summary>
/// Build and iterate a contiguous stream of variable-sized structures.
/// This is used by Settings to store persistent data while reducing allocation count.
/// We store the chunk size first, and align the final size on 4 bytes boundaries.
/// The tedious/zealous amount of casting is to avoid -Wcast-align warnings.
/// </summary>
/// <remarks>
/// This type is EXTREMELY unsafe and generally should never be used in managed code, it's here only for proper mapping of native structures.
/// </remarks>
/// <typeparam name="T"></typeparam>
public unsafe struct ImChunkStream<T> where T : unmanaged
{
	private ImVector<byte> _buf;
	/// <summary>
	/// Gets the underlying buffer of the chunk stream.
	/// </summary>
	public ref ImVector<byte> Buffer => ref this._buf;

	/// <summary>
	/// Checks if the chunk stream is empty.
	/// </summary>
	public bool IsEmpty => Buffer.IsEmpty;
	/// <summary>
	/// Gets the size of the chunk stream in bytes.
	/// </summary>
	public int Size => Buffer.Size;

	/// <summary>
	/// Clears the chunk stream, removing all chunks.
	/// </summary>
	public void Clear() => Buffer.Clear();
	/// <summary>
	/// Allocates a new chunk in the stream with the specified size.
	/// </summary>
	/// <param name="size"> Size of the chunk to allocate in bytes.</param>
	/// <returns>A reference to the allocated chunk.</returns>
	public ref T AllocateChunk(long size)
	{
		var headerSize = sizeof(int); // the header of the chunk, which is the size of the chunk itself.
		// Add extra size for the header value and ensure the total allocated size is such that the next allocated chunk will be aligned to 4 bytes.
		size = IM_MEMALIGN(headerSize + size, 4u);
		var offset = Buffer.Size; // current offset
		// Resize the buffer to accommodate the new chunk.
		Buffer.Resize(offset + (int)size);
		var sizePtr = (int*)Buffer.Get(offset);
		*sizePtr = (int)size; // write the header
		// return a reference to the allocated chunk, which is right after the header.
		return ref UnsafeUtility.AsRef<T>((T*)Buffer.Get(offset + headerSize));
	}
	public ref T First()
	{
		var headerSize = sizeof(int);
		if(Buffer.Size <= 0)
			return ref UnsafeUtility.AsRef<T>(null);
		return ref UnsafeUtility.AsRef<T>((T*)Buffer.Get(headerSize));
	}
	public ref T GetNextChunk(ref T p)
	{
		fixed(T* pp = &p)
		fixed(T* pFirst = &First())
		fixed(byte* pEnd = &Buffer.GetLast())
		{
			if(pp < pFirst || pp >= pEnd)
				throw new ArgumentOutOfRangeException(nameof(p));
			var result = ((byte*)pp) + GetChunkSize(ref p);
			if(result >= pEnd)
				return ref UnsafeUtility.AsRef<T>(null);
			return ref UnsafeUtility.AsRef<T>(result);
		}
	}
	public int GetChunkSize(ref T p)
	{
		fixed(T* pp = &p)
			return ((int*)pp)[-1];
	}
	public int OffsetFromPtr(ref T p)
	{
		fixed(T* pp = &p)
			return Buffer.IndexOf(ref UnsafeUtility.AsRef<byte>((byte*)pp));
	}
	public ref T PtrFromOffset(int off) => ref *(T*)Buffer.Get(off);

	/// <summary>
	/// Aligns the given offset to the specified alignment.
	/// </summary>
	/// <param name="offset"></param>
	/// <param name="align"></param>
	/// <returns></returns>
	private static long IM_MEMALIGN(long offset, long align) => (offset + (align - 1)) & ~(align - 1);
}