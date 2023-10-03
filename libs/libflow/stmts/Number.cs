using System;
using System.Collections.Generic;

namespace libflow.stmts
{
    public class Number : Expression
    {
        public Number(decimal value) 
        {
            Value = value;
        }

        public decimal Value { get; }

        public override AstNodeType AstNodeType => AstNodeType.Number;

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
