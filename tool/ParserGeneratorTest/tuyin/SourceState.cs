namespace Tuitor.packages.richtext.format
{
    struct SourceState
    {
        public SourceState(int index, ushort state)
        {
            Index = index;
            State = state;
        }

        public int Index { get; }

        public ushort State { get; }
    }
}
