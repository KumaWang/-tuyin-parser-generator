using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Tuitor.packages.richtext.format
{
    struct Match : IEquatable<Match>
    {
        public Match(SourceSpan sourceSpan, ushort token)
        {
            SourceSpan = sourceSpan;
            Token = token;
        }

        public Match(ushort token, int start, int end)
        {
            Token = token;
            SourceSpan = new SourceSpan(start, end);
        }

        public SourceSpan SourceSpan { get; }

        public ushort Token { get; }

        public string GetContent(string str) => str.Substring(SourceSpan.Start, SourceSpan.Length);

        public unsafe string GetContent(char* intptr) => new string(intptr, SourceSpan.Start, SourceSpan.Length);

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return obj is Match && Equals((Match)obj);
        }

        public bool Equals(Match other)
        {
            return SourceSpan.Equals(other.SourceSpan) && Token.Equals(other.Token);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SourceSpan, Token);
        }

        public static implicit operator bool(Match match) => match.Token != 0;

        public static implicit operator ushort(Match match) => match.Token;

        public static Match operator |(Match left, Match right) => !left.Equals(default) ? left : right;
    }
}
