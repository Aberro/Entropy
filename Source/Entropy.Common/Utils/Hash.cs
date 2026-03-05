using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace Entropy.Common.Utils;

internal class Hash64
{
	private struct State
	{
		public const uint Prime32_1 = 0x9E3779B1U;
		public const uint Prime32_2 = 0x85EBCA77U;
		public const uint Prime32_3 = 0xC2B2AE3DU;
		public const uint Prime32_4 = 0x27D4EB2FU;
		public const uint Prime32_5 = 0x165667B1U;

		private uint _acc1;
		private uint _acc2;
		private uint _acc3;
		private uint _acc4;
		private readonly uint _smallAcc;
		private bool _hadFullStripe;

		internal State(uint seed)
		{
			_acc1 = seed + unchecked(Prime32_1 + Prime32_2);
			_acc2 = seed + Prime32_2;
			_acc3 = seed;
			_acc4 = seed - Prime32_1;

			_smallAcc = seed + Prime32_5;
			_hadFullStripe = false;
		}

		internal void ProcessStripe(ReadOnlySpan<byte> source)
		{
			source = source[..StripeSize];

			_acc1 = ApplyRound(_acc1, source);
			_acc2 = ApplyRound(_acc2, source[sizeof(uint)..]);
			_acc3 = ApplyRound(_acc3, source[(2 * sizeof(uint))..]);
			_acc4 = ApplyRound(_acc4, source[(3 * sizeof(uint))..]);

			_hadFullStripe = true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private readonly uint Converge()
		{
			return
				RotateLeft(_acc1, 1) +
				RotateLeft(_acc2, 7) +
				RotateLeft(_acc3, 12) +
				RotateLeft(_acc4, 18);
		}

		private static uint ApplyRound(uint acc, ReadOnlySpan<byte> lane)
		{
			acc += BinaryPrimitives.ReadUInt32LittleEndian(lane) * Prime32_2;
			acc = RotateLeft(acc, 13);
			acc *= Prime32_1;

			return acc;
		}

		internal readonly uint Complete(int length, ReadOnlySpan<byte> remaining)
		{
			var acc = _hadFullStripe ? Converge() : _smallAcc;

			acc += (uint)length;

			while(remaining.Length >= sizeof(uint))
			{
				var lane = BinaryPrimitives.ReadUInt32LittleEndian(remaining);
				acc += lane * Prime32_3;
				acc = RotateLeft(acc, 17);
				acc *= Prime32_4;

				remaining = remaining[sizeof(uint)..];
			}

			for(var i = 0; i < remaining.Length; i++)
			{
				uint lane = remaining[i];
				acc += lane * Prime32_5;
				acc = RotateLeft(acc, 11);
				acc *= Prime32_1;
			}

			acc ^= (acc >> 15);
			acc *= Prime32_2;
			acc ^= (acc >> 13);
			acc *= Prime32_3;
			acc ^= (acc >> 16);

			return acc;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static uint RotateLeft(uint value, int offset)
			=> (value << offset) | (value >> (32 - offset));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static ulong RotateLeft(ulong value, int offset)
			=> (value << offset) | (value >> (64 - offset));
	}
	private const int StripeSize = 4 * sizeof(uint);
	public static uint HashToUInt64(ReadOnlySpan<byte> source)
	{
		var totalLength = source.Length;
		var state = new State(0);

		while(source.Length >= StripeSize)
		{
			state.ProcessStripe(source);
			source = source[StripeSize..];
		}

		return state.Complete(totalLength, source);
	}
}
