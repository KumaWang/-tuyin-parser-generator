using librule.generater;

namespace librule.productions
{
    class ActionProduction<TAction> : ProductionBase<TAction>
    {
        public override ProductionType ProductionType => ProductionType.Action;

        private ProductionBase<TAction> production;
        private TAction action;

        public ActionProduction(ProductionBase<TAction> production, TAction action)
        {
            this.production = production;
            this.action = action;
        }

        internal override IEnumerable<Token> GetTokens(Production<TAction> self)
        {
            return production.GetTokens(self);
        }

        internal override IGraphEdgeStep<TMetadata> InternalCreate<TMetadata>(GraphFigure<TMetadata, TAction> figure, IGraphEdgeStep<TMetadata> last, IGraphEdgeStep<TMetadata> entry)
        {
            figure.GraphBox.StartCollect();
            var result = production.Create(figure, last, entry);
            var edge = figure.GraphBox.EndCollect();

            var rights = edge.Where(x => result.End.Contains(x.Target)).ToArray();
            foreach (var right in rights)
            {
                var meta = figure.GraphBox.CreateMetadata(right, action, false);
                if (right.Metadata.Equals(figure.GetDefaultMetadata()))
                    right.Metadata = meta;
                else
                {
                    var metas = new TMetadata[2];
                    metas[1] = meta;
                    metas[0] = right.Metadata;
                    right.Metadata = figure.GraphBox.MergeMetadatas(metas);
                }
            }

            return result;
        }

        public override IEnumerable<ProductionBase<TAction>> GetChildrens()
        {
            yield return production;
        }

        public override int GetCompuateHashCode()
        {
            return production.GetCompuateHashCode() ^ 103;
        }
    }
}
