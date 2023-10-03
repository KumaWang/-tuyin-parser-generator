using System.Collections.Generic;

namespace libflow.stmts
{
    public class If : Branch
    {
        public If(IConditional cond, IAstNode consequent)
        {
            Condition = cond;
            Consequent = consequent;
        }

        public override AstNodeType AstNodeType => AstNodeType.If;

        public override IConditional Condition { get; set; }

        public IAstNode Consequent { get; }

        public override IEnumerable<IAstNode> GetChildrens()
        {
            yield return Condition;
            yield return Consequent;
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            yield return Condition;
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            yield return Consequent;
        }
    }
}
