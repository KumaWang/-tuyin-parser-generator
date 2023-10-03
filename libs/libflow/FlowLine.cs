using libgraph;

namespace libflow
{
    public struct FlowLine<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        public FlowLine(TEdge edge, FlowLineType lineType)
        {
            Edge = edge;
            LineType = lineType;
        }

        public TEdge Edge { get; }

        public FlowLineType LineType { get; }

        public override string ToString()
        {
            return $"{Edge.Source.Index}->{Edge.Target.Index}";
        }
    }
}
