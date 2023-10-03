namespace libgraph
{
    public interface IGraphPipline<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
    }
}
