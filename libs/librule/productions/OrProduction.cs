using librule.generater;

namespace librule.productions
{
    class OrProduction<TAction> : ProductionBase<TAction>
    {
        private bool mHasEmpty;

        public override ProductionType ProductionType => ProductionType.Or;

        public IList<ProductionBase<TAction>> Forks { get; private set; }

        public OrProduction(IEnumerable<ProductionBase<TAction>> forks)
        {
            Forks = Init(Simplify(forks.ToArray()));
        }

        public OrProduction(ProductionBase<TAction> left, ProductionBase<TAction> right)
        {
            Forks = Init(Simplify(left, right));
        }

        internal override IEnumerable<Token> GetTokens(Production<TAction> self)
        {
            return Forks.SelectMany(x => x.GetTokens(self));
        }

        private IList<ProductionBase<TAction>> Init(List<ProductionBase<TAction>> forks)
        {
            var empties = forks.Where(X => X.ProductionType == ProductionType.Empty).ToArray();
            forks.RemoveAll(x => empties.Contains(x));

            mHasEmpty = empties.Length > 0;
            if (mHasEmpty)
            {
                var empty = empties.FirstOrDefault();
                if (empty != null)
                    forks.Insert(0, empty);
            }

            return forks.ToArray();
        }

        private List<ProductionBase<TAction>> Simplify(params ProductionBase<TAction>[] productions)
        {
            var prods = new List<ProductionBase<TAction>>();

            for (var i = 0; i < productions.Length; i++)
            {
                var prod = productions[i];

                if (prod.ProductionType == ProductionType.Or)
                {
                    prods.AddRange((prod as OrProduction<TAction>).Forks);
                }
                else
                {
                    prods.Add(prod);
                }
            }

            return prods;
        }

        internal override IGraphEdgeStep<TMetadata> InternalCreate<TMetadata>(GraphFigure<TMetadata, TAction> figure, IGraphEdgeStep<TMetadata> last, IGraphEdgeStep<TMetadata> entry)
        {
            var results = new IGraphEdgeStep<TMetadata>[Forks.Count];
            for (int i = 0; i < Forks.Count; i++)
                results[i] = Forks[i].Create(figure, last, entry);

            return new NextStep<TMetadata>(entry.End, results.SelectMany(x => x.End));
        }

        public override IEnumerable<ProductionBase<TAction>> GetChildrens()
        {
            return Forks;
        }

        public override int GetCompuateHashCode()
        {
            var hash = Forks[0].GetCompuateHashCode();
            for (var i = 1; i < Forks.Count; i++)
                hash = hash ^ 108 ^ Forks[i].GetCompuateHashCode();

            return hash;
        }
    }
}
