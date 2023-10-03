using System.Collections.Generic;

namespace libflow.stmts
{
    public class While : Statement
    {
        public While(IConditional cond, IAstNode stmt)
        {
            Condition = cond;
            Body = stmt;
        }

        public override AstNodeType AstNodeType => AstNodeType.While;

        public IConditional Condition { get; }

        public IAstNode Body { get; }

        public override IEnumerable<IAstNode> GetChildrens()
        {
            yield return Condition;
            yield return Body;
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            yield return Condition;
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            yield return Body;
        }
    }
}
