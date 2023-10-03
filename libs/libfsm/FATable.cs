using libgraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace libfsm
{
    public abstract partial class FATable<T>
    {
        private T mDefaultMetadata;
        private FATableFlags mFlags;
        private int mMaxInlineDeep;
        private int mTimeout;
        private bool mHasLayoutTransitions;
        private readonly List<ushort> mSubsets = new List<ushort>();
        private readonly List<FABuildStep<T>> mBuildSteps = new List<FABuildStep<T>>();
        private readonly Dictionary<ushort, ushort> mSubsetReplaces = new Dictionary<ushort, ushort>();

        public IReadOnlyList<FABuildStep<T>> BuildSteps => mBuildSteps;

        /// <summary>
        /// 移进表
        /// </summary>
        public IList<FATransition<T>> Transitions 
        { 
            get;
            private set; 
        }

        /// <summary>
        /// 状态容量
        /// </summary>
        public int StateCount 
        {
            get;
            protected set;
        }

        /// <summary>
        /// 生成数据
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TEdge"></typeparam>
        /// <param name="graph">用户图</param>
        /// <param name="stateDataCapacity">单个状态数据量</param>
        /// <param name="metadataSelector"></param>
        /// <param name="flags">生成选项</param>
        public void Generate<TVertex, TEdge>(IGraph<TVertex, TEdge> graph, T defaultMetadata, int maxInlineDeep, int timeout, FATableFlags flags)
            where TEdge : IEdge<TVertex>
            where TVertex : IVertex
        {
            mFlags = flags;
            mTimeout = timeout;
            mMaxInlineDeep = Math.Max(1, maxInlineDeep);
            mDefaultMetadata = defaultMetadata;

            StateCount = graph.VerticesCount + 1;
            Transitions = CreateGraph(graph, flags);
            
            mBuildSteps.AddRange(Transitions.Select(x => new FABuildStep<T>(FABuildStage.CreateGraph, FABuildType.Add, x)));

            Update();
        }

        #region 创建图

        struct SubsetEntry : IEquatable<SubsetEntry>
        {
            public SubsetEntry(FATransition<T>[] transitions)
            {
                Transitions = transitions;
            }

            public FATransition<T>[] Transitions { get; }

            public IEnumerable<FASymbol> Symbols => Transitions.Select(x => x.Symbol);

            /// <summary>
            /// 左侧id
            /// </summary>
            public ushort Left => Transitions[^1].Left;

            /// <summary>
            /// 右侧id
            /// </summary>
            public ushort Right => Transitions[^1].Right;

            /// <summary>
            /// 原始图左侧id
            /// 用于引用层级关系，被使用在制表函数中
            /// </summary>
            public ushort SourceLeft => Transitions[^1].SourceLeft;

            /// <summary>
            /// 原始图右侧id
            /// 用于引用层级关系，被使用在制表函数中
            /// </summary>
            public ushort SourceRight => Transitions[^1].SourceRight;

            /// <summary>
            /// 移进符
            /// </summary>
            public EdgeInput Input => Transitions[^1].Input;

            /// <summary>
            /// 连线所带的用户元数据
            /// </summary>
            public T Metadata => Transitions[^1].Metadata;

            public override bool Equals(object obj)
            {
                return obj is SubsetEntry entry && Equals(entry);
            }

            public bool Equals(SubsetEntry other)
            {
                return Transitions[^1].Equals(other.Transitions[^1]);
            }

            public override int GetHashCode()
            {
                return Transitions[^1].GetHashCode();
            }
        }

        struct SubsetGroup 
        {
            public SubsetGroup(ushort entryPoint, IList<FATransition<T>> transitions)
            {
                EntryPoint = entryPoint;
                Transitions = transitions;
            }

            public ushort EntryPoint { get; }

            public IList<FATransition<T>> Transitions { get; }
        }

        class CreateGraphMemoryModel : ShiftMemoryModel
        {
            private Dictionary<ushort, SubsetGroup> mGroups;

            public CreateGraphMemoryModel(IList<FATransition<T>> transitions, IList<FATransition<T>> subsets) 
                : base(transitions)
            {
                mGroups = subsets.GroupBy(x => x.Symbol.Value).Select(x => new SubsetGroup(x.Key, x.ToList())).ToDictionary(x => x.EntryPoint);
            }

            public SubsetGroup? GetOne() 
            {
                if (mGroups.Count > 0)
                {
                    var group = mGroups.First();
                    mGroups.Remove(group.Key);
                    return group.Value;
                }

                return null;
            }

            public override void Add(FATransition<T> tran)
            {
                base.Add(tran);
                if (tran.Symbol.Value != 0 && mGroups.ContainsKey(tran.Symbol.Value))
                {
                    var group = mGroups[tran.Symbol.Value];
                    group.Transitions.Add(tran);
                }
            }

            public override void Insert(int index, FATransition<T> tran)
            {
                base.Insert(index, tran);
                if (tran.Symbol.Value != 0 && mGroups.ContainsKey(tran.Symbol.Value))
                {
                    var group = mGroups[tran.Symbol.Value];
                    group.Transitions.Add(tran);
                }
            }

            public override void Remove(FATransition<T> tran)
            {
                base.Remove(tran);
                if (tran.Symbol.Value != 0 && mGroups.ContainsKey(tran.Symbol.Value))
                {
                    var group = mGroups[tran.Symbol.Value];
                    group.Transitions.Remove(tran);
                }
            }
        }

        /// <summary>
        /// 从用户Graph中创建FA状态机
        /// </summary>
        private IList<FATransition<T>> CreateGraph<TVertex, TEdge>(IGraph<TVertex, TEdge> graph, FATableFlags flags)
            where TEdge : IEdge<TVertex>
            where TVertex : IVertex
        {
            var stateCount = StateCount;

            // 得到静态
            var transitions = new List<FATransition<T>>();
            var dynamicbag = new List<FATransition<T>>();
            var connect = new TransitionMetadata<EdgeInput, bool>();

            // 得到原始数据
            foreach (var edge in graph.Edges)
            {
                var subset = edge.Subset?.Index ?? 0;
                var types = FASymbolType.None;
                if (edge.Flags.HasFlag(EdgeFlags.SpecialPoint)) types |= FASymbolType.Report;
                if (edge.Flags.HasFlag(EdgeFlags.Close)) types |= FASymbolType.Close;
                //if (edge.Flags.HasFlag(EdgeFlags.SpecialPoint)) types |= FASymbolType.Any;
                var symbol = new FASymbol(types, subset, 0);
                var metadata = GetMetadata<TVertex, TEdge>(edge);

                var input = edge.GetInput();
                var tran = new FATransition<T>(
                    edge.Source.Index,
                    edge.Target.Index, 
                    edge.Source.Index, 
                    edge.Target.Index, 
                    input, 
                    symbol, 
                    metadata);

                if (!transitions.Contains(tran))
                    transitions.Add(tran);

                if (edge.ConnectSubset)
                {
                    dynamicbag.Add(tran);
                    connect[tran.Left, tran.Right, input] = true;
                }
            }

            var memory = new CreateGraphMemoryModel(transitions, dynamicbag);
            // 合并完全一致头
            //MergeHead(memory);

            // 设置所有请求/汇报符号和修正本地递归
            if (flags.HasFlag(FATableFlags.ConnectSubset))
            {
                var delay = new List<(ushort, List<FATransition<T>>)>();

                SubsetGroup? group = null;
                while ((group = memory.GetOne()).HasValue)
                {
                    var subset = group.Value.EntryPoint;
                    var externals = group.Value.Transitions;
                    var internals = new List<FATransition<T>>();
                    var leftRecursions = new List<FATransition<T>>();
                    var recursions = new Dictionary<FATransition<T>, int>();

                    delay.Add((subset, internals));
                    mSubsets.Add(subset);

                    // 查找内部头部至有效数据(符号集和单个input)
                    IEnumerable<SubsetEntry> ShiftRight(List<FATransition<T>> list, FATransition<T> tran)
                    {
                        var isSubset = connect[tran.Left, tran.Right, tran.Input];
                        if (isSubset)
                        {
                            if (recursions.ContainsKey(tran))
                            {
                                recursions[tran]++;
                            }
                        }

                        if (!recursions.ContainsKey(tran))
                        {
                            list.Add(tran);

                            if (!isSubset)
                            {
                                // 修改引用符号,可优化
                                for (var i = 0; i < list.Count - 1; i++)
                                {
                                    var item = list[i];

                                    // 移除内部左递归
                                    if (item.Symbol.Value == subset)
                                    {
                                        list.RemoveAt(i);
                                        i--;
                                        continue;
                                    }

                                    list[i] = new FATransition<T>(
                                       item.Left,
                                       item.Right,
                                       item.SourceLeft,
                                       item.SourceRight,
                                       item.Input,
                                       new FASymbol(FASymbolType.Request | item.Symbol.Type, item.Symbol.Value, 0), item.Metadata);
                                }

                                yield return new SubsetEntry(list.ToArray());
                            }
                            else
                            {
                                var index = tran.Symbol.Value;
                                recursions[tran] = 0;
                                foreach (var right in memory.GetRights(index))
                                {
                                    foreach (var next in ShiftRight(list, right))
                                    {
                                        yield return next;
                                    }
                                }
                            }

                            list.Remove(tran);
                        }
                    }

                    // 查找内部尾部
                    var ends = FindEnds(memory, subset, stateCount, fa =>
                    {
                        if (externals.Contains(fa))
                        {
                            // 如果左递归不包含该移进则填入到内部引用
                            internals.Add(fa);
                            externals.Remove(fa);
                        }
                        return true;
                    }).ToArray();

                    var starts = memory.GetRights(subset).SelectMany(x => ShiftRight(new List<FATransition<T>>(), x)).Distinct().ToArray();
                    // 如果是循环递归的话则抛出异常
                    if (starts.Length == 0)
                        throw new LeftRecursionOverflowException<T>(recursions.OrderByDescending(x => x.Value).FirstOrDefault().Key);

                    // 外部递归符号修改
                    for (int i = 0; i < externals.Count; i++)
                    {
                        var refer = externals[i];
                        foreach (var start in starts)
                        {
                            var meta = refer.Metadata;
                            var dst = new FATransition<T>(
                               refer.Left,
                               refer.Right,
                               refer.SourceLeft,
                               refer.SourceRight,
                               start.Input,
                               new FASymbol(FASymbolType.Request | refer.Symbol.Type, subset, 0),
                               meta);

                            memory.Add(dst);
                        }

                        memory.Remove(refer);
                    }
                }

                // 后处理
                IEnumerable<FATransition<T>> GetHeaders(FATransition<T> tran)
                {
                    var result = FindEnds(memory, tran.Right, stateCount).ToArray();
                    if (result.Length == 0)
                        result = new FATransition<T>[] { tran };

                    return result;
                }

                // 内部递归符号修改
                foreach (var item in delay)
                {
                    var subset = item.Item1;
                    var internals = item.Item2;
                    var entries = memory.GetRights(subset).Where(x => !internals.Contains(x)).ToArray();
                    var headers = entries.SelectMany(GetHeaders).ToArray();

                    // 如果存在左递归则移除所有头
                    if (internals.Any(x => x.Left == subset))
                        foreach (var header in headers)
                            memory.Remove(header);

                    // 最后处理左递归
                    foreach (var refer in internals.OrderBy(x => x.Left == subset))
                    {
                        memory.Remove(refer);

                        if (refer.Left == subset)
                        {
                            var ends = FindEnds(memory, refer.Right, stateCount).ToArray();

                            foreach (var header in headers)
                            {
                                var dst = new FATransition<T>(
                                    header.Left,
                                    refer.Right,
                                    header.SourceLeft,
                                    header.SourceRight,
                                    header.Input,
                                    header.Symbol,
                                    // 原来是单用tran.metadata,但是出现元数据没有转移问题所以合并了refer.metadata
                                    // 注意:这里没有好好调试
                                    // 如果出现元数据引用顺序问题相关BUG在这里调换下这2个元数据的顺序后在测试一次
                                    MergeMetadatas(refer.Metadata, header.Metadata));

                                memory.Add(dst);
                            }

                            foreach (var tran in ends)
                            {
                                var isSelf = tran.Symbol.Value == subset;
                                if(!isSelf)
                                {
                                    memory.Remove(tran);
                                    var dst = new FATransition<T>(
                                       tran.Left,
                                       refer.Right,
                                       tran.SourceLeft,
                                       tran.SourceRight,
                                       tran.Input,
                                       tran.Symbol,
                                       // 原来是单用tran.metadata,但是出现元数据没有转移问题所以合并了refer.metadata
                                       // 注意:这里没有好好调试
                                       // 如果出现元数据引用顺序问题相关BUG在这里调换下这2个元数据的顺序后在测试一次
                                       MergeMetadatas(refer.Metadata, tran.Metadata));

                                    memory.Add(dst);
                                }
                            }
                        }
                        else
                        {
                            foreach (var tran in entries)
                            {
                                var dst = new FATransition<T>(
                                    refer.Left,
                                    refer.Right,
                                    refer.SourceLeft,
                                    refer.SourceRight,
                                    tran.Input,
                                    new FASymbol(FASymbolType.Request | refer.Symbol.Type, subset, 0),
                                    refer.Metadata);

                                memory.Add(dst);
                            }
                        }
                    }
                }
            }

            return transitions;
        }

        private void MergeHead(IShiftMemoryModel model) 
        {
            var merges = model.Transitions.GroupBy(x => new { Left = x.Left, Symbol = x.Symbol, Metadata = x.Metadata, Input = x.Input }).Where(x => x.Count() > 1).Select(x => x.ToArray()).ToArray();
            foreach (var merge in merges)
            {
                var first = merge[0];
                for (var i = 1; i < merge.Length; i++)
                {
                    var follow = merge[i];
                    var rights = model.GetRights(follow.Right).ToArray();
                    for (var x = 0; x < rights.Length; x++)
                    {
                        var last = rights[x];
                        var dst = new FATransition<T>(
                            first.Right, last.Right, last.SourceLeft, last.SourceRight,
                            last.Input, last.Symbol, last.Metadata);

                        model.Add(dst);
                        model.Remove(last);
                    }

                    var lefts = model.GetLefts(follow.Right).ToArray();
                    for (var x = 0; x < lefts.Length; x++)
                    {
                        var last = lefts[x];
                        var dst = new FATransition<T>(
                            last.Left, first.Right, last.SourceLeft, last.SourceRight,
                            last.Input, last.Symbol, last.Metadata);

                        model.Add(dst);
                        model.Remove(last);
                    }

                    model.Remove(follow);
                }
            }
        }

        #endregion

        #region 函数

        private ushort GetSubset(ushort subset) 
        {
            if (subset == 0)
                return 0;

            return mSubsetReplaces.ContainsKey(subset) && !mHasLayoutTransitions ? mSubsetReplaces[subset] : subset;
        }

        /// <summary>
        /// 获得所有子图入口点
        /// </summary>
        public IEnumerable<ushort> GetSubsets() 
        {
            for (var i = 0; i < mSubsets.Count; i++)
                yield return GetSubset(mSubsets[i]);
        }

        /// <summary>
        /// 找到所有入口点
        /// </summary>
        public IEnumerable<ushort> GetEntryPoints()
        {
            var queueGroup = Transitions.AsParallel().
                GroupBy(x => x.Right).
                ToDictionary(x => x.Key);

            return Transitions.
                Where(x => !queueGroup.ContainsKey(x.Left)).
                Select(x => x.Left).Distinct();
        }

        /// <summary>
        /// 找到所有出口点
        /// </summary>
        public IEnumerable<ushort> GetExitPoints()
        {
            var queueGroup = Transitions.AsParallel().
                GroupBy(x => x.Left).
                ToDictionary(x => x.Key);

            var exitPoints = Transitions.
                Where(x => !queueGroup.ContainsKey(x.Right)).
                Distinct().
                ToHashSet();


            var loops = GetLoops().
                Where(x => queueGroup[x[^1].Left].Where(y => !x.Contains(y)).Count() == 0);

            return exitPoints.Select(x => x.Right).Concat(loops.Select(x => x[^1].Left)).Distinct();
        }

        /// <summary>
        /// 获取内部入口点，主要用于获取循环时的入口点
        /// </summary>
        private IList<ushort> GetInternalEntryPoints(FATableFlags flags) 
        {
            var points = new List<ushort>();
            points.Add(1);
            if (flags.HasFlag(FATableFlags.KeepSubset))
                foreach (var subset in GetSubsets())
                    points.Add(subset);

            return points;
        }

        /// <summary>
        /// 获取实际转跳移进
        /// </summary>
        private static FATransition<T> GetJumpTransition(IShiftRightMemoryModel model, FATransition<T> tran)
        {
            if (!tran.Symbol.Type.HasFlag(FASymbolType.Request) || tran.Symbol.Value == 0)
                throw new NotImplementedException("内部错误");

            var rights = model.GetRights(tran.Symbol.Value);
            var trans = rights.Where(x => x.Input.GetCommonDivisor(tran.Input).IsVaild()).ToArray();

            if (trans.Length == 0)
                throw new NotImplementedException("前期预测1步移进丢失造成的内部错误，出现的情况复杂，需要通过可视图调试。");

            if (trans.Length != 1)
                throw new NotImplementedException("前期冲突解决未完全处理造成的内部错误。");

            return trans[0];
        }

        /// <summary>
        /// 获得有效路径点
        /// </summary>
        protected HashSet<ushort> GetVaildStates(IShiftRightMemoryModel model, FATableFlags flags) 
        {
            var entryPoints = new HashSet<ushort>();
            if (flags.HasFlag(FATableFlags.KeepSubset))
                for (var i = 0; i < mSubsets.Count; i++)
                    entryPoints.Add(GetSubset(mSubsets[i]));

            entryPoints.Add(1);

            bool[] visitor = new bool[StateCount + 1];
            var stack = new Stack<ushort>(entryPoints);
            visitor[0] = true;

            while (stack.Count > 0)
            {
                var state = stack.Pop();
                if (visitor[state])
                    continue;

                visitor[state] = true;
                foreach (var next in model.GetRights(state))
                {
                    if (next.Symbol.Value != 0)
                        stack.Push(GetSubset(next.Symbol.Value));

                    stack.Push(next.Right);
                }
            }

            return visitor.Select((x, i) => (x, i)).Where(x => x.x).Select(x => (ushort)x.i).ToHashSet();
        }

        /// <summary>
        /// 快速获取从指定状态开始得到所有循环
        /// </summary>
        public IList<TransitionLoop> GetLoops()
        {
            return GetLoops(new ShiftRightMemoryModel(Transitions), StateCount, GetInternalEntryPoints(mFlags));
        }

        /// <summary>
        /// 快速获取从指定状态开始得到所有循环
        /// </summary>
        protected static IList<TransitionLoop> GetLoops(IShiftRightMemoryModel model, int stateCount, IEnumerable<ushort> points) 
        {
            var loops = new List<TransitionLoop>();
            var loopStack = new Queue<TransitionLoop>();
            var loopsVisitor = new bool[stateCount + 1];
            var pointStack = new Stack<ushort>(points);

            // 查找所有循环，创建一个 Stack 对象，用于存储转换后的 LoopLine 对象
            // 遍历所有步骤分支入口
            while(pointStack.Count > 0)
            {
                var point = pointStack.Pop();
                // 获取当前步骤分支入口指向的所有步骤分支线
                var currentForks = model.GetRights(point);

                // 遍历当前入口指向的所有步骤分支线
                for (var lineIndex = 0; lineIndex < currentForks.Count; lineIndex++)
                {
                    // 创建当前步骤分支线对应的 LoopLine 对象,并压入栈给后续算法使用
                    var currentLoop = new TransitionLoop();
                    var currentFork = currentForks[lineIndex];
                    if (loopsVisitor[currentFork.Right])
                        continue;

                    currentLoop.Add(currentFork);
                    loopStack.Enqueue(currentLoop);

                    var visitRights = new HashSet<ushort>();
                    visitRights.Add(currentFork.Right);

                    while (loopStack.Count > 0)
                    {
                        currentLoop = loopStack.Dequeue();
                        currentFork = currentLoop[^1];
                        var currentForkRights = model.GetRights(currentFork.Right);

                        if (currentFork.Symbol.Type.HasFlag(FASymbolType.Request))
                            pointStack.Push(currentFork.Symbol.Value);

                        if (currentForkRights.Count > 1)
                        {
                            // 如果存在多个分叉线，则将其分裂成多个循环
                            for (var i = 0; i < currentForkRights.Count; i++)
                            {
                                var nextLoop = new TransitionLoop(currentLoop);
                                var right = currentForkRights[i];
                                if (!loopsVisitor[right.Right])
                                {
                                    nextLoop.Add(right);

                                    if (!nextLoop.CheckLoop())
                                        loopStack.Enqueue(nextLoop);
                                    else
                                        loops.Add(nextLoop);

                                    //loopsVisitor[right.Right] = true;
                                    visitRights.Add(right.Right);
                                }
                            }
                        }
                        else if (currentForkRights.Count == 1)
                        {
                            var right = currentForkRights[0];
                            if (!loopsVisitor[right.Right])
                            {
                                currentLoop.Add(right);
                                if (currentLoop.CheckLoop())
                                    loops.Add(currentLoop);
                                else
                                    loopStack.Enqueue(currentLoop);

                                //loopsVisitor[right.Right] = true;
                                visitRights.Add(right.Right);
                            }
                        }
                    }

                    foreach (var right in visitRights)
                        loopsVisitor[right] = true;
                }
            }

            return loops;
        }

        private static IEnumerable<FATransition<T>> FindEndsWithoutLoop(IShiftRightMemoryModel model, ushort index, int stateCount) 
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<FATransition<T>> FindEnds(IShiftRightMemoryModel model, ushort index, int stateCount) 
        {
            return FindEnds(model, index, stateCount, null);
        }

        private static IEnumerable<FATransition<T>> FindEnds(IShiftRightMemoryModel model, ushort index, int stateCount, Func<FATransition<T>, bool> step)
        {
            var visitor = new bool[stateCount];
            var queue = new Queue<ushort>();
            queue.Enqueue(index);
            while (queue.Count > 0)
            {
                var state = queue.Dequeue();
                if (visitor[state])
                    continue;

                visitor[state] = true;
                foreach (var right in model.GetRights(state))
                {
                    var vaild = step == null || step(right);
                    if (vaild)
                    {
                        if (model.GetRights(right.Right).Count <= 0)
                        {
                            yield return right;
                        }
                        else
                        {
                            queue.Enqueue(right.Right);
                        }
                    }
                }
            }
        }

        public static int GetDeep(IShiftRightMemoryModel model, ushort index, int stateCount)
        {
            int deep = 0;

            var visitor = new bool[stateCount];
            var queue = new Queue<IEnumerable<ushort>>();
            queue.Enqueue(new ushort[] { index });
            while (queue.Count > 0)
            {
                var states = queue.Dequeue();
                var rights = states.SelectMany(model.GetRights).Select(x => x.Right).Distinct().Where(x => !visitor[x]).ToArray();
                if (rights.Length > 0) 
                {
                    for (var i = 0; i < rights.Length; i++)
                        visitor[rights[i]] = true;

                    deep++;
                    queue.Enqueue(rights);
                }
            }

            return deep;
        }

        public static void WalkToEnds(IShiftRightMemoryModel model, ushort index, int stateCount, Action<FATransition<T>> step)
        {
            var visitor = new bool[stateCount];
            var queue = new Queue<ushort>();
            queue.Enqueue(index);
            while (queue.Count > 0)
            {
                var state = queue.Dequeue();
                if (state < 0 || state >= visitor.Length || visitor[state])
                    continue;

                visitor[state] = true;
                foreach (var right in model.GetRights(state))
                {
                    step(right);
                    if (model.GetRights(right.Right).Count <= 0)
                    {
                    }
                    else
                    {
                        queue.Enqueue(right.Right);
                    }
                }
            }
        }

        private static IEnumerable<FATransition<T>> FindStarts(IDictionary<ushort, IList<FATransition<T>>> lefts, int stateCount, ushort index) 
        {
            var visitor = new bool[stateCount];
            var queue = new Queue<ushort>();
            queue.Enqueue(index);
            while (queue.Count > 0)
            {
                var state = queue.Dequeue();
                if (visitor[state])
                    continue;

                visitor[state] = true;
                if (lefts.ContainsKey(state))
                {
                    foreach (var left in lefts[state])
                    {
                        if (!lefts.ContainsKey(left.Left) || lefts[left.Left].Count <= 0)
                        {
                            yield return left;
                        }
                        else
                        {
                            queue.Enqueue(left.Right);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 向右查找可用移进(包含转跳)
        /// </summary>
        /// <param name="index">查找状态点</param>
        public IEnumerable<FATransition<T>> FindSubsetRights(ushort index) 
        {
            return FindSubsetRights(new ShiftRightMemoryModel(Transitions), index);
        }

        /// <summary>
        /// 向右查找可用移进
        /// </summary>
        public IEnumerable<FATransition<T>> FindSubsetRights(IShiftRightMemoryModel model, ushort index) 
        {
            var visitor = new bool[StateCount];
            var queue = new Queue<ushort>();
            queue.Enqueue(index);

            while (queue.Count > 0)
            {
                var state = queue.Dequeue();
                if (visitor[state])
                    continue;

                visitor[state] = true;

                var rights = model.GetRights(state);
                for (var i = 0; i < rights.Count; i++)
                {
                    var right = rights[i];
                    if (right.Symbol.Value == 0)
                    {
                        yield return right;
                    }
                    else
                    {
                        queue.Enqueue(right.Symbol.Value);
                    }
                }        
            }
        }

        private static IEnumerable<FATransition<T>> FindSubsetRights(IReadOnlyDictionary<ushort, List<FATransition<T>>> rights, ushort index, int stateCount)
        {
            var trans = rights.SelectMany(x => x.Value).ToArray();
            var connect = new TransitionMetadata<EdgeInput, bool>();
            for (var i = 0; i < trans.Length; i++)
            {
                var tran = trans[i];
                connect[tran.Left, tran.Right, tran.Input] = true;
            }

            var visitor = new bool[stateCount];
            var queue = new Queue<ushort>();
            queue.Enqueue(index);

            while (queue.Count > 0)
            {
                var state = queue.Dequeue();
                if (visitor[state])
                    continue;

                visitor[state] = true;
                if (rights.ContainsKey(state))
                {
                    var nexts = rights[state];
                    for (var i = 0; i < nexts.Count; i++)
                    {
                        var next = nexts[i];
                        if (connect[next.Left, next.Right, next.Input])
                        {
                            if (next.Symbol.Value == 0)
                            {
                                yield return next;
                            }
                            else
                            {
                                queue.Enqueue(next.Symbol.Value);
                            }
                        }
                        else
                        {
                            yield return next;
                        }
                    }
                }
            }
        }

        private static IList<FATransition<T>> FindSubsetStarts(IDictionary<ushort, IList<FATransition<T>>> rights, ushort index, int stateCount, TransitionMetadata<EdgeInput, bool> connect)
        {
            var visitor = new bool[stateCount];
            var starts = FindStarts(rights, stateCount, index).ToList();
            while (true)
            {
                var @break = true;
                for (var i = 0; i < starts.Count; i++)
                {
                    var target = starts[i];
                    if (connect[target.Left, target.Right, target.Input])
                    {
                        starts.RemoveAt(i);
                        i--;

                        if (!visitor[target.Left] && rights.ContainsKey(target.Left))
                            starts.AddRange(rights[target.Left]);

                        visitor[target.Left] = true;
                        @break = false;
                    }
                }

                if (@break) break;
            }

            return starts;
        }

        private static IEnumerable<FATransition<T>> FindSubsetEnds(IDictionary<ushort, FATransition<T>[]> rights, ushort index, int stateCount, TransitionMetadata<EdgeInput, bool> connect)
        {
            if (rights.ContainsKey(index))
            {
                var visitor = new TransitionMetadata<EdgeInput, bool>();
                var stack = new Stack<FATransition<T>>(rights[index]);
                while (stack.Count > 0)
                {
                    var tran = stack.Pop();
                    if (visitor[tran.Left, tran.Right, tran.Input])
                        continue;
 
                    visitor[tran.Left, tran.Right, tran.Input] = true;

                    //进入右侧
                    if (rights.ContainsKey(tran.Right))
                    {
                        foreach (var right in rights[tran.Right])
                        {
                            stack.Push(right);
                        }
                    }
                    // 进入子集
                    else if (connect[tran.Left, tran.Right, tran.Input])
                    {
                        if (rights.ContainsKey(tran.Symbol.Value))
                        {
                            foreach (var right in rights[tran.Symbol.Value])
                            {
                                stack.Push(right);
                            }
                        }
                    }
                    else
                    {
                        yield return tran;
                    }
                }
            }
        }

        private static IEnumerable<FATransition<T>> ForEach(IList<FATransition<T>> transitions, ushort index, int stateCount) 
        {
            return ForEach(transitions.GroupBy(x => x.Left).ToDictionary(x => x.Key, x => x.ToArray()), index, stateCount);
        }

        private static IEnumerable<FATransition<T>> ForEach(IDictionary<ushort, FATransition<T>[]> rights, ushort index, int stateCount) 
        {
            var visitor = new bool[stateCount];
            var queue = new Queue<ushort>();
            queue.Enqueue(index);
            while (queue.Count > 0)
            {
                var state = queue.Dequeue();
                if (visitor[state])
                    continue;

                visitor[state] = true;

                if (rights.ContainsKey(state))
                {
                    foreach (var next in rights[state]) 
                    {
                        queue.Enqueue(next.Right);
                        yield return next;
                    }
                }
            }
        }

        private static void ForEachSubset(IDictionary<ushort, FATransition<T>[]> rights, ushort index, int stateCount, Action<FATransition<T>> action)
        {
            var visitor = new bool[stateCount];
            var queue = new Queue<ushort>();
            queue.Enqueue(index);
            while (queue.Count > 0)
            {
                var state = queue.Dequeue();
                if (visitor[state])
                    continue;

                visitor[state] = true;

                if (rights.ContainsKey(state))
                {
                    foreach (var next in rights[state])
                    {
                        if(next.Symbol.Value != 0)
                            queue.Enqueue(next.Symbol.Value);

                        queue.Enqueue(next.Right);
                        action(next);
                    }
                }
            }
        }

        private static List<List<TElement>> GetCombinations<TElement>(int currentIndex, List<TElement[]> containers)
        {
            if (currentIndex == containers.Count)
            {
                // Skip the items for the last container
                var combinations2 = new List<List<TElement>>();
                combinations2.Add(new List<TElement>());
                return combinations2;
            }

            var combinations = new List<List<TElement>>();

            var containerItemList = containers[currentIndex];
            while (containerItemList == null)
            {
                containerItemList = containers[++currentIndex];
                if (currentIndex >= containers.Count)
                {
                    break;
                }
            }
            // Get combination from next index
            var suffixList = GetCombinations(currentIndex + 1, containers);
            int size = containerItemList == null ? 0 : containerItemList.Length;
            for (int ii = 0; ii < size; ii++)
            {
                TElement containerItem = containerItemList[ii];
                if (suffixList != null)
                {
                    foreach (var suffix in suffixList)
                    {
                        var nextCombination = new List<TElement>();
                        nextCombination.Add(containerItem);
                        nextCombination.AddRange(suffix);
                        combinations.Add(nextCombination);
                    }
                }
            }
            return combinations;
        }

        static TwoKeyDictionary<TKey, TKey2, TValue> ToTwoKeyDictionary<TSource, TKey, TKey2, TValue>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TKey2> keySelector2, Func<TSource, TValue> valueSelector)
        {
            var dict = new TwoKeyDictionary<TKey, TKey2, TValue>();
            foreach (var item in source)
                dict[keySelector(item), keySelector2(item)] = valueSelector(item);

            return dict;
        }

        #endregion

        #region 结构类型

        public interface IShiftRightMemoryModel : IReadOnlyDictionary<ushort, List<FATransition<T>>>
        {
            Dictionary<ushort, List<FATransition<T>>> Rights { get; }

            IList<FATransition<T>> Transitions { get; }

            IList<FATransition<T>> GetRights(ushort state);

            void Insert(int index, FATransition<T> transition);

            void Add(FATransition<T> transition);

            void Remove(FATransition<T> transition);

            bool Contains(FATransition<T> transition);
        }

        public interface IShiftMemoryModel : IShiftRightMemoryModel
        {
            IList<FATransition<T>> GetLefts(ushort state);
        }

        public class ShiftRightMemoryModel : IShiftRightMemoryModel, IEnumerable<FATransition<T>>
        {
            protected internal readonly static FATransition<T>[] EMPTY = new FATransition<T>[0];

            private IList<FATransition<T>> mTransitions;
            private Dictionary<ushort, List<FATransition<T>>> mRights;

            public IList<FATransition<T>> Transitions => mTransitions;

            public IEnumerable<ushort> Keys => mRights.Keys;

            public IEnumerable<List<FATransition<T>>> Values => mRights.Values;

            public int Count => mRights.Count;

            public Dictionary<ushort, List<FATransition<T>>> Rights => mRights;

            public List<FATransition<T>> this[ushort key] => mRights[key];

            public ShiftRightMemoryModel(IList<FATransition<T>> transitions)
            {
                mTransitions = transitions;
                mRights = transitions.GroupBy(x => x.Left).
                    ToDictionary(x => x.Key, x => x.ToList());
            }

            public IList<FATransition<T>> GetRights(ushort state)
            {
                if (!mRights.ContainsKey(state))
                    return EMPTY;

                return mRights[state];
            }

            public virtual void Add(FATransition<T> tran)
            {
                if (!mRights.ContainsKey(tran.Left))
                    mRights[tran.Left] = new List<FATransition<T>>();

                mTransitions.Add(tran);
                mRights[tran.Left].Add(tran);
            }

            public virtual void Remove(FATransition<T> tran)
            {
                mTransitions.Remove(tran);
                mRights[tran.Left].Remove(tran);
            }

            public virtual void Insert(int index, FATransition<T> tran)
            {
                if (!mRights.ContainsKey(tran.Left))
                    mRights[tran.Left] = new List<FATransition<T>>();

                mRights[tran.Left].Insert(index, tran);
                mTransitions.Add(tran);
            }

            public virtual bool Contains(FATransition<T> transition)
            {
                return mTransitions.Contains(transition);
            }

            public bool ContainsKey(ushort key)
            {
                return mRights.ContainsKey(key);
            }

            public bool TryGetValue(ushort key, out List<FATransition<T>> value)
            {
                return mRights.TryGetValue(key, out value);
            }

            IEnumerator<KeyValuePair<ushort, List<FATransition<T>>>> IEnumerable<KeyValuePair<ushort, List<FATransition<T>>>>.GetEnumerator()
            {
                return mRights.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return mRights.GetEnumerator();
            }

            IEnumerator<FATransition<T>> IEnumerable<FATransition<T>>.GetEnumerator()
            {
                return Transitions.GetEnumerator();
            }
        }

        public class ShiftMemoryModel : ShiftRightMemoryModel, IShiftMemoryModel
        {

            private IDictionary<ushort, List<FATransition<T>>> mLeftTrans;

            public ShiftMemoryModel(IList<FATransition<T>> transitions)
                : base(transitions)
            {
                mLeftTrans = transitions.GroupBy(x => x.Right).ToDictionary(x => x.Key, x => x.ToList());
            }

            public IList<FATransition<T>> GetLefts(ushort state)
            {
                return mLeftTrans.ContainsKey(state) ? mLeftTrans[state] : EMPTY;
            }

            public override void Add(FATransition<T> tran)
            {
                base.Add(tran);

                if (mLeftTrans != null && !mLeftTrans.ContainsKey(tran.Right))
                    mLeftTrans[tran.Right] = new List<FATransition<T>>();

                mLeftTrans[tran.Right].Add(tran);
            }

            public override void Insert(int index, FATransition<T> tran)
            {
                base.Insert(index, tran);
                if (mLeftTrans != null && !mLeftTrans.ContainsKey(tran.Right))
                    mLeftTrans[tran.Right] = new List<FATransition<T>>();

                mLeftTrans[tran.Right].Insert(index, tran);
            }

            public override void Remove(FATransition<T> tran)
            {
                base.Remove(tran);
                if (mLeftTrans.ContainsKey(tran.Right))
                    mLeftTrans[tran.Right].Remove(tran);
            }
        }

        public class TransitionLoop : List<FATransition<T>>
        {
            public TransitionLoop()
            {
            }

            public TransitionLoop(TransitionLoop parent)
            {
                AddRange(parent);
            }

            public TransitionLoop Parent { get; set; }

            public ushort Left => this[0].Left;

            public ushort Right => this[^1].Right;

            internal int FindPointLineIndex(ushort point)
            {
                for (var i = 0; i < Count; i++)
                {
                    var line = this[i];
                    if (line.Left == point)
                        return i;
                }

                return -1;
            }

            internal bool CheckLoop()
            {
                var last = this[Count - 1];
                var index = FindPointLineIndex(last.Right);
                if (index != -1)
                {
                    for (var i = 0; i < index; i++)
                        RemoveAt(0);

                    return true;
                }

                return false;
            }
        }

        struct IntersectionPoint 
        {
            public IntersectionPoint(ushort state, FATransition<T> shortStart, FATransition<T> longStart)
            {
                State = state;
                ShortStart = shortStart;
                LongStart = longStart;
            }

            public ushort State { get; }

            public FATransition<T> ShortStart { get; }

            public FATransition<T> LongStart { get; }
        }

        class TransitionMetadata<TData, TMetadata> : IEnumerable<TMetadata>
        {
            Dictionary<ushort, Dictionary<ushort, Dictionary<TData, TMetadata>>> mDict;

            public IEnumerable<TData> Datas => mDict.SelectMany(x => x.Value.Values.SelectMany(y => y.Keys));

            public IEnumerable<(ushort left, ushort right)> Edges => mDict.SelectMany(x => x.Value.Select(y => (x.Key, y.Key)));

            public TMetadata this[ushort left, ushort right, TData data]
            {
                get
                {
                    if (!mDict.ContainsKey(left))
                        return default;

                    if (!mDict[left].ContainsKey(right))
                        return default;

                    if (!mDict[left][right].ContainsKey(data))
                        return default;

                    return mDict[left][right][data];
                }
                set
                {
                    if (!mDict.ContainsKey(left))
                        mDict[left] = new Dictionary<ushort, Dictionary<TData, TMetadata>>();

                    if (!mDict[left].ContainsKey(right))
                        mDict[left][right] = new Dictionary<TData, TMetadata>();

                    mDict[left][right][data] = value;
                }
            }

            public bool ContainsKey(ushort left, ushort right, TData data) 
            {
                return mDict.ContainsKey(left) && mDict[left].ContainsKey(right) && mDict[left][right].ContainsKey(data);
            }

            public void Remove(ushort left, ushort right, TData data)
            {
                if (ContainsKey(left, right, data)) 
                {
                    mDict[left][right].Remove(data);
                }
            }

            public IEnumerator<TMetadata> GetEnumerator()
            {
                return mDict.SelectMany(x => x.Value.SelectMany(y => y.Value.Select(z => z.Value))).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Claer()
            {
                mDict.Clear();
            }

            public TransitionMetadata() 
            {
                mDict = new Dictionary<ushort, Dictionary<ushort, Dictionary<TData, TMetadata>>>();
            }
        }

        #endregion

        #region 抽象实现

        /// <summary>
        /// 获得空符号
        /// </summary>
        public abstract EdgeInput EmptyInput { get; }

        /// <summary>
        /// 当数据退化时,合并元数据并返回移进
        /// 可能会造成<paramref name="degradation"/>移进信息消失
        /// </summary>
        /// <param name="keep">在数据退化时保持存在的移进</param>
        /// <param name="degradation">在数据退化时消失的移进</param>
        /// <returns>返回一个用户指定的元数据</returns>
        protected virtual FATransition<T> DegradationReduction(FATransition<T> keep, FATransition<T> degradation)
        {
            return keep;
        }

        /// <summary>
        /// 获取边的元数据
        /// </summary>
        protected abstract T GetMetadata<TVertex, TEdge>(TEdge edge)
            where TEdge : IEdge<TVertex>
            where TVertex : IVertex;

        /// <summary>
        /// 安排元数据在边中的位置
        /// </summary>
        /// <param name="metadata">元数据</param>
        /// <param name="front">此值为0时代表不提前<paramref name="metadata"/>位置，否则应在<paramref name="metadata"/>应用时按数值越大越先使用</param>
        protected abstract T ArrangeMetadata(T metadata, int front);

        /// <summary>
        /// 获得合并后的元数据
        /// </summary>
        protected T MergeMetadatas(params T[] metadatas) 
        {
            return MergeMetadatas(metadatas, metadatas.Length - 1);
        }

        /// <summary>
        /// 获得合并后的元数据
        /// </summary>
        protected T MergeMetadatas(int tokenIndex, params T[] metadatas)
        {
            return MergeMetadatas(metadatas, tokenIndex);
        }

        /// <summary>
        /// 获得合并后的元数据
        /// </summary>
        protected abstract T MergeMetadatas(IList<T> metadatas, int tokenIndex);

        /// <summary>
        /// 判断是否为空移进
        /// </summary>
        protected abstract bool IsEmptyTransition(FATransition<T> tran);

        /// <summary>
        /// 判断元数据是否为默认元数据
        /// </summary>
        protected virtual bool IsDefaultMetadata(T metadata) 
        {
            return metadata.Equals(mDefaultMetadata);
        }

        /// <summary>
        /// 抛出冲突异常
        /// </summary>
        protected virtual void ThrowConflictException(IShiftRightMemoryModel model, ConflictException<T> ex) => throw ex;

        #endregion
    }
}
