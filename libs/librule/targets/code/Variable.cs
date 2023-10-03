using libflow.stmts;

namespace librule.targets.code
{
    class Variable : Operator, IEquatable<Variable>
    {
        public Variable(ushort state, string name)
        {
            State = state;
            Name = name;
        }

        public ushort State { get; }

        public string Name { get; }

        public override IEnumerable<IAstNode> GetChildrens()
        {
            return Array.Empty<IAstNode>();
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            return Array.Empty<IAstNode>();
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            return Array.Empty<IAstNode>();
        }

        public override bool Equals(object obj)
        {
            return obj is Variable other && this.Equals(other);
        }

        public bool Equals(Variable other)
        {
            return State == other?.State;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(State);
        }

        public override string ToString()
        {
            return Name;
        }

        public override string ToString(CodeTargetVisitor visitor)
        {
            return Name;
        }
    }
}
