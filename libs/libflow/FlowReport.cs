using libflow.stmts;
using libgraph;

namespace libflow
{
    public sealed class FlowReport<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        public FlowReport(IAstNode astNode, FlowFigure<TVertex, TEdge> figure)
        {
            AstNode = astNode;
            Figure = figure;
        }

        public IAstNode AstNode { get; }

        public FlowFigure<TVertex, TEdge> Figure { get; }
    }
}
