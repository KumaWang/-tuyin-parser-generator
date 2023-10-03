using libflow;
using libflow.stmts;
using libgraph;
using librule.generater;

namespace librule.targets.code
{
    internal class TokenConstructor : AstConstructor<GraphState<TargetMetadatas>, GraphEdge<TargetMetadatas>>
    {
        private CodeTargetVisitor mVisitor;
        private TargetBuilder mTargetBuilder;

        public TokenConstructor(CodeTargetVisitor visitor, TargetBuilder builder)
        {
            mVisitor = visitor;
            mTargetBuilder = builder;
        }

        public override AstCreateFlags CreateFlags => AstCreateFlags.LimitIf;

        public override int OverBranchCountToSwitch => 3;

        public TargetBuilder TargetBuilder => mTargetBuilder;

        public CodeTargetVisitor CodeTargetVisitor => mVisitor;

        /// <summary>
        /// 根据边创建文法
        /// </summary>
        /// <param name="edge">所使用的边</param>
        /// <param name="conditional">代数,如果该值不为null且与当前步骤所使用的判断条件一致
        /// 则代表该步骤的判断条件已经被上层所使用过,需要在这里直接去除掉</param>
        public override IAstNode CreateStatement(GraphEdge<TargetMetadatas> edge, IConditional conditional)
        {
            if (edge.Metadata.Count > 0)
            {
                var metas = new List<IAstNode>();
                for (var i = 0; i < edge.Metadata.Count; i++)
                {
                    var metadata = edge.Metadata[i];
                    if (!metadata.IsVaild)
                        continue;

                    if (metadata is TargetStatement control)
                        metas.Add(control.Statement);
                    else
                        metas.Add(new Metadata(metadata));
                }

                // 如果是终结点则代表它是一个可以被返回的值
                if (edge.Flags.HasFlag(EdgeFlags.SpecialPoint))
                    metas[^1] = new Return(metas[^1]);

                IAstNode stmt = metas.Count > 0 ? metas[0] : null;
                for (var i = 1; i < metas.Count; i++)
                    stmt = new Concatenation(stmt, metas[i]);

                if (mVisitor.Debug)
                {
                    stmt = stmt == null ?
                        new Debug($"{edge.Source.Index}->{edge.Target.Index}\n") :
                        new Concatenation(new Debug($"{edge.Source.Index}->{edge.Target.Index}\n"), stmt);
                }

                return stmt;
            }

            return null;
        }

        public override IObstructive CreateObstructiveBranch(GraphEdge<TargetMetadatas> edge, IConditional condition)
        {
            return new Obstructive(condition, mVisitor.CreateObstructiveReason(edge, condition));
        }

        public override IAstNode CreateWaitOne(GraphEdge<TargetMetadatas> edge)
        {
            return new BackOne();
        }

        public override IAstNode GetIllegalValue(GraphEdge<TargetMetadatas>[] edges)
        {
            return null;
        }
    }
}
