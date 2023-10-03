using libflow.stmts;

namespace librule.targets.code
{
    abstract class Operator : IAstNode
    {
        public AstNodeType AstNodeType => AstNodeType.External;

        public virtual T Visit<T>(AstNodeVisitor<T> visitor) => visitor.Visit(this);

        public abstract IEnumerable<IAstNode> GetChildrens();

        public abstract IEnumerable<IAstNode> GetStarts();

        public abstract IEnumerable<IAstNode> GetEnds();

        public abstract string ToString(CodeTargetVisitor visitor);
    }
}
