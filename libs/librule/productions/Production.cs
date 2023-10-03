using librule.generater;
using librule.utils;

namespace librule.productions
{
    class Production<TAction> : ProductionBase<TAction>
    {
        enum CreateState
        {
            NoGraph,
            Createing,
            Created
        }

        private bool mInChildrenLoop;
        private bool mHasRecursive;
        private CreateState mState;
        private object mEntry;
        private ProductionBase<TAction> mRule;

        private string mProductionName;

        public override ProductionType ProductionType => ProductionType.Recursive;

        public override string ProductionName => mProductionName;

        public bool HasRecursive
        {
            get { return mHasRecursive; }
        }

        public ProductionBase<TAction> Rule
        {
            get { return mRule; }
            set
            {
                if (mRule != value)
                {
                    mRule = value;
                    mHasRecursive = value.Scan(null, p => p == this).Count() > 0;
                }
            }
        }

        public Production(string productionName)
        {
            mProductionName = productionName;
        }

        internal override IGraphEdgeStep<TMetadata> InternalCreate<TMetadata>(GraphFigure<TMetadata, TAction> figure, IGraphEdgeStep<TMetadata> last, IGraphEdgeStep<TMetadata> entry) where TMetadata : struct
        {
            if (mState == CreateState.NoGraph)
            {
                var newFigure = figure.GraphBox.Figure(mProductionName);
                mEntry = newFigure;
                mState = CreateState.Createing;
                var step = Rule.Create(newFigure, last, new Entry<TMetadata>(newFigure));

                foreach (var edge in newFigure.GetRights(mEntry as GraphFigure<TMetadata, TAction>))
                    edge.IsEntry = true;

                foreach (var edge in newFigure.Edges)
                    edge.FromProduction = mProductionName;

                mState = CreateState.Created;
            }

            return ConnectToWarp(figure, last, entry);
        }

        private IGraphEdgeStep<TMetadata> ConnectToWarp<TMetadata>(GraphFigure<TMetadata, TAction> figure, IGraphEdgeStep<TMetadata> last, IGraphEdgeStep<TMetadata> entry) where TMetadata : struct
        {
            var end = figure.State();
            var entryFigure = mEntry as GraphFigure<TMetadata, TAction>;
            foreach (var start in entry.End)
            {
                var edge = figure.Edge(start, end, new GraphEdgeValue(entryFigure.Index), figure.GraphBox.GetDefaultMetadata());
                edge.Subset = entryFigure;
                edge.Descrption = mProductionName;
            }

            return new NextStep<TMetadata>(entry.End, end);
        }

        internal override IEnumerable<Token> GetTokens(Production<TAction> self)
        {
            if (!mInChildrenLoop && self == this)
            {
                mInChildrenLoop = true;
                var results = Rule.GetTokens(self);
                mInChildrenLoop = false;
                return results;
            }

            return Array.Empty<Token>();
        }

        public override IEnumerable<ProductionBase<TAction>> GetChildrens()
        {
            if (!mInChildrenLoop)
            {
                mInChildrenLoop = true;
                yield return Rule;
                mInChildrenLoop = false;
            }
        }

        public override int GetCompuateHashCode()
        {
            if (!mInChildrenLoop && Rule != null)
            {
                mInChildrenLoop = true;
                var value = Rule.GetCompuateHashCode() ^ 104;
                mInChildrenLoop = false;
                return value;
            }
            else
            {
                return 0;
            }
        }
    }
}
