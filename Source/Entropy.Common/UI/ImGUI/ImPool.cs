#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using ImPoolIdx = int;
using ImGuiID = uint;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using System.Diagnostics.CodeAnalysis;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImPool<T> where T : unmanaged
{
	struct ImPoolSlot
	{
		[SuppressMessage("Performance", "CA1823:Avoid unused private fields", Justification = "<Pending>")]
		T _value;
		public ref T Value
		{
			get
			{
				fixed (ImPoolSlot* ptr = &this)
					return ref UnsafeUtility.AsRef<T>(ptr);
			}
		}
		public ref int NextFreeIdx
		{
			get
			{
				fixed (ImPoolSlot* ptr = &this)
					return ref UnsafeUtility.AsRef<int>(ptr);
			}
		}

		public ImPoolSlot(T value) : this() => Value = value;
	}
	/// <summary>
	/// Contiguous data
	/// </summary>
	private ImVector<ImPoolSlot> _buf;
	/// <summary>
	/// ID->Index
	/// </summary>
	private ImGuiStorage _map;
	/// <summary>
	/// Next free idx to use
	/// </summary>
	private ImPoolIdx _freeIdx;
	/// <summary>
	/// Number of active/alive items (for display purpose)
	/// </summary>
	private ImPoolIdx _aliveCount;

	public readonly int AliveCount => this._aliveCount;
	public readonly int BufferSize => this._buf.Size;
	public readonly int MapSize => this._map.Data.Size;

	public ref T GetByKey(ImGuiID key)
	{
		var idx = this._map.GetInt(key, -1);
		if(idx == -1)
			throw new KeyNotFoundException($"Key {key} not found in ImPool<{typeof(T).Name}>.");
		return ref this._buf.Get(idx).Value;
	}
	public readonly ref T GetByIndex(ImPoolIdx n) => ref this._buf.Get(n).Value;
	public readonly ImPoolIdx GetIndex(ref T p) => this._buf.IndexOf(ref AsSlot(ref p));

	public ref T GetOrAddByKey(ImGuiID key)
	{
		var pIdx = this._map.GetIntRef(key, -1);
		var idx = *pIdx;
		if(idx != -1)
			return ref AsRef(ref this._buf.Get(idx));
		*pIdx = this._freeIdx;
		return ref Add();
	}
	public bool Contains(ref T p)
	{
		return this._buf.Contains(ref AsSlot(ref p));
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
	public ref T Add()
	{
		var idx = this._freeIdx;
		if(idx == this._buf.Size)
		{
			this._buf.Resize(this._buf.Size + 1);
			this._freeIdx++;
		}
		else
		{
			this._freeIdx = this._buf.Get(idx).NextFreeIdx;
		}

		ref var slot = ref this._buf.Get(idx);
		slot.Value = new T();
		this._aliveCount++;
		return ref AsRef(ref this._buf.Get(idx));
	}
	public void Remove(ImGuiID key, ref T p) => Remove(key, GetIndex(ref p));
	public void Remove(ImGuiID key, ImPoolIdx idx)
	{
		ref var slot = ref this._buf.Get(idx);
		slot.NextFreeIdx = this._freeIdx;
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
	public readonly bool TryGetMapData(ImPoolIdx n, ref T result)
	{
		var idx = this._map.Data.Get(n).Int;
		if(idx == -1)
		{
			return false;
		}
		result = ref GetByIndex(idx);
		return true;
	}
	private static ref ImPoolSlot AsSlot(ref T p)
	{
		fixed(T* ptr = &p)
			return ref UnsafeUtility.AsRef<ImPoolSlot>(ptr);
	}
	private static ref T AsRef(ref ImPoolSlot slot)
	{
		fixed(ImPoolSlot* ptr = &slot)
			return ref UnsafeUtility.AsRef<T>(ptr);
	}
}