using libgraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace libfsm
{
    partial class FATable<T>
    {
        class MinimizeResult
        {
            public IList<MinimizeGroup> Mergeables { get; }

            public MinimizeResult(IList<MinimizeGroup> mergeables)
            {
                Mergeables = mergeables;
            }
        }

        struct MinimizeGroup
        {
            private IList<MinimizeGroupItem> mItems;

            public MinimizeGroup(bool merge, IList<MinimizeGroupItem> items)
            {
                Merged = merge;
                mItems = items;
            }

            public bool Merged { get; }

            public MinimizeGroupItem Target => mItems[0];

            public IList<MinimizeGroupItem> Items => mItems;
        }

        struct MinimizeGroupItem
        {
            public MinimizeGroupItem(int hashCode, MinimizeCompareKey compreKey)
            {
                HashCode = hashCode;
                CompareKey = compreKey;
            }

            public int HashCode { get; }

            public MinimizeCompareKey CompareKey { get; }

            public ushort Left => CompareKey.Transition.Left;

            public ushort Right => CompareKey.Transition.Right;
        }

        struct MinimizeCompareKey
        {
            public int Level { get; }

            public int CompredCount { get; }

            public FATransition<T> Transition { get; }

            public MinimizeCompareKey(int level, int compredCount, FATransition<T> transition)
            {
                Level = level;
                CompredCount = compredCount;
                Transition = transition;
            }

            public override string ToString()
            {
                return Transition.ToString();
            }

            public override bool Equals(object obj)
            {
                return obj is MinimizeCompareKey key && Transition.Equals(key.Transition);
            }

            public override int GetHashCode()
            {
                return Transition.GetHashCode();
            }
        }

        class MinimizeMemoryModel : IShiftMemoryModel
        {
            private IShiftMemoryModel mOrigin;
            private HashSet<ushort> mTempPoints;
            private IDictionary<ushort, List<FATransition<T>>> mTempTrans;

            public MinimizeMemoryModel(IShiftMemoryModel origin)
            {
                mOrigin = origin;
                mTempPoints = new HashSet<ushort>();
                mTempTrans = new Dictionary<ushort, List<FATransition<T>>>();
            }

            public List<FATransition<T>> this[ushort key] => mOrigin[key];

            public IList<FATransition<T>> Transitions => mOrigin.Transitions;

            public IEnumerable<ushort> Keys => mOrigin.Keys;

            public IEnumerable<List<FATransition<T>>> Values => mOrigin.Values;

            public Dictionary<ushort, List<FATransition<T>>> Rights => mOrigin.Rights;

            public int Count => mOrigin.Count;

            public void AddTemp(FATransition<T> transition)
            {
                if (mTempTrans != null && !mTempTrans.ContainsKey(transition.Right))
                    mTempTrans[transition.Right] = new List<FATransition<T>>();

                mTempTrans[transition.Right].Add(transition);
                mTempPoints.Add(transition.Right);
            }

            public void Add(FATransition<T> transition)
            {
                mOrigin.Add(transition);
            }

            public bool Contains(FATransition<T> transition)
            {
                return mOrigin.Contains(transition);
            }

            public bool ContainsKey(ushort key)
            {
                return mOrigin.ContainsKey(key);
            }

            public IEnumerator<KeyValuePair<ushort, List<FATransition<T>>>> GetEnumerator()
            {
                return mOrigin.GetEnumerator();
            }

            public IList<FATransition<T>> GetLefts(ushort state)
            {
                return mOrigin.GetLefts(state);
            }

            public IList<FATransition<T>> GetTempLefts(ushort state)
            {
                if (mTempPoints.Contains(state))
                    return mTempTrans[state];

                return mOrigin.GetLefts(state);
            }

            public IList<FATransition<T>> GetRights(ushort state)
            {
                return mOrigin.GetRights(state);
            }

            public void Insert(int index, FATransition<T> transition)
            {
                mOrigin.Insert(index, transition);
            }

            public void Remove(FATransition<T> transition)
            {
                mOrigin.Remove(transition);
            }

            public bool TryGetValue(ushort key, out List<FATransition<T>> value)
            {
                return mOrigin.TryGetValue(key, out value);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        /// <summary>
        /// 最小化
        /// </summary>
        protected IEnumerable<FABuildStep<T>> Minimize(IList<FATransition<T>> transitions, FATableFlags flags, IEnumerable<ushort> checkPoints)
        {
            return Minimize(new ShiftMemoryModel(transitions), flags, checkPoints);
        }

        /// <summary>
        /// 最小化
        /// </summary>
        protected virtual IEnumerable<FABuildStep<T>> Minimize(IShiftMemoryModel model, FATableFlags flags, IEnumerable<ushort> checkPoints)
        {
            var minimizeModel = new MinimizeMemoryModel(model);
            var specialPoints = new bool[StateCount + 1];
            foreach (var specialPoint in model.Rights.Where(x => x.Value.Count > 1).Select(x => x.Key))
                specialPoints[specialPoint] = true;

            var stack = new Stack<ushort>(ComputeMinimizeCheckPoints(minimizeModel, checkPoints));
            var visitor = new bool[StateCount + 1];
            while (stack.Count > 0)
            {
                var point = stack.Pop();
                if (visitor[point])
                    continue;

                visitor[point] = true;

                // 去重
                var nexts = new List<MinimizeCompareKey>(minimizeModel.GetTempLefts(point).Select(x => new MinimizeCompareKey(1, 0, x)).Distinct());

            RESTART:
                var mergeResult = GetMinimizeGroups(minimizeModel, nexts, specialPoints);
                // 未找到结果那么继续向左搜索
                if (mergeResult.Mergeables.Count == 0)
                {
                    foreach (var nextPoint in nexts.Where(x => x.CompredCount == 0).
                        Select(x => x.Transition.Left).Distinct()) 
                        stack.Push(nextPoint);

                    continue;
                }

                // 清空
                nexts.Clear();

                // 对可合并分组进行进行合并,首先处理合并的分支
                for (int i = 0; i < mergeResult.Mergeables.Count; i++)
                {
                    MinimizeGroup group = mergeResult.Mergeables[i];

                    // 合并成功插入该边左侧,否则保留该边
                    if (group.Merged)
                    {
                        // 检查是否完全同源
                        for (int z = 1; z < group.Items.Count; z++)
                        {
                            MinimizeGroupItem item = group.Items[z];

                            if (group.Target.Left != item.Left)
                                mSubsetReplaces[item.Left] = group.Target.Left;

                            var lefts = minimizeModel.GetLefts(item.CompareKey.Transition.Left).ToArray();
                            for (var x = 0; x < lefts.Length; x++)
                            {
                                FATransition<T> left = lefts[x];
                                var appendTran = new FATransition<T>(
                                    left.Left,
                                    group.Target.Left,
                                    left.SourceLeft,
                                    left.SourceRight,
                                    left.Input,
                                    left.Symbol,
                                    left.Metadata);

                                if (!left.Equals(appendTran))
                                {
                                    minimizeModel.Remove(left);
                                    yield return new FABuildStep<T>(FABuildStage.Minimize, FABuildType.Delete, left);

                                    if (!minimizeModel.Contains(appendTran))
                                    {
                                        minimizeModel.Insert(0, appendTran);
                                        yield return new FABuildStep<T>(FABuildStage.Minimize, FABuildType.Add, appendTran);
                                    }
                                }
                            }
                        }
                    }

                    // 保留该次以便和后续继续匹配
                    nexts.Add(new MinimizeCompareKey(
                        group.Target.CompareKey.Level,
                        group.Target.CompareKey.CompredCount + 1,
                        group.Target.CompareKey.Transition));

                    // 插入新任务
                    if (group.Target.CompareKey.CompredCount == 0 && !visitor[group.Target.CompareKey.Transition.Left])
                    {
                        visitor[group.Target.CompareKey.Transition.Left] = true;
                        var lefts = minimizeModel.GetLefts(group.Target.CompareKey.Transition.Left);
                        for (int x = 0; x < lefts.Count; x++)
                            nexts.Add(new MinimizeCompareKey(group.Target.CompareKey.Level + 1, 0, lefts[x]));
                    }
                }

                // 继续匹配
                if(nexts.Where(x => x.CompredCount == 0).Count() > 0) goto RESTART;
            }

            // 合并左右相同移进
            foreach (var step in MergeSameLeftRightTransitions(model))
                yield return step;
        }

        /// <summary>
        /// 合并同源移进
        /// </summary>
        private IEnumerable<FABuildStep<T>> MergeSameLeftRightTransitions(IShiftMemoryModel model) 
        {
            var groups = model.Transitions.
                Where(x => x.Left != x.Right).
                GroupBy(x => new { Left = x.Left, Right = x.Right, Metadata = x.Metadata, Symbol = x.Symbol }).
                Select(x => x.ToArray());

            foreach (var group in groups) 
            {
                if (group.Length < 2)
                    continue;

                EdgeInput input = default;
                var first = group[0];
                for (var i = 0; i < group.Length; i++) 
                {
                    var tran = group[i];
                    model.Remove(tran);
                    yield return new FABuildStep<T>(FABuildStage.Minimize, FABuildType.Delete, tran);

                    if (input.IsVaild())
                        input = input.Combine(tran.Input);
                    else
                        input = tran.Input;
                }

                var dst = new FATransition<T>(
                    first.Left, first.Right,
                    first.SourceLeft, first.SourceRight,
                    input, first.Symbol, first.Metadata);

                model.Insert(0, dst);
                yield return new FABuildStep<T>(FABuildStage.Minimize, FABuildType.Add, dst);
            }
        }

        /// <summary>
        /// 获取可去重分组
        /// </summary>
        private MinimizeResult GetMinimizeGroups(IShiftMemoryModel model, IList<MinimizeCompareKey> keys, bool[] specialPoints)
        {
            // 创建算子
            var equalHashs = new int[keys.Count];

            // 找到至交汇点的算子
            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                var hashs = model.GetRights(key.Transition.Left).Select(x => x.Left == x.Right ?
                    HashCode.Combine(x.Input, x.Metadata, x.Symbol, 1) :
                    HashCode.Combine(x.Input, x.Metadata, x.Symbol)).OrderBy(x => x).ToList();

                // 注意这里不能与上面文法合并否则会影响hash生成顺序
                hashs.AddRange(model.GetRights(key.Transition.Right).Select(x => x.Left == x.Right ?
                    HashCode.Combine(x.Input, x.Metadata, x.Symbol, 1) :
                    HashCode.Combine(x.Input, x.Metadata, x.Symbol)).OrderBy(x => x).ToArray());

                var hash = 0;
                for (var x = 0; x < hashs.Count; x++)
                    hash = HashCode.Combine(hash, hashs[x]);

                equalHashs[i] = hash;
            }

            // 得到数据包含信息结构,抽出下级最大一致部分
            var groups = new List<MinimizeGroup>();

            // 一组数据完全相等于另一组数据时可以进行合并
            var mergeIndex = 0;
            var mergeGroups = equalHashs.
                Select(x => new MinimizeGroupItem(x, keys[mergeIndex++])).
                GroupBy(x => x.HashCode);

            // 移除上级匹配点,首先获得移除分支集合,并提供分组
            foreach (var group in mergeGroups)
            {
                // 排除所有前置分支,首先进行level排序
                var sortItems = group.OrderByDescending(x => x.CompareKey.Level + (specialPoints[x.CompareKey.Transition.Left] ? 1 : 0)).OrderBy(x => x.Right).ToList();
                if (sortItems.Count > 1)
                {
                    // 移除同源
                    groups.Add(new MinimizeGroup(sortItems.Count > 1, sortItems));
                }
            }

            // 处理keeps
            return new MinimizeResult(groups);
        }

        /// <summary>
        /// 计算获得最小化时真正使用的运算点
        /// </summary>
        private IEnumerable<ushort> ComputeMinimizeCheckPoints(MinimizeMemoryModel model, IEnumerable<ushort> checkPoints)
        {
            // 首先对每个运算进行向上检测直到查找到subset或者状态点1
            // 这样可以有效规避循环结构所带来的无法查找到入口点的问题
            var subsets = mSubsets.Select(GetSubset).ToHashSet();
            var points = checkPoints.ToArray();
            var froms = new ushort[points.Length];
            var visitor = new bool[StateCount + 1];
            for (int x = 0; x < points.Length; x++)
            {
                var checkPoint = points[x];
                var stack = new Stack<ushort>();
                stack.Push(checkPoint);
                while (stack.Count > 0)
                {
                    var point = stack.Pop();
                    if (visitor[point])
                        continue;

                    visitor[point] = true;

                    if (point == 1 || subsets.Contains(point))
                    {
                        froms[x] = point;
                        break;
                    }

                    var lefts = model.GetLefts(point);
                    if (lefts.Count == 0)
                    {
                        froms[x] = point;
                        break;
                    }

                    for (var i = 0; i < lefts.Count; i++)
                        stack.Push(lefts[i].Left);
                }

                Array.Clear(visitor, 0, visitor.Length);
            }

            // 到这一步已经获得了每个检查点对应的子图入口（包括细分后的入口)，现在可以对每个检查点进行分组
            var groups = points.Select((x, i) => (x, froms[i])).GroupBy(x => x.Item2).Select(x => x.Select(y => y.x));

            // 为分组内的每个检查点使用一个临时移进连接到一个共同终点
            foreach (var group in groups)
            {
                var endPoint = (ushort)StateCount++;
                yield return endPoint;

                foreach (var point in group)
                {
                    model.AddTemp(new FATransition<T>(point, endPoint, point, endPoint, default, default, mDefaultMetadata));
                }
            }
        }
    }
}
