using libgraph;

namespace libflow
{
    public sealed class FlowPath<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        public FlowPath(FlowStep<TVertex, TEdge> step, FlowFigure<TVertex, TEdge> figure)
        {
            Step = step;
            Figure = figure;
        }

        public FlowStep<TVertex, TEdge> Step { get; }

        public FlowFigure<TVertex, TEdge> Figure { get; }
    }
}
