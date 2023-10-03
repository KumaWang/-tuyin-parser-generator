using System;
using System.Collections.Generic;

namespace libgraph
{
    public class GraphException<TVertex, TEdge> : Exception
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        public GraphException(string message, params TEdge[] edges)
         : base(message)
        {
            Edges = edges;
        }

        public GraphException(string message, IReadOnlyList<TEdge> edges) 
            : base(message)
        {
            Edges = edges;
        }

        public IReadOnlyList<TEdge> Edges { get; }
    }
}
