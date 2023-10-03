using System.Collections.Generic;
using System.Linq;

namespace libfsm
{
    partial class FATable<T>
    {
        /// <summary>
        /// 谓词预测
        /// 主要为解决循环退出问题
        /// </summary>
        protected IEnumerable<FABuildStep<T>> Predicate(IList<FATransition<T>> transitions, IEnumerable<ushort> checkPoints)
        { 
            return Predicate(new ShiftRightMemoryModel(transitions), checkPoints);
        }

        /// <summary>
        /// 谓词预测
        /// 主要为解决循环退出问题
        /// </summary>
        protected virtual IEnumerable<FABuildStep<T>> Predicate(IShiftRightMemoryModel model, IEnumerable<ushort> checkPoints)
        {
            return new FABuildStep<T>[0];

            // 首先获得所有循环
            var loops = GetLoops(model, StateCount, checkPoints);
            var groups = loops.GroupBy(x => x[^1].Right);
            foreach (var group in groups)
            {
                var follow = model.GetRights(group.Key).SelectMany(x => FindSubsetRights(model, x.Left, StateCount)).Distinct().Where(x => !group.Any(y => y.Contains(x))).ToArray();

                foreach (var loop in group)
                {
                    for (var i = 0; i < follow.Length; i++) 
                    {
                        var f = follow[i];
                       

                        for (var x = 0; x < loop.Count; x++) 
                        {
                            var c = loop[x].Input.GetCommonDivisor(f.Input);
                            if (c.IsVaild())
                            {
                                var r = loop[x].Input.Eliminate(c);
                                if (!r.IsVaild())
                                {

                                }
                                else 
                                {

                                }
                            }

                           
                        }
                    }
                }
            }

            return new FABuildStep<T>[0];
        }

    }
}
