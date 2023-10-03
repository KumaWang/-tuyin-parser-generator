using System;
using System.Collections.Generic;

namespace libflow.stmts
{
    public class DefineLabel : Statement
    {
        public DefineLabel(List<Label> labels)
        {
            Labels = labels;
        }

        public override AstNodeType AstNodeType => AstNodeType.Label;

        public List<Label> Labels { get; }

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
