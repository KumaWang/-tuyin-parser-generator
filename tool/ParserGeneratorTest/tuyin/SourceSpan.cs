using System;

namespace Tuitor.packages.richtext.format
{
    struct SourceSpan : IEquatable<SourceSpan>
    {
        public int Start { get; set; }

        public int End { get; set; }

        public int Length => End - Start;

        public SourceSpan(int start, int end)
        {
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

        public SourceSpan Combine(SourceSpan sourceSpan)
        {
            return new SourceSpan(Start < sourceSpan.Start ? Start : sourceSpan.Start, End > sourceSpan.End ? End : sourceSpan.End);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SourceSpan))
                return false;

            return Equals((SourceSpan)obj);
        }

        public override int GetHashCode()
        {
            return Start << 16 ^ End;
        }

        public bool Equals(SourceSpan other)
        {
            return Start == other.Start && End == other.End;
        }

        public override string ToString()
        {
            return string.Format("start {0} end {1}", Start, End);
        }
    }

}
