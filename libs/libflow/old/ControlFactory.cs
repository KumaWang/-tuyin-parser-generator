using libgraph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace libcfg
{
    /// <summary>
    /// Graph数据结构化工厂
    /// </summary>
    /// <typeparam name="T">外部文法内部参数类型</typeparam>
    public static class ControlFactory
    {
        public static IEnumerable<IControl> Create<TVertex, TEdge>(this IGraph<TVertex, TEdge> graph, ControlConstructor<TVertex, TEdge> ctor)
            where TEdge : IEdge<TVertex>
            where TVertex : IVertex
        {
            var converter = new Converter<TVertex, TEdge>(ctor);
            var scanner = new Scanner<TVertex, TEdge>();
            var optimizer = new Optimizer<TVertex, TEdge>();
            foreach (var step in graph.GetSteps())
            {
                // 需要确定如何得到Function文法的Formals
                var stmt = converter.Create(step);
                // 扫描出所有被应用的Label
                var labels = scanner.Visit(x => x is ControlGoto gs ? gs.Label : null, stmt).ToHashSet();
                // 对文法进行清理,移除未使用的Label文法和重引用Delay.Statement并对循环条件进行优化
                stmt = optimizer.Visit(stmt, labels);
                // 返回函数
                yield return new ControlFunction(step.GetSources().First().Source.Index, stmt);
            }
        }

        class Converter<TVertex, TEdge> : StepVisitor<TVertex, TEdge, IControl, IConditional>
            where TEdge : IEdge<TVertex>
            where TVertex : IVertex
        {
            private readonly static ForkItemComparer sForkItemComparer = new ForkItemComparer();

            private Dictionary<ushort, Label> mLabels;
            private ControlConstructor<TVertex, TEdge> mCtor;
            private Stack<StepLoop<TVertex, TEdge>> mLoopStacks;
            private StepWalker<TVertex, TEdge, IEdgeStep<TVertex, TEdge>> mWalker;
            private StepWalker<TVertex, TEdge, bool> mWalker2;
            private Dictionary<GraphStep<TVertex, TEdge>, HashSet<TEdge>> mLoopFollows;

            public Converter(ControlConstructor<TVertex, TEdge> ctor)
            {
                mCtor = ctor;
                mLoopStacks = new Stack<StepLoop<TVertex, TEdge>>();
                mWalker = new StepWalker<TVertex, TEdge, IEdgeStep<TVertex, TEdge>>();
                mWalker2 = new StepWalker<TVertex, TEdge, bool>();
                mLoopFollows = new Dictionary<GraphStep<TVertex, TEdge>, HashSet<TEdge>>();
            }

            public IControl Create(GraphStep<TVertex, TEdge> step)
            {
                mLabels = mWalker.Visit(x =>
                {
                    if (x.StepType == GraphStepType.Upward || x.StepType == GraphStepType.Downward)
                        return x as IEdgeStep<TVertex, TEdge>;

                    return null;
                }, step).Where(x => x != null).Select(x => x.Edge.Target.Index).Distinct().ToDictionary(x => x, x => new Label(x));

                mCtor.OnEntry(step.GetSources().First().Source.Index);
                return step.Visit(null, this);
            }

            public override IControl Visit(IConditional conditional, GraphStep<TVertex, TEdge> step)
            {
                var first = mCtor.BeforeVisit(step);
                var stmt = base.Visit(conditional, step);
                if (first != null)
                    stmt = new ControlConcatenation(first, stmt);

                var follow = mCtor.AfterVisit(step);
                if (follow != null)
                    stmt = new ControlConcatenation(stmt, follow);

                return mCtor.Check(stmt);
            }

            protected override IControl VisitConcatenation(IConditional conditional, StepConcatenation<TVertex, TEdge> step)
            {
                var left = step.Left.Visit(conditional, this);
                var right = step.Right.Visit(null, this);

                if (left.ControlType == ControlType.Empty)
                    return right;

                if (right.ControlType == ControlType.Empty)
                    return left;

                return new ControlConcatenation(left, right);
            }

            protected override IControl VisitNext(IConditional conditional, StepNext<TVertex, TEdge> step)
            {
                // 调用外部生成文法
                var stmt = mCtor.CreateStatement(step.Edge, conditional);

                // 判断是否存在label
                /*
                if (mLabels.ContainsKey(step.Edge.Source.Index))
                    stmt = stmt == null ?
                      new ControlLabel(mLabels[step.Edge.Source.Index]) :
                      new ControlConcatenation(new ControlLabel(mLabels[step.Edge.Source.Index]), stmt) as IControl;
                */

                return stmt ?? new ControlEmpty();
            }

            protected override IControl VisitDownward(IConditional conditional, StepDownward<TVertex, TEdge> step)
            {
                // 在downward中确定goto|break
                var stmt = VisitNext(conditional, step);

                // 通过step.Edge.Target.Index判断向下转跳到的位置是否是当前循环结束位置
                // 1.如果当前不存在循环则代表是goto
                // 2.否则创建后期处理文法
                if (mLoopStacks.Count == 0)
                {
                    if (mCtor.CreateFlags.HasFlag(ControlCreateFlags.LimitGotoBack))
                        throw new NotImplementedException();

                    stmt = new ControlConcatenation(stmt, new ControlGoto(mLabels[step.Edge.Target.Index]));
                }
                else
                {
                    // 步骤2
                    // Downward一定是循环退出点，在这里需要判断是否使用了switch 阻挡了break退出
                    // 如果是不支持Goto的输出则需要在引用到该位置的地方标记不可使用switch
                    if (!mCtor.CreateFlags.HasFlag(ControlCreateFlags.LimitBreak))
                    {
                        stmt = new ControlConcatenation(stmt, new ControlBreak());
                    }
                    else
                    {
                        if (mCtor.CreateFlags.HasFlag(ControlCreateFlags.LimitGotoBack))
                            throw new NotImplementedException();

                        stmt = new ControlConcatenation(stmt, new ControlGoto(mLabels[step.Edge.Target.Index]));
                    }
                }

                return stmt;
            }

            protected override IControl VisitUpward(IConditional conditional, StepUpward<TVertex, TEdge> step)
            {
                // 在upward中确定goto|continue|break
                var stmt = VisitNext(conditional, step);

                // 通过step.Edge.Target.Index判断向上转跳到的位置是否是当前循环结束位置
                // 1.如果当前不存在循环则代表是goto
                // 2.如果是当前循环结束位置则代表是break,否则是goto
                // 3.如果step.Edge.Target.Index是循环开头则代表是continue
                if (mLoopStacks.Count == 0)
                {
                    if (mCtor.CreateFlags.HasFlag(ControlCreateFlags.LimitGotoFront))
                        throw new NotImplementedException();

                    stmt = new ControlConcatenation(stmt, new ControlGoto(mLabels[step.Edge.Target.Index]));
                }
                else
                {
                    var currLoop = mLoopStacks.Last();
                    // currLoop.GetSources().Contains(step.Edge) 备选方案,性能低
                    if (step.Edge.Target.Index != currLoop.GetSources().First().Source.Index)
                    {
                        // 步骤2
                        // 同理为VisitDownward第2部处理方法
                        // 但如果step.Edge.Target转跳到的是上级循环(如果当前循环外还有一个循环)的开头文法则代表同样是一个break
                        // stmt = AddToDelay(new Delay<TVertex, TEdge>(stmt, mLoopStacks.ToArray(), step));
                        throw new NotImplementedException("还未实现，等待例子重现。");
                    }
                    else
                    {
                        // 步骤3
                        if (!mCtor.CreateFlags.HasFlag(ControlCreateFlags.LimitContinue))
                        {
                            stmt = new ControlConcatenation(stmt, new ControlContinue());
                        }
                        else
                        {
                            if (mCtor.CreateFlags.HasFlag(ControlCreateFlags.LimitGotoFront))
                                throw new NotImplementedException();

                            stmt = new ControlConcatenation(stmt, new ControlGoto(mLabels[step.Edge.Target.Index]));
                        }
                    }
                }

                return stmt;
            }

            protected override IControl VisitFork(IConditional conditional, StepFork<TVertex, TEdge> step)
            {
                ControlConditional result = null;
                // 在fork中确定switch|if|if..else
                // 1.如果分支条件中的源是一致(或null)的则使用switch
                // 2.如果分支直接转跳到上层(up|down)空间则是if,否则是if..else
                // 3.如果分支条件中存在多个null源则抛出异常
                // 其他情况:step.Steps < 2 按道理说不应该出现在这一步直接抛出异常，并检查生成ForkStep是否没有展开
                if (step.Steps.Count < 2)
                    throw new NotImplementedException("内部错误");

                var stmts = new List<IControl>();

                var hasBreak = false;
                var items = new (IConditional Condition, int Index)[step.Steps.Count];
                var allEdges = step.GetSources().ToArray();
                for (var i = 0; i < step.Steps.Count; i++)
                {
                    var edges = step.Steps[i].GetSources().ToArray();
                    if (edges.Length == 0)
                        throw new GraphException<TVertex, TEdge>($"未找到分支，无法确定用于构造文法的判断条件。", edges);

                    hasBreak = hasBreak || mWalker2.Visit(x =>
                    {
                        return x.StepType == GraphStepType.Downward;
                    }, step.Steps[i]).Any();

                    if (edges.Length != 1)
                    {
                        var first = mCtor.CreateTryMatch(edges[0], allEdges);
                        for (var x = 1; x < edges.Length; x++)
                            first = new ControlOr(first, mCtor.CreateTryMatch(edges[x], allEdges));

                        // if..else,多条件分支
                        items[i] = (first, i);
                    }
                    else
                    {
                        items[i] = (mCtor.CreateTryMatch(edges[0], allEdges), i);
                    }
                }

                if (items.Where(x => x.Condition != null).Where(x => !(x.Condition is ControlOr) && !(x.Condition is ControlAnd)).Count(x => x.Condition.Value == null) > 1)
                    throw new GraphException<TVertex, TEdge>("无法确定分支条件源，它存在多个null转跳。", 
                        items.Where(x => x.Condition != null && x.Condition.Value == null).SelectMany(x => step.Steps[x.Index].GetSources()).ToArray());

                // 如果在循环内存在break则不能使用switch
                // 优先使用[转换switch为if..else]正确使用break来代替goto文法从switch内退出循环的问题
                // 因为并非所有输出语言都支持使用goto，例如js
                // var inLoopBreak = !mCtor.CreateFlags.HasFlag(ControlCreateFlags.LimitBreak) && mLoopStacks.Count > 0 && hasBreak;

                // 判断是否使用switch
                // var isSwitch = !mCtor.CreateFlags.HasFlag(ControlCreateFlags.LimitSwitch);

                // 调换null条件顺序到最后
                Array.Sort(items, sForkItemComparer);

                IConditional condition = null;
                if (!mCtor.CreateFlags.HasFlag(ControlCreateFlags.LimitConditionalMultiplexing) && conditional != null)
                    condition = new ControlValue(conditional.Source);

                IConditional GetIfConditional(IConditional cond) 
                {
                    if (condition != null)
                        return new ControlEquatable(condition.Source, cond.Value);

                    return cond;
                }

                var groups = items.GroupBy(x => x.Condition.Source);
                foreach (var group in groups) 
                {
                    var tempGroupStmt = result;

                    var groupItems = group.ToArray();
                    if (!mCtor.CreateFlags.HasFlag(ControlCreateFlags.LimitIf) && groupItems.Length < 3)
                    {
                        if (groupItems.Length == 1)
                        {
                            var first = step.Steps[groupItems[0].Index].Visit(condition, this);
                            result = new ControlIf(groupItems[0].Condition, first);
                        }
                        else 
                        {
                            var firstCond = GetIfConditional(groupItems[0].Condition);
                            var followCond = GetIfConditional(groupItems[1].Condition);
                            var first = step.Steps[groupItems[0].Index].Visit(firstCond, this);
                            var follow = step.Steps[groupItems[1].Index].Visit(followCond, this);

                            // 判断第一个文法是否为Empty
                            if (firstCond.Value == null)
                            {
                                // 如果为empty则创建if(condition) first
                                if (first.ControlType == ControlType.Empty && mCtor.CreateFlags.HasFlag(ControlCreateFlags.EmptyReturn))
                                {
                                    result = new ControlIfElse(followCond, follow, new ControlConcatenation(mCtor.CreateWaitOne(step.GetSources().First()), new ControlReturn()));
                                }
                                else if (first.ControlType != ControlType.Empty)
                                {
                                    result = new ControlIfElse(followCond, follow, first);
                                }
                                else
                                {
                                    result = new ControlIf(firstCond, first);
                                }
                            }
                            else if (followCond.Value == null)
                            {
                                // 如果为empty则创建if(condition) first
                                if (follow.ControlType == ControlType.Empty && mCtor.CreateFlags.HasFlag(ControlCreateFlags.EmptyReturn))
                                {
                                    result = new ControlIfElse(firstCond, first, new ControlConcatenation(mCtor.CreateWaitOne(step.GetSources().First()), new ControlReturn()));
                                }
                                else if (follow.ControlType != ControlType.Empty)
                                {
                                    result = new ControlIfElse(firstCond, first, follow);
                                }
                                else
                                {
                                    result = new ControlIf(followCond, follow);
                                }
                            }
                            else if (first.ControlType == ControlType.Empty)
                            {
                                // 如果为empty则创建if(condition) follow
                                if (mCtor.CreateFlags.HasFlag(ControlCreateFlags.EmptyReturn))
                                {
                                    result = new ControlIfElse(followCond, follow, new ControlConcatenation(mCtor.CreateWaitOne(step.GetSources().First()), new ControlReturn()));
                                }
                                else
                                {
                                    result = new ControlIf(followCond, follow);
                                }
                            }
                            else if (follow.ControlType == ControlType.Empty)
                            {
                                // 如果为empty则创建if(condition) first
                                if (mCtor.CreateFlags.HasFlag(ControlCreateFlags.EmptyReturn))
                                {
                                    result = new ControlIfElse(firstCond, first, new ControlConcatenation(mCtor.CreateWaitOne(step.GetSources().First()), new ControlReturn()));
                                }
                                else
                                {
                                    result = new ControlIf(firstCond, first);
                                }
                            }
                            
                            else
                            {
                                // 否则创建if..else
                                followCond = groupItems[0].Condition.CanMerge && groupItems[1].Condition.CanMerge ?
                                    new ControlEquatable(firstCond.Source, groupItems[1].Condition.Value) :
                                    followCond;

                                result = new ControlIfElse(firstCond, first, new ControlIf(followCond, follow));
                            }
                        }
                    }
                    else 
                    {
                        condition = condition ?? mCtor.CreateTryMatchAny(step.GetSources().First());

                        var cases = new ControlSwitchCase[groupItems.Length];
                        for (var i = 0; i < groupItems.Length; i++)
                        {
                            var item = groupItems[i];
                            var stmt = step.Steps[item.Index].Visit(condition, this);
                            if (stmt is ControlConcatenation cc)
                            {
                                var lastType = cc.GetLast()?.ControlType ?? ControlType.Empty;
                                if (lastType != ControlType.Break && lastType != ControlType.Continue && lastType != ControlType.Return)
                                    stmt = new ControlConcatenation(stmt, new ControlBreak());
                            }
                            else if (stmt.ControlType != ControlType.Break && stmt.ControlType != ControlType.Continue && stmt.ControlType != ControlType.Return)
                            {
                                stmt = new ControlConcatenation(stmt, new ControlBreak());
                            }

                            stmts.Add(stmt);
                            var cond = item.Condition;
                            cases[i] = cond.Source == null ?
                                new ControlSwitchDefault(stmt) :
                                new ControlSwitchCase(cond, stmt);
                        }

                        result = new ControlSwitch(condition, cases);
                    }

                    if (tempGroupStmt != null) 
                    {
                        var tempResult = result;
                        result = new ControlIfElse(result.Condition, result, tempGroupStmt);
                        tempResult.Condition = new ControlValue(result.Condition.Source);
                    }
                }    

                // 判断是否后续文法完全一致
                return result;
            }

            protected override IControl VisitLoop(IConditional conditional, StepLoop<TVertex, TEdge> step)
            {
                mLoopFollows[step] = new HashSet<TEdge>();
                mLoopStacks.Push(step);

                // 得到文法头部所有边
                var first = step.Steps[0];
                var edges = first.GetSources().ToArray();

                // 检查下一步
                if (first.StepType == GraphStepType.Loop)
                    throw new GraphException<TVertex, TEdge>(
                        $"循环步内不会紧接一个循环步，可能是内部逻辑异常导致。", edges);

                // 单纯的cfg控制流只会创建while文法，do..while与for循环需要在后期修改，需要借助IR信息
                // 文法头部所有可能性,if|if..else|switch|next
                var conds = new IConditional[edges.Length];
                for (var i = 0; i < edges.Length; i++)
                    conds[i] = mCtor.CreateTryMatch(edges[i], edges);

                // 判断循环中是否存在空循环
                var emptyConds = conds.Select((x, i) => (x, i)).Where(x => x.x.Value == null).ToArray();
                if (emptyConds.Length > 0)
                {
                    if (emptyConds.Length > 1 || !conds.Any(x => x.Value != null)) 
                    {
                        // 应该在librule.dsl中实现这个判断和抛出错误提示到控制台，这里需要实现，因为存在其他项目调用该库
                        // 无法获得循环退出条件，它存在一个或多个空匹配转跳，这会导致无限循环
                        // 这时候应该使用true直接表达，并通过判断内部条件匹配后是否为null得到是否继续循环
                        throw new GraphException<TVertex, TEdge>("无法获得循环退出条件，它存在一个或多个空匹配转跳，这会导致无限循环。", emptyConds.Select(x => edges[x.i]).ToArray());
                    }
                }

                // 生成文法
                if (emptyConds.Length == 0)
                {
                    // 如果条件只有1个则while(cond)否则while(true)交给分支退出，考虑while(cond)后续文法如何避免重复使用cond文法
                    // 1.多条件，比如switch,if..else if时使用while(TryMatch(out v))
                    // 2.如果是单一条件则比如next或是if文法则while(TryMatch(token, out v)),因为单一条件只匹配同一token
                    // 3.是否为单一条件由isSameCond决定
                    // 4.在librule.targets生成目标代码时，处于while文法内的所有头部文法不被应用
                    // 5.如果头部文法是分支或其他组合文法则深入查找到内部所有第一条，分支可包含多条头部文法
                    // 6.ReplaceValue意思为替换Graph中Match步骤的使用源
                    // 7.如果是while紧接switch文法,在switch文法中已经存在了一个TryMatchAny,则将它移动位置
                    var isSameCond = conds.GroupBy(x => new { Source = x.Source, Value = x.Value }).Count() == 1;
                    var condition = isSameCond ? conds[0] : mCtor.CreateTryMatchAny(edges[0]);
    
                    var stmt = step.Steps[0].Visit(condition, this);
                    for (var i = 1; i < step.Steps.Count; i++)
                        stmt = new ControlConcatenation(stmt, step.Steps[i].Visit(null, this));

                    if (stmt is ControlConcatenation whileCC) 
                    {
                        var whileCCLast = whileCC.GetLast();
                        if (whileCCLast.ControlType == ControlType.Continue) 
                        {
                            IControl ResetWhileLast(ControlConcatenation ccStmt) 
                            {
                                var right = ccStmt.Right;
                                if (right is ControlConcatenation ccRight)
                                    right = ResetWhileLast(ccRight);
                                else if (right.ControlType == ControlType.Continue)
                                    return ccStmt.Left;

                                return new ControlConcatenation(ccStmt.Left, right);
                            }

                            stmt = ResetWhileLast(whileCC);
                        }
                    }

                    stmt = new ControlConcatenation(new ControlWhile(condition, stmt), mCtor.CreateWaitOne(edges[0]));
                    mLoopStacks.Pop();
                    return stmt;
                }
                else 
                {
                    var emptyCond = emptyConds.First();
                    var firstIndex = emptyCond.i == 0 ? 1 : 0;
                    var isSameCond = conds.Select((x, i) => (x, i)).Where(x => x.i != emptyCond.i).GroupBy(x => new { Source = x.x.Source, Value = x.x.Value }).Count() == 1;
                    var condition = isSameCond ? conds[firstIndex] : mCtor.CreateTryMatchAny(edges[0]);

                    var stmt = step.Steps[firstIndex].Visit(condition, this);
                    for (var i = firstIndex + 1; i < step.Steps.Count; i++)
                        if (i != emptyCond.i)
                            stmt = new ControlConcatenation(stmt, step.Steps[i].Visit(null, this));

                    stmt = new ControlConcatenation(new ControlDoWhile(condition, stmt), mCtor.CreateWaitOne(edges[0]));

                    var emptyStmt = step.Steps[emptyCond.i].Visit(null, this);
                    if (emptyStmt == null || emptyStmt is ControlEmpty)
                        stmt = new ControlIf(condition, stmt);
                    else
                        stmt = new ControlIfElse(condition, stmt, emptyStmt);

                    return stmt;
                }
            }
        }

        class ForkItemComparer : IComparer<(IConditional Cond, int Index)>
        {
            public int Compare((IConditional Cond, int Index) x, (IConditional Cond, int Index) y)
            {
                var xv = x.Cond == null ? 0 : (x.Cond.Value == null ? 1 : 2);
                var yv = y.Cond == null ? 0 : (y.Cond.Value == null ? 1 : 2);
                return yv - xv;
            }
        }

        class Scanner<TVertex, TEdge> : ControlWalker<Label>
            where TEdge : IEdge<TVertex>
            where TVertex : IVertex
        {
            public override IEnumerable<Label> Visit(IControl stmt)
            {
                foreach (var item in base.Visit(stmt))
                    if (item != null) yield return item;
            }
        }

        class Optimizer<TVertex, TEdge> : ControlVisitor<IControl>
            where TEdge : IEdge<TVertex>
            where TVertex : IVertex
        {
            private HashSet<Label> mApplyLabels;

            public IControl Visit(IControl stmt, HashSet<Label> labels)
            {
                mApplyLabels = labels;
                return stmt.Visit(this);
            }

            public override IControl Visit(IControl stmt)
            {
                return base.Visit(stmt);
            }

            protected override IControl VisitConcatenation(ControlConcatenation stmt)
            {
                var left = stmt.Left.Visit(this);
                var right = stmt.Right.Visit(this);
                if (left == null) return right;
                if (right == null) return left;
                return new ControlConcatenation(left, right);
            }

            protected override IControl VisitLabel(ControlLabel stmt)
            {
                return mApplyLabels.Contains(stmt.Label) ? stmt : new ControlEmpty() as IControl;
            }

            protected override IControl VisitExternal(IControl stmt)
            {
                return stmt;
            }

            protected override IControl VisitBreak(ControlBreak stmt)
            {
                return stmt;
            }

            protected override IControl VisitContinue(ControlContinue stmt)
            {
                return stmt;
            }

            protected override IControl VisitGoto(ControlGoto stmt)
            {
                return stmt;
            }

            protected override IControl VisitIf(ControlIf stmt)
            {
                return new ControlIf(stmt.Condition.Visit(this) as IConditional, stmt.Consequent.Visit(this));
            }

            protected override IControl VisitIfElse(ControlIfElse stmt)
            {
                return new ControlIfElse(stmt.Condition.Visit(this) as IConditional, stmt.Consequent.Visit(this), stmt.Alternate.Visit(this));
            }

            protected override IControl VisitSwitch(ControlSwitch stmt)
            {
                var cond = stmt.Condition.Visit(this);
                var cases = new ControlSwitchCase[stmt.Cases.Count];
                for (var i = 0; i < stmt.Cases.Count; i++)
                {
                    var c = stmt.Cases[i];
                    cases[i] = c.Condition == null ?
                       new ControlSwitchDefault(c.Body.Visit(this)) :
                       new ControlSwitchCase(c.Condition.Visit(this), c.Body.Visit(this));
                }

                return new ControlSwitch(cond as IConditional, cases);
            }

            protected override IControl VisitWhile(ControlWhile stmt)
            {
                return new ControlWhile(stmt.Condition?.Visit(this) as IConditional, stmt.Body.Visit(this));
            }

            protected override IControl VisitDoWhile(ControlDoWhile stmt)
            {
                return new ControlDoWhile(stmt.Condition?.Visit(this) as IConditional, stmt.Body.Visit(this));
            }

            protected override IControl VisitEmpty(ControlEmpty stmt)
            {
                return stmt;
            }

            protected override IControl VisitOr(ControlOr stmt)
            {
                return new ControlOr(stmt.Left?.Visit(this) as IConditional, stmt.Right?.Visit(this) as IConditional);
            }

            protected override IControl VisitAnd(ControlAnd stmt)
            {
                return new ControlAnd(stmt.Left?.Visit(this) as IConditional, stmt.Right?.Visit(this) as IConditional);
            }

            protected override IControl VisitEquatable(ControlEquatable stmt)
            {
                return new ControlEquatable(stmt.Source?.Visit(this), stmt.Value?.Visit(this));
            }

            protected override IControl VisitNot(ControlNot stmt)
            {
                return new ControlNot(stmt.Source?.Visit(this));
            }

            protected override IControl VisitValue(ControlValue stmt)
            {
                return new ControlValue(stmt.Source?.Visit(this));
            }

            protected override IControl VisitReturn(ControlReturn stmt)
            {
                return stmt;
            }
        }
    }
}
