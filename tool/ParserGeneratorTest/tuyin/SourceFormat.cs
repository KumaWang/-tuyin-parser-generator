namespace Tuitor.packages.richtext.format
{
    struct SourceFormat
    {
        public SourceFormat(int index, int indent, int interval)
        {
            Index = index;
            Indent = indent;
            Interval = interval;
        }

        public int Index { get; }

        public int Indent { get; }

        public int Interval { get; }
    }
}
