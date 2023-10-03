using System;
using System.Collections.Generic;

namespace libflow.stmts
{
    public class Empty : Statement
    {
        public Empty()
        {
        }

        public override AstNodeType AstNodeType => AstNodeType.Empty;

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
