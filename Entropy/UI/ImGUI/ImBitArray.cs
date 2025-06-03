#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImGuiNET;
using System.Runtime.InteropServices;

namespace Entropy.UI.ImGUI
{

	public interface IImBitArrayStorage
	{
		Span<uint> Storage { get; }
		int Offset { get; }
		int Bitcount { get; }
	}
	public static class ImBitArray
	{
		public static void ClearAllBits<T>(this ref T storage) where T : unmanaged, IImBitArrayStorage
		{
			for(var i = 0; i < 5; i++)
				storage.Storage[i] = 0;
		}

		public static void SetAllBits<T>(this ref T storage) where T : unmanaged, IImBitArrayStorage
		{
			for(var i = 0; i < 5; i++)
				storage.Storage[i] = 0xFFFFFFFF;
		}

		public static bool TestBit<T>(this ref T storage, int n) where T : unmanaged, IImBitArrayStorage
		{
			n += storage.Offset;
			if((uint)n >= storage.Bitcount)
				throw new ArgumentOutOfRangeException();
			var index = n >> 5;
			var bit = n & 31;
			return (storage.Storage[index] & (1u << bit)) != 0;
		}

		public static void SetBit<T>(this ref T storage, int n) where T : unmanaged, IImBitArrayStorage
		{
			n += storage.Offset;
			if((uint)n >= storage.Bitcount)
				throw new ArgumentOutOfRangeException();
			var index = n >> 5;
			var bit = n & 31;
			storage.Storage[index] |= 1u << bit;
		}

		public static void ClearBit<T>(this ref T storage, int n) where T : unmanaged, IImBitArrayStorage
		{
			n += storage.Offset;
			if((uint)n >= storage.Bitcount)
				throw new ArgumentOutOfRangeException();
			var index = n >> 5;
			var bit = n & 31;
			storage.Storage[index] &= ~(1u << bit);
		}

		public static void SetBitRange<T>(this ref T storage, int n, int n2) where T : unmanaged, IImBitArrayStorage
		{
			n += storage.Offset;
			n2 += storage.Offset;
			if(n < 0 || n2 > storage.Bitcount || n2 <= n)
				throw new ArgumentOutOfRangeException();
			for(var i = n; i < n2; i++)
				storage.SetBit(i - storage.Offset); // pass original n
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct ImBitArrayForNamedKeys : IImBitArrayStorage
	{
		private const int OFFSET = -(int)ImGuiNET.ImGuiKey.NamedKey_BEGIN;
		private const int BITCOUNT = (int)ImGuiNET.ImGuiKey.NamedKey_COUNT;
		private const int STORAGE_SIZE = (BITCOUNT + 31) >> 5; // = 5

		private unsafe fixed uint storage[STORAGE_SIZE]; // 5 = (133 + 31) / 32

		public unsafe Span<uint> Storage
		{
			get
			{
				fixed(uint* ptr = this.storage)
				{
					return new Span<uint>(ptr, STORAGE_SIZE);
				}
			}
		}
		public int Offset => OFFSET;
		public int Bitcount => BITCOUNT;
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member