using librule.utils;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace librule
{
    public unsafe struct StringRef : IEquatable<StringRef>
    {
        private char* mPtr;
        private int mHashCode;

        public int Length { get; }

        public char* IntPtr => mPtr;

        public unsafe char this[int index]
        {
            get { return *(mPtr + index); }
        }

        public StringRef(char* ptr, int length)
            : this(ptr, 0, length)
        {
        }

        public StringRef(char* ptr, int index, int length)
        {
            mPtr = ptr + index;
            Length = length;
            mHashCode = 0;
        }

        public unsafe StringRef(ReadOnlySpan<char> span)
            : this(span, 0, span.Length)
        {
        }

        public unsafe StringRef(ReadOnlySpan<char> span, int length)
            : this(span, 0, length)
        {
        }

        public unsafe StringRef(ReadOnlySpan<char> span, int index, int length)
        {
            mHashCode = 0;
            fixed (char* src = &span[0])
            {
                mPtr = src + index;
                Length = length;
            }
        }

        public int IndexOf(char c)
        {
            for (var i = 0; i < Length; i++)
            {
                if (this[i] == c)
                {
                    return i;
                }
            }

            return -1;
        }

        public StringRef Splice(int index, int length)
        {
            return new StringRef(mPtr + index, Length - length - index);
        }

        public StringRef Substring(int index, int length)
        {
            return new StringRef(mPtr + index, length);
        }

        public static implicit operator ReadOnlySpan<char>(StringRef value)
        {
            return value.AsSpan();
        }

        public ReadOnlySpan<char> AsSpan()
        {
            return new ReadOnlySpan<char>(mPtr, Length);
        }

        public ReadOnlySpan<char> AsSpan(int index, int length)
        {
            return new ReadOnlySpan<char>(IntPtr + index, length);
        }

        public override string ToString()
        {
            var chars = new char[Length];
            Marshal.Copy((IntPtr)mPtr, chars, 0, Length);
            return new string(chars); // mPtr, 0, Length);
        }

        public override bool Equals(object obj)
        {
            if (obj is StringRef @ref)
                return Equals(@ref);

            return false;
        }

        public bool Equals(StringRef other)
        {
            return GetHashCode().Equals(other.GetHashCode());
        }

        public override int GetHashCode()
        {
            if (Length == 0)
                return 0;

            if (mHashCode == 0)
                mHashCode = (int)HashCode32.Hash(new ReadOnlySpan<byte>(mPtr, Length * 2));

            return mHashCode;
        }

        public static bool operator ==(StringRef left, StringRef right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StringRef left, StringRef right)
        {
            return !left.Equals(right);
        }

        public static bool IsNullOrEmpty(StringRef @ref)
        {
            return @ref.GetHashCode() == 0;
        }

        public unsafe static StringRef Join(string v, StringRef[] fullPath)
        {
            var offset = 0;
            var chars = new char[v.Length * (fullPath.Length - 1) + fullPath.Sum(x => x.Length)];
            fixed (char* src = &chars[0], vSrc = &v.AsSpan()[0])
            {
                byte* bv = (byte*)vSrc;

                for (var i = 0; i < fullPath.Length; i++)
                {
                    var path = fullPath[i];

                    FastBuffer.ParallelBlockCopyLR((byte*)path.mPtr, (byte*)(src + offset * 2), path.Length * 2);

                    offset = offset + path.Length;
                    if (i < fullPath.Length - 1)
                    {
                        FastBuffer.ParallelBlockCopyLR(bv, (byte*)(src + offset * 2), v.Length * 2);
                        offset = offset + v.Length;
                    }
                }
            }
            return new StringRef(chars);
        }

        public static implicit operator string(StringRef @ref)
        {
            return @ref.ToString();
        }
    }
}
