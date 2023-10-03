using libflow.stmts;
using libfsm;
using librule.generater;
using librule.utils;
using Boolean = libflow.stmts.Boolean;
using Index = libflow.stmts.Index;

namespace librule.targets.code
{
    static class TokenScanner
    {
        internal static TokenScanResult Scan(TokenConstructor ctor, ProductionTokenTable productionTable)
        {
            int nameIndex = 0;

            var targetStates = new Dictionary<ushort, GraphState<TargetMetadatas>>();
            var targetEdges = new List<GraphEdge<TargetMetadatas>>();

            void CheckState(ushort state)
            {
                if (!targetStates.ContainsKey(state))
                    targetStates[state] = new GraphState<TargetMetadatas>(state, GraphStateFlags.None);
            }

            var table = productionTable.TokenTable;
            var rights = table.Transitions.GroupBy(x => x.Left).ToDictionary(x => x.Key, x => x.ToArray());
            foreach (var state in rights)
            {
                // 得到包含关系并合并范围input
                var min = ushort.MaxValue;
                var max = ushort.MinValue;
                var hash = 0;
                var dict = new Dictionary<ushort, ushort>();
                for (var i = 0; i < state.Value.Length; i++)
                    foreach (var c in state.Value[i].Input.GetChars(ushort.MaxValue))
                    {
                        dict[c] = (ushort)(i + 1);
                        if (min > c) min = c;
                        if (max < c) max = c;
                        hash = HashCode.Combine(hash, c, i + 1);
                    }

                // 确定类型
                var hashCtx = $"_input_{(hash < 0 ? "_" : "")}{Math.Abs(hash)}";
                var hashDiff = dict.Select(x => x.Value).Distinct().Count();
                var readCondition = GetReadConditional(state.Key, dict, hashCtx, hashDiff, ref min, ref max, out var defaultValue, out var isHash);

                CheckState(state.Key);
                if (isHash && !ctor.CodeTargetVisitor.ContainsTokenSwitch(hash))
                {
                    var arr = new int[max - min + 1];
                    if (defaultValue != 0)
                    {
                        for (var i = 0; i < arr.Length; i++)
                        {
                            var dictIndex = (ushort)(i + min);
                            if (dict.ContainsKey(dictIndex))
                                arr[i] = dict[dictIndex];
                            else
                                arr[i] = defaultValue;
                        }
                    }
                    else
                    {
                        foreach (var item in dict)
                            arr[item.Key - min] = item.Value;
                    }

                    // 条件表达式
                    ctor.CodeTargetVisitor.CreateTokenSwitch(hashCtx, hash, arr);
                }

                for (var i = 0; i < state.Value.Length; i++)
                {
                    var value = (ushort)(i + 1);
                    var tran = state.Value[i];

                    CheckState(tran.Right);
                    if (tran.Symbol.Type.HasFlag(FASymbolType.Request)) 
                        CheckState(tran.Symbol.Value);

                    var edge = new GraphEdge<TargetMetadatas>(targetStates[tran.Left], targetStates[tran.Right], tran.Symbol.Value != 0 ? targetStates[tran.Symbol.Value] : null, null, null);

                    var metas = new TargetMetadatas();
                    var actions = table.Graph.GetAction(tran.Metadata.Value);
                    var insertIndex = 0;
                    for (var x = 0; x < actions.Count; x++)
                    {
                        var action = actions[x];
                        if (action.Front == 0)
                            insertIndex = x;

                        metas.Add(new TargetMetadata(new TargetMetadataStep(action.Context, false, true)));
                    }

                    if (tran.Metadata.Token != 0) 
                    {
                        var insertToken = Concatenation.From(new Assign(
                                    new Variable(tran.Left, Settings.TOKEN_LITERAL),
                                    new Number(tran.Metadata.Token)),
                                new Colon());

                        metas.Insert(insertIndex, new TargetStatement(insertToken, true));
                    }

                    var obsCondition = readCondition;
                    if (hashDiff > 1)
                        obsCondition = new Logic(new Parenthese(obsCondition.Source), new Number(value), LogicType.Equal);

                    metas.Insert(insertIndex, new TargetStatement(ctor.CreateObstructiveBranch(edge, obsCondition), true));
                    edge.Metadata = metas;
                    targetEdges.Add(edge);
                }
            }

            var targetGraph = new TargetGraph();
            targetGraph.AddRange(targetEdges);
            return new TokenScanResult(targetGraph);
        }

        static IConditional GetReadConditional(ushort state, Dictionary<ushort, ushort> hash, string expr, int hashDiff, ref ushort min, ref ushort max, out ushort defaultValue, out bool isHash)
        {
            defaultValue = 0;
            isHash = false;
            if (hash.Count > ushort.MaxValue - 5)
            {
                // 找到缺失部分并设置为0
                var miss = new List<ushort>();
                for (ushort i = 0; i < ushort.MaxValue; i++)
                    if (!hash.ContainsKey(i))
                        miss.Add(i);

                // 找到最大同质化部分
                var group = hash.GroupBy(x => x.Value).ToDictionary(x => x.Key, x => x.ToArray());
                var maxGroup = group.MaxItem(x => x.Value.Length);
                if (maxGroup.Value.Length > miss.Count)
                {
                    defaultValue = maxGroup.Key;

                    foreach (var item in maxGroup.Value)
                        hash.Remove(item.Key);

                    for (var i = 0; i < miss.Count; i++)
                        hash[miss[i]] = 0;
                }

                min = ushort.MaxValue;
                max = ushort.MinValue;
                foreach (var c in hash.Select(x => x.Key))
                {
                    if (min > c) min = c;
                    if (max < c) max = c;
                }
            }

            var input = new Variable(state, Settings.INPUT_LITERAL);
            var input_cache = new Variable(state, Settings.INPUT_VARIABLE_LITERAL);
            IAstNode source = new Parenthese(new Assign(input_cache, new Index(input, new Postfix(new Literal($"{Settings.INDEX_LITERAL}"), PostfixType.Increment))));
            IConditional condition = null;
            if (hash.Count == 1)
            {
                condition = new Logic(source, new Number(hash.First().Key), LogicType.Equal);
            }
            else
            {
                condition = new Logic(source, new Number(max), LogicType.LessEqual);
                if (min != 0)
                    condition = new Logic(condition, new Logic(input_cache, new Number(min), LogicType.GreaterEqual), LogicType.And);

                bool HasInterval()
                {

                    var hasInterval = false;
                    var lastHash = hash.First().Key - 1;
                    foreach (var item in hash)
                    {
                        if (item.Key != lastHash + 1)
                        {
                            hasInterval = true;
                            break;
                        }

                        lastHash = item.Key;
                    }

                    return hasInterval;
                }

                if (hashDiff > 1 || defaultValue != 0 || HasInterval())
                {
                    isHash = true;

                    var expectValue = defaultValue;
                    var values = hash.Select(x => x.Value).Distinct().Where(x => x != expectValue).ToArray();

                    IAstNode index = min == 0 ? input_cache : new Arithmetic(input_cache, new Number(min), ArithmeticType.Sub);
                    condition = new Logic(new Parenthese(new Conditional(condition, new Index(new Literal($"{expr}"), index), new Number(defaultValue))), new Number(0), LogicType.NotEqual);
                }
                else
                {
                    condition = new Logic(new Parenthese(condition), new Boolean(true), LogicType.Equal);
                }
            }

            return condition;
        }
    }
}
