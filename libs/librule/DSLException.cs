using librule.generater;

namespace librule
{
    public class DSLException<TMetadata> : Exception
    {
        internal DSLException(string message, GraphTable<TMetadata> table, params GraphEdge<TMetadata>[] edges)
         : base(message)
        {
            Table = table;
            Edges = edges;
        }

        internal DSLException(string message, GraphTable<TMetadata> table, IReadOnlyList<GraphEdge<TMetadata>> edges)
            : base(message)
        {
            Table = table;
            Edges = edges;
        }

        public GraphTable<TMetadata> Table { get; }

        public IReadOnlyList<GraphEdge<TMetadata>> Edges { get; }
    }
}
