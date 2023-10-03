using System;
using System.Collections.Generic;
using System.Linq;

namespace libgraph
{
    public static class GraphExtands 
    {
        public static IEnumerable<TVertex> GetVertices<TVertex, TEdge>(this IGraph<TVertex, TEdge> graph)
            where TEdge : IEdge<TVertex>
            where TVertex : IVertex
        {
            return graph.Vertices;
        }
    }

    public interface IGraph<TVertex, TEdge>
        where TEdge : IEdge<TVertex> 
        where TVertex : IVertex
    {
        /// <summary>
        /// 获取顶点数量
        /// </summary>
        int VerticesCount => Vertices.Count();

        /// <summary>
        /// 获取顶点集合
        /// </summary>
        IEnumerable<TVertex> Vertices => Edges.Select(x => x.Target).Union(Edges.Select(x => x.Source)).Where(x => x != null).Distinct();

        /// <summary>
        /// 获取连接边
        /// </summary>
        IReadOnlyList<TEdge> Edges { get; }

        DebugGraph CreateDebugGraph(Func<TEdge, string> descrptioner) 
        {
            var graph = new DebugGraph("debug");
            var debugVertexs = new Dictionary<int, DebugVertex>();
            foreach (var vertex in Vertices)
                debugVertexs[vertex.Index] = new DebugVertex(vertex.Index, vertex.Index.ToString(), VertexFlags.None);

            foreach (var edge in Edges)
                graph.AddEdge(new DebugEdge(edge.Flags, descrptioner(edge), debugVertexs[edge.Source.Index], debugVertexs[edge.Target.Index], edge.Subset == null ? null : debugVertexs[edge.Subset.Index]));

            return graph;
        }
    }
}
