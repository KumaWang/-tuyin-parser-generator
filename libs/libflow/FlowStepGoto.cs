using libgraph;

namespace libflow
{
    public abstract class FlowStepGoto<TVertex, TEdge> : FlowStepNext<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        protected FlowStepGoto(TEdge edge)
            : base(edge)
        {
        }
    }
}
