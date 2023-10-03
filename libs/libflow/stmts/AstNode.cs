using System.Collections.Generic;

namespace libflow.stmts
{
    public abstract class AstNode : IAstNode
    {
        public abstract AstNodeType AstNodeType { get; }

        public abstract IEnumerable<IAstNode> GetChildrens();

        public abstract IEnumerable<IAstNode> GetStarts();

        public abstract IEnumerable<IAstNode> GetEnds();

        public virtual T Visit<T>(AstNodeVisitor<T> visitor) => visitor.Visit(this);
    }
}
