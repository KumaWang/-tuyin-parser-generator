using System;
using System.Collections.Generic;

namespace libflow.stmts
{
    public class For : Statement
    {
        public For(IAstNode init, IConditional cond, IAstNode end, IAstNode stmt)
        {
            Condition = cond;
        }

        public override AstNodeType AstNodeType => AstNodeType.For;

        public IConditional Condition { get; }

        public override IEnumerable<IAstNode> GetChildrens()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            throw new NotImplementedException();
        }
    }
}
