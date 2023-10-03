using System.Collections;
using System.Runtime.CompilerServices;

namespace librule.utils
{
    public interface IReadOnlyArray<out T> : IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>
    {
        int Length { get; }
    }

    public interface IArray<T> : IReadOnlyArray<T>
    {
        new T this[int index] { get; set; }

        void Clear();

        void Resize(int length);
    }

    public class DynamicArray<T> : IArray<T>
    {
        private static int SIZE = Unsafe.SizeOf<T>();

        private int mLength;
        private T[] mItems;

        public T[] Items => mItems;

        public T this[int index]
        {
            get
            {
                return mItems[index];
            }
            set
            {
                CheckLength(index + 1);
                mItems[index] = value;
            }
        }

        public int Length
        {
            get { return mLength; }
            set { mLength = value; }
        }

        public int Count => Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int length)
        {
            Array.Resize(ref mItems, length);
        }

        public DynamicArray(int size)
        {
            mItems = new T[size];
        }

        public DynamicArray(List<T> items)
            : this(items.Count, items)
        {
        }

        public DynamicArray(int count, List<T> items)
            : this(Math.Max(count, items.Count))
        {
            for (var i = 0; i < items.Count; i++)
            {
                mItems[i] = items[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            this[mLength] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            mLength = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckLength(int length)
        {
            if (mLength < length)
                mLength = length;

            lock (mItems)
            {
                if (mItems.Length <= length)
                    Array.Resize(ref mItems, length * 2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Insert(int index, T v)
        {
            CheckLength(mLength + 1);
            var dst = (byte*)Unsafe.AsPointer(ref mItems[index + 1]);
            var src = (byte*)Unsafe.AsPointer(ref mItems[index]);
            FastBuffer.ParallelBlockCopyRL(src, dst, (mLength - index) * SIZE);
            mItems[index] = v;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < mLength; i++)
                yield return mItems[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Remove(T item)
        {
            var index = FindIndex(0, item);
            if (index != -1)
            {
                RemoveAt(index);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
            for (var i = index; i < mLength - 1; i++)
            {
                mItems[i] = mItems[i + 1];
            }

            mLength--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int FindIndex(int index, T target)
        {
            for (var i = index; i < mItems.Length; i++)
            {
                var item = mItems[i];
                if (Equals(item, target))
                {
                    return i;
                }
            }

            return -1;
        }

        public DynamicArray<T> Clone()
        {
            var clone = new DynamicArray<T>(Length);
            Array.Copy(mItems, clone.mItems, Length);
            return clone;
        }
    }

    class IntArray : IEnumerable<int>
    {
        private int mLength;
        private int[] mItems;
        private List<ScaleStruct> mScales;
        private object mLock = new object();

        public int this[int index]
        {
            get
            {
                if (index < mItems.Length)
                    return mItems[index];

                return 0;
            }
            set
            {
                CheckLength(index + 1);
                mItems[index] = value;
            }
        }

        public int Length => mLength;

        public IntArray(int size)
        {
            mItems = new int[size];
            mScales = new List<ScaleStruct>(size / 4);
        }

        public void Clear()
        {
            mLength = 0;
        }

        private void CheckLength(int length)
        {
            if (mLength < length)
            {
                mLength = length;
            }

            if (mItems.Length <= length)
            {
                lock (mLock)
                {
                    Array.Resize(ref mItems, Math.Max(1, mItems.Length) * 2);
                }
            }
        }

        public unsafe void Scale(int index, int count)
        {
            var endIndex = mLength;
            CheckLength(mLength + count);
            fixed (int* end = &mItems[mLength], start = &mItems[index])
            {
                var ptr = end;
                for (var i = endIndex; i >= index; i--)
                {
                    *ptr-- = mItems[i];
                }

                ptr = start;
                for (var i = 0; i < count; i++)
                {
                    *ptr++ = 0;
                }
            }
        }

        public unsafe void Final()
        {
            if (mScales.Count == 0) return;

            var mapping = mLength - 1;
            // 计算获得最大长度
            var length = mScales.Sum(x => x.Length);
            CheckLength(mLength + length);

            var scaleIndex = mScales.Count - 1;
            var scale = mScales[scaleIndex];

            var index = mLength - 1;

            // 从后往前
            fixed (int* start = &mItems[mLength])
            {
                int* ptr = start - 1;
                while (mapping > 0)
                {
                    mItems[index] = mItems[mapping];

                    if (scale != null && mapping < scale.Index)
                    {
                        index = scale.Index;

                        for (var i = 0; i < scale.Length; i++)
                        {
                            mItems[index + i] = 0;
                        }

                        if (scaleIndex > 0)
                        {
                            scale = mScales[--scaleIndex];
                        }
                        else
                        {
                            scale = null;
                        }
                    }

                    index--;
                    mapping--;
                }
            }
        }

        public IEnumerator<int> GetEnumerator()
        {
            for (var i = 0; i < mLength; i++)
                yield return mItems[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class ScaleStruct
        {
            public int Index { get; }

            public int Length { get; }

            public ScaleStruct(int index, int length)
            {
                Index = index;
                Length = length;
            }
        }
    }
}
