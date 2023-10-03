namespace librule
{
    public struct SourceLocation : IEquatable<SourceLocation>
    {
        public int Line { get; }

        public int Start { get; }

        public int End { get; }

        public int Length => End - Start;

        public SourceLocation(int line, int start, int end)
        {
            Line = line;
            Start = start;
            End = end;
        }

        public bool Contains(int charIndex)
        {
            return charIndex >= Start && charIndex <= End;
        }

        public bool Contains(SourceSpan span)
        {
            return Contains(span.Start) && Contains(span.End);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SourceLocation))
                return false;

            return Equals((SourceLocation)obj);
        }

        public override int GetHashCode()
        {
            return Line ^ Start << 16 ^ End;
        }

        public bool Equals(SourceLocation other)
        {
            return Line == other.Line && Start == other.Start && End == other.End;
        }

        public override string ToString()
        {
            return string.Format("line {0} start {1} end {2}", Line, Start, End);
        }
    }

}
