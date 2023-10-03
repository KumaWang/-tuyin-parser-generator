using libgraph;
using System.Collections.Generic;

namespace libflow
{
    public sealed class FlowFigure<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        internal FlowFigure(GraphFigure<TVertex, TEdge> figure, List<FlowLayer<TVertex, TEdge>> layers)
        {
            GraphFigure = figure;
            Layers = layers;
        }

        public GraphFigure<TVertex, TEdge> GraphFigure { get; }

        public List<FlowLayer<TVertex, TEdge>> Layers { get; }
    }
}
