using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace librule.utils
{
    static class HashCode32
    {
        private const uint PRIME32_1 = 2654435761U;
        private const uint PRIME32_2 = 2246822519U;
        private const uint PRIME32_3 = 3266489917U;
        private const uint PRIME32_4 = 668265263U;
        private const uint PRIME32_5 = 374761393U;

        [StructLayout(LayoutKind.Sequential)]
        private struct QuadUint
        {
            public uint v1;
            public uint v2;
            public uint v3;
            public uint v4;
        }

        public unsafe static uint Hash(byte* buffer, int length)
        {
            return Hash(new ReadOnlySpan<byte>(buffer, length));
        }

        public static uint Hash(in ReadOnlySpan<byte> buffer)
        {
            unchecked
            {
                uint h32;

                var remainingBytes = buffer;
                var bulkuints = remainingBytes.PopAll<QuadUint>();

                if (!bulkuints.IsEmpty)
                {
                    uint v1 = PRIME32_1 + PRIME32_2;
                    uint v2 = PRIME32_2;
                    uint v3 = 0;
                    uint v4 = 0 - PRIME32_1;

                    for (int i = 0; i < bulkuints.Length; i++)
                    {
                        ref readonly QuadUint val = ref bulkuints[i];
                        v1 += val.v1 * PRIME32_2;
                        v2 += val.v2 * PRIME32_2;
                        v3 += val.v3 * PRIME32_2;
                        v4 += val.v4 * PRIME32_2;

                        v1 = RotateLeft(v1, 13);
                        v2 = RotateLeft(v2, 13);
                        v3 = RotateLeft(v3, 13);
                        v4 = RotateLeft(v4, 13);

                        v1 *= PRIME32_1;
                        v2 *= PRIME32_1;
                        v3 *= PRIME32_1;
                        v4 *= PRIME32_1;
                    }

                    h32 = MergeValues(v1, v2, v3, v4);
                }
                else
                {
                    h32 = PRIME32_5;
                }

                h32 += (uint)buffer.Length;


                ref uint remainingInt = ref Unsafe.As<byte, uint>(ref MemoryMarshal.GetReference(remainingBytes));


                switch (remainingBytes.Length >> 2)
                {
                    case 3:
                        h32 = RotateLeft(h32 + remainingInt * PRIME32_3, 17) * PRIME32_4;
                        remainingInt = ref Unsafe.Add(ref remainingInt, 1);
                        goto case 2;
                    case 2:
                        h32 = RotateLeft(h32 + remainingInt * PRIME32_3, 17) * PRIME32_4;
                        remainingInt = ref Unsafe.Add(ref remainingInt, 1);
                        goto case 1;
                    case 1:
                        h32 = RotateLeft(h32 + remainingInt * PRIME32_3, 17) * PRIME32_4;
                        remainingInt = ref Unsafe.Add(ref remainingInt, 1);
                        break;
                }


                ref byte remaining = ref Unsafe.As<uint, byte>(ref remainingInt);

                switch (remainingBytes.Length % sizeof(uint))
                {
                    case 3:
                        h32 = RotateLeft(h32 + remaining * PRIME32_5, 11) * PRIME32_1;
                        remaining = ref Unsafe.Add(ref remaining, 1);
                        goto case 2;
                    case 2:
                        h32 = RotateLeft(h32 + remaining * PRIME32_5, 11) * PRIME32_1;
                        remaining = ref Unsafe.Add(ref remaining, 1);
                        goto case 1;
                    case 1:
                        h32 = RotateLeft(h32 + remaining * PRIME32_5, 11) * PRIME32_1;
                        break;
                }

                h32 ^= h32 >> 15;
                h32 *= PRIME32_2;
                h32 ^= h32 >> 13;
                h32 *= PRIME32_3;
                h32 ^= h32 >> 16;

                return h32;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint RotateLeft(uint val, int bits) => val << bits | val >> 32 - bits;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint MergeValues(uint v1, uint v2, uint v3, uint v4)
        {
            return RotateLeft(v1, 1) + RotateLeft(v2, 7) + RotateLeft(v3, 12) + RotateLeft(v4, 18);
        }
    }

    internal static class Utils
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<TTo> PopAll<TTo>(this ref ReadOnlySpan<byte> @this) where TTo : struct
        {
#if NETCOREAPP3_0
			var totBytes = @this.Length;
			var toLength = (totBytes / Unsafe.SizeOf<TTo>());
			var sliceLength = toLength * Unsafe.SizeOf<TTo>();
			ref var thisRef = ref MemoryMarshal.GetReference(@this);
			@this = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref thisRef, sliceLength), totBytes - sliceLength);
			return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<byte, TTo>(ref thisRef), toLength);
#else
            return @this.PopAll<TTo, byte>();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<TTo> PopAll<TTo, TFrom>(this ref ReadOnlySpan<TFrom> @this) where TFrom : struct where TTo : struct
        {
            var totBytes = @this.Length * Unsafe.SizeOf<TFrom>();
            var toLength = totBytes / Unsafe.SizeOf<TTo>();
            var sliceLength = toLength * Unsafe.SizeOf<TTo>() / Unsafe.SizeOf<TFrom>();

#if NETSTANDARD2_0
			var result = MemoryMarshal.Cast<TFrom, TTo>(@this);
#else
            var result = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<TFrom, TTo>(ref MemoryMarshal.GetReference(@this)), toLength);
#endif
            @this = @this.Slice(sliceLength);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AsLittleEndian(this uint @this)
        {
            if (BitConverter.IsLittleEndian) { return @this; }
            return BinaryPrimitives.ReverseEndianness(@this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong AsLittleEndian(this ulong @this)
        {
            if (BitConverter.IsLittleEndian) { return @this; }
            return BinaryPrimitives.ReverseEndianness(@this);
        }

        public static bool TryPop<TTo>(this ref ReadOnlySpan<byte> @this, int count, out ReadOnlySpan<TTo> popped) where TTo : struct
        {
            var byteCount = count * Unsafe.SizeOf<TTo>();
            if (@this.Length >= byteCount)
            {
                popped = MemoryMarshal.Cast<byte, TTo>(@this.Slice(0, byteCount));
                @this = @this.Slice(byteCount);
                return true;
            }
            popped = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly TTo First<TTo>(this ReadOnlySpan<byte> @this) where TTo : struct
        {
            return ref MemoryMarshal.Cast<byte, TTo>(@this)[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly TTo Last<TTo>(this ReadOnlySpan<byte> @this) where TTo : struct
        {
            return ref MemoryMarshal.Cast<byte, TTo>(@this.Slice(@this.Length - Unsafe.SizeOf<TTo>()))[0];
        }

        public static ref readonly TTo First<TFrom, TTo>(this ReadOnlySpan<TFrom> @this) where TTo : struct where TFrom : struct
        {
            return ref MemoryMarshal.Cast<TFrom, TTo>(@this)[0];
        }

    }

    public static class Safeish
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly TTo As<TFrom, TTo>(in TFrom from) where TTo : struct where TFrom : struct
        {
            if (Unsafe.SizeOf<TFrom>() < Unsafe.SizeOf<TTo>()) { throw new InvalidCastException(); }
            return ref Unsafe.As<TFrom, TTo>(ref Unsafe.AsRef(from));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TTo AsMut<TFrom, TTo>(ref TFrom from) where TTo : struct where TFrom : struct
        {
            if (Unsafe.SizeOf<TFrom>() < Unsafe.SizeOf<TTo>()) { throw new InvalidCastException(); }
            return ref Unsafe.As<TFrom, TTo>(ref from);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<TTo> AsSpan<TFrom, TTo>(in TFrom from) where TTo : struct where TFrom : struct
        {
#if NETSTANDARD2_0
			var asSpan = CreateReadOnlySpan(ref Unsafe.AsRef(from));
#else
            var asSpan = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(from), 1);
#endif
            return MemoryMarshal.Cast<TFrom, TTo>(asSpan);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<TTo> AsMutableSpan<TFrom, TTo>(ref TFrom from) where TTo : struct where TFrom : struct
        {
#if NETSTANDARD2_0
			var asSpan = CreateSpan(ref Unsafe.AsRef(from));
#else
            var asSpan = MemoryMarshal.CreateSpan(ref from, 1);
#endif
            return MemoryMarshal.Cast<TFrom, TTo>(asSpan);
        }

#if NETSTANDARD2_0
		private static unsafe Span<T> CreateSpan<T>(ref T from) where T : struct
		{
			void* ptr = Unsafe.AsPointer(ref from);
			return new Span<T>(ptr, 1);
		}
		
		private static unsafe ReadOnlySpan<T> CreateReadOnlySpan<T>(ref T from) where T : struct
		{
			void* ptr = Unsafe.AsPointer(ref from);
			return new ReadOnlySpan<T>(ptr, 1);
		}
#endif


    }
}
