using libflow.stmts;

namespace librule.targets.code
{
    class Skip : Operator
    {
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
            return visitor.HasSkipTable ? $"{new Call(visitor.SkipTableName, true, visitor.UserFormals).Visit(visitor)};\n" : string.Empty;
        }
    }
}