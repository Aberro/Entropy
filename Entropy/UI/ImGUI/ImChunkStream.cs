#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Entropy.UI.ImGUI;

public unsafe struct ImChunkStream<T> where T : unmanaged
{
	static long IM_MEMALIGN(long offset, long align) => (offset + (align - 1)) & ~(align - 1);
	public ImVector<byte> Buf;

	public bool IsEmpty => this.Buf.IsEmpty;
	public int Size => this.Buf.Size;

	public void Clear() => this.Buf.Clear();
	public T* AllocateChunk(long size)
	{
		var HDR_SZ = 4;
		size = IM_MEMALIGN(HDR_SZ + size, 4u);
		var off = this.Buf.Size;
		this.Buf.Resize(off + (int)size);
		((int*)this.Buf[off])[0] = (int)size;
		return (T*)this.Buf[off + HDR_SZ];
	}
	public T* First()
	{
		var HDR_SZ = 4;
		if(this.Buf.Size <= 0)
			return null;
		return (T*)this.Buf[HDR_SZ];
	}
	public T* GetNextChunk(T* p)
	{
		var HDR_SZ = 4;
		if(p < First() || p >= Last())
			throw new ArgumentOutOfRangeException();
		p = (T*)((byte*)p + GetChunkSize(p));
		if(p == (T*)((byte*)Last() + HDR_SZ))
			return (T*)0;
		if(p >= Last())
			throw new ArgumentOutOfRangeException();
		return p;
	}
	public int GetChunkSize(T* p) => ((int*)p)[-1];
	public T* Last() => (T*)this.Buf[this.Buf.Size];
	public int OffsetFromPtr(T* p) => this.Buf.IndexFromPtr((byte*)p);
	public T* PtrFromOffset(int off) => (T*)this.Buf[off];
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member