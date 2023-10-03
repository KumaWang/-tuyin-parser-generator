using libgraph;

namespace libflow.steps
{
    abstract class AnalysisStep<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        protected internal abstract FlowFigure<TVertex, TEdge> Run(FlowFigure<TVertex, TEdge> figure, GraphModel<TVertex, TEdge> model);
    }
}
