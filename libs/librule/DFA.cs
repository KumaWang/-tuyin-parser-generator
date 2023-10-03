using libfsm;

namespace librule
{
    public class DFA<T>
    {
        private FAValue<T>[,] mData;

        public int Length0 => mData.GetLength(0);

        public int Length1 => mData.GetLength(1);

        public FAValue<T> this[int w, int h] => mData[w, h];

        public DFA(FAValue<T>[,] mData)
        {
            this.mData = mData;
        }
    }
}
