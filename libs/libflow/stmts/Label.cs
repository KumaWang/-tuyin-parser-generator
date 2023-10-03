using System;

namespace libflow.stmts
{
    public sealed class Label : IEquatable<Label>
    {
        public Label(ushort index)
        {
            Index = index;
        }

        public ushort Index { get; }

        public bool Equals(Label other)
        {
            return Index == other.Index;
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }
    }
}
