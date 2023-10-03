using System;
using System.Collections.Generic;

namespace libflow.stmts
{
    public class Break : Statement
    {
        public override AstNodeType AstNodeType => AstNodeType.Break;

        public override IEnumerable<IAstNode> GetChildrens()
        {
            return Array.Empty<IAstNode>();
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            return Array.Empty<IAstNode>();
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            return Array.Empty<IAstNode>();
        }
    }
}
