using libfsm;
using libgraph;
using System.Text.RegularExpressions;

namespace librule.generater
{

    class ProductionTable : GraphTable<ProductionMetadata>
    {
        private ProductionMetadata mDefaultMetadata;
        private ProductionGraph mGraph;

        public ProductionTable(ProductionGraph graph, FATableFlags flags)
            : base(graph, flags)
        {
            mGraph = graph;
            mDefaultMetadata = mGraph.GetDefaultMetadata();
        }

        public new ProductionGraph Graph => mGraph;

        public override EdgeInput EmptyInput => new EdgeInput(mGraph.Lexicon.Missing.Index);

        public override string GetMetadataDescrption(ProductionMetadata metadata, string header)
        {
            var actions = mGraph.GetAction(metadata.Value);
            return $"{metadata.Token}={header}\n{string.Join("\n", actions.Select(x => $"{(x.Token == 0 ? string.Empty : $"{x.Token}=")}{x.Context}"))}";
        }

        protected override FATransition<ProductionMetadata> GetSingleTransition(IShiftMemoryModel model, HashSet<FATransition<ProductionMetadata>> visitor)
        {
            return model.Transitions.FirstOrDefault(x => 
                x.Input.Chars.Length == 1 &&
                Graph.Lexicon.Tokens[x.Input.Chars[0]].IsPrevious &&
                !visitor.Contains(x) &&
                model.GetLefts(x.Left).Count == 0 && 
                model.GetRights(x.Right).Count == 0);
            
        }

        protected override bool MetadataConflictResolution(IShiftRightMemoryModel model, MetadataGroup<ProductionMetadata>[] groups, ref int max, out ProductionMetadata result)
        {
            var transitions = groups.SelectMany(x => x.Transitions).ToArray();
            var token = (ushort)0;

            var tokens = new HashSet<ushort>();

            // 首先得到所有不是请求的移进
            var requestTransitions = transitions.Where(x => x.Symbol.Type.HasFlag(FASymbolType.Request)).ToArray();
            var matchTransitions = transitions.Where(x => !x.Symbol.Type.HasFlag(FASymbolType.Request)).ToList();

            for (var i = 0; i < requestTransitions.Length; i++)
                matchTransitions.AddRange(FindSubsetRights(model, requestTransitions[i].Symbol.Value));

            for (var i = 0; i < matchTransitions.Count; i++)
                tokens.Add(matchTransitions[i].Metadata.Token);

            tokens.Remove(0);
            if (tokens.Count > 0)
            {
                if (tokens.Count != 1)
                    throw new MetadataConflictException<ProductionMetadata>(groups);

                token = tokens.First();
            }

            var baseResult = base.MetadataConflictResolution(model, groups, ref max, out result);
            result = new ProductionMetadata(result.Value, token);
            return baseResult;
        }

        protected override bool IsDefaultMetadata(ProductionMetadata metadata)
        {
            return metadata.Value == mDefaultMetadata.Value;
        }

        protected override FATransition<ProductionMetadata> DegradationReduction(FATransition<ProductionMetadata> keep, FATransition<ProductionMetadata> degradation)
        {
            var actions = mGraph.GetAction(keep.Metadata.Value).ToArray();
            for (var i = 0; i < actions.Length; i++) 
            {
                var action = actions[i];
                if (action.Token == 0)
                {
                    actions[i] = new TableAction(action.Context, action.Front, keep.Metadata.Token);
                }
            }

            //if (actions.Length == 1 && actions[0].Context == $"${actions[0].Token}")
            //    actions = new TableAction[0];

            return new FATransition<ProductionMetadata>(
                keep.Left, keep.Right,
                keep.SourceLeft, keep.SourceRight,
                keep.Input, keep.Symbol, new ProductionMetadata(mGraph.GetActionNumber(actions), degradation.Metadata.Token));
        }

        protected override ProductionMetadata ArrangeMetadata(ProductionMetadata metadata, int front)
        {
            if (metadata.Equals(mDefaultMetadata))
                return metadata;

            var actions = new List<TableAction>();
            foreach (var v in mGraph.GetAction(metadata.Value))
                actions.Add(new TableAction(v.Context, Math.Max(v.Front, front), v.Token));

            if (actions.Count == 0)
                return metadata;

            actions.Sort(TableActionComparer.Instance);
            return new ProductionMetadata(mGraph.GetActionNumber(actions), metadata.Token);
        }

        protected override ProductionMetadata MergeMetadatas(IList<ProductionMetadata> metadatas, int tokenIndex)
        {
            var first = new List<ProductionMetadata>();
            var start = metadatas.Count;
            for (var i = 0; i < metadatas.Count; i++)
            {
                var follow = metadatas[i];
                if (!follow.Equals(mDefaultMetadata))
                {
                    first.Add(follow);
                    start = i + 1;
                    break;
                }
            }

            for (var i = start; i < metadatas.Count; i++)
            {
                var follow = metadatas[i];
                if (follow.Equals(mDefaultMetadata) || follow.Equals(first[^1]))
                    continue;

                first.Add(follow);
            }

            ushort metadataToken = 0;
            var resultActions = new List<TableAction>();
            var tableActions = first.Select(x => mGraph.GetAction(x.Value)).ToList();
            for (var i = 0; i < tableActions.Count; i++) 
            {
                var tableSubActions = tableActions[i];
                var hasFront = tableSubActions.Any(x => x.Front != 0);

                for (var x = 0; x < tableSubActions.Count; x++)
                {
                    var action = tableSubActions[x];

                    if (metadataToken == 0 && !hasFront)
                        metadataToken = first[i].Token;

                    if (x == tableSubActions.Count - 1)
                    {
                        if (i < tableActions.Count - 1)
                        {
                            var nextToken = first[i + 1].Token;
                            if ((action.Token == 0 || !hasFront) && nextToken != 0)
                                action = new TableAction(action.Context, action.Front, nextToken);
                        }
                    }

                    if (IsVaildContext(action.Context, action.Token.ToString()))
                        resultActions.Add(action);
                }
            }

            if(metadataToken == 0)
                metadataToken = metadatas[tokenIndex].Token;

            resultActions.Sort(TableActionComparer.Instance);
            return new ProductionMetadata(mGraph.GetActionNumber(resultActions), metadataToken);
        }


        private bool IsVaildContext(string context, string target)
        {
            const string pattern = @"\$(\w+)$";

            var match = Regex.Match(context, pattern);
            if (match.Success)
            {
                string x = match.Groups[1].Value;
                return x != target && target != "0";
            }

            return true;
        }

        protected override bool IsEmptyTransition(FATransition<ProductionMetadata> tran)
        {
            return mGraph.IsEmptyTransition(tran);
        }
    }
}
