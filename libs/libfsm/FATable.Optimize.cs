using System;
using System.Collections.Generic;
using System.Linq;

namespace libfsm
{
    partial class FATable<T>
    {
        /// <summary>
        /// 优化表
        /// </summary>
        public void Optimize() 
        {
            Optimize(new ShiftMemoryModel(Transitions), mFlags).ToArray();
        }

        /// <summary>
        /// 优化表
        /// </summary>
        private IEnumerable<FABuildStep<T>> Optimize(IShiftMemoryModel model, FATableFlags flags)
        {
            return LayoutTransitions(model.Transitions, GetInternalEntryPoints(flags));
        }

        /// <summary>
        /// 为Report节点添加退出移进
        /// </summary>
        protected virtual IEnumerable<FABuildStep<T>> ReportTransitions(IShiftMemoryModel model, FATableFlags flags)
        {
            var loops = GetLoops(model, StateCount, GetInternalEntryPoints(flags)).Select(x => x.Right).ToHashSet();

            var vailds = GetVaildStates(model, flags);
            var stateCount = StateCount;
            // 为report添加miss退出
            var visitor = new bool[StateCount];
            var reports = model.Transitions.Where(x =>
                x.Symbol.Type.HasFlag(FASymbolType.Report) &&
                vailds.Contains(x.Left) &&
                model.GetRights(x.Right).Count > 0).ToArray();

            foreach (var right in model.GetLefts(0))
                visitor[right.Left] = true;

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
                    var dst = new FATransition<T>(
                        report.Right, dstRight,
                        report.SourceRight, dstRight,
                        EmptyInput, new FASymbol(FASymbolType.Report, 0, 0), mDefaultMetadata);

                    model.Add(dst);
                    yield return new FABuildStep<T>(FABuildStage.Optimize, FABuildType.Add, dst);
                }
            }

            StateCount = stateCount;
        }

        /// <summary>
        /// 简化循环
        /// </summary>
        protected virtual IEnumerable<FABuildStep<T>> SimplifyLoop(IShiftRightMemoryModel model, FATableFlags flags)
        {
            var loops = GetLoops(model, StateCount, GetInternalEntryPoints(flags));
            var sames = new bool[loops.Count];

            int GetCompreHash(TransitionLoop loop, FATransition<T> tran)
            {
                var hash = HashCode.Combine(tran.Input, tran.Symbol, tran.Metadata);
                var rights = model.GetRights(tran.Right).Where(x => !loop.Contains(x));
                foreach (var right in rights)
                    hash = HashCode.Combine(hash, right.Right, right.Input, right.Symbol, right.Metadata);

                return hash;
            }

            for (var i = 0; i < loops.Count; i++)
            {
                var loop = loops[i];
                if (loop.Count > 1)
                {
                    // 判断循环是否每一步都一致
                    var isSame = true;
                    var first = GetCompreHash(loop, loop[0]);
                    for (var x = 1; x < loop.Count; x++)
                    {
                        if (first != GetCompreHash(loop, loop[x]))
                        {
                            isSame = false;
                            break;
                        }
                    }

                    sames[i] = isSame;
                }
            }

            for (var i = 0; i < loops.Count; i++)
            {
                var loop = loops[i];
                if (sames[i])
                {
                    var tran0 = loop[0];
                    var dst = new FATransition<T>(tran0.Left, tran0.Left, tran0.SourceLeft, tran0.SourceRight, tran0.Input, tran0.Symbol, tran0.Metadata);
                    model.Add(dst);
                    yield return new FABuildStep<T>(FABuildStage.Optimize, FABuildType.Add, dst);

                    for (var x = 0; x < loop.Count; x++)
                    {
                        var subLoop = loop[x];
                        model.Remove(subLoop);
                        yield return new FABuildStep<T>(FABuildStage.Optimize, FABuildType.Delete, subLoop);
                    }
                }
            }
        }

        /// <summary>
        /// 清理无效路径
        /// </summary>
        protected virtual IEnumerable<FABuildStep<T>> CleanupInvalidPaths(IShiftRightMemoryModel model, IEnumerable<ushort> entryPoints)
        {
            var visitor = new bool[StateCount + 1];
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

            for (var i = 0; i < model.Transitions.Count; i++)
            {
                var tran = model.Transitions[i];
                if (!visitor[tran.Left])
                {
                    model.Remove(tran);
                    yield return new FABuildStep<T>(FABuildStage.Optimize, FABuildType.Delete, tran);
                    i--;
                }
            }
        }

        /// <summary>
        /// 内存布局优化
        /// </summary>
        protected virtual IEnumerable<FABuildStep<T>> LayoutTransitions(IList<FATransition<T>> transitions, IEnumerable<ushort> entryPoints)
        {
            var group = transitions.GroupBy(x => x.Left).ToDictionary(x => x.Key, x => x.ToList());
            bool[] visitor = new bool[StateCount + 1];
            var index = new Dictionary<ushort, ushort>();
            var stack = new Stack<ushort>(entryPoints);

            visitor[0] = true;
            index[0] = 0;

            while (stack.Count > 0)
            {
                var state = stack.Pop();
                if (visitor[state])
                    continue;

                visitor[state] = true;
                index[state] = (ushort)index.Count;
                if (group.ContainsKey(state))
                {
                    foreach (var next in group[state])
                    {
                        if (next.Symbol.Value != 0)
                            stack.Push(GetSubset(next.Symbol.Value));

                        stack.Push(next.Right);
                    }
                }
            }

            for (var i = 0; i < transitions.Count; i++)
            {
                var item = transitions[i];
                if (visitor[item.Left])
                {
                    var symbol = item.Symbol;
                    symbol = new FASymbol(
                        symbol.Type,
                        index[GetSubset(item.Symbol.Value)],
                        symbol.K);

                    transitions[i] = new FATransition<T>(
                        index[item.Left],
                        index[item.Right],
                        item.SourceLeft,
                        item.SourceRight,
                        item.Input,
                        symbol,
                        item.Metadata);
                }
                else
                {
                    yield return new FABuildStep<T>(FABuildStage.Optimize, FABuildType.Delete, transitions[i]);
                    transitions.RemoveAt(i);
                    i--;
                }
            }

            for (var i = 0; i < mSubsets.Count; i++)
            {
                var subset = GetSubset(mSubsets[i]);
                if (visitor[subset])
                {
                    mSubsets[i] = index[subset];
                }
                else
                {
                    mSubsets.RemoveAt(i);
                    i--;
                }
            }

            mHasLayoutTransitions = true;
            StateCount = index.Count;
        }
    }
}
