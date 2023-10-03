using libfsm;

namespace librule.generater
{
    public abstract class GraphTable<TMetadata> : FATable<TMetadata>
    {
        private FATableFlags mFlags;
        private IProductionGraph<TMetadata> mGraph;

        internal GraphTable(IProductionGraph<TMetadata> graph, FATableFlags flags)
        {
            mFlags = flags;
            mGraph = graph;
        }

        public IProductionGraph<TMetadata> Graph => mGraph;

        public abstract string GetMetadataDescrption(TMetadata metadata, string header);

        public GraphTable<TMetadata> Generate(int inline, int timeout, FATableFlags flags)
        {
            if (Transitions != null)
                return this;

            Generate(mGraph, mGraph.GetDefaultMetadata(), inline, timeout, mFlags | flags);
            return this;
        }

        protected override TMetadata GetMetadata<TVertex, TEdge>(TEdge edge)
        {
            return (edge as GraphEdge<TMetadata>).Metadata;
        }

        public void Walk(int max, Action<ushort, ushort, FASymbolType, FAValue<TMetadata>> func, Func<FATransition<TMetadata>, TMetadata> metadataFiliter = null)
        {
            if (Transitions == null)
                throw new NotSupportedException("调用Walk前需要先调用Generate(Func<IList<TMetadata>, TMetadata>)");

            if (metadataFiliter != null)
            {
                Parallel.ForEach(Transitions, tran =>
                {
                    foreach (var c in tran.Input.GetChars(max))
                    {
                        var meta = metadataFiliter(tran);
                        func(c, tran.Left, tran.Symbol.Type, new FAValue<TMetadata>(
                            tran.Right, tran.Symbol.Value, meta));
                    }
                });
            }
            else
            {
                Parallel.ForEach(Transitions, tran =>
                {
                    foreach (var c in tran.Input.GetChars(max))
                        func(c, tran.Left, tran.Symbol.Type, new FAValue<TMetadata>(
                            tran.Right, tran.Symbol.Value, tran.Metadata));
                });
            }
        }

        public DFA<TMetadata> ToDFA(int max, Func<FATransition<TMetadata>, TMetadata> metadataFiliter = null)
        {
            var columns = StateCount;
            var data = new FAValue<TMetadata>[columns, max];
            Walk(max, (c, state, symbol, value) => data[state, c] = value, metadataFiliter);
            return new DFA<TMetadata>(data);
        }
    }
}
