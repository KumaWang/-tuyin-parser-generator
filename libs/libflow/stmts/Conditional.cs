using System.Collections.Generic;

namespace libflow.stmts
{
    public class Conditional : Expression
    {
        public Conditional(IConditional condition, IAstNode consequent, IAstNode alternate)
        {
            Condition = condition;
            Consequent = consequent;
            Alternate = alternate;
        }

        public IConditional Condition { get; set; }

        public IAstNode Consequent { get; }

        public IAstNode Alternate { get; }

        public override AstNodeType AstNodeType => AstNodeType.Conditional;

        public IAstNode Left => Condition;

        public IAstNode Right => Consequent;

        public bool CanMerge => false;

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
