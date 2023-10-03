using libflow.stmts;

namespace librule.targets.code
{
    class Metadata : Operator
    {
        public Metadata(ITargetMetadata context)
        {
            Context = context;
        }

        public ITargetMetadata Context { get; internal set; }

        public override IEnumerable<IAstNode> GetChildrens()
        {
            if (Context is TargetStatement control)
                yield return control.Statement;
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            if (Context is TargetStatement control)
                yield return control.Statement;
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            if (Context is TargetStatement control)
                yield return control.Statement;
        }

        public override string ToString(CodeTargetVisitor visitor)
        {
            if (Context == null)
                return string.Empty;

            return Context.Visit(visitor);
        }
    }
}
