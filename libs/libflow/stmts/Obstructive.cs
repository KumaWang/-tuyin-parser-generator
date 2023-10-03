using System.Collections.Generic;

namespace libflow.stmts
{
    public class Obstructive : Statement, IObstructive
    {
        public Obstructive(IConditional condition, IAstNode reason)
        {
            Condition = condition;
            Reason = reason;
        }

        public IConditional Condition { get; set; }

        public IAstNode Reason { get; set; }

        public override AstNodeType AstNodeType => AstNodeType.Obstructive;

        public override IEnumerable<IAstNode> GetChildrens()
        {
            yield return Condition;
            yield return Reason;
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            if (Reason != null)
                yield return Reason;
            else
                yield return Condition;
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            yield return Condition;
        }
    }
}
