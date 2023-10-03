using System.Collections.Generic;

namespace libflow.stmts
{
    public class Parenthese : Expression
    {
        public Parenthese(IAstNode node)
        {
            Node = node;
        }

        public IAstNode Node { get; set; }

        public override AstNodeType AstNodeType => AstNodeType.Parenthese;

        public override IEnumerable<IAstNode> GetChildrens()
        {
            yield return Node;
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            yield return Node;
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            yield return Node;
        }
    }
}
