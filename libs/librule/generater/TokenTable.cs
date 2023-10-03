using libfsm;
using libgraph;

namespace librule.generater
{
    class TokenTable : GraphTable<TokenMetadata>
    {
        private TokenGraph mGraph;
        private TokenMetadata mDefaultMetadata;
        private HashSet<ushort> mConflictTokens;
        private List<ClarityToken> mClarityTokens;
        private Dictionary<Token, ushort> mTokenConverter;
        private HashSet<FATransition<TokenMetadata>> mVisitor;
        private TransitionComparer mTransitionComparer;
        private CompreTransitionComparer mCompreTransitionComparer;

        internal TokenTable(TokenGraph graph, FATableFlags flags)
            : base(graph, flags)
        {
            mTransitionComparer = new TransitionComparer();
            mCompreTransitionComparer = new CompreTransitionComparer();

            mGraph = graph;
            mDefaultMetadata = graph.GetDefaultMetadata();
            mConflictTokens = new HashSet<ushort>();
            mClarityTokens = new List<ClarityToken>();
            mTokenConverter = new Dictionary<Token, ushort>();
            mVisitor = new HashSet<FATransition<TokenMetadata>>();
        }

        internal bool HasTokenConverter => mTokenConverter.Count > 0;

        public override EdgeInput EmptyInput => new EdgeInput(mGraph.Lexicon.Missing.Index);

        public IReadOnlyCollection<ushort> ConflictTokens => mConflictTokens;

        public IReadOnlyList<ClarityToken> ClarityTokens => mClarityTokens;

        public Lexicon Lexicon => mGraph.Lexicon;

        public new TokenGraph Graph => mGraph;

        public override string GetMetadataDescrption(TokenMetadata metadata, string header)
        {
            var actions = mGraph.GetAction(metadata.Value);
            return $"{metadata.Token}={header}\n{string.Join("\n", actions.Select(x => $"{(x.Token == 0 ? string.Empty : $"{x.Token}=")}{x.Context}"))}";
        }

        public ushort ConvertTokenIndex(Token token)
        {
            if (!mTokenConverter.ContainsKey(token))
            {
                var index = (ushort)(mTokenConverter.Count + 1);
                mTokenConverter[token] = index;
                for (var i = 0; i < Transitions.Count; i++)
                {
                    var tran = Transitions[i];
                    if (tran.Metadata.Token == token.Index && !mVisitor.Contains(tran))
                    {
                        Transitions[i] = tran.ChangeMetadata(new TokenMetadata(tran.Metadata.Value, index));
                        mVisitor.Add(Transitions[i]);
                    }
                }
            }

            return mTokenConverter[token];
        }

        public IEnumerable<ushort> GetTokens() => Transitions.Select(x => x.Metadata.Token).Distinct().Where(x => x != Lexicon.Missing.Index);

        public ushort GetTokenIndex(ushort index) => mTokenConverter.First(x => x.Value == index).Key.Index;

        protected override TokenMetadata ArrangeMetadata(TokenMetadata metadata, int front)
        {
            if (metadata.Equals(mDefaultMetadata))
                return metadata;

            var actions = new List<TableAction>();
            foreach (var v in mGraph.GetAction(metadata.Value))
                actions.Add(new TableAction(v.Context, Math.Max(v.Front, front), v.Token));

            if (actions.Count == 0)
                return metadata;

            actions.Sort(TableActionComparer.Instance);
            return new TokenMetadata(mGraph.GetActionNumber(actions), metadata.Token);
        }

        protected override TokenMetadata MergeMetadatas(IList<TokenMetadata> metadatas, int tokenIndex)
        {
            var token = metadatas[tokenIndex].Token;

            var first = new List<TokenMetadata>();
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

            var values = first.Select(x => x.Value);
            var actions = values.SelectMany(mGraph.GetAction).ToList();
            actions.Sort(TableActionComparer.Instance);
            return new TokenMetadata(mGraph.GetActionNumber(actions), token);
        }

        protected override void ThrowConflictException(IShiftRightMemoryModel model, ConflictException<TokenMetadata> ex)
        {
            var tokens = ex.Transitions.Select(x => x.Metadata.Token).Distinct().OrderBy(x => x).ToArray();
            if (!mGraph.SupportClarity || !ClarityTokens.Any(x => x.Tokens.Select(y => y.Index).OrderBy(x => x).SequenceEqual(tokens)))
                base.ThrowConflictException(model, ex);
        }

        protected override bool IsEmptyTransition(FATransition<TokenMetadata> tran)
        {
            return false;
        }

        protected override bool SymbolConflictResolution(IShiftRightMemoryModel memroy, SymbolGroup<TokenMetadata>[] groups, ref int max, out FASymbol result)
        {
            // 找到expression最大循环请求
            var compreVals = groups.SelectMany(y =>
                y.Transitions.Select(x => new CompreTransition(x, Graph.GetMetadataCompreValue(x.Metadata)))).ToArray();

            // 查找compreValue最小的Metadata
            var minMetadata = compreVals.OrderBy(x => x, mCompreTransitionComparer).First();

            result = minMetadata.Transition.Symbol;
            return true;
        }

        protected override bool MetadataConflictResolution(IShiftRightMemoryModel memroy, MetadataGroup<TokenMetadata>[] groups, ref int max, out TokenMetadata result)
        {
            // 找到expression最大循环请求
            var compreVals = groups.SelectMany(y =>
                y.Transitions.Select(x => new CompreTransition(x, Graph.GetMetadataCompreValue(x.Metadata)))).ToArray();

            // 如果存在不同的元数据则处理失败
            var non_default = compreVals.Where(x => !x.Transition.Metadata.Equals(mDefaultMetadata)).ToArray();
            if (non_default.GroupBy(x => x.Transition.Metadata.Value).Count() != 1)
            {
                result = mDefaultMetadata;
                return false;
            }

            // 如果同目标存在不同token则处理失败
            if (non_default.GroupBy(x => x.Transition.Right).Any(x => x.GroupBy(y => y.Transition.Metadata.Token).Count() != 1)) 
            {
                result = mDefaultMetadata;
                return false;
            }

            // 查找compreValue最小的Metadata
            var minMetadata = compreVals.OrderBy(x => x, mCompreTransitionComparer).First();

            // 多义词支持
            var tokens = groups.Where(x => x.Metadata.Token != 0).Select(x => Lexicon.Tokens[x.Metadata.Token]).ToArray();
            if (mGraph.SupportClarity && tokens.Length > 1)
            {
                throw new FAException("Token歧义正在测试。");

                foreach (var group in groups)
                    mConflictTokens.Add(group.Metadata.Token);

                var clarity = Lexicon.Clarity(tokens);
                clarity.InProcessing = true;
                mClarityTokens.Add(clarity);
                result = new TokenMetadata(mDefaultMetadata.Value, clarity.Index);
            }
            else
            {
                result = minMetadata.Transition.Metadata;
            }
            return true;
        }

        protected override IList<FATransition<TokenMetadata>> GetConflicts(IShiftRightMemoryModel model)
        {
            foreach (var right in model.Rights.OrderByDescending(x => x.Key))
            {
                for (var x = right.Value.Count - 1; x >= 0; x--)
                {
                    var l = right.Value[x];
                    var s = l.Input;
                    var r = new List<FATransition<TokenMetadata>>();
                    for (var y = right.Value.Count - 1; y >= 0; y--)
                    {
                        if (x == y) continue;

                        var n = right.Value[y];
                        var c = s.GetCommonDivisor(n.Input);
                        if (c.IsVaild())
                        {
                            s = c;
                            r.Add(n);
                        }
                    }

                    if (r.Count > 0)
                    {
                        r.Add(l);
                        r.Sort(mTransitionComparer);
                        return r;
                    }
                }
            }

            return null;
        }

        protected override IEnumerable<FABuildStep<TokenMetadata>> Minimize(IShiftMemoryModel model, FATableFlags flags, IEnumerable<ushort> checkPoints)
        {
            foreach (var step in base.Minimize(model, flags, checkPoints))
                yield return step;

            // 查找所有边to0
            foreach (var tran in model.Transitions.ToArray())
                if (model.GetRights(tran.Right).Count == 0)
                {
                    model.Remove(tran);
                    yield return new FABuildStep<TokenMetadata>(FABuildStage.Minimize, FABuildType.Delete, tran);

                    var dst = new FATransition<TokenMetadata>(
                        tran.Left, 0, tran.SourceLeft, tran.SourceRight,
                        tran.Input, tran.Symbol, tran.Metadata);
                    model.Add(dst);
                    yield return new FABuildStep<TokenMetadata>(FABuildStage.Minimize, FABuildType.Add, dst);
                }

        }

        protected override IEnumerable<FABuildStep<TokenMetadata>> ReportTransitions(IShiftMemoryModel model, FATableFlags flags)
        {
            return Array.Empty<FABuildStep<TokenMetadata>>();

            /*
            var points = new List<ushort>();
            foreach (var subset in GetSubsets())
                points.Add(subset);

            points.Add(1);
            var loops = GetLoops(model, StateCount, points).Select(x => x.Right).ToHashSet();

            var vailds = GetVaildStates(model, flags);
            var stateCount = StateCount;
            // 为report添加miss退出
            var visitor = new bool[StateCount];
            var reports = model.Transitions.Where(x =>
                x.Symbol.Type.HasFlag(FASymbolType.Report) &&
                vailds.Contains(x.Left) &&
                model.GetRights(x.Right).Count > 0).ToArray();

            //foreach (var right in model.GetRights(1))
            //    visitor[right.Right] = true;

            foreach (var report in reports)
            {
                if (visitor[report.Right] || loops.Contains(report.Right))
                    continue;

                visitor[report.Right] = true;
                // 检查右侧是否包含miss移进
                var rights = model.GetRights(report.Right);
                var hasMiss = rights.Any(x => IsEmptyTransition(x));
                if (!hasMiss)
                {
                    // 如果不包含miss移进则创建一个
                    var dstRight = (ushort)++stateCount;
                    var dst = new FATransition<TokenMetadata>(
                        report.Right, dstRight,
                        report.SourceRight, dstRight,
                        EmptyInput, new FASymbol(FASymbolType.Report, 0, 0), mDefaultMetadata);

                    model.Add(dst);
                    yield return new FABuildStep<TokenMetadata>(FABuildStage.Optimize, FABuildType.Add, dst);
                }
            }

            StateCount = stateCount;
            */
        }

        internal bool IsConflictToken(ushort token)
        {
            return mConflictTokens.Contains(token);
        }

        class TransitionComparer : IComparer<FATransition<TokenMetadata>>
        {
            public TransitionComparer()
            {
            }

            public int Compare(FATransition<TokenMetadata> x, FATransition<TokenMetadata> y)
            {
                var c = x.Input.GetCommonDivisor(y.Input);
                var xr = x.Input.Eliminate(c);
                var yr = y.Input.Eliminate(c);

                if (!xr.IsVaild() && !yr.IsVaild())
                    return 0;

                if (xr.IsVaild())
                    return 1;

                if (yr.IsVaild())
                    return -1;

                throw new NotImplementedException();
            }
        }

        struct CompreTransition 
        {
            public CompreTransition(FATransition<TokenMetadata> transition, int compreValue)
            {
                Transition = transition;
                CompreValue = compreValue;
            }

            public FATransition<TokenMetadata> Transition { get; }

            public int CompreValue { get; }
        }

        class CompreTransitionComparer : IComparer<CompreTransition>
        {
            public int Compare(CompreTransition x, CompreTransition y)
            {
                long xr = x.CompreValue;
                long yr = y.CompreValue;

                if (y.Transition.Metadata.Token != 0 && x.Transition.Symbol.Type.HasFlag(FASymbolType.Report))
                    xr = xr + int.MaxValue + 1;

                if (x.Transition.Metadata.Token != 0 && y.Transition.Symbol.Type.HasFlag(FASymbolType.Report))
                    yr = yr + int.MaxValue + 1;

                return xr.CompareTo(yr);
            }
        }
    }
}
