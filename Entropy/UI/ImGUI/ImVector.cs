#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using ImGuiNET;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Entropy.UI.ImGUI;

public readonly unsafe struct ImVectorPtr<T>(ImVector<T>* nativePtr) where T : unmanaged
{
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
	public ImVector<T>* NativePtr { get; } = nativePtr;

	public ImVectorPtr(IntPtr nativePtr) : this((ImVector<T>*)(void*)nativePtr)
	{
	}

	public static implicit operator ImVectorPtr<T>(ImVector<T>* nativePtr) => new(nativePtr);
	public static implicit operator ImVector<T>*(ImVectorPtr<T> wrappedPtr) => wrappedPtr.NativePtr;
	public static implicit operator ImVectorPtr<T>(IntPtr nativePtr) => new(nativePtr);

	public bool IsEmpty => NativePtr->IsEmpty;
	public int Size => NativePtr->Size;
	public int SizeInBytes => NativePtr->SizeInBytes;
	public static int MaxSize => ImVector<T>.MaxSize;

	public T Get(int i) => NativePtr->Get(i);

	public T GetLast() => NativePtr->Get(NativePtr->Size - 1);

	public T GetFirst() => NativePtr->Get(0);
	public void Set(int i, T v) => NativePtr->Set(i, v);
	public T* GetPtr(int i) => NativePtr->GetPtr(i);
	public void SetPtr(int i, T* v) => NativePtr->SetPtr(i, v);
	public int Capacity() => NativePtr->Capacity();
	public ImVector<T>.Enumerator GetEnumerator() => NativePtr->GetEnumerator();
	public void Clear() => NativePtr->Clear();
	public void Resize(int newSize) => NativePtr->Resize(newSize);
	public void Shrink(int newSize) => NativePtr->Shrink(newSize);
	public void Reserve(int newCapacity) => NativePtr->Reserve(newCapacity);
	public void ReserveClear(int newCapacity) => NativePtr->ReserveClear(newCapacity);
	public T* Add(T* v) => NativePtr->Add(v);
	public void Pop() => NativePtr->Pop();
	public void Remove(T* item) => NativePtr->Remove(item);
	public void Remove(T* from, T* to) => NativePtr->Remove(from, to);
	public void RemoveAt(int index) => NativePtr->RemoveAt(index);
	public void RemoveWithLastItem(T* item) => NativePtr->RemoveWithLastItem(item);
	public T* Insert(int index, T* v) => NativePtr->Insert(index, v);
	public bool Contains(T* v) => NativePtr->Contains(v);
	public int IndexOf(T* item) => NativePtr->IndexOf(item);
	public int IndexFromPtr(T* item) => NativePtr->IndexFromPtr(item);
}
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
			get => this._vector.GetPtr(this._index);
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
	public readonly T Get(int i)
	{
		if(i < 0 || i >= this._size || this._data == null)
			throw new ArgumentOutOfRangeException();
		return *(this._data + i);
	}
	public T GetLast() => Get(Size - 1);
	public T GetFirst() => Get(0);
	public void Set(int i, T v)
	{
		if(i < 0 || i >= this._size || this._data == null)
			throw new ArgumentOutOfRangeException();
		UnsafeUtility.MemCpy(this._data + i, &v, sizeof(T));
	}
	public readonly T* GetPtr(int i)
	{
		if(i < 0 || i >= this._size || this._data == null)
			throw new ArgumentOutOfRangeException();
		return this._data + i;
	}
	public void SetPtr(int i, T* v)
	{
		if(i < 0 || i >= this._size || this._data == null)
			throw new ArgumentOutOfRangeException();
		UnsafeUtility.MemCpy(this._data + i, v, sizeof(T));
	}
	public readonly int Capacity() => this._capacity;

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
	public readonly bool Contains(T* v)
	{
		var dataStart = this._data;
		var dataEnd = this._data + this._size;
		if(v >= dataStart && v < dataEnd)
			return true;
		while (dataStart < dataEnd)
			if ((*dataStart++).Equals(*v))
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