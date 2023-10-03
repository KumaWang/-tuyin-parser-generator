using System.Collections;
using System.Runtime.CompilerServices;

namespace Tuitor.utils
{
    public interface IReadOnlyArray<out T> : IReadOnlyCollection<T>, IEnumerable<T>
    {
        T this[int index] { get; }

        int Length { get; }
    }

    public interface IArray<T> : IReadOnlyArray<T>
    {
        new T this[int index] { get; set; }

        void Clear();

        void Resize(int length);
    }

    public class DynamicArray<T> : IArray<T>, IDisposable
    {
        private int mLength;
        private T[] mItems;

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= mItems.Length)
                    return default;

                //CheckLength(index + 1);
                return mItems[index];
            }
            set
            {
                CheckLength(index + 1);
                mItems[index] = value;
            }
        }

        public T[] Items => mItems;

        public int Length
        {
            get { return mLength; }
            set { mLength = value; }
        }

        public int Count => Length;

        public void Resize(int length)
        {
            Array.Resize(ref mItems, length);
        }

        public DynamicArray(int size)
        {
            mItems = new T[Math.Max(4, size)];
        }

        public bool Contains(T item)
        {
            for (var i = 0; i < mLength; i++)
                if (mItems[i].Equals(item))
                    return true;

            return false;
        }

        public void Clear()
        {
            mLength = 0;
        }

        public void DeepClear() 
        {
            for (var i = 0; i < mLength; i++)
                mItems[i] = default;

            mLength = 0;
        }

        private void CheckLength(int length)
        {
            if (mLength < length)
            {
                mLength = length;
            }

            lock (mItems)
            {
                if (mItems.Length <= length)
                {
                    Array.Resize(ref mItems, length * 2);
                }
            }
        }

        internal void Scale(int index, int count)
        {
            for (var i = 0; i < count; i++)
            {
                Insert(index, default);
            }
        }

        public void Insert(int index, T v)
        {
            CheckLength(mLength + 1);
            for (var i = mLength; i > index; i--)
            {
                mItems[i] = mItems[i - 1];
            }

            mItems[index] = v;
        }

        public void RemoveAt(int index)
        {
            for (var i = index; i < mLength - 1; i++)
            {
                mItems[i] = mItems[i + 1];
            }

            mLength--;
        }

        public void RemoveRange(int index, int length) 
        {
            for (var i = index + length; i < mLength - 1; i++)
                mItems[i - length] = mItems[i];

            mLength = mLength - length;
        }

        public void Add(T item)
        {
            this[mLength] = item;
        }

        public void Remove(T item)
        {
            var index = FindIndex(0, item);
            if (index != -1)
            {
                RemoveAt(index);
            }
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


        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < mLength; i++)
                yield return mItems[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            mLength = 0;
            mItems = null;
        }
    }
}
