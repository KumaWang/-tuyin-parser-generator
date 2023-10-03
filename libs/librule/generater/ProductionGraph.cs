using libfsm;
using librule.utils;
using System.Data;

namespace librule.generater
{
    class ProductionGraph : GraphBox<ProductionMetadata, TableAction>
    {
        private Dictionary<GraphState<ProductionMetadata>, ushort> mTokens;
        private TwoKeyDictionary<GraphEdge<ProductionMetadata>, TableAction, bool> mSkipTokens;

        public ProductionGraph(Lexicon lexicon)
        {
            Lexicon = lexicon;
            mTokens = new Dictionary<GraphState<ProductionMetadata>, ushort>();
            mSkipTokens = new TwoKeyDictionary<GraphEdge<ProductionMetadata>, TableAction, bool>();
        }

        public override Lexicon Lexicon { get; }

        internal void Final()
        {
            var rights = Edges.GroupBy(x => x.Source.Index).ToDictionary(x => x.Key, x => x.ToList());

            // 通过Production创建的图不存在循环结构，所以可以不需要使用访问器来规避已访问边
            foreach (var tokenItem in mTokens)
            {
                var point = tokenItem.Key.Index;
                var token = tokenItem.Value;
                var stack = new Stack<GraphEdge<ProductionMetadata>>(rights[point]);

                while (stack.Count > 0)
                {
                    var edge = stack.Pop();

                    if (rights.ContainsKey(edge.Target.Index) && rights[edge.Target.Index].Count > 0)
                    {
                        foreach (var right in rights[edge.Target.Index])
                        {
                            stack.Push(right);
                        }
                    }
                    else
                    {
                        var actions = GetAction(edge.Metadata.Value);
                        if (actions.Count > 0)
                        {
                            var tokenInsert = actions.Count == 1;
                            for (var i = 0; i < actions.Count; i++)
                            {
                                var action = actions[i];
                                actions[i] = new TableAction(action.Context, action.Front, token);
                            }
                        }

                        if (actions.Count == 0 && edge.Metadata.Token == 0)
                            edge.Metadata = new ProductionMetadata(edge.Metadata.Value, token);
                    }
                }
            }

            mTokens.Clear();
            mTokens = null;
        }

        public override ProductionMetadata CreateMetadata(GraphEdge<ProductionMetadata> edge, TableAction action, bool skipToken)
        {
            mSkipTokens[edge, action] = skipToken;
            return new ProductionMetadata(GetActionNumber(action), 0);
        }

        public override ProductionMetadata MergeMetadatas(IEnumerable<ProductionMetadata> metadatas)
        {
            var token = metadatas.First().Token;
            var values = metadatas.Select(x => x.Value);
            var actions = values.SelectMany(GetAction).ToList();
            actions.Sort(TableActionComparer.Instance);
            return new ProductionMetadata(GetActionNumber(actions), token);
        }

        public override void TokenEdge(GraphEdge<ProductionMetadata> edge, ushort token)
        {
            var actions = GetAction(edge.Metadata.Value);
            for(var i = 0; i < actions.Count; i++) 
            {
                var action = actions[i];
                if (mSkipTokens.ContainsKey(edge, action))
                    continue;

                if (action.Token != 0)
                    throw new NotImplementedException();

                actions[i] = new TableAction(action.Context, action.Front, token);
            }

            var subset = edge.Subset != null && edge.Source.Index == edge.Subset.Index ? edge.Subset : null;
            if (subset != null)
                if (!mTokens.ContainsKey(subset))
                    mTokens[subset] = token;
                else if (mTokens[subset] != token)
                    throw new NotImplementedException();

            if (edge.Metadata.Token != 0 && edge.Metadata.Token != token)
                throw new NotImplementedException();

            edge.Metadata = new ProductionMetadata(edge.Metadata.Value, token);
        }

        public override GraphTable<ProductionMetadata> Tabulate()
        {
            return new ProductionTable(this, FATableFlags.ConnectSubset | FATableFlags.Subdivision | FATableFlags.Predicate | FATableFlags.AmbiguityResolution);
        }

        public override int GetMetadataCompreValue(ProductionMetadata metadata) => 0;

        public bool IsEmptyTransition(FATransition<ProductionMetadata> tran)
        {
            return tran.Input.Contains((char)Lexicon.Missing.Index);
        }

        public override ProductionMetadata GetDefaultMetadata() => new ProductionMetadata(-1, 0);

        public override GraphEdgeValue GetMissValue() => new GraphEdgeValue(Lexicon.Missing.Index);
    }
}
