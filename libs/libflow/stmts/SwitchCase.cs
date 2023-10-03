using System.Collections.Generic;
using System;

namespace libflow.stmts
{
    public class SwitchCase
    {
        public SwitchCase(IConditional cond, IAstNode stmt)
        {
            Conditional = cond;
            Body = stmt;
        }

        public SwitchCase(IAstNode cond, IAstNode stmt)
        {
            Conditional = new InnerConditional(cond);
            Body = stmt;
        }

        internal IConditional Conditional { get; }

        public IAstNode Condition => Conditional?.Right;

        public IAstNode Body { get; set; }

        class InnerConditional : IConditional
        {
            public InnerConditional(IAstNode value)
            {
                Right = value;
            }

            public IAstNode Right { get; }

            public bool CanMerge => throw new System.NotImplementedException();

            IAstNode IConditional.Left 
            {
                get => throw new NotImplementedException();
                set { }
            }

            AstNodeType IAstNode.AstNodeType => Right.AstNodeType;

            public T Visit<T>(AstNodeVisitor<T> visitor)
            {
                return Right.Visit(visitor);
            }

            public IEnumerable<IAstNode> GetChildrens()
            {
                yield return Right;
            }

            public IEnumerable<IAstNode> GetStarts()
            {
                yield return Right;
            }

            public IEnumerable<IAstNode> GetEnds()
            {
                yield return Right;
            }

            public IEnumerable<IAstNode> Expand()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
