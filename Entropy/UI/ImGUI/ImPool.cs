#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImPoolIdx = int;
using ImGuiID = uint;

namespace Entropy.UI.ImGUI;

public unsafe struct ImPool<T> where T : unmanaged
{
	private ImVector<T> _buf;        // Contiguous data
	private ImGuiStorage _map;        // ID->Index
	private ImPoolIdx _freeIdx;    // Next free idx to use
	private ImPoolIdx _aliveCount; // Number of active/alive items (for display purpose)

	public ImPool() => this._freeIdx = this._aliveCount = 0;
	public T* GetByKey(ImGuiID key)
	{
		var idx = this._map.GetInt(key, -1);
		return idx != -1 ? this._buf.GetPtr(idx) : null;
	}
	public readonly T* GetByIndex(ImPoolIdx n) => this._buf.GetPtr(n);
	public readonly ImPoolIdx GetIndex(T* p) => this._buf.IndexOf(p);

	public T* GetOrAddByKey(ImGuiID key)
	{
		var pIdx = this._map.GetIntRef(key, -1);
		var idx = *pIdx;
		if(idx != -1)
			return this._buf.GetPtr(idx);
		*pIdx = this._freeIdx;
		return Add();
	}
	public bool Contains(T* p)
	{
		return this._buf.Contains(p);
	}
	public bool ContainsKey(ImGuiID key)
	{
		var idx = this._map.GetInt(key, -1);
		return idx != -1;
	}
	public void Clear()
	{
		this._map.Clear();
		this._buf.Clear();
		this._freeIdx = this._aliveCount = 0;
	}
	public T* Add()
	{
		var idx = this._freeIdx;
		if(idx == this._buf.Size)
		{
			this._buf.Resize(this._buf.Size + 1);
			this._freeIdx++;
		}
		else
			// This is some C++ bullshit, I have no fucking clue what it does and just hope I rewrote it correctly in C#...
			this._freeIdx = *(int*)this._buf.GetPtr(idx);

		*this._buf.GetPtr(idx) = new T();
		this._aliveCount++;
		return this._buf.GetPtr(idx);
	}
	public void Remove(ImGuiID key, T* p) => Remove(key, GetIndex(p));
	public void Remove(ImGuiID key, ImPoolIdx idx)
	{ 
		*(int*)this._buf.GetPtr(idx) = this._freeIdx;
		this._freeIdx = idx;
		this._map.SetInt(key, -1);
		this._aliveCount--;
	}
	public void Reserve(int capacity)
	{
		this._buf.Reserve(capacity);
		this._map.Data.Reserve(capacity);
	}

	// To iterate a ImPool: for (int n = 0; n < pool.GetMapSize(); n++) if (T* t = pool.TryGetMapData(n)) { ... }
	// Can be avoided if you know .Remove() has never been called on the pool, or AliveCount == GetMapSize()
	public readonly int GetAliveCount() => this._aliveCount;
	public readonly int GetBufSize() => this._buf.Size;
	public readonly int GetMapSize() => this._map.Data.Size;
	public readonly T* TryGetMapData(ImPoolIdx n)
	{
		var idx = this._map.Data.GetPtr(n)->val_i;
		if(idx == -1)
			return null;
		return GetByIndex(idx);
	}
};
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member