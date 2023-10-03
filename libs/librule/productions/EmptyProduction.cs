using libgraph;
using librule.generater;

namespace librule.productions
{
    class EmptyProduction<TAction> : ProductionBase<TAction>
    {
        public override ProductionType ProductionType => ProductionType.Empty;

        public EmptyProduction()
        {
        }

        internal override IEnumerable<Token> GetTokens(Production<TAction> self)
        {
            return Array.Empty<Token>();
        }

        internal override IGraphEdgeStep<TMetadata> InternalCreate<TMetadata>(GraphFigure<TMetadata, TAction> figure, IGraphEdgeStep<TMetadata> last, IGraphEdgeStep<TMetadata> entry)
        {
            var newState = figure.State();
            foreach (var end in entry.End)
            {
                var edge = figure.Edge(end, newState, null, figure.GraphBox.GetMissValue(), figure.GraphBox.GetDefaultMetadata());
                edge.Flags = edge.Flags | EdgeFlags.Optional;
            }

            return new NextStep<TMetadata>(entry.End, new GraphState<TMetadata>[] { newState });
        }

        public override IEnumerable<ProductionBase<TAction>> GetChildrens()
        {
            return new ProductionBase<TAction>[0];
        }

        public override int GetCompuateHashCode()
        {
            return 0;
        }
    }
}
