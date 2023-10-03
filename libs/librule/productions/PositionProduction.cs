using librule.generater;

namespace librule.productions
{
    class PositionProduction<TAction> : ProductionBase<TAction>
    {
        public override ProductionType ProductionType => ProductionType.Position;

        private ProductionBase<TAction> production;
        private SourceLocation loc;

        internal PositionProduction(ProductionBase<TAction> production, SourceLocation loc)
        {
            this.production = production;
            this.loc = loc;
        }

        internal override IEnumerable<Token> GetTokens(Production<TAction> self)
        {
            return production.GetTokens(self);
        }

        public override IEnumerable<ProductionBase<TAction>> GetChildrens()
        {
            yield return production;
        }

        public override int GetCompuateHashCode()
        {
            return production.GetCompuateHashCode() ^ 115;
        }

        internal override IGraphEdgeStep<TMetadata> InternalCreate<TMetadata>(GraphFigure<TMetadata, TAction> figure, IGraphEdgeStep<TMetadata> last, IGraphEdgeStep<TMetadata> entry)
        {
            figure.GraphBox.StartCollect();
            var result = production.Create(figure, last, entry);
            var edge = figure.GraphBox.EndCollect();

            var rights = edge.Where(x => result.End.Contains(x.Target)).ToArray();
            foreach (var right in rights)
                right.SourceLocation = loc;

            return result;
        }
    }
}
