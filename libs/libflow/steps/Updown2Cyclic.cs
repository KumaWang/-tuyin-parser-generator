using libgraph;

namespace libflow.steps
{
    internal class Updown2Cyclic<TVertex, TEdge> : AnalysisStep<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        protected internal override FlowFigure<TVertex, TEdge> Run(FlowFigure<TVertex, TEdge> figure, GraphModel<TVertex, TEdge> model)
        {
            return figure;
        }
    }
}
