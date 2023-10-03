using libgraph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace libflow
{
    public sealed class GraphModel<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        private Dictionary<ushort, TEdge[]> lefts;
        private Dictionary<ushort, TEdge[]> rights;

        public GraphModel(IGraph<TVertex, TEdge> graph)
        {
            MaxState = graph.VerticesCount + 1;

            // 整理所有边，并根据源节点和目标节点进行分组
            rights = graph.Edges.GroupBy(x => x.Source.Index).ToDictionary(x => x.Key, x => x.ToArray());
            lefts = graph.Edges.GroupBy(x => x.Target.Index).ToDictionary(x => x.Key, x => x.ToArray());
        }

        public int MaxState { get; }

        public IReadOnlyList<TEdge> GetLefts(ushort state) => lefts.ContainsKey(state) ? lefts[state] : Array.Empty<TEdge>();

        public IReadOnlyList<TEdge> GetRights(ushort state) => rights.ContainsKey(state) ? rights[state] : Array.Empty<TEdge>();
    }
}
