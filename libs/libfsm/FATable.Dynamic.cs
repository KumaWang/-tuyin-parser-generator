using System.Diagnostics;
using System.Linq;

namespace libfsm
{
    partial class FATable<T>
    {
        public void Insert(FATransition<T> tran) 
        {
            Transitions.Add(tran);
            /*
            Transitions.Add(tran);
            var mergeResult = Minimize(Transitions, new ushort[] { tran.Right });
            var confictResult = ConflictResolution(Transitions, ConflictsDetectionFlags.Metadata | ConflictsDetectionFlags.Symbol);
            mBuildSteps.Add(new FABuildStep<T>(FABuildStage.Dynamic, FABuildType.Add, tran));
            mBuildSteps.AddRange(mergeResult);
            mBuildSteps.AddRange(confictResult);
            */
        }

        public void Remove(FATransition<T> tran) 
        {
            Transitions.Remove(tran);
            /*
            if (Transitions.Remove(tran))
            {
                var mergeResult = Minimize(Transitions, new ushort[] { tran.Right });
                var confictResult = ConflictResolution(Transitions, ConflictsDetectionFlags.Metadata | ConflictsDetectionFlags.Symbol);
                mBuildSteps.Add(new FABuildStep<T>(FABuildStage.Dynamic, FABuildType.Delete, tran));
                mBuildSteps.AddRange(mergeResult);
                mBuildSteps.AddRange(confictResult);
            }
            */
        }

        public void InsertDelay(FATransition<T> tran) 
        {
            Transitions.Add(tran);
        }

        public void RemoveDelay(FATransition<T> tran) 
        {
            Transitions.Remove(tran);
        }

        public void Update() 
        {
            StateCount = Transitions.Count > 0 ? Transitions.Max(x => x.Right) + 1 : 1;
          
            var model = new ShiftMemoryModel(Transitions);

            // 清理一次无效路径
            mBuildSteps.AddRange(CleanupInvalidPaths(model, new ushort[] { 1 }));

            // 谓词
            //if (mFlags.HasFlag(FATableFlags.Predicate))
            //    Predicate(model, GetEntryPoints());

            // 解决连接冲突
            if (mFlags.HasFlag(FATableFlags.EdgeConflicts))
            {
                mBuildSteps.AddRange(ConflictResolution(model, mFlags));

                // 清理一次无效路径
                mBuildSteps.AddRange(CleanupInvalidPaths(model, new ushort[] { 1 }));

#if DEBUG
                Debug.Assert(Transitions.GroupBy(x => new { C = x.Left, D = x.Right, E = x.Input }).Where(x => x.Count() > 1).Count() == 0, "存在未解决的冲突。");
#endif
            }

            // 是否拆分subset为完全独立图
            if (mFlags.HasFlag(FATableFlags.Subdivision))
                mBuildSteps.AddRange(Subdivision(model, mFlags));

            // 校正
            mBuildSteps.AddRange(Correction(model, mFlags));

            // 清理一次无效路径
            mBuildSteps.AddRange(CleanupInvalidPaths(model, new ushort[] { 1 }));

            // 内联
            mBuildSteps.AddRange(Inline(model, mMaxInlineDeep, mFlags));

            // 简化循环
            mBuildSteps.AddRange(SimplifyLoop(model, mFlags));

            // 添加report退出
            mBuildSteps.AddRange(ReportTransitions(model, mFlags));

            // 合并等效部分
            if (mFlags.HasFlag(FATableFlags.Minimize))
                mBuildSteps.AddRange(Minimize(model, mFlags, GetExitPoints()));

            // 优化和清理
            mBuildSteps.AddRange(Optimize(model, mFlags));
        }
    }
}
