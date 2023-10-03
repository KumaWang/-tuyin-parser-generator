using System.Collections.Generic;

namespace libflow.stmts
{
    public class Switch : Branch
    {
        public Switch(IConditional cond, IReadOnlyList<SwitchCase> cases)
        {
            Condition = cond;
            Cases = cases;
        }

        public override IConditional Condition { get; set; }

        public IReadOnlyList<SwitchCase> Cases { get; }

        public override AstNodeType AstNodeType => AstNodeType.Switch;

        public override IEnumerable<IAstNode> GetChildrens()
        {
            yield return Condition;
            for (var i = 0; i < Cases.Count; i++)
            {
                if (Cases[i].Conditional != null)
                    yield return Cases[i].Conditional;

                yield return Cases[i].Body;
            }
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            yield return Condition;
            for (var i = 0; i < Cases.Count; i++)
                if (Cases[i].Conditional != null)
                    yield return Cases[i].Conditional;
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            for (var i = 0; i < Cases.Count; i++)
                yield return Cases[i].Body;
        }
    }
}
