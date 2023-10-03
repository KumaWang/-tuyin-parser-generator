using libflow.stmts;

namespace librule.targets.code
{
    internal class Literal : Operator, IEquatable<Literal>
    {
        private string content;

        public Literal(string ctx)
        {
            this.content = ctx;
        }

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
            return obj is Literal other && Equals(other);
        }

        public bool Equals(Literal other)
        {
            return content == other.content;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AstNodeType, content);
        }

        public override string ToString(CodeTargetVisitor visitor)
        {
            return content;
        }
    }
}
