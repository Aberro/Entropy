#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using ImGuiNET;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Entropy.UI.ImGUI;
public unsafe struct ImVector<T> where T : unmanaged
{
	public ref struct Enumerator
	{
		private readonly ImVector<T> _vector;
		private int _index;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Enumerator(ImVector<T> vector)
		{
			this._vector = vector;
			this._index = -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			var num = this._index + 1;
			if(num >= this._vector._size)
				return false;
			this._index = num;
			return true;
		}

		public readonly T* Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => this._vector[this._index];
		}
	}
	int _size;
	int _capacity;
	T* _data;

	// Constructors, destructor
	public ImVector()
	{
		this._size = this._capacity = 0;
		this._data = null;
	}
	public ImVector(ref ImVector<T> src)
	{
		Resize(src._size);
		if(src._data != null)
			UnsafeUtility.MemCpy(this._data, src._data, this._size * sizeof(T));
	}

	public readonly bool IsEmpty => this._size == 0;
	public readonly int Size => this._size;
	public readonly int SizeInBytes => this._size * sizeof(T);
	public static int MaxSize => 0x7FFFFFFF / sizeof(T);
	public readonly int Capacity() => this._capacity;
	public readonly T* this[int i]
	{
		get
		{
			if(i < 0 || i >= this._size || this._data == null)
				throw new ArgumentOutOfRangeException();
			return this._data + i;
		}
	}

	public readonly Enumerator GetEnumerator() => new(this);
	public void Clear()
	{
		if(this._data != null)
		{
			this._size = this._capacity = 0;
			ImGui.MemFree((IntPtr)this._data);
		}
	}
	public void Resize(int newSize)
	{
		if (newSize > this._capacity)
			Reserve(GrowCapacity(newSize));
		this._size = newSize;
	}
	/// <summary>
	/// Resize a vector to a smaller size, guaranteed not to cause a reallocation
	/// </summary>
	public void Shrink(int newSize)
	{
		if(newSize > this._size)
			throw new ArgumentOutOfRangeException();
		this._size = newSize;
	}
	public void Reserve(int newCapacity)
	{
		if (newCapacity <= this._capacity)
			return;
		var newData = (T*)ImGui.MemAlloc((uint)newCapacity * (uint)sizeof(T));
		if (this._data != null)
		{
			UnsafeUtility.MemCpy(newData, this._data, this._size * sizeof(T));
			ImGui.MemFree((IntPtr)this._data);
		}
		this._data = newData;
		this._capacity = newCapacity; 
	}
	public void ReserveClear(int newCapacity)
	{
		if (newCapacity <= this._capacity)
			return;
		if (this._data != null)
			ImGui.MemFree((IntPtr)this._data);
		this._data = (T*)ImGui.MemAlloc((uint)newCapacity * (uint)sizeof(T));
		this._capacity = newCapacity;
	}

	public T* Add(T* v)
	{
		if (this._size == this._capacity)
			Reserve(GrowCapacity(this._size + 1));
		if(v >= this._data && v < this._data + this._size)
			throw new ArgumentException("Add() called with a reference pointing inside the ImVector data itself");
		UnsafeUtility.MemCpy(&this._data[this._size], v, sizeof(T));
		return this._data + this._size++;
	}

	public void Pop()
	{
		if(this._size <= 0)
			throw new IndexOutOfRangeException("Pop() called on empty vector");
		this._size--;
	}
	public void Remove(T* item)
	{
		if(item < this._data || item >= this._data + this._size)
			throw new ArgumentOutOfRangeException();
		var off = item - this._data;
		UnsafeUtility.MemMove(this._data + off, this._data + off + 1, ((uint)this._size - (uint)off - 1) * sizeof(T));
		this._size--;
	}
	public void Remove(T* from, T* to)
	{
		if(from < this._data || from >= this._data + this._size || to < from || to > this._data + this._size)
			throw new ArgumentOutOfRangeException();
		var count = to - from;
		var off = from - this._data;
		UnsafeUtility.MemMove(this._data + off, this._data + off + count, ((uint)this._size - (uint)off - (uint)count) * sizeof(T));
		this._size -= (int)count;
	}
	public void RemoveAt(int index)
	{
		if(index < 0 || index >= this._size)
			throw new ArgumentOutOfRangeException();
		UnsafeUtility.MemMove(this._data + index, this._data + index + 1, ((uint)this._size - (uint)index - 1) * sizeof(T));
		this._size--;
	}
	public void RemoveWithLastItem(T* item)
	{
		if(item < this._data || item >= this._data + this._size)
			throw new ArgumentOutOfRangeException();
		var off = item - this._data;
		if (item < this._data + this._size - 1)
			UnsafeUtility.MemCpy(this._data + off, this._data + this._size - 1, sizeof(T));
		this._size--;
	}
	public T* Insert(int index, T* v)
	{
		if(index < 0 || index > this._size)
			throw new ArgumentOutOfRangeException();
		if(this._size == this._capacity)
			Reserve(GrowCapacity(this._size + 1));
		if(index < this._size)
			UnsafeUtility.MemMove(this._data + index + 1, this._data + index, ((uint)this._size - (uint)index) * sizeof(T));
		UnsafeUtility.MemCpy(this._data + index, v, sizeof(T));
		this._size++;
		return this._data + index;
	}
	public readonly bool Contains(ref T v)
	{
		var dataStart = this._data;
		var dataEnd = this._data + this._size;
		while (dataStart < dataEnd)
			if ((*dataStart++).Equals(v))
				return true;
		return false;
	}
	public readonly int IndexOf(T* item)
	{
		if(item == null)
			throw new ArgumentNullException(nameof(item));
		var dataStart = this._data;
		var dataEnd = this._data + this._size;
		if(item < dataStart || item >= dataEnd)
		{
			// item is not in the vector, search for an item with same value
			for(var i = 0; i < this._size; i++)
				if(dataStart[i].Equals(*item))
					return i;
			return -1;
		}
		// item is in the vector, just calculate the offset
		var off = item - this._data;
		if(off < 0 || off >= this._size)
			throw new ArgumentOutOfRangeException();
		return (int)off;
	}

	public readonly int IndexFromPtr(T* item)
	{
		if(item == null || item < this._data || item >= this._data + this._size)
			throw new ArgumentOutOfRangeException();
		var off = item - this._data;
		return (int)off;
	}

	private readonly int GrowCapacity(int sz)
	{
		var newCapacity = this._capacity != 0 ? this._capacity + (this._capacity / 2) : 8;
		return newCapacity > sz ? newCapacity : sz;
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member