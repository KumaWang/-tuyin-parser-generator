using System.Collections.Generic;

namespace libflow.stmts
{
    public class Postfix : Expression
    {
        public Postfix(IAstNode source, PostfixType postfixType)
        {
            Source = source;
            PostfixType = postfixType;
        }

        public IAstNode Source { get; }

        public PostfixType PostfixType { get; }

        public override AstNodeType AstNodeType => AstNodeType.Postfix;

        public override IEnumerable<IAstNode> GetChildrens()
        {
            yield return Source;
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            yield return Source;
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            yield return Source;
        }
    }
}
