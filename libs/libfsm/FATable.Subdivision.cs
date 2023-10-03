using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace libfsm
{
    partial class FATable<T>
    {
        /// <summary>
        /// 子图细分
        /// </summary>
        protected IEnumerable<FABuildStep<T>> Subdivision(IList<FATransition<T>> transitions, FATableFlags flags)
        {
            return Subdivision(new ShiftMemoryModel(transitions), flags);
        }

        /// <summary>
        /// 子图细分
        /// </summary>
        protected virtual IEnumerable<FABuildStep<T>> Subdivision(IShiftMemoryModel model, FATableFlags flags)
        {
            var subsets = model.Transitions.Where(x => x.Symbol.Type.HasFlag(FASymbolType.Request) && x.Symbol.Value != 0).Select(x => x.Symbol.Value).Where(x => model.GetLefts(x).Count > 0).ToHashSet();
            var offsets = new List<(FATransition<T> Transition, ushort Subset)>();

            var entryVisitor = new bool[StateCount + 1];

            // 搜索以 state 为起点的所有节点，包括 state 节点本身
            void BFSFindSubset(ushort state)
            {
                // 如果已经访问过，直接返回
                if (entryVisitor[state])
                    return;

                // 创建队列，用于存放待访问节点
                var entryQueue = new Queue<ushort>();
                entryQueue.Enqueue(state);

                // 开始查找子图
                while (entryQueue.Count > 0)
                {
                    // 取出队列的头部节点，作为当前节点访问
                    var point = entryQueue.Dequeue();

                    // 如果已经访问过，跳过此次循环
                    if (entryVisitor[point])
                        continue;

                    // 标记为已访问
                    entryVisitor[point] = true;

                    // 获取该边集，并遍历其中的所有边
                    var rights = model.GetRights(point);
                    for (var i = 0; i < rights.Count; i++)
                    {
                        var tran = rights[i];

                        // 如果该边指向的节点有子集节点，则递归访问其子集节点
                        if (tran.Symbol.Type.HasFlag(FASymbolType.Request) && tran.Symbol.Value != 0)
                            BFSFindSubset(tran.Symbol.Value);

                        if (subsets.Contains(tran.Right))
                        {
                            // 如果包含终结点则返回结果
                            offsets.Add((tran, tran.Right));
                        }
                        else
                        {
                            // 将该边指向的节点加入待访问队列
                            entryQueue.Enqueue(tran.Right);
                        }
                    }
                }
            }

            // 从状态1开始查找入口点
            BFSFindSubset(1);

            if (flags.HasFlag(FATableFlags.KeepSubset))
                for (var i = 0; i < mSubsets.Count; i++)
                    BFSFindSubset(GetSubset(mSubsets[i]));

            foreach (var offset in offsets)
            {
                var tran = offset.Transition;

                var first = new FATransition<T>(
                    tran.Left,
                    (ushort)(StateCount++),
                    tran.SourceLeft,
                    tran.SourceRight,
                    tran.Input,
                    tran.Symbol,
                    tran.Metadata);

                var starts = model.GetRights(offset.Subset);
                foreach (var start in starts)
                {
                    Debug.Assert(start.Symbol.Value == 0 || start.Symbol.Value == offset.Subset || start.Left == start.Right, "上级函数应该已经将头处理成不需要请求的连接。");

                    var subset = offset.Subset;
                    var follow = new FATransition<T>(
                        first.Right,
                        (ushort)(StateCount++),
                        start.SourceLeft,
                        start.SourceRight,
                        start.Input,
                        new FASymbol(FASymbolType.Request | FASymbolType.Report | start.Symbol.Type, subset, start.Symbol.K),
                        mDefaultMetadata);

                    model.Add(follow);
                    yield return new FABuildStep<T>(FABuildStage.SubsetSubdivision, FABuildType.Add, follow);
                }

                model.Add(first);
                yield return new FABuildStep<T>(FABuildStage.SubsetSubdivision, FABuildType.Add, first);

                model.Remove(tran);
                yield return new FABuildStep<T>(FABuildStage.SubsetSubdivision, FABuildType.Delete, tran);

                //mSubsets.Remove(tran.Left);
                //mSubsets.Add(first.Left);
            }
        }
    }
}
