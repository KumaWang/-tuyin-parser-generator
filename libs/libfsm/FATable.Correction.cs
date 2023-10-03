using System;
using System.Collections.Generic;
using System.Linq;

namespace libfsm
{
    partial class FATable<T>
    {
        /// <summary>
        /// 修正可能存在的异常
        /// </summary>
        protected IEnumerable<FABuildStep<T>> Correction(IList<FATransition<T>> transitions, FATableFlags flags) 
        {
            return Correction(new ShiftRightMemoryModel(transitions), flags);
        }

        /// <summary>
        /// 修正可能存在的异常
        /// </summary>
        protected virtual IEnumerable<FABuildStep<T>> Correction(IShiftRightMemoryModel model, FATableFlags flags)
        {
            var empties = model.Transitions.Where(x => IsEmptyTransition(x)).ToArray();

            //  p? p* -> p+ 因为p?的空链接后续依然是p，参考list3图
            for (var i = 0; i < empties.Length; i++)
            {
                var empty = empties[i];
                var emptyHeader = model.GetRights(empty.Left).Where(x => !x.Equals(empty)).Select(x => x.Input).Distinct().ToArray();
                if (emptyHeader.Length > 0)
                {
                    var emptyRights = model.GetRights(empty.Right).Select(x => x.Input).Distinct().ToArray();
                    if (emptyRights.Length > 0)
                    {
                        var headerHash = HashCode.Combine(emptyHeader[0]);
                        var rightHash = HashCode.Combine(emptyRights[0]);

                        for (var x = 1; x < emptyHeader.Length; x++)
                            headerHash = HashCode.Combine(emptyHeader[x]);

                        for (var x = 1; x < emptyRights.Length; x++)
                            rightHash = HashCode.Combine(emptyRights[x]);

                        // 如果右侧input完全包含头
                        if (headerHash == rightHash && empty.Left != empty.Right)
                        {
                            model.Remove(empty);
                            yield return new FABuildStep<T>(FABuildStage.Correction, FABuildType.Delete, empty);

                            var dst = new FATransition<T>(
                                empty.Left, (ushort)StateCount++,
                                empty.SourceLeft, empty.SourceRight,
                                empty.Input, empty.Symbol, empty.Metadata);

                            model.Add(dst);
                            yield return new FABuildStep<T>(FABuildStage.Correction, FABuildType.Add, dst);
                        }
                    }
                }
            }
        }
    }
}
