using System.Collections.Generic;

namespace libflow.stmts
{
    public class IfElse : If
    {
        public IfElse(IConditional cond, IAstNode consequent, IAstNode alternate)
            : base(cond, consequent)
        {
            Alternate = alternate;
        }

        public override AstNodeType AstNodeType => AstNodeType.IfElse;

        public IAstNode Alternate { get; }

        public IAstNode FindLastAlternate() 
        {
            if (Alternate is IfElse ifelse)
                return ifelse.FindLastAlternate();

            return Alternate;
        }

        public override IEnumerable<IAstNode> GetChildrens()
        {
            yield return Condition;
            yield return Consequent;
            yield return Alternate;
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            yield return Condition;
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            yield return Consequent;
            yield return Alternate;
        }
    }
}
