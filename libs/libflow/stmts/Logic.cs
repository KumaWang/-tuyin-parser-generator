using System.Collections.Generic;

namespace libflow.stmts
{

    public class Logic : Expression, IConditional
    {
        public Logic(IAstNode left, IAstNode right, LogicType type)
        {
            Left = left;
            Right = right;
            Type = type;
        }

        int IConditional.ConditionalCount =>
            ((Left is IConditional lc) ? lc.ConditionalCount : 0) +
            ((Right is IConditional rc) ? rc.ConditionalCount : 1);

        public override AstNodeType AstNodeType => AstNodeType.Binary;

        public LogicType Type { get; }

        public IAstNode Left { get; set; }

        public IAstNode Right { get; }

        public bool CanMerge => true;

        public IEnumerable<IAstNode> Expand(LogicType binaryType) 
        {
            if (Type == binaryType)
            {
                if (Left is Logic leftBranch && leftBranch.Type == binaryType)
                    foreach (var item in leftBranch.Expand(binaryType))
                        yield return item;
                else
                    yield return Left;

                if (Right is Logic rightBranch && rightBranch.Type == binaryType)
                    foreach (var item in rightBranch.Expand(binaryType))
                        yield return item;
                else
                    yield return Right;
            }
            else yield return this;
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
    }
}
