using System;
using System.Collections.Generic;

namespace libflow.stmts
{
    public class Boolean : Expression
    {
        public Boolean(bool value)
        {
            Value = value;
        }

        public bool Value { get; }

        public override AstNodeType AstNodeType => AstNodeType.Boolean;

        public override IEnumerable<IAstNode> GetChildrens()
        {
            return Array.Empty<IAstNode>();
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            return Array.Empty<IAstNode>();
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            return Array.Empty<IAstNode>();
        }
    }
}
