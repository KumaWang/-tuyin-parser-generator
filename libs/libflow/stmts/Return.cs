using System.Collections.Generic;

namespace libflow.stmts
{
    public class Return : Statement
    {
        public Return() 
        {
        }

        public Return(IAstNode value)
        {
            ReturnValue = value;
        }

        public IAstNode ReturnValue { get; }

        public override AstNodeType AstNodeType => AstNodeType.Return;

        public override IEnumerable<IAstNode> GetChildrens()
        {
            if (ReturnValue != null) yield return ReturnValue;
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            if (ReturnValue != null) yield return ReturnValue;
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            if (ReturnValue != null) yield return ReturnValue;
        }
    }
}
