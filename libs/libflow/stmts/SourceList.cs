using System.Collections.Generic;

namespace libflow.stmts
{
    class SourceList : List<IAstNode>, IAstNode
    {
        public AstNodeType AstNodeType => throw new System.NotImplementedException();

        public IEnumerable<IAstNode> GetChildrens()
        {
            return this;
        }

        public IEnumerable<IAstNode> GetEnds()
        {
            yield return this[^1];
        }

        public IEnumerable<IAstNode> GetStarts()
        {
            yield return this[0];
        }

        public T Visit<T>(AstNodeVisitor<T> visitor)
        {
            throw new System.NotImplementedException();
        }
    }
}
