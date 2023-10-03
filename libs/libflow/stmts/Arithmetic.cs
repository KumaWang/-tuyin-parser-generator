using System;
using System.Collections.Generic;

namespace libflow.stmts
{
    public class Arithmetic : Expression, IArithmetic
    {
        public Arithmetic(IAstNode left, IAstNode right, ArithmeticType type)
        {
            Left = left;
            Right = right;
            Type = type;
        }

        public IAstNode Left { get; }

        public IAstNode Right { get; }

        public ArithmeticType Type { get; }

        public override AstNodeType AstNodeType => AstNodeType.Arithmetic;

        public override IEnumerable<IAstNode> GetChildrens()
        {
            yield return Left;
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            yield return Right;
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            yield return Left;
            yield return Right;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Left.GetHashCode(), Right.GetHashCode(), Type.GetHashCode());
        }
    }
}
