using libgraph;
using librule.generater;

namespace librule.productions
{
    class ReportProduction<TAction> : ProductionBase<TAction>
    {
        public override ProductionType ProductionType => ProductionType.Report;

        private ProductionBase<TAction> production;

        internal ReportProduction(ProductionBase<TAction> production)
        {
            this.production = production;
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
            return production.GetCompuateHashCode() ^ 113;
        }

        internal override IGraphEdgeStep<TMetadata> InternalCreate<TMetadata>(GraphFigure<TMetadata, TAction> figure, IGraphEdgeStep<TMetadata> last, IGraphEdgeStep<TMetadata> entry)
        {
            figure.GraphBox.StartCollect();
            var result = production.Create(figure, last, entry);
            var edge = figure.GraphBox.EndCollect();

            var rights = edge.Where(x => result.End.Contains(x.Target)).ToArray();
            foreach (var right in rights)
                right.Flags |= EdgeFlags.SpecialPoint;

            return result;
        }
    }
}
