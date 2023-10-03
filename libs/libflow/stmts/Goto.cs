using System;
using System.Collections.Generic;

namespace libflow.stmts
{
    public class Goto : Statement
    {
        public Goto(Label label)
        {
            Label = label;
        }

        public override AstNodeType AstNodeType => AstNodeType.Goto;

        public Label Label { get; }

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
