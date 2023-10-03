using System.Collections.Generic;

namespace libflow.stmts
{
    public interface IAstNode
    {
        bool IsVaild => true;

        AstNodeType AstNodeType { get; }

        IEnumerable<IAstNode> GetChildrens();

        IEnumerable<IAstNode> GetStarts();

        IEnumerable<IAstNode> GetEnds();

        IEnumerable<IAstNode> GetExpand() 
        {
            foreach (var child in GetChildrens())
            {
                if (child.AstNodeType == AstNodeType.Concatenation)
                    foreach (var subChild in child.GetExpand())
                        yield return subChild;
                else
                    yield return child;
            }
        }

        T Visit<T>(AstNodeVisitor<T> visitor);
    }
}
