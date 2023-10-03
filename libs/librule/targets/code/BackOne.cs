using libflow.stmts;

namespace librule.targets.code
{
    class BackOne : Operator
    {
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

        public override string ToString(CodeTargetVisitor visitor)
        {
            return $"{Settings.INDEX_LITERAL}--;";
        }
    }
}
