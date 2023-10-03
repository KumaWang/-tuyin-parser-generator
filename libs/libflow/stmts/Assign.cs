using System;
using System.Collections.Generic;

namespace libflow.stmts
{
    public class Assign : Expression, IArithmetic, IConditional
    {
        public Assign(IAstNode left, IAstNode right)
        {
            Left = left;
            Right = right;
        }

        public IAstNode Left { get; set; }

        public IAstNode Right { get; }

        public bool CanMerge => false;

        public override AstNodeType AstNodeType => AstNodeType.Assign;

        public override IEnumerable<IAstNode> GetChildrens()
        {
            yield return Left;
            yield return Right;
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            yield return Right;
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            yield return Left;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Left.GetHashCode(), Right.GetHashCode());
        }
    }
}
