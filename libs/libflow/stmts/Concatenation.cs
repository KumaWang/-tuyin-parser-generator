using System.Collections.Generic;

namespace libflow.stmts
{
    public class Concatenation : Statement
    {
        public Concatenation(IAstNode left, IAstNode right)
        {
            Left = left;
            Right = right;
        }

        public override AstNodeType AstNodeType => AstNodeType.Concatenation;

        public IAstNode Left { get; set; }

        public IAstNode Right { get; set; }

        public IAstNode GetLast()
        {
            if (Right is Concatenation rightCC)
                return rightCC.GetLast();
            else
                return Right;
        }

        public override IEnumerable<IAstNode> GetChildrens()
        {
            yield return Left;
            yield return Right;
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            yield return Left;
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            yield return Right;
        }

        public static IAstNode From(params IAstNode[] nodes)
        {
            var first = nodes[0];
            for (var i = 1; i < nodes.Length; i++)
                first = new Concatenation(first, nodes[i]);

            return first;
        }
    }
}
