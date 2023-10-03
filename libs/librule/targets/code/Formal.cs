using libflow.stmts;

namespace librule.targets.code
{
    class Formal : Operator
    {
        public Formal(string name, string type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }

        public string Type { get; }

        public override IEnumerable<IAstNode> GetChildrens()
        {
            return Array.Empty<IAstNode>();
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            return Array.Empty<IAstNode>();
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            return Array.Empty<IAstNode>();
        }

        public override string ToString(CodeTargetVisitor visitor)
        {
            return Name;
        }
    }
}
