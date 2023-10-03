using System.Collections.Generic;

namespace libflow.stmts
{
    public class DoWhile : Statement
    {
        public DoWhile(IConditional cond, IAstNode stmt)
        {
            Condition = cond;
            Body = stmt;
        }

        public override AstNodeType AstNodeType => AstNodeType.DoWhile;

        public IConditional Condition { get; }

        public IAstNode Body { get; }

        public override IEnumerable<IAstNode> GetChildrens()
        {
            yield return Body;
            yield return Condition;
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            yield return Body;
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            yield return Condition;
        }
    }
}
