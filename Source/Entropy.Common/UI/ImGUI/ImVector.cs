#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA1815 // Override equals and operator equals on value types

using ImGuiNET;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Entropy.Common.UI.ImGUI;

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

		public readonly ref T Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref this._vector.Get(this._index);
		}
	}

	private int _size;
	private int _capacity;
	private T* _data;

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
			UnsafeUtility.MemCpy(this._data, src._data, this._size * Marshal.SizeOf<T>());
	}

	/// <summary>
	/// Gets a reference to the item at index i.
	/// </summary>
	/// <param name="i"> Index of the item to get</param>
	/// <returns> Reference to the item at index i</returns>
	public ref T this[int i]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref Get(i);
	}
	/// <summary>
	/// Checks if the vector is empty, i.e. has no items in it.
	/// </summary>
	public readonly bool IsEmpty
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => this._size == 0;
	}
	/// <summary>
	/// Gets the number of items in the vector.
	/// </summary>
	public readonly int Size
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => this._size;
	}
	/// <summary>
	/// Gets the capacity of the vector, i.e. the number of items it can hold without reallocating.
	/// </summary>
	public readonly int Capacity() => this._capacity;
	/// <summary>
	/// Gets the size of the vector in bytes, calculated as Size * sizeof(T).
	/// </summary>
	public readonly int SizeInBytes
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => this._size * Marshal.SizeOf<T>();
	}
	/// <summary>
	/// Gets the maximum number of items that can potentially be stored in the vector (at maximally possible capacity).
	/// The value depends on the size of T and is calculated as 0x7FFFFFFF / sizeof(T).
	/// </summary>
	public static int MaxSize
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => 0x7FFFFFFF / Marshal.SizeOf<T>();
	}
	/// <summary>
	/// Returns a reference to the item at index i.
	/// </summary>
	/// <param name="i"> Index of the item to get</param>
	/// <returns> Reference to the item at index i</returns>
	/// <exception cref="ArgumentOutOfRangeException"> Thrown when i is out of range (less than 0 or greater than or equal to Size)</exception>
	public readonly ref T Get(int i)
	{
		if(i < 0 || i >= this._size || this._data == null)
			throw new ArgumentOutOfRangeException(nameof(i), "Get() called with index out of range");
		return ref *(this._data + i);
	}
	/// <summary>
	/// Returns a reference to the last item in the vector.
	/// </summary>
	/// <returns> Reference to the last item in the vector</returns>
	public ref T GetLast() => ref Get(Size - 1);
	/// <summary>
	/// Returns a reference to the first item in the vector.
	/// </summary>
	/// <returns> Reference to the first item in the vector</returns>
	public ref T GetFirst() => ref Get(0);
	/// <summary>
	/// Sets the item at index i to the value of v.
	/// </summary>
	/// <param name="i"> Index of the item to set</param>
	/// <param name="item"> Reference to the item to set</param>
	/// <exception cref="ArgumentOutOfRangeException"> Thrown when i is out of range (less than 0 or greater than or equal to Size)</exception>
	public void Set(int i, ref T item)
	{
		if(i < 0 || i >= this._size || this._data == null)
			throw new ArgumentOutOfRangeException(nameof(i), "Set() called with index out of range");
		fixed(T* itemPtr = &item)
		{
			UnsafeUtility.MemCpy(this._data + i, itemPtr, Marshal.SizeOf<T>());
		}
	}

	public readonly Enumerator GetEnumerator() => new(this);
	/// <summary>
	/// Clears the vector, releasing any allocated memory and setting size and capacity to 0.
	/// </summary>
	public void Clear()
	{
		if(this._data != null)
		{
			this._size = this._capacity = 0;
			ImGui.MemFree((IntPtr)this._data);
		}
	}

	/// <summary>
	/// Resizes a vector to a new size.
	/// </summary>
	/// <param name="newSize"> The new size of the vector</param>
	/// <remarks>
	/// When the new size is larger than the current capacity, the vector will be reallocated to a larger size.
	/// Otherwise, the capacity would rempain unchanged and no memory reallocation would occur.
	/// </remarks>
	public void Resize(int newSize)
	{
		if (newSize > this._capacity)
			Reserve(GrowCapacity(newSize));
		this._size = newSize;
	}
	/// <summary>
	/// Resize a vector to a smaller size, guaranteed not to cause a reallocation
	/// </summary>
	/// <param name="newSize">The new size of the vector</param>
	public void Shrink(int newSize)
	{
		if(newSize > this._size)
			throw new ArgumentOutOfRangeException(nameof(newSize), "Shrink() called with newSize greater than current Size");
		this._size = newSize;
	}
	/// <summary>
	/// Reserves memory for the vector to hold at least <paramref name="newCapacity"/> items.
	/// </summary>
	/// <param name="newCapacity"> The new capacity of the vector</param>
	public void Reserve(int newCapacity)
	{
		if (newCapacity <= this._capacity)
			return;
		var newData = (T*)ImGui.MemAlloc((uint)newCapacity * (uint)Marshal.SizeOf<T>());
		if (this._data != null)
		{
			UnsafeUtility.MemCpy(newData, this._data, this._size * Marshal.SizeOf<T>());
			ImGui.MemFree((IntPtr)this._data);
		}
		this._data = newData;
		this._capacity = newCapacity; 
	}
	/// <summary>
	/// Reserves memory for the vector to hold at least <paramref name="newCapacity"/> items and clears the vector.
	/// </summary>
	/// <param name="newCapacity"> The new capacity of the vector</param>
	public void ReserveClear(int newCapacity)
	{
		if (newCapacity <= this._capacity)
			return;
		if (this._data != null)
			ImGui.MemFree((IntPtr)this._data);
		this._data = (T*)ImGui.MemAlloc((uint)newCapacity * (uint)Marshal.SizeOf<T>());
		var defaultValue = default(T);
		UnsafeUtility.MemCpyReplicate(this._data, &defaultValue, newCapacity * Marshal.SizeOf<T>(), newCapacity);
		this._capacity = newCapacity;
	}

	/// <summary>
	/// Adds a new item to the end of the vector, resizing it if necessary.
	/// </summary>
	/// <param name="item">Reference to the item to add</param>
	/// <returns>Reference to the newly added item inside the vector</returns>
	/// <exception cref="ArgumentException"> Thrown when the item reference points inside the vector data itself</exception>
	public ref T Add(ref T item)
	{
		if (this._size == this._capacity)
			Reserve(GrowCapacity(this._size + 1));
		fixed(T* itemPtr = &item)
		{
			if(itemPtr >= this._data && itemPtr < this._data + this._size)
				throw new ArgumentException("Add() called with a reference pointing inside the ImVector data itself");
			UnsafeUtility.MemCpy(&this._data[this._size], itemPtr, Marshal.SizeOf<T>());
		}
		return ref *(this._data + this._size++);
	}

	/// <summary>
	/// Removes the last item from the vector.
	/// </summary>
	/// <exception cref="IndexOutOfRangeException"> Thrown when the vector is empty</exception>
	public void Pop()
	{
		if(this._size <= 0)
			throw new IndexOutOfRangeException("Pop() called on empty vector");
		this._size--;
	}
	/// <summary>
	/// Removes an item from the vector by its pointer.
	/// </summary>
	/// <param name="item"> Pointer to the item to remove</param>
	/// <exception cref="InvalidOperationException"> Thrown when the item pointer is not within the vector data</exception>
	public void Remove(ref T item)
	{
		fixed(T* itemPtr = &item)
		{
			if(itemPtr < this._data || itemPtr >= this._data + this._size)
				throw new InvalidOperationException("Remove() called for an item not in the vector");
			var off = itemPtr - this._data;
			UnsafeUtility.MemMove(this._data + off, this._data + off + 1, ((uint)this._size - (uint)off - 1) * Marshal.SizeOf<T>());
		}
		this._size--;
	}
	/// <summary>
	/// Removes a range of items from the vector, specified by two references to items inside the vector.
	/// </summary>
	/// <param name="from"> Reference to the first item in the range to remove</param>
	/// <param name="to"> Reference to the item after the last item in the range to remove</param>
	/// <exception cref="InvalidOperationException"> Thrown when either of the item references is not within the vector data, or when <paramref name="to"/> is before <paramref name="from"/> inside the vector.</exception>
	public void Remove(ref T from, ref T to)
	{
		fixed(T* fromPtr = &from, toPtr = &to)
		{
			if(fromPtr < this._data || fromPtr >= this._data + this._size || toPtr < fromPtr || toPtr > this._data + this._size)
				throw new InvalidOperationException("Remove() called with one or both pointers referencing outside the vector data");
			var count = toPtr - fromPtr;
			var off = fromPtr - this._data;
			UnsafeUtility.MemMove(this._data + off, this._data + off + count, ((uint)this._size - (uint)off - (uint)count) * Marshal.SizeOf<T>());
			this._size -= (int)count;
		}
	}
	/// <summary>
	/// Removes an item from the vector at specified index and shifts all items after it one position to the left.
	/// </summary>
	/// <param name="index"> Index of the item to remove</param>
	/// <exception cref="ArgumentOutOfRangeException"> Thrown when the index is out of range (less than 0 or greater than or equal to Size)</exception>
	public void RemoveAt(int index)
	{
		if(index < 0 || index >= this._size)
			throw new ArgumentOutOfRangeException(nameof(index), "RemoveAt() called with index out of range");
		UnsafeUtility.MemMove(this._data + index, this._data + index + 1, ((uint)this._size - (uint)index - 1) * Marshal.SizeOf<T>());
		this._size--;
	}
	/// <summary>
	/// Removes an item from the vector by its pointer, moving the last item in the vector to the position of the removed item.
	/// </summary>
	/// <param name="item"> Pointer to the item to remove</param>
	/// <exception cref="ArgumentOutOfRangeException"> Thrown when the item pointer is not within the vector data</exception>
	public void RemoveWithLastItem(ref T item)
	{
		fixed(T* itemPtr = &item)
		{
			if(itemPtr < this._data || itemPtr >= this._data + this._size)
				throw new ArgumentOutOfRangeException(nameof(item), "RemoveWithLastItem() called for an item not in the vector");
			var off = itemPtr - this._data;
			if(itemPtr < this._data + this._size - 1)
				UnsafeUtility.MemCpy(this._data + off, this._data + this._size - 1, Marshal.SizeOf<T>());
		}
		this._size--;
	}
	/// <summary>
	/// Inserts a new item at the specified index, shifting all items after it one position to the right.
	/// </summary>
	/// <param name="index"> Index at which to insert the new item</param>
	/// <param name="item"> Reference to the item to insert</param>
	/// <returns>Reference to the newly inserted item inside the vector</returns>
	/// <exception cref="ArgumentOutOfRangeException"> Thrown when the index is out of range (less than 0 or greater than Size)</exception>
	public ref T Insert(int index, ref T item)
	{
		fixed(T* itemPtr = &item)
		{
			if(index < 0 || index > this._size)
				throw new ArgumentOutOfRangeException(nameof(index), "Insert() called with index out of range");
			if (this._size == this._capacity)
				Reserve(GrowCapacity(this._size + 1));
			if(index < this._size)
				UnsafeUtility.MemMove(this._data + index + 1, this._data + index, ((uint)this._size - (uint)index) * Marshal.SizeOf<T>());
			UnsafeUtility.MemCpy(this._data + index, itemPtr, Marshal.SizeOf<T>());
			this._size++;
			return ref *(this._data + index);
		}
	}
	/// <summary>
	/// Checks if the vector contains a specific item by its reference.
	/// </summary>
	/// <param name="item"> Reference to the item to check</param>
	/// <returns>True if the vector contains the item, false otherwise</returns>
	public readonly bool Contains(ref T item)
	{
		var dataStart = this._data;
		var dataEnd = this._data + this._size;
		fixed(T* itemPtr = &item)
		{
			if(itemPtr >= dataStart && itemPtr < dataEnd)
				return true;
		}
		while (dataStart < dataEnd)
		{
			if ((*dataStart++).Equals(item))
				return true;
		}

		return false;
	}
	/// <summary>
	/// Checks if the vector contains a specific item by its value.
	/// </summary>
	/// <param name="item"> Reference to the item to check</param>
	/// <returns> True if the vector contains the item, false otherwise</returns>
	public readonly bool ContainsValue(ref T item)
	{
		var dataStart = this._data;
		var dataEnd = this._data + this._size;
		fixed(T* itemPtr = &item)
		{
			// A quick check if the item reference is inside the vector data
			if(itemPtr >= dataStart && itemPtr < dataEnd)
				return true;
			// Otherwise, do a linear search for an item with the same value
			while(dataStart < dataEnd)
			{
				if(UnsafeUtility.MemCmp((void*)dataStart++, (void*)itemPtr, Marshal.SizeOf<T>()) == 0)
						return true;
			}
		}
		return false;
	}
	/// <summary>
	/// Finds the index of a specific item in the vector by its reference.
	/// </summary>
	/// <param name="item"> Reference to the item to find</param>
	/// <returns> Index of the item in the vector, or -1 if not found</returns>
	/// <exception cref="ArgumentOutOfRangeException"> Thrown when the item reference points inside the vector data itself</exception>
	public readonly int IndexOf(ref T item)
	{
		var dataStart = this._data;
		var dataEnd = this._data + this._size;
		fixed(T* itemPtr = &item)
		{
			if(itemPtr >= dataStart && itemPtr < dataEnd)
			{
				// item is in the vector, just calculate the offset
				var off = itemPtr - this._data;
				// sanity check
				if(off < 0 || off >= this._size)
					throw new ArgumentOutOfRangeException(nameof(item), "IndexOf() calculated an index that is out of range, this should never happen");
				return (int)off;
			}
			else
			{
				throw new ArgumentOutOfRangeException(nameof(item), "IndexOf() called with an item that is not in the vector data");
			}
		}
	}

	/// <summary>
	/// Finds the index of a specific item in the vector by its value.
	/// </summary>
	/// <param name="item"> Reference to the item to find</param>
	/// <returns> Index of the item in the vector, or -1 if not found</returns>
	public readonly int IndexOfValue(ref T item)
	{
		var dataStart = this._data;
		var dataEnd = this._data + this._size;
		fixed(T* itemPtr = &item)
		{
			if(itemPtr < dataStart || itemPtr >= dataEnd)
			{
				// item is not in the vector, search for an item with same value
				for(var i = 0; i < this._size; i++)
				{
					if (UnsafeUtility.MemCmp(dataStart+i, itemPtr, Marshal.SizeOf<T>()) == 0)
						return i;
				}

				return -1;
			}
			else
			{
				return IndexOf(ref item);
			}
		}
	}

	private readonly int GrowCapacity(int sz)
	{
		var newCapacity = this._capacity != 0 ? this._capacity + (this._capacity / 2) : 8;
		return newCapacity > sz ? newCapacity : sz;
	}
}