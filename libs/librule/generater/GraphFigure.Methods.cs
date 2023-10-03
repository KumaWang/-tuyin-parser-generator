namespace librule.generater
{
    partial class GraphFigure<TMetadata, TAction>
    {
        public void SourceMoveTo(GraphEdge<TMetadata> edge, GraphState<TMetadata> target) 
        {
            Remove(edge);
            var newEdge = Edge(target, edge.Target, edge.Subset, edge.Value, edge.Metadata);
            newEdge.Descrption = edge.Descrption;
            newEdge.SourceLocation = edge.SourceLocation;
            newEdge.FromProduction = edge.FromProduction;
            newEdge.IsEntry = edge.IsEntry;
        }

        public void TargetMoveTo(GraphEdge<TMetadata> edge, GraphState<TMetadata> target) 
        {
            Remove(edge);
            var newEdge = Edge(edge.Source, target, edge.Subset, edge.Value, edge.Metadata);
            newEdge.Descrption = edge.Descrption;
            newEdge.SourceLocation = edge.SourceLocation;
            newEdge.FromProduction = edge.FromProduction;
            newEdge.IsEntry = edge.IsEntry;
        }
    }
}
