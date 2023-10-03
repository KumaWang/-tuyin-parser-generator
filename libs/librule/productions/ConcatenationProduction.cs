using librule.generater;

namespace librule.productions
{
    class ConcatenationProduction<TAction> : ProductionBase<TAction>
    {
        public override ProductionType ProductionType => ProductionType.Concatenation;

        private ProductionBase<TAction> production;
        private ProductionBase<TAction> production2;
        private bool inget = false;

        public ConcatenationProduction(ProductionBase<TAction> production, ProductionBase<TAction> production2)
        {
            this.production = production;
            this.production2 = production2;
        }

        internal override IEnumerable<Token> GetTokens(Production<TAction> self)
        {
            if (!inget)
            {
                inget = true;
                foreach (var item in production.GetTokens(self).Concat(production2.GetTokens(self)))
                    yield return item;
                inget = false;
            }
        }

        internal override IGraphEdgeStep<TMetadata> InternalCreate<TMetadata>(GraphFigure<TMetadata, TAction> figure, IGraphEdgeStep<TMetadata> last, IGraphEdgeStep<TMetadata> entry)
        {
            var first = production.Create(figure, last, entry);
            var follow = production2.Create(figure, entry, first);
            return new NextStep<TMetadata>(first.Start, follow.End);
        }

        public override IEnumerable<ProductionBase<TAction>> GetChildrens()
        {
            yield return production;
            yield return production2;
        }

        public override int GetCompuateHashCode()
        {
            return production.GetCompuateHashCode() ^ 102 ^ production2.GetCompuateHashCode();
        }
    }
}
