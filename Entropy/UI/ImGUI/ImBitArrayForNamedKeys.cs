#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Runtime.InteropServices;

namespace Entropy.UI.ImGUI;

[StructLayout(LayoutKind.Sequential)]
public struct ImBitArrayForNamedKeys
{
	private const int OFFSET = -(int)ImGuiNET.ImGuiKey.NamedKey_BEGIN;
	private const int BITCOUNT = (int)ImGuiNET.ImGuiKey.NamedKey_COUNT;
	private const int STORAGE_SIZE = (BITCOUNT + 31) >> 5; // = 5

	private unsafe fixed uint storage[STORAGE_SIZE]; // 5 = (133 + 31) / 32

	public unsafe void ClearAllBits()
	{
		for(var i = 0; i < 5; i++)
			this.storage[i] = 0;
	}

	public unsafe void SetAllBits()
	{
		for(var i = 0; i < 5; i++)
			this.storage[i] = 0xFFFFFFFF;
	}

	public unsafe bool TestBit(int n)
	{
		n += OFFSET;
		if((uint)n >= BITCOUNT)
			throw new ArgumentOutOfRangeException();
		var index = n >> 5;
		var bit = n & 31;
		return (this.storage[index] & (1u << bit)) != 0;
	}

	public unsafe void SetBit(int n)
	{
		n += OFFSET;
		if((uint)n >= BITCOUNT)
			throw new ArgumentOutOfRangeException();
		var index = n >> 5;
		var bit = n & 31;
		this.storage[index] |= 1u << bit;
	}

	public unsafe void ClearBit(int n)
	{
		n += OFFSET;
		if((uint)n >= BITCOUNT)
			throw new ArgumentOutOfRangeException();
		var index = n >> 5;
		var bit = n & 31;
		this.storage[index] &= ~(1u << bit);
	}

	public void SetBitRange(int n, int n2)
	{
		n += OFFSET;
		n2 += OFFSET;
		if(n < 0 || n2 > BITCOUNT || n2 <= n)
			throw new ArgumentOutOfRangeException();
		for(var i = n; i < n2; i++)
			SetBit(i - OFFSET); // pass original n
	}

	public bool this[int n] => TestBit(n);
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member