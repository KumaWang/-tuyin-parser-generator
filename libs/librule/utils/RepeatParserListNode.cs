using System.Collections;

namespace librule.utils
{
    sealed class RepeatParserListNode<T> : IEnumerable<T>
    {
        public RepeatParserListNode(T value, IEnumerable<T> next)
        {
            Value = value;
            Next = next;
        }

        public RepeatParserListNode() : this(default, null) { }

        public T Value { get; private set; }

        public IEnumerable<T> Next { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            yield return Value;
            if (Next != null)
                foreach (var item in Next)
                    yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}