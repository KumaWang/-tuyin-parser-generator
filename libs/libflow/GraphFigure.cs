using libgraph;
using System.Collections.Generic;

namespace libflow
{
    public class GraphFigure<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        public GraphFigure(ushort entryPoint)
        {
            EntryPoint = entryPoint;
            InDeeps = new Dictionary<ushort, int>();
            Relations = new Dictionary<ushort, HashSet<ushort>>();
            OwnerLayer = new Dictionary<ushort, ushort>();
        }

        /// <summary>
        /// 图结构入口
        /// </summary>
        public ushort EntryPoint { get; }

        /// <summary>
        /// 状态点入度
        /// </summary>
        public Dictionary<ushort, int> InDeeps { get; }

        /// <summary>
        /// 全部的层连接关系
        /// </summary>
        public Dictionary<ushort, HashSet<ushort>> Relations { get; }

        /// <summary>
        /// 状态点所属的层入口
        /// </summary>
        public Dictionary<ushort, ushort> OwnerLayer { get; }

        /// <summary>
        /// 获取左侧层是否会连接到右侧层
        /// </summary>
        /// <param name="left">左侧层</param>
        /// <param name="right">右侧层</param>
        public bool IsConnect(FlowLayer<TVertex, TEdge> left, FlowLayer<TVertex, TEdge> right)
        {
            return Relations.ContainsKey(left.State) && Relations[left.State].Contains(right.State);
        }
    }
}
