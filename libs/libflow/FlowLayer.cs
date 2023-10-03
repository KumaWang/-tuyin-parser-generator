using libgraph;

namespace libflow
{
    public sealed class FlowLayer<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        internal FlowLayer(FlowFigure<TVertex, TEdge> figure, int level, ushort state, FlowLayer<TVertex, TEdge> parent)
        {
            Level = level;
            State = state;
            Parent = parent;
            Figure = figure;
            Tree = new FlowNode<TVertex, TEdge>();
        }

        public int Level { get; internal set; }

        public ushort State { get; }

        public FlowLayer<TVertex, TEdge> Parent { get; internal set; }

        public FlowNode<TVertex, TEdge> Tree { get; }

        public FlowFigure<TVertex, TEdge> Figure { get; }
    }
}
