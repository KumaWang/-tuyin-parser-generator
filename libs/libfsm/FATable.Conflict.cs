using System;
using System.Collections.Generic;
using System.Linq;

namespace libfsm
{
    partial class FATable<T>
    {
        struct ConflictPrediction
        {
            public ConflictPrediction(FATransition<T> origin, FATransition<T> shift)
            {
                Origin = origin;
                Shift = shift;
            }

            public FATransition<T> Origin { get; }

            public FATransition<T> Shift { get; }
        }

        struct LoopConflict
        {
            public LoopConflict(FATransition<T> head, FATransition<T> left, FATransition<T> right, FATransition<T> loop)
            {
                Head = head;
                Left = left;
                Right = right;
                Loop = loop;
            }

            public FATransition<T> Head { get; }

            public FATransition<T> Left { get; }

            public FATransition<T> Right { get; }

            public FATransition<T> Loop { get; }
        }

        /// <summary>
        /// 获得当前存在的冲突
        /// </summary>
        protected virtual IList<FATransition<T>> GetConflicts(IShiftRightMemoryModel model)
        {
            foreach (var right in model.Rights.OrderByDescending(x => x.Key))
            {
                for (var x = right.Value.Count - 1; x >= 0; x--)
                {
                    var l = right.Value[x];
                    var r = new List<FATransition<T>>();
                    for (var y = right.Value.Count - 1; y >= 0; y--)
                    {
                        if (x == y) continue;
                        var n = right.Value[y];
                        if (l.Equals2(n))
                        {
                            r.Add(n);
                            continue;
                        }

                        if (l.Input.GetCommonDivisor(n.Input).IsVaild())
                        {
                            r.Add(n);
                        }
                    }

                    if (r.Count > 0)
                    {
                        r.Add(l);
                        return r;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 计算一组移进的HashCode
        /// </summary>
        private static int ComputeTransitionHashCode(IList<FATransition<T>> transitions)
        {
            var hashs = transitions.OrderBy(x => x.Right).Select(x => x.GetHashCode()).ToArray();
            var hash = hashs[0];
            for (var i = 1; i < hashs.Length; i++)
                hash = HashCode.Combine(hash, hashs[i]);

            return hash;
        }

        /// <summary>
        /// 解决连接冲突
        /// </summary>
        /// <param name="transitions">移进状态</param>
        /// <param name="edgeConflictsFlags">冲突检测选项</param>
        protected IEnumerable<FABuildStep<T>> ConflictResolution(IList<FATransition<T>> transitions, FATableFlags flags)
        {
            return ConflictResolution(new ShiftRightMemoryModel(transitions), flags);
        }

        /// <summary>
        /// 解决连接冲突
        /// </summary>
        /// <param name="transitions">移进状态</param>
        /// <param name="edgeConflictsFlags">冲突检测选项</param>
        protected virtual IEnumerable<FABuildStep<T>> ConflictResolution(IShiftRightMemoryModel model, FATableFlags flags)
        {
            var edgeConflictsFlags = ConflictsDetectionFlags.Next;
            if (flags.HasFlag(FATableFlags.MetadataConflicts))
                edgeConflictsFlags |= ConflictsDetectionFlags.Metadata;

            if (flags.HasFlag(FATableFlags.SymbolConflicts))
                edgeConflictsFlags |= ConflictsDetectionFlags.Symbol;

            var front = int.MaxValue - 1;
            var max = StateCount;
            var conflicts = GetConflicts(model);

            var derives = new Dictionary<FATransition<T>, FATransition<T>>();
            var memory = new Dictionary<int, FATransition<T>>();

            var startTime = DateTime.Now;
            while (conflicts != null)
            {
                // 移除完全相同部分
                var sameList = conflicts.GroupBy(x => new { a = x.Left, b = x.Right, c = x.Input, d = x.Symbol, e = x.Metadata }).Select(x => x.ToArray()).ToArray();
                var restart = false;
                for (var i = 0; i < sameList.Length; i++)
                {
                    var list = sameList[i];
                    for (var x = 1; x < list.Length; x++)
                    {
                        var tran = list[x];
                        model.Remove(tran);
                        restart = true;
                    }
                }

                if (restart)
                {
                    // 得到下次任务
                    conflicts = GetConflicts(model);
                    continue;
                }

                // 查询衍生
                var froms = conflicts.Select(x => derives.ContainsKey(x) ? derives[x] : x).ToArray();

                // 计算来源哈希码
                var fromHash = ComputeTransitionHashCode(froms);

                // 对冲突分组
                var subsetGroups = conflicts.GroupBy(x => x.Symbol.Value).ToArray();
                var isExpand = subsetGroups.Length > 1;

                // 获取最大符号冲突
                FASymbol symbol = default;
                var symbolTrans = conflicts.GroupBy(x => x.Symbol).Select(x => new SymbolGroup<T>(x.Key, x.ToArray())).ToArray();
                if (symbolTrans.Length > 1)
                {
                    // 解决符号冲突
                    if (!edgeConflictsFlags.HasFlag(ConflictsDetectionFlags.Symbol))
                        // 符号不同则抛出
                        throw new SymbolConflictException<T>(symbolTrans);
                    else
                    {
                        if (!SymbolConflictResolution(model, symbolTrans, ref max, out symbol))
                            throw new SymbolConflictException<T>(symbolTrans);
                    }
                }
                else symbol = isExpand ? default : symbolTrans[0].Transitions[0].Symbol;

                // 元数据冲突
                T metadata = default;
                var metadataTrans = conflicts.GroupBy(x => x.Metadata).Select(x => new MetadataGroup<T>(x.Key, x.ToArray())).ToArray();
                if (metadataTrans.Length > 1 || isExpand)
                {
                    // 解决元数据冲突
                    if (!edgeConflictsFlags.HasFlag(ConflictsDetectionFlags.Metadata))
                        throw new MetadataConflictException<T>(metadataTrans);
                    else
                    {
                        if (!MetadataConflictResolution(model, metadataTrans, ref max, out metadata))
                            throw new MetadataConflictException<T>(metadataTrans);
                    }
                }
                else metadata = metadataTrans[0].Transitions[0].Metadata;

                // 保留未冲突部分
                var commonInput = conflicts[0].Input.GetCommonDivisor(conflicts[1].Input);

                // 提取每条冲突有效部分
                if (commonInput.IsVaild())
                {
                    for (var i = 0; i < conflicts.Count; i++)
                    {
                        var transition = conflicts[i];
                        var input = transition.Input.Eliminate(commonInput);
                        if (input.IsVaild())
                        {
                            var vaild = new FATransition<T>(
                                transition.Left,
                                transition.Right,
                                transition.SourceLeft,
                                transition.SourceRight,
                                input,
                                transition.Symbol,
                                transition.Metadata);

                            model.Add(vaild);
                            yield return new FABuildStep<T>(FABuildStage.ConflictResolution, FABuildType.Add, vaild);
                        }
                    }
                }
                else throw new FAException("无法确定冲突符号");

                // 找到三角冲突         
                var intersection = conflicts.FirstOrDefault(x => x.Input.Equals(commonInput));
                var conflictDone = false;
                var newState = (ushort)(++max);
                var subsetNewState = (ushort)(++max);
                var predictions = new List<ConflictPrediction>();

                // 还原移进
                //var transformToken = false;
                //var anchorContent = $"{intersection.Left}->{newState}";
                foreach (var group in subsetGroups)
                {
                    foreach (var transition in group.OrderByDescending(x => x.Symbol.Type.HasFlag(FASymbolType.Request)))
                    {
                        mSubsetReplaces[transition.Right] = newState;

                        var subsetConectCount = 0;
                        model.Remove(transition);
                        yield return new FABuildStep<T>(FABuildStage.ConflictResolution, FABuildType.Delete, transition);

                        var rights = model.GetRights(transition.Right);
                        conflictDone = conflictDone || rights.Count > 0;
                        var isSubset = transition.Symbol.Type.HasFlag(FASymbolType.Request) && isExpand;
                        var addivtes = new List<T>();
                        if (isSubset)
                        {
                            // 查找到指定冲突边的右侧
                            var subsetRights = model.GetRights(transition.Symbol.Value).Where(x => x.Input == transition.Input).ToArray();

                            // 链接到新状态
                            for (var z = 0; z < subsetRights.Length; z++)
                            {
                                var doFront = false;
                                var subsetRight = subsetRights[z];
                                var subsetRightMetadata = subsetRight.Metadata.Equals(metadata) ? mDefaultMetadata : subsetRight.Metadata;

                            RERIGHT:
                                var subsetLoopSelf = subsetRight.Left == subsetRight.Right;
                                var subsetRightRights = model.GetRights(subsetRight.Right);
                                if (subsetRightRights.Count == 0)
                                {
                                    if (subsetRight.Symbol.Type.HasFlag(FASymbolType.Request))
                                    {
                                        subsetRight = model.GetRights(subsetRight.Symbol.Value).First(x => x.Input == transition.Input);
                                        subsetRightMetadata = MergeMetadatas(subsetRight.Metadata, subsetRightMetadata);
                                        goto RERIGHT;
                                    }
                                    else
                                    {
                                        addivtes.Add(subsetRightMetadata);
                                    }
                                }
                                else
                                {
                                    var currSubsetRight = subsetRight;
                                    while (currSubsetRight.Symbol.Type.HasFlag(FASymbolType.Request)) 
                                    {
                                        currSubsetRight = model.GetRights(currSubsetRight.Symbol.Value).FirstOrDefault(x => x.Input == transition.Input);
                                        if (!currSubsetRight.Equals(default))
                                        {
                                            subsetRightMetadata = MergeMetadatas(currSubsetRight.Metadata, subsetRightMetadata);
                                            doFront = true;
                                        }
                                    }
                                }

                                for (var x = 0; x < subsetRightRights.Count; x++)
                                {
                                    subsetConectCount++;
                                    conflictDone = true;

                                    var subsetRightRight = subsetRightRights[x];

                                    // 检查目标队列是否存在更下的步骤
                                    var subsetRightHasNext = model.GetRights(subsetRightRight.Right).Count > 0;
                                    var subsetSymbol = subsetRightHasNext ? new FASymbol(transition.Symbol.Type | FASymbolType.Request, subsetRightRight.Left, 0) : subsetRightRight.Symbol;
  
                                    var subsetMetadata = subsetRightRight.Metadata;
                                    if (doFront)
                                    {
                                        subsetRightMetadata = ArrangeMetadata(subsetRightMetadata, --front);

                                        // 因为doFront代表此移进是从其他子图外部移动而来
                                        // 所以通知后续动作需要对使用的token进行转换
                                        // 从而避免与当前图token使用冲突
                                        //subsetRightMetadata = TransformMetadata(subsetRightMetadata, anchorContent);

                                        // 设置transformToken为true来启用当前移进token转换
                                        //transformToken = true;
                                    }

                                    if (!subsetRightMetadata.Equals(default))
                                        subsetMetadata = MergeMetadatas(subsetRightHasNext ? 2 : 1, subsetRightMetadata, subsetMetadata, transition.Metadata);
                                    else
                                        subsetMetadata = MergeMetadatas(subsetRightHasNext ? 1 : 0, subsetMetadata, transition.Metadata);

                                    var subsetMoveTran = new FATransition<T>(
                                        newState, subsetNewState,
                                        subsetRightRight.SourceLeft, subsetRightRight.SourceRight,
                                        subsetRightRight.Input, subsetSymbol, subsetMetadata);

                                    // 检查可否消除预测
                                    if (flags.HasFlag(FATableFlags.Predicate) && model.GetRights(subsetRightRight.Right).Count(x => x.Left != x.Right) == 0)
                                        predictions.Add(new ConflictPrediction(subsetRightRight, subsetMoveTran));

                                    model.Add(subsetMoveTran);
                                    yield return new FABuildStep<T>(FABuildStage.ConflictResolution, FABuildType.Add, subsetMoveTran);
                                }
                            }

                            if (subsetConectCount == 0) isSubset = false;
                        }

                        #region 空链接

                        var edge0s = new List<FATransition<T>>();

                        FABuildStep<T> Edge0(T addivte)
                        {
                            if ((subsetConectCount == 0 || !IsDefaultMetadata(addivte) || IsDefaultMetadata(transition.Metadata)) && transition.Symbol.Type.HasFlag(FASymbolType.Report))
                            {
                                var edge0 = new FATransition<T>(
                                    newState, (ushort)(++max),
                                    transition.SourceLeft, transition.SourceRight,
                                    EmptyInput, new FASymbol(FASymbolType.Report, 0, 0), MergeMetadatas(ArrangeMetadata(addivte, --front), transition.Metadata));

                                model.Add(edge0);
                                edge0s.Add(edge0);
                                symbol = new FASymbol(symbol.Type &~ FASymbolType.Report, symbol.Value, symbol.K);
                                return new FABuildStep<T>(FABuildStage.ConflictResolution, FABuildType.Add, edge0);
                            }

                            return default;
                        }

                        FABuildStep<T> Edge1()
                        {
                            if (subsetConectCount == 0 && transition.Symbol.Type.HasFlag(FASymbolType.Report) && !IsDefaultMetadata(transition.Metadata))
                            {
                                var edge1Left = isSubset ? subsetNewState : newState;
                                var edge1 = new FATransition<T>(
                                    edge1Left, (ushort)(++max),
                                    transition.SourceLeft, transition.SourceRight,
                                    EmptyInput, new FASymbol(FASymbolType.Report, 0, 0), transition.Metadata);

                                model.Add(edge1);
                                symbol = new FASymbol(symbol.Type &~ FASymbolType.Report, symbol.Value, symbol.K);
                                return new FABuildStep<T>(FABuildStage.ConflictResolution, FABuildType.Add, edge1);
                            }

                            return default;
                        }

                        if (flags.HasFlag(FATableFlags.AmbiguityResolution))
                        {
                            var do0 = metadata.Equals(transition.Metadata);
                            for (var x = 0; x < addivtes.Count; x++)
                                if (!IsDefaultMetadata(addivtes[x]))
                                {
                                    var step0 = Edge0(addivtes[x]);
                                    if (!step0.Equals(default(FABuildStep<T>)))
                                        yield return step0;

                                    do0 = true;
                                }

                            if (!do0)
                            {
                                var step1 = Edge1();
                                if (!step1.Equals(default(FABuildStep<T>)))
                                    yield return step1;
                            }
                        }

                        #endregion

                        // 使用逆行移进消除预测，测试功能，如发生严重异常可以移除
                        if (predictions.Count > 0)
                        {
                            // 查找有效用的预测
                            for (var i = 0; i < rights.Count; i++)
                            {
                                var right = rights[i];
                                var shifts = predictions.Where(x => x.Origin.Input.Equals(right.Input)).ToArray();
                                if (shifts.Length > 0)
                                {
                                    for (var x = 0; x < shifts.Length; x++)
                                    {
                                        var shift = shifts[x].Shift;
                                        model.Remove(shift);
                                        yield return new FABuildStep<T>(FABuildStage.ConflictResolution, FABuildType.Delete, shift);

                                        var predictionTran = shifts[x].Origin;
                                        var dst = new FATransition<T>(
                                                newState, newState,
                                                predictionTran.SourceLeft, predictionTran.SourceRight,
                                                predictionTran.Input, predictionTran.Symbol, predictionTran.Metadata);

                                        model.Add(dst);
                                        yield return new FABuildStep<T>(FABuildStage.ConflictResolution, FABuildType.Add, dst);

                                        predictions.Remove(shifts[x]);
                                    }

                                    rights.RemoveAt(i);
                                    i--;
                                }
                            }
                        }

                        // 处理重移进
                        IEnumerable<FABuildStep<T>> ReconnectRights(ushort left, IList<FATransition<T>> rights, bool resetMetadata)
                        {
                            // 处理后续
                            for (var i = 0; i < rights.Count; i++)
                            {
                                var right = rights[i];
                                var meta = IsDefaultMetadata(metadata) && resetMetadata ? MergeMetadatas(ArrangeMetadata(transition.Metadata, --front), right.Metadata) : right.Metadata;

                                // 如果存在记忆则修改终点
                                var target = memory.ContainsKey(fromHash) ?
                                    memory[fromHash].Right : right.Right;

                                var moveRight = new FATransition<T>(
                                    left, target,
                                    right.SourceLeft, right.SourceRight,
                                    right.Input, right.Symbol, meta);

                                model.Add(moveRight);
                                derives[moveRight] = right;
                                yield return new FABuildStep<T>(FABuildStage.ConflictResolution, FABuildType.Add, moveRight);
                            }
                        }

                        if (edge0s.Count > 0)
                        {
                            for (var x = 0; x < edge0s.Count; x++)
                                foreach (var step in ReconnectRights(edge0s[x].Right, rights, false))
                                    yield return step;
                        }
                        else
                        {
                            foreach (var step in ReconnectRights(isSubset ? subsetNewState : newState, rights.ToArray(), true))
                                yield return step;
                        }
                    }

                    predictions.Clear();
                }

                // 查询完全冲突
                if (!conflictDone && metadataTrans.Length > 1)
                    ThrowConflictException(model, new ConflictException<T>(conflicts));

                // 还原合并集
                var merged = new FATransition<T>(
                    intersection.Left,
                    newState,
                    intersection.SourceLeft,
                    intersection.SourceRight,
                    commonInput,
                    symbol,
                    metadata);

                model.Add(merged);
                yield return new FABuildStep<T>(FABuildStage.ConflictResolution, FABuildType.Add, merged);

                // 对合并进行记录
                if (!memory.ContainsKey(fromHash)) memory.Add(fromHash, merged);

                // 得到下次任务
                conflicts = GetConflicts(model);

                // 检测超时
                //if (mTimeout > 0)
                //    if (DateTime.Now - startTime > TimeSpan.FromMilliseconds(mTimeout))
                //        throw new TimeoutException<T>(conflicts);
            }

            StateCount = max + 1;
        }

        /// <summary>
        /// 当元数据发生冲突时
        /// </summary>
        protected virtual bool MetadataConflictResolution(IShiftRightMemoryModel model, MetadataGroup<T>[] groups, ref int max, out T result)
        {
            result = mDefaultMetadata;

            // 如果冲突其一为默认元数据且元数据来源于请求边
            if (groups.Length == 2)
            {
                var same = true;
                var defaultGroup = groups.FirstOrDefault(x => IsDefaultMetadata(x.Metadata) && x.Transitions.All(x => x.Symbol.Type.HasFlag(FASymbolType.Request)));
                if (defaultGroup != null)
                {
                    var currentGroup = defaultGroup == groups[0] ? groups[1] : groups[0];
                    // 找到这条边的实际使用的源数据
                    for (var i = 0; i < currentGroup.Transitions.Count; i++)
                    {
                        var transition = currentGroup.Transitions[i];
                        var subsetRights = model.GetRights(transition.Symbol.Value).Where(x => x.Input == transition.Input).ToArray();

                        for (var x = 0; x < subsetRights.Length; x++)
                        {
                            if (!transition.Metadata.Equals(subsetRights[x].Metadata))
                            {
                                same = false;
                                break;
                            }
                        }
                    }

                    if (same) result = currentGroup.Metadata;
                }
            }

            return true;
        }

        /// <summary>
        /// 当符号使用位发生冲突时
        /// </summary>
        protected virtual bool SymbolConflictResolution(IShiftRightMemoryModel model, SymbolGroup<T>[] groups, ref int max, out FASymbol result)
        {
            result = default;

            // 如果请求位置完全一致则合并type
            if (groups.Select(x => x.Symbol.Value).Distinct().Count() == 1)
            {
                FASymbolType stype = FASymbolType.None;
                foreach (var type in groups.Select(x => x.Symbol.Type))
                    stype = stype | type;

                result = new FASymbol(stype, groups[0].Symbol.Value, 0);
            }
            else
            {
                // 如果冲突其一为默认元数据且元数据来源于请求边
                if (groups.Length == 2)
                {
                    var same = true;
                    var defaultGroup = groups.FirstOrDefault(x => x.Symbol.Equals(default(FASymbol)) && x.Transitions.All(x => x.Symbol.Type.HasFlag(FASymbolType.Request)));
                    if (defaultGroup != null)
                    {
                        var currentGroup = defaultGroup == groups[0] ? groups[1] : groups[0];
                        // 找到这条边的实际使用的源数据
                        for (var i = 0; i < currentGroup.Transitions.Count; i++)
                        {
                            var transition = currentGroup.Transitions[i];
                            var subsetRights = model.GetRights(transition.Symbol.Value).Where(x => x.Input == transition.Input).ToArray();

                            for (var x = 0; x < subsetRights.Length; x++)
                            {
                                if (!transition.Symbol.Equals(subsetRights[x].Symbol))
                                {
                                    same = false;
                                    break;
                                }
                            }
                        }

                        if (same) result = currentGroup.Symbol;
                    }
                }
            }
            return true;
        }

    }
}
