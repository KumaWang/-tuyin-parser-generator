using System;
using System.Collections.Generic;
using System.Linq;

namespace libfsm
{
    partial class FATable<T>
    {
        protected virtual FATransition<T> GetSingleTransition(IShiftMemoryModel model, HashSet<FATransition<T>> visitor) 
        {
            return model.Transitions.FirstOrDefault(x => !visitor.Contains(x) && model.GetLefts(x.Left).Count == 0 && model.GetRights(x.Right).Count == 0);
        }

        /// <summary>
        /// 内联
        /// </summary>
        protected virtual IEnumerable<FABuildStep<T>> Inline(IShiftMemoryModel model, int maxInlineDeep, FATableFlags flags)
        {
            return MoveSingleTransition(model);
            //return Array.Empty<FABuildStep<T>>();
            //return MoveSingleAny(model);
            /*
            var newState = StateCount;
            ushort GetNewState(Dictionary<ushort, ushort> transform, ushort state) 
            {
                if (!transform.ContainsKey(state))
                    transform[state] = (ushort)(++newState);

                return transform[state];
            }

            // 查找所有请求点
            var requests = model.Transitions.Where(x => x.Symbol.Type.HasFlag(FASymbolType.Request) && x.Symbol.Value != 0).ToArray();

            // 统计所有入口点深度
            var deeps = requests.Select(x => x.Symbol.Value).Distinct().Select(x => new { State = x, Deep = GetDeep(model, x, StateCount) }).ToDictionary(x => x.State, x => x.Deep);

            // 对请求点进行转移
            for (var i = 0; i < requests.Length; i++) 
            {
                var request = requests[i];
                var deep = deeps[request.Symbol.Value];

                // 如果小于最大内联深度则进行内联
                if (deep <= maxInlineDeep) 
                {
                    var transform = new Dictionary<ushort, ushort>();
                    var steps = new List<FABuildStep<T>>();
                    var subetTran = model.GetRights(request.Symbol.Value).First(x => x.Input.Equals(request.Input));

                    WalkToEnds(model, request.Symbol.Value, StateCount, tran =>
                    {
                        var left = tran.Left == request.Symbol.Value ? request.Left : GetNewState(transform, tran.Left);
                        var right = model.GetRights(tran.Right).Count > 0 ? GetNewState(transform, tran.Right) : request.Right;

                        var meta = tran.Metadata;
                        if (right == request.Right)
                            meta = MergeMetadatas(0, meta, request.Metadata);

                        // 解决循环结构移进
                        var trantran = new FATransition<T>(left, right, tran.SourceLeft, tran.SourceRight, tran.Input, tran.Symbol, meta);
                        model.Add(trantran);
                        steps.Add(new FABuildStep<T>(FABuildStage.Inline, FABuildType.Add, trantran));
                    });

                    for (var x = 0; x < steps.Count; x++)
                        yield return steps[x];

                    StateCount = newState;

                    model.Remove(request);
                    yield return new FABuildStep<T>(FABuildStage.Inline, FABuildType.Delete, request);
                }
            }

#if DEBUG
            var anys = model.Transitions.Where(x => model.GetRights(x.Right).Count == 0 && x.Input.Chars.Length == 1 && IsAnyTransition(x)).OrderByDescending(x => x.Left).ToArray();
            if (anys.Length > 0)
                throw new NotImplementedException();
#endif
            */
        }

        private IEnumerable<FABuildStep<T>> MoveSingleTransition(IShiftMemoryModel model) 
        {
            var visitor = new HashSet<FATransition<T>>();

            var signleTransition = GetSingleTransition(model, visitor);
            while (signleTransition.Left != 0) 
            {
                // 查找所有请求点
                var requests = model.Transitions.Where(
                    x => x.Symbol.Type.HasFlag(FASymbolType.Request) && GetSubset(x.Symbol.Value) == signleTransition.Left && x.Input.Equals(signleTransition.Input)).ToArray();

                for (var x = 0; x < requests.Length; x++)
                {
                    var request = requests[x];
                    var hasReport = request.Symbol.Type.HasFlag(FASymbolType.Report);

                    var symbol = signleTransition.Symbol;
                    if (!hasReport)
                        symbol = new FASymbol(symbol.Type & ~FASymbolType.Report, symbol.Value, symbol.K);

                    // 移进
                    var dst = new FATransition<T>(
                        request.Left, request.Right, request.SourceLeft, request.SourceRight,
                        signleTransition.Input, symbol, MergeMetadatas(0, signleTransition.Metadata, request.Metadata));

                    model.Add(dst);
                    yield return new FABuildStep<T>(FABuildStage.Inline, FABuildType.Add, dst);

                    // 移除请求
                    model.Remove(request);
                    yield return new FABuildStep<T>(FABuildStage.Inline, FABuildType.Delete, request);

                    // 移除单边
                    model.Remove(signleTransition);
                    yield return new FABuildStep<T>(FABuildStage.Inline, FABuildType.Delete, signleTransition);
                }

                visitor.Add(signleTransition);
                signleTransition = GetSingleTransition(model, visitor);
            }
        }
    }
}
