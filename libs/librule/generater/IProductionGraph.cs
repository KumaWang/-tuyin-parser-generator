using libgraph;

namespace librule.generater
{
    public interface IProductionGraph<TMetadata> : IGraph<GraphState<TMetadata>, GraphEdge<TMetadata>>
    {
        Lexicon Lexicon { get; }

        GraphEdgeValue GetMissValue();

        int GetMetadataCompreValue(TMetadata metadata);

        TMetadata GetDefaultMetadata();

        GraphTable<TMetadata> Tabulate();

        IEnumerable<GraphEdge<TMetadata>> GetLefts(GraphState<TMetadata> state);

        IEnumerable<GraphEdge<TMetadata>> GetRights(GraphState<TMetadata> state);
    }
}
