using libflow.stmts;
using libgraph;
using System.Collections.Generic;

namespace libflow
{
    /// <summary>
    /// 文法构造器
    /// </summary>
    public abstract class AstConstructor<TVertex, TEdge>
        where TVertex : IVertex
        where TEdge : IEdge<TVertex>
    {
        public abstract AstCreateFlags CreateFlags { get; }

        /// <summary>
        /// 获取超出N个分支条件时转换成switch文法
        /// </summary>
        public abstract int OverBranchCountToSwitch { get; }

        /// <summary>
        /// 创建边所使用的文法
        /// </summary>
        /// <param name="edge">边</param>
        /// <param name="conditional">传入条件</param>
        public abstract IAstNode CreateStatement(TEdge edge, IConditional conditional);

        /// <summary>
        /// 获取边的统一非法条件 
        /// </summary>
        public abstract IAstNode GetIllegalValue(TEdge[] edges);

        /// <summary>
        /// 创建条件分支
        /// </summary>
        public abstract IObstructive CreateObstructiveBranch(TEdge edge, IConditional readCondition);

        /// <summary>
        /// 创建等待步
        /// </summary>
        public abstract IAstNode CreateWaitOne(TEdge edge);

        /// <summary>
        /// 在进入子图时
        /// </summary>
        /// <param name="entry">子图入口点</param>
        /// 
        public virtual void OnEntry(ushort entry) { }
    }
}
