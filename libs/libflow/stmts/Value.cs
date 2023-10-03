using System.Collections.Generic;

namespace libflow.stmts
{
    public class Value : Expression, IConditional
    {
        public Value(IAstNode source)
        {
            Left = source;
        }

        public override AstNodeType AstNodeType => AstNodeType.Value;

        public IAstNode Left { get; set; }

        IAstNode IConditional.Right => null;

        public bool CanMerge => true;

        public override IEnumerable<IAstNode> GetChildrens()
        {
            yield return Left;
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            yield return Left;
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            yield return Left;
        }
    }
}
