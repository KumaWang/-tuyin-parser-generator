using libflow.stmts;
using libfsm;
using libgraph;
using librule.generater;
using librule.utils;
using System.Collections;
using System.Collections.Concurrent;
using System.Text;

namespace librule.targets.code
{
    static class MetadataScanner
    {
        internal static MetadataScanResult Scan(CodeConstructor ctor, ProductionGraph graph, ProductionTable table, TokenTableManager manager)
        {
            var target = ctor.TargetBuilder;

            // 重新整理成目标Graph
            var targetGraph = new TargetGraph();
            var targetStates = new Dictionary<ushort, GraphState<TargetMetadatas>>();
            var targetEdges = new ConcurrentBag<GraphEdge<TargetMetadatas>>();

            void CheckStates(ushort left, ushort right, ushort subset)
            {
                if (!targetStates.ContainsKey(left))
                    targetStates[left] = new GraphState<TargetMetadatas>(left, GraphStateFlags.None);

                if (!targetStates.ContainsKey(right))
                    targetStates[right] = new GraphState<TargetMetadatas>(right, GraphStateFlags.None);

                if (subset != 0 && !targetStates.ContainsKey(subset))
                    targetStates[subset] = new GraphState<TargetMetadatas>(subset, GraphStateFlags.None);
            }

            var defaultMetadata = graph.GetDefaultMetadata();
            var visitors = new Dictionary<ushort, BFSSubsetVisitor>();
            var funcs = new Dictionary<string, BFSFunction>();
            var model = new FATable<ProductionMetadata>.ShiftMemoryModel(table.Transitions);
            var loops = table.GetLoops().Select(x => x[^1]).ToHashSet();
            var caches = new Dictionary<FATransition<ProductionMetadata>, List<ActionParameter>>();

            string BFSCreateLocal(BFSFunction bfsFunc, ushort token, string suffix, string type, ITargetMetadata metadata)
            {
                var localName = bfsFunc.CreateLocal(token, suffix, type);

                // 否则为参数生成使用变量名
                SetLocalNames(localName, metadata);

                return localName;
            }

            void SetLocalNames(string localName, ITargetMetadata metadata) 
            {
                // 否则为参数生成使用变量名
                if (metadata is TargetMetadata literal)
                {
                    for (var z = 0; z < literal.Count; z++)
                    {
                        var ctx = literal[z];
                        if (ctx.IsResult)
                            if (!ctx.Values.Contains(localName))
                                ctx.Values.Add(localName);
                    }
                }
                else if (metadata is TargetStatement control)
                {
                    control.LocalNames.Add(localName);
                }
                else throw new NotImplementedException();
            }

            // 访问节点
            IEnumerable<BFSVisitStep> BFSVisitStep(BFSFunction bfsFunc, BFSVisitStep step)
            {
                var tran = step.Transition;
                var bfsParameters = step.Parameters;
                if (bfsParameters.IsVisited(tran))
                    return Array.Empty<BFSVisitStep>();

                ctor.OnEntry(bfsFunc.EntryPoint);

                // 生成目标链接
                CheckStates(tran.Left, tran.Right, tran.Symbol.Value);
                var edge = new GraphEdge<TargetMetadatas>(
                    targetStates[tran.Left],
                    targetStates[tran.Right],
                    tran.Symbol.Value == 0 ? null : targetStates[tran.Symbol.Value],
                    new GraphEdgeValue(false, tran.Input.Chars), null);

                if (tran.Symbol.Type.HasFlag(FASymbolType.Report))
                    edge.Flags |= EdgeFlags.SpecialPoint;

                if (tran.Symbol.Type.HasFlag(FASymbolType.Close))
                    edge.Flags |= EdgeFlags.Close;

                // 如果该边指向的节点有子集节点，则递归访问其子集节点
                if (tran.Symbol.Type.HasFlag(FASymbolType.Request) && tran.Symbol.Value != 0)
                    BFSVisitSubset(tran.Symbol.Value);

                ITargetMetadata DequeueMetadata(ushort token, string type) 
                {
                    var queue = bfsParameters.ContainsKey(token) ? bfsParameters[token] : null;
                    var metadata = queue?.Pop();
                    return metadata;
                }

                string GetParamStep(ushort token, string type, bool createArg, bool peek)
                {
                    var queue = bfsParameters.ContainsKey(token) ? bfsParameters[token] : null;
                    var metadata = peek ? queue?.Peek() : queue?.Pop();

                    // 如果无法从队列中获取参数则代表参数不在子图内，应该是一个函数传参
                    if (metadata == null)
                    {
                        if (createArg)
                            return bfsFunc.CreateArg(token, type);

                        return null;
                    }
                    else
                    {
                        return BFSCreateLocal(bfsFunc, token, (peek ? queue.Count - 1 : queue.Count).ToString(), type, metadata);
                    }
                }

                if (caches.TryGetValue(tran, out List<ActionParameter> cacheSteps))
                {
                    for (var i = 0; i < cacheSteps.Count; i++)
                    {
                        var cacheStep = cacheSteps[i];
                        GetParamStep(cacheStep.Token, cacheStep.Type, false, false);
                    }
                }
                else
                {
                    var cache = caches[tran] = new List<ActionParameter>();
                    var rule = target.GetRuleFromOriginState(tran.Symbol.Value);
                    var isCall = tran.Symbol.Type.HasFlag(FASymbolType.Request) && tran.Symbol.Value != 0;
                    var insetIndex = -1;

                    // 判断是否存在metadata
                    IList<TableAction> actions = null;
                    if (!tran.Metadata.Value.Equals(defaultMetadata.Value))
                    {
                        // 获得所有动作
                        actions = graph.GetAction(tran.Metadata.Value).ToList();
                        if (isCall || !graph.IsEmptyTransition(tran))
                        {
                            for (var x = 0; x < actions.Count; x++)
                            {
                                var action = actions[x];
                                if (action.Front == 0)
                                {
                                    insetIndex = x;
                                    break;
                                }
                            }

                            if (insetIndex == -1)
                            {
                                actions.Add(new TableAction($"{rule.Name}", 0, tran.Metadata.Token));
                                insetIndex = actions.Count - 1;
                            }
                            else
                            {
                                actions.Insert(insetIndex, new TableAction($"{rule.Name}", 0, tran.Metadata.Token));
                            }
                        }
                    }
                    else
                    {
                        if (isCall || !graph.IsEmptyTransition(tran))
                        {
                            insetIndex = 0;
                            actions = new TableAction[] { new TableAction($"{rule.Name}", 0, tran.Metadata.Token) };
                        }
                    }

                    // 对动作进行解析
                    var targetActions = new TargetMetadatas();
                    if (actions != null)
                    {
                        for (var x = 0; x < actions.Count; x++)
                        {
                            ITargetMetadata parameters = null;
                            var usingParams = new List<(ushort, string)>();

                            var action = actions[x];
                            if (x == insetIndex)
                            {
                                IAstNode stmt = null;
                                if (isCall)
                                {
                                    var call = new Call(action.Context, true);

                                    // 注意在这里处理可能造成循环引用时arg信息未创建从而查找不到传参造成错误
                                    var sb = new StringBuilder();
                                    foreach (var arg in funcs[action.Context].Args)
                                    {
                                        call.Formals.Add(new Literal(GetParamStep(arg.Key, arg.Value.Type, true, true)));
                                        usingParams.Add((arg.Key, arg.Value.Type));
                                    }

                                    foreach (var formal in ctor.CodeVisitor.UserFormals)
                                        call.Formals.Add(formal);

                                    if (tran.Left != 1 && tran.Left == bfsFunc.EntryPoint && model.GetRights(tran.Left).Count == 1)
                                        call.Formals.Add(new Literal(Settings.READ_STEP_LITERAL));

                                    stmt = call;
                                }
                                else
                                {
                                    stmt = ctor.CreateObstructiveBranch(edge, null);
                                }

                                parameters = new TargetStatement(stmt, true);
                            }
                            else
                            {
                                var actionSteps = ParseAction(target, action.Context).ToArray();
                                var targetMetadata = new TargetMetadata(actionSteps.Length + 1);

                                var hasReturn = actionSteps.Any(x => x is ActionResult);
                                if (!hasReturn)
                                    targetMetadata.Add(new TargetMetadataStep(null, true, true));

                                // 处理动作步骤
                                for (var y = 0; y < actionSteps.Length; y++)
                                {
                                    var actionStep = actionSteps[y];
                                    if (actionStep is ActionParameter parameter)
                                    {
                                        cache.Add(parameter);

                                        var localName = GetParamStep(parameter.Token, parameter.Type, true, true);
                                        targetMetadata.Add(new TargetMetadataStep(localName, false, true));
                                        usingParams.Add((parameter.Token, parameter.Type));

                                        var setSelf = loops.Contains(tran) && action.Token == parameter.Token;
                                        if (setSelf)
                                            SetLocalNames(localName, targetMetadata);
                                    }
                                    else if (actionStep is ActionResult)
                                    {
                                        targetMetadata.Add(new TargetMetadataStep(null, true, false));
                                    }
                                    else
                                    {
                                        targetMetadata.Add(new TargetMetadataStep(actionStep.ToString(), false, true));
                                    }
                                }

                                parameters = targetMetadata;
                            }

                            if (x == insetIndex)
                                targetActions.Insert(0, parameters);
                            else
                                targetActions.Add(parameters);

                            // 弹出该动作所使用的全部参数
                            foreach (var item in usingParams.GroupBy(x => x))
                                DequeueMetadata(item.Key.Item1, item.Key.Item2);

                            // 将参数置入队列中
                            if (action.Token != 0)
                            {
                                if (!bfsParameters.ContainsKey(action.Token))
                                    bfsParameters[action.Token] = new ParameterQueue<ITargetMetadata>();

                                bfsParameters[action.Token].Push(parameters);
                            }
                        }
                    }

                    edge.Metadata = targetActions;
                    targetEdges.Add(edge);
                }

                // 获取该边集，并遍历其中的所有边
                if (bfsParameters.IsVisited(tran.Right))
                    return Array.Empty<BFSVisitStep>();

                return model.GetRights(tran.Right).Select(x => new BFSVisitStep(x, new BFSSubsetParameter(bfsParameters)));
            }

            // 从上至下进行访问，搜索以 state 为起点的所有节点，包括 state 节点本身
            void BFSVisitSubset(ushort state)
            {
                // 创建变量集合
                if (!visitors.ContainsKey(state))
                {
                    var rule = target.GetRuleFromOriginState(state);
                    var bfsFunc = new BFSFunction(state, rule.Name, rule.Type);
                    funcs.Add(rule.Name, bfsFunc);

                    visitors[state] = new BFSSubsetVisitor(bfsFunc, model.GetRights(state));
                }

                var visitor = visitors[state];
                while (visitor.Queue.Count > 0)
                    foreach (var next in BFSVisitStep(visitor.Function, visitor.Queue.Dequeue()))
                        visitor.Queue.Enqueue(next);
            }

            // 从状态1开始查找入口点
            BFSVisitSubset(1);
            foreach (var subset in table.GetSubsets())
                BFSVisitSubset(subset);

            // 扫描全部引用
            var createParentheses = new HashSet<Parenthese>();
            foreach (var edge in targetEdges) 
            {
                for (var i = 0; i < edge.Metadata.Count; i++) 
                {
                    if (edge.Metadata[i] is TargetStatement stmt && stmt.LocalNames.Count > 0)  
                    {
                        if (stmt.Statement is IObstructive obs)
                        {
                            bool isParenthese = obs.Condition.Left is Parenthese;

                            // 如果使用源不在文法头则需要插入赋值到目标文法
                            foreach (var local in stmt.LocalNames) 
                            {
                                var left = obs.Condition.Left;
                                if (createParentheses.Contains(left))
                                {
                                    var parentese = left as Parenthese;
                                    parentese = new Parenthese(new Assign(new Variable(edge.Source.Index, local), parentese.Node));
                                    createParentheses.Add(parentese);

                                    obs.Condition.Left = parentese;
                                }
                                else 
                                {
                                    if (!isParenthese)
                                        left = new Parenthese(left);

                                    var parentese = new Parenthese(new Assign(new Variable(edge.Source.Index, local), left));
                                    createParentheses.Add(parentese);
                                    obs.Condition.Left = parentese;
                                }
                            }
                        }
                        else 
                        {
                            foreach (var local in stmt.LocalNames)
                                stmt.Statement = new Assign(new Variable(edge.Source.Index, local), stmt.Statement);
                        }
                    }
                }
            }

            targetGraph.AddRange(targetEdges.OrderBy(x => x.Source.Index));
            return new MetadataScanResult(targetGraph, funcs.Values);
        }

        static IEnumerable<IActionStep> ParseAction(TargetBuilder target, string context)
        {
            var numIndex = 0;
            var numMode = false;
            var lastIndex = 0;

            for (var i = 0; i < context.Length; i++)
            {
                var c = context[i];
                var l = i == context.Length - 1 && char.IsDigit(c);
                if (numMode)
                {
                    if (char.IsDigit(c) && !l)
                    {
                    }
                    else
                    {
                        numMode = false;
                        var numLen = i - numIndex - (l ? 0 : 1);
                        if (numLen != 0)
                        {
                            var numStr = context.Substring(numIndex + 1, numLen);
                            var token = ushort.Parse(numStr);
                            var tmpLen = numIndex - lastIndex;
                            var literal = tmpLen > 0 ? context.Substring(lastIndex, tmpLen) : string.Empty;

                            if (!string.IsNullOrWhiteSpace(literal)) yield return new ActionLiteral(literal);
                            yield return new ActionParameter(token, target.GetTokenType(token));
                        }
                        else
                        {
                            var tmpLen = numIndex - lastIndex;
                            var literal = tmpLen > 0 ? context.Substring(lastIndex, tmpLen) : string.Empty;
                            if (!string.IsNullOrWhiteSpace(literal)) yield return new ActionLiteral(literal);
                            yield return new ActionResult();
                        }

                        lastIndex = l ? i + 1 : i;
                    }
                }
                else
                {
                    if (c == '$')
                    {
                        numMode = true;
                        numIndex = i;
                    }
                }
            }

            var lastLen = context.Length - lastIndex;
            if (lastLen > 0)
                yield return new ActionLiteral(context.Substring(lastIndex, lastLen));
        }

        class BFSSubsetVisitor
        {
            public BFSSubsetVisitor(BFSFunction function, IEnumerable<FATransition<ProductionMetadata>> transitions)
            {
                Function = function;
                Queue = new Queue<BFSVisitStep>(transitions.Select(x => new BFSVisitStep(x, new BFSSubsetParameter())));
            }

            public BFSFunction Function { get; }

            public Queue<BFSVisitStep> Queue { get; }
        }

        class BFSVisitStep
        {
            public BFSVisitStep(FATransition<ProductionMetadata> transition, BFSSubsetParameter parameters)
            {
                Transition = transition;
                Parameters = parameters;
            }

            public FATransition<ProductionMetadata> Transition { get; }

            public BFSSubsetParameter Parameters { get; }
        }

        class BFSSubsetParameter : Dictionary<ushort, ParameterQueue<ITargetMetadata>>
        {
            private HashSet<ushort> rights;
            private FATransition<ProductionMetadata> transition;
            private BFSSubsetParameter parent;

            public BFSSubsetParameter()
            {
                rights = new HashSet<ushort>();
            }

            public BFSSubsetParameter(BFSSubsetParameter parent)
                : this()
            {
                this.parent = parent;
                foreach (var item in parent)
                    Add(item.Key, new ParameterQueue<ITargetMetadata>(item.Value));
            }

            public bool IsVisited(FATransition<ProductionMetadata> transition)
            {
                var curr = this;
                while (curr != null)
                {
                    if (curr.transition.Equals(transition))
                        return true;

                    curr = curr.parent;
                }

                if (!this.transition.Equals(default))
                    throw new NotImplementedException();

                this.transition = transition;
                return false;
            }

            public bool IsVisited(ushort right)
            {
                var curr = this;
                while (curr != null)
                {
                    if (curr.rights.Contains(right))
                        return true;

                    curr = curr.parent;
                }

                rights.Add(right);
                return false;
            }
        }

        class ParameterQueue<T> : IEnumerable<T>
        {
            private int index;
            private List<T> list;
            private ParameterQueue<T> parent;

            public ParameterQueue()
            {
                list = new List<T>();
            }

            public ParameterQueue(ParameterQueue<T> parent)
                : this()
            {
                this.parent = parent;
                this.index = parent.index;
            }

            public int Capacity => (parent?.index ?? 0) + list.Count;

            public int Count => index;

            public T Pop() 
            {
                var curr = this;
                var i = index;
                while (curr != null)
                {
                    if (i > (curr.parent?.Count ?? 0))
                        break;

                    curr = curr.parent;
                }

                if (curr == null)
                    return default;

                index--;
                i = i - (curr.parent?.Count ?? 0);
                return curr.list[^i];
            }

            public T Peek() 
            {
                var curr = this;
                var i = index;
                while (curr != null)
                {
                    if (i > (curr.parent?.Count ?? 0))
                        break;

                    curr = curr.parent;
                }

                if (curr == null)
                    return default;

                i = i - (curr.parent?.Count ?? 0);
                return curr.list[^i];
            }

            public void Push(T item)
            {
                list.Add(item);
                index++;
            }

            public IEnumerator<T> GetEnumerator()
            {
                var queues = new List<ParameterQueue<T>>();
                var curr = this;
                while (curr != null)
                {
                    queues.Insert(0, curr);
                    curr = curr.parent;
                }

                for (var i = 0; i < queues.Count; i++) 
                {
                    var queue = queues[i];
                    if (queue.index < queue.list.Count)
                    {
                        for (var x = queue.index; x < queue.list.Count; x++)
                            yield return queue.list[x];
                    }
                    else break;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        class BFSFunction : IFunctionConstructor
        {
            public BFSFunction(ushort state, string functionName, string returnType)
            {
                EntryPoint = state;
                FunctionName = functionName;
                ReturnType = returnType;
                Args = new Dictionary<ushort, FunctionVariable>();
                Locals = new Dictionary<ushort, Dictionary<string, FunctionVariable>>();
            }

            public ushort EntryPoint { get; }

            public string FunctionName { get; }

            public string ReturnType { get; }

            internal Dictionary<ushort, FunctionVariable> Args { get; }

            internal Dictionary<ushort, Dictionary<string, FunctionVariable>> Locals { get; }

            IEnumerable<FunctionVariable> IFunctionConstructor.Args => Args.Values;

            IEnumerable<FunctionVariable> IFunctionConstructor.Locals => Locals.Values.SelectMany(x => x.Select(y => y.Value));

            internal string CreateLocal(ushort token, string suffix, string type)
            {
                if (!Locals.ContainsKey(token))
                    Locals[token] = new Dictionary<string, FunctionVariable>();

                if (Locals[token].Count > 0 && Locals[token].First().Value.Type != type)
                    throw new NotImplementedException();

                var name = $"{Settings.LOCAL_LITERAL}_{token}_{suffix}";
                if (!Locals[token].ContainsKey(name))
                    Locals[token].Add(name, new FunctionVariable(name, type));

                return name;
            }

            internal string CreateArg(ushort token, string type)
            {
                var name = $"{Settings.ARG_LITERAL}{token}";
                if (Args.ContainsKey(token))
                {
                    if (Args[token].Type != type)
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    Args.Add(token, new FunctionVariable(name, type));
                }
                return name;
            }
        }

        interface IActionStep
        {
        }

        class ActionLiteral : IActionStep
        {
            public ActionLiteral(string context)
            {
                Context = context;
            }

            public string Context { get; }

            public override string ToString()
            {
                return Context;
            }
        }

        class ActionParameter : IActionStep
        {
            public ActionParameter(ushort token, string type)
            {
                Token = token;
                Type = type;
            }

            public ushort Token { get; }

            public string Type { get; }

            public override string ToString()
            {
                return $"[{Token}]";
            }
        }

        class ActionResult : IActionStep
        {
        }
    }
}
