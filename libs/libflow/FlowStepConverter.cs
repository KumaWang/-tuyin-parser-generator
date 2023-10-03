using libflow.stmts;
using libgraph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace libflow
{
    class FlowStepConverter<TVertex, TEdge> : FlowStepVisitor<TVertex, TEdge, IAstNode, IConditional>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        class LabelControl
        {
            public LabelControl(IConditional conditional, IAstNode control, Concatenation parent, FlowStep<TVertex, TEdge> step)
            {
                Conditional = conditional;
                Control = control;
                Parent = parent;
                Step = step;
            }

            public IConditional Conditional { get; }

            public IAstNode Control { get; }

            public Concatenation Parent { get; }

            public FlowStep<TVertex, TEdge> Step { get; }
        }

        private Dictionary<ushort, Label> mLabels;
        private Dictionary<ushort, LabelControl> mLabelControls;

        private HashSet<ushort> mApplyLabels;
        private AstConstructor<TVertex, TEdge> mCtor;
        private Stack<FlowStepLoop<TVertex, TEdge>> mLoopStacks;
        private Stack<FlowStepFork<TVertex, TEdge>> mForkStacks;

        public FlowStepConverter(AstConstructor<TVertex, TEdge> ctor)
        {
            mCtor = ctor;
            mApplyLabels = new HashSet<ushort>();
            mForkStacks = new Stack<FlowStepFork<TVertex, TEdge>>();
            mLoopStacks = new Stack<FlowStepLoop<TVertex, TEdge>>();
            mLabelControls = new Dictionary<ushort, LabelControl>();
        }

        public IAstNode Create(FlowPath<TVertex, TEdge> path)
        {
            var step = path.Step;
            var updowns = step.Walk().Where(x => x is FlowStepGoto<TVertex, TEdge>).Cast<FlowStepGoto<TVertex, TEdge>>().ToArray();
            // 检查ups
            foreach (var updown in updowns)
                if (updown is FlowStepUpward<TVertex, TEdge> upward)
                    mLabelControls[upward.Edge.Target.Index] = null;

            // 生成labels
            mLabels = updowns.Select(x => x.Edge.Target.Index).Distinct().ToDictionary(x => x, x => new Label(x));

            var entryPoint = step.GetSources().First().Source.Index;
            mCtor.OnEntry(entryPoint);
            var result = step.Visit(null, this);

            // 为没有退出条件的label替换第一个条件的body源，如果label后续文法不存在条件则生成条件并插入跳出动作
            foreach (var lc in mLabelControls)
            {
                var index = lc.Key;
                var label = lc.Value;

                // 找到第一个条件文法
                var conditional = label.Control.Walk().FirstOrDefault(x => x is IConditional) as IConditional;
                if (conditional == null)
                {
                    var sources = label.Step.GetSources().ToArray();
                    if (sources.Length != 1)
                        throw new GraphException<TVertex, TEdge>($"无法确定源，多条件分支应该已经在分支步内处理完成，这里不应该存在找不到条件文法的情况。", sources);

                    // 如果不存在条件文法则创建
                    conditional = label.Conditional ?? mCtor.CreateObstructiveBranch(sources[0], null);

                    // 重新创建该文法
                    var call2 = label.Control.Next().FirstOrDefault(x => x is Call cl && cl.IsMutableFormals) as Call;
                    if (call2 != null)
                        call2.Formals.Add(conditional.GetDeepSource());

                    // 替换上级文法
                    label.Parent.Right = new If(new Logic(conditional.Left, conditional.Right, LogicType.Equal), label.Control);
                }

                // 检查是否存在goto到其他层
                var gotoStmts = label.Parent.Walk().Where(x => x is Goto).Cast<Goto>().ToArray();

                // 确定文法尾部是否有任意不是goto到该label的文法
                var hasBreak = label.Parent.Right.Ends().Any(x => !(x is Goto g) || g.Label.Index != index);

                // 如果不存在上述条件则检查内部是否存在goto到其他层的文法
                var hasExit = hasBreak || (gotoStmts.Length > 0 ? gotoStmts.Any(x => x.Label.Index != index) : false);

                // 如果条件不满足则代表这个label是一个无法退出的无限循环块，需要替换body为goto到下一层
                if (!hasExit)
                {
                    // 首先得到当前层
                    var currLayer = path.Figure.Layers.FirstOrDefault(x => x.State == path.Figure.GraphFigure.OwnerLayer[index]);
                    if (currLayer == null)
                        throw new NotImplementedException();

                    var currIndex = path.Figure.Layers.IndexOf(currLayer);

                    // 固定目标是转跳下一层
                    var nextLayer = currIndex < path.Figure.Layers.Count - 2 ? path.Figure.Layers[currIndex + 1] : null;

                    // 如果不存在下一层则代表转跳到函数结尾
                    var nextIndex = nextLayer?.State ?? 0;
                    if (!mLabels.ContainsKey(nextIndex))
                        mLabels[nextIndex] = new Label(nextIndex);

                    var gotoStmt = new Goto(mLabels[nextIndex]);
                    if (conditional is IObstructive obs)
                        obs.Reason = new Concatenation(mCtor.CreateWaitOne(label.Step.GetSources().First()), gotoStmt);
                    else
                    {
                        // 如果当前不是层结尾或不连接到目标层则修改源
                        // 意思为后续如果还有其他文法则
                        var targets = label.Control.Ends().ToArray();
                        var hasOtehrStmt = targets.Any(x => !(x is Goto g) || g.Label.Index != index);
                        if (!hasOtehrStmt)
                        {
                            var sources = label.Step.GetSources().ToArray();
                            if (sources.Length < 1)
                                throw new GraphException<TVertex, TEdge>($"无法确定源，多条件分支应该已经在分支步内处理完成，这里不应该存在找不到条件文法的情况。", sources);

                            var otherObs = mCtor.CreateObstructiveBranch(sources[0], null);
                            if (otherObs == null)
                                label.Parent.Right = Concatenation.From(label.Control, gotoStmt);
                            else
                            {
                                // 重新创建该文法
                                var call2 = label.Control.Walk().FirstOrDefault(x => x is Call cl && cl.IsMutableFormals) as Call;
                                if (call2 != null)
                                    call2.Formals.Add(otherObs.GetDeepSource());

                                label.Parent.Right = new IfElse(otherObs.Condition, label.Control, gotoStmt);
                            }
                        }
                    }
                }
                else if (label.Control.AstNodeType == AstNodeType.Switch) 
                {
                    var @switch = label.Control as Switch;
                    // 确定文法尾部是否有任意不是goto到该label的文法
                    var breaks = @switch.Cases.Where(x => x.Body.Ends().Any(y => !(y is Goto g) || g.Label.Index != index)).ToArray();
                    if (breaks.Length > 0)
                    {
                        if (!mLabels.ContainsKey(0))
                            mLabels.Add(0, new Label(0));

                        var gotoStmt = new Goto(mLabels[0]);
                        for (var i = 0; i < breaks.Length; i++)
                        {
                            var @break = breaks[i];
                            if (@break.Condition == null)
                            {
                                @break.Body = gotoStmt;
                            }
                        }
                    }
                }
            }

            if (mLabels.ContainsKey(0) && !mApplyLabels.Contains(0))
                result = new Concatenation(result, new DefineLabel(new List<Label>() { mLabels[0] }));

            // 检查返回
            return result;
        }

        public override IAstNode Visit(IConditional conditional, FlowStep<TVertex, TEdge> step)
        {
            DefineLabel controlLabel = null;
            var labels = step.GetSources().Select(x => x.Source.Index).Distinct().Where(x => mLabels.ContainsKey(x)).ToHashSet();
            if (labels.Count > 0)
            {
                var usings = labels.Where(x => !mApplyLabels.Contains(x)).Select(x => mLabels[x]).ToList();
                if (usings.Count > 0)
                    controlLabel = new DefineLabel(usings);

                for (var i = 0; i < usings.Count; i++)
                    mApplyLabels.Add(usings[i].Index);
            }

            var stmt = base.Visit(conditional, step);
            if (stmt is Concatenation cc && cc.Left is DefineLabel label)
            {
                var upLabels = label.Labels.Where(x => labels.Contains(x.Index)).ToList();
                if (upLabels.Count > 0)
                {
                    if (upLabels.Count == label.Labels.Count)
                        stmt = cc.Right;
                    else
                        stmt = Concatenation.From(new DefineLabel(label.Labels.Where(x => !labels.Contains(x.Index)).ToList()), cc.Right);

                    controlLabel = new DefineLabel(upLabels);
                }
            }

            if (controlLabel != null)
            {
                var labelStmt = Concatenation.From(controlLabel, stmt) as Concatenation;
                for (var i = 0; i < controlLabel.Labels.Count; i++)
                    mLabelControls[controlLabel.Labels[i].Index] = new LabelControl(conditional, stmt, labelStmt, step);

                stmt = labelStmt;
            }

            return stmt;
        }

        protected override IAstNode VisitConcatenation(IConditional conditional, FlowStepConcatenation<TVertex, TEdge> step)
        {
            var left = step.Left.Visit(conditional, this);
            var right = step.Right.Visit(null, this);

            if (left.AstNodeType == AstNodeType.Empty)
                return right;

            if (right.AstNodeType == AstNodeType.Empty)
                return left;

            return new Concatenation(left, right);
        }

        protected override IAstNode VisitNext(IConditional conditional, FlowStepNext<TVertex, TEdge> step)
        {
            // 调用外部生成文法
            var stmt = mCtor.CreateStatement(step.Edge, conditional) ?? new Empty();
            // 检查生成的文法是否为多条件
            var brachStruct = GetBrachStruct(step.GetSources().First(), stmt, false);
            // 得到条件表达
            if (brachStruct?.Conditional is Logic binary && binary.Type == LogicType.Or) 
            {
                // 如果是多条件则进行额外的分支合并
                (brachStruct.AstNodes[brachStruct.FrontCount] as IObstructive).Condition = 
                   MergeSource(conditional, brachStruct.Conditional, step.GetSources().ToArray(), true);
            }

            return stmt;
        }

        protected override IAstNode VisitDownward(IConditional conditional, FlowStepDownward<TVertex, TEdge> step)
        {
            // 在downward中确定goto|break
            var stmt = VisitNext(conditional, step);

            // 通过step.Edge.Target.Index判断向下转跳到的位置是否是当前循环结束位置
            // 1.如果当前不存在循环则代表是goto
            // 2.否则创建后期处理文法
            if (mLoopStacks.Count == 0)
            {
                if (mCtor.CreateFlags.HasFlag(AstCreateFlags.LimitGotoBack))
                    throw new NotImplementedException();

                stmt = new Concatenation(stmt, new Goto(mLabels[step.Edge.Target.Index]));
            }
            else
            {
                // 步骤2
                // Downward一定是循环退出点，在这里需要判断是否使用了switch 阻挡了break退出
                // 如果是不支持Goto的输出则需要在引用到该位置的地方标记不可使用switch
                if (!mCtor.CreateFlags.HasFlag(AstCreateFlags.LimitBreak))
                {
                    if (mForkStacks.Count == 0)
                    {
                        stmt = new Concatenation(stmt, new Break());
                    }
                    else
                    {
                        var sources = mLoopStacks.Peek().GetSources().Select(x => x.Source.Index).Distinct().ToHashSet();
                        var targets = mLoopStacks.Peek().GetTargets().Select(x => x.Target.Index).Distinct().Where(x => !sources.Contains(x)).ToArray();
                        if (targets.Length != 1)
                            throw new GraphException<TVertex, TEdge>($"无法确定循环结尾。", mLoopStacks.Peek().GetTargets().ToArray());

                        stmt = new Goto(mLabels[targets[0]]); // new ControlConcatenation(stmt, new ControlGoto(mLabels[targets[0]]));
                    }
                }
                else
                {
                    if (mCtor.CreateFlags.HasFlag(AstCreateFlags.LimitGotoBack))
                        throw new NotImplementedException();

                    stmt = new Concatenation(stmt, new Goto(mLabels[step.Edge.Target.Index]));
                }
            }

            return stmt;
        }

        protected override IAstNode VisitUpward(IConditional conditional, FlowStepUpward<TVertex, TEdge> step)
        {
            // 在upward中确定goto|continue|break
            var stmt = VisitNext(conditional, step);

            // 通过step.Edge.Target.Index判断向上转跳到的位置是否是当前循环结束位置
            // 1.如果当前不存在循环则代表是goto
            // 2.如果是当前循环结束位置则代表是break,否则是goto
            // 3.如果step.Edge.Target.Index是循环开头则代表是continue
            if (mLoopStacks.Count == 0)
            {
                if (mCtor.CreateFlags.HasFlag(AstCreateFlags.LimitGotoFront))
                    throw new NotImplementedException();

                stmt = new Concatenation(stmt, new Goto(mLabels[step.Edge.Target.Index]));
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
                    if (!mCtor.CreateFlags.HasFlag(AstCreateFlags.LimitContinue))
                    {
                        stmt = new Concatenation(stmt, new Continue());
                    }
                    else
                    {
                        if (mCtor.CreateFlags.HasFlag(AstCreateFlags.LimitGotoFront))
                            throw new NotImplementedException();

                        stmt = new Concatenation(stmt, new Goto(mLabels[step.Edge.Target.Index]));
                    }
                }
            }

            return stmt;
        }

        protected override IAstNode VisitFork(IConditional conditional, FlowStepFork<TVertex, TEdge> step)
        {
            // 首先创建所有文法
            var items = new List<(BranchStruct Branch, IAstNode Statement, IAstNode Front)>();
            var emptys = new List<TEdge>();
            for (var i = 0; i < step.Steps.Count; i++)
            {
                var stmt = step.Steps[i].Visit(conditional, this);
                // 如果存在空文法(无后续文法也无判断条件)可忽略退出条件
                if (stmt.AstNodeType == AstNodeType.Empty)
                    continue;

                // 如果这一步已经是分支则直接加入到集合中
                if (step.Steps[i].StepType == FlowStepType.Fork)
                {
                    items.Add((new BranchStruct(stmt.GetExpand().FirstOrDefault(x => x is IConditional) as IConditional, -1, null), stmt, null));
                    continue;
                }

                // 在检出中断表达
                var edges = step.Steps[i].GetSources().ToArray();
                if (edges.Length > 1)
                    throw new GraphException<TVertex, TEdge>("发生内部错误。", edges);

                var brachStruct = GetBrachStruct(edges[0], stmt, true);
                // 对fork内case调用的第一个函数进行参数添加
                if (brachStruct.Conditional != null)
                {
                    var call = brachStruct.AstNodes[0].Walk().FirstOrDefault(x => x is Call call && call.IsMutableFormals) as Call;
                    if (call != null)
                        call.Formals.Add(brachStruct.Conditional.GetDeepSource());

                    // 得到条件表达
                    var branch = MergeSource(conditional, brachStruct.Conditional, step.Steps[i].GetSources().ToArray(), false);
                    // 添加到后续处理中
                    items.Add((new BranchStruct(branch, brachStruct), stmt, null));
                }
                else 
                {
                    // 添加到空转跳 
                    items.Add((brachStruct, stmt, null));
                    emptys.AddRange(edges);
                }
            }

            // 判断是否存在多个空转跳
            if (items.Count(x => x.Branch.Conditional == null) > 1)
                throw new GraphException<TVertex, TEdge>("存在多个空转跳，无法生成分支，检查图是否生成合法。", emptys);

            // 根据条件分支对文法进行分组
            var nodes = new List<(IAstNode Node, Branch Branch)>();
            var groups = items.OrderByDescending(x => x.Branch.Conditional?.ConditionalCount ?? 0).GroupBy(x => x.Branch.Conditional?.GetDeepSource());
            foreach (var group in groups)
            {
                IAstNode node = null;
                Branch branch = null;

                var groupItems = group.ToArray();
                // 查询前置文法是否相同
                var isSameFront = IsFrontStatementSame(groupItems.Select(x => x.Branch).ToArray());
                // 如果前置文法相同则在该块头部设置
                IAstNode frontStatement = null;
                if (isSameFront)
                {
                    frontStatement = groupItems[0].Branch.AstNodes[0];
                    for (var i = 1; i < groupItems[0].Branch.FrontCount; i++)
                        frontStatement = new Concatenation(frontStatement, groupItems[0].Branch.AstNodes[i]);
                }

                // 重设后续文法
                for (var i = 0; i < groupItems.Length; i++)
                {
                    var groupItem = groupItems[i];
                    var startIndex = groupItems[i].Branch.FrontCount + 1;
                    if (startIndex < groupItem.Branch.AstNodes?.Length)
                    {
                        var first = groupItem.Branch.AstNodes[startIndex];
                        for (var x = startIndex + 1; x < groupItem.Branch.AstNodes.Length; x++)
                            first = new Concatenation(first, groupItem.Branch.AstNodes[x]);

                        var front = startIndex == 0 ? null : groupItem.Branch.AstNodes[0];
                        for (var x = 0; x < startIndex; x++)
                            front = new Concatenation(first, groupItem.Branch.AstNodes[x]);

                        groupItems[i].Statement = first;
                        groupItems[i].Front = front;
                    }
                    else 
                    {
                        groupItems[i].Statement = new Empty();
                    }
                }

                var conditionCount = groupItems.Sum(x => x.Branch.Conditional?.ConditionalCount ?? 0);
                if (!mCtor.CreateFlags.HasFlag(AstCreateFlags.LimitIf) && conditionCount < mCtor.OverBranchCountToSwitch)
                {
                    var sourceCondition = MergeSource(groupItems.Select(x => x.Branch.Conditional));
                    var sourceDeep = sourceCondition?.GetDeepSource();

                    var orderItems = groupItems.OrderByDescending(x => x.Branch.Conditional == sourceCondition).ToArray();

#if DEBUG

                    if (orderItems[0].Branch.Conditional != sourceCondition)
                        throw new NotImplementedException("使用OrderByDescending已经将sourceCondition放置在orderItems最顶端，如果顶端不是sourceCondition需要检查BUG。");

#endif

                    var firstCondition = sourceCondition != orderItems[^1].Branch.Conditional && sourceDeep != null ?
                        new Logic(sourceDeep, orderItems[^1].Branch.Conditional.Right, LogicType.Equal) :
                        sourceCondition;

                    branch = new If(firstCondition, orderItems[^1].Statement);
                    for (var i = orderItems.Length - 2; i >= 0; i--)
                    {
#if DEBUG
                        if (sourceDeep == null)
                            throw new NotImplementedException("通过以上步骤后，进入到这里时一定会存在条件表达式，如果sourceDeep为null请检查其他代码块是否出现BUG。");
#endif

                        var ifCondition = i == 0 ?
                            sourceCondition :
                            new Logic(sourceDeep, orderItems[i].Branch.Conditional.Right, LogicType.Equal);

                        branch = new IfElse(ifCondition, orderItems[i].Statement, branch);
                    }
                }
                else
                {
                    mForkStacks.Push(step);
                    var condition = MergeSource(groupItems.Select(x => x.Branch.Conditional));
                    var cases = new List<SwitchCase>();
                    for (var i = 0; i < groupItems.Length; i++)
                    {
                        var item = groupItems[i];
                        var stmt = item.Statement;
                        if (stmt.Ends().Any(x =>
                            x.AstNodeType != AstNodeType.Goto &&
                            x.AstNodeType != AstNodeType.Continue &&
                            x.AstNodeType != AstNodeType.Return))
                            stmt = new Concatenation(stmt, new Break());

                        var cond = item.Branch.Conditional;
                        // 如果条件是binary则展开
                        if (cond is Logic binary)
                        {
                            foreach (IConditional binaryCond in binary.Expand(LogicType.Or))
                            {
                                cases.Add(binaryCond.Right == null ?
                                    new SwitchDefault(stmt) :
                                    new SwitchCase(binaryCond.Right, stmt));
                            }
                        }
                        else
                        {
                            cases.Add(cond.Right == null ?
                                new SwitchDefault(stmt) :
                                new SwitchCase(cond.Right, stmt));
                        }
                    }

                    if (!cases.Any(x => x is SwitchDefault))
                    {
                        var stmt = groupItems[^1].Branch.Reason;
                        if (stmt.Ends().Any(x =>
                            x.AstNodeType != AstNodeType.Goto &&
                            x.AstNodeType != AstNodeType.Continue &&
                            x.AstNodeType != AstNodeType.Return))
                                stmt = new Concatenation(stmt, new Break());

                        cases.Add(new SwitchDefault(stmt));
                    }

                    mForkStacks.Pop();
                    branch = new Switch(new Value(condition.Source), cases);
                }

                // 合并前置文法
                if (frontStatement != null)
                    node = new Concatenation(frontStatement, branch);
                else
                    node = branch;

                nodes.Add((node, branch));
            }

            IAstNode result = null;

            // 合并块
            if (nodes.Count > 1)
            {
                // 根据条件数量排序
                var sortNodes = nodes.OrderBy(x => x.Branch.Condition?.ConditionalCount ?? 0).ToArray();
                var blocks = new List<(IConditional Condition, IAstNode Statement)>();
                for (var i = 0; i < sortNodes.Length; i++)
                {
                    var node = sortNodes[i];
                    // 如果结尾是if
                    if (node.Node is If || ((node.Node as IfElse)?.FindLastAlternate() is If))
                    {
                        if (node.Node is If @if)
                        {
                            blocks.Add((@if.Condition, @if.Consequent));
                        }
                        else
                        {
                            var ifelse = node.Node as IfElse;
                            while (ifelse != null)
                            {
                                blocks.Add((ifelse.Condition, ifelse.Consequent));
                                ifelse = ifelse.Alternate as IfElse;

                                if (ifelse == null && ifelse.Alternate != null)
                                {
                                    blocks.Add((null, ifelse.Alternate));
                                }
                            }
                        }
                    }
                    else
                    {
                        blocks.Add((node.Branch.Condition, node.Node));
                    }
                }

                // 创建退出文法块
                bool createReason = items[^1].Branch.Reason != null;
                if (!blocks.Any(x => x.Condition == null) && items[^1].Branch.Reason != null)
                    blocks.Add((null, items[^1].Branch.Reason));

                // 检测空分支条件数量
                if (blocks.Count(x => x.Condition == null) > 1)
                    throw new GraphException<TVertex, TEdge>("存在多个空分支，无法确定空分支源。", step.GetSources().ToArray());

                IAstNode CheckSwitch(IConditional condition, IAstNode node) 
                {
                    if (node is Switch @switch)
                        @switch.Condition = new Value(condition.GetDeepSource(true));

                    return node;
                }

                var sortBlocks = blocks.OrderBy(x => x.Condition == null ? int.MaxValue / 2 : x.Condition.ConditionalCount).ToArray();
                IAstNode branch = sortBlocks[^1].Condition == null ? sortBlocks[^1].Statement : new If(sortBlocks[^1].Condition, CheckSwitch(sortBlocks[^1].Condition, sortBlocks[^1].Statement));
                for (var i = sortBlocks.Length - 2; i >= 0; i--)
                {
                    if (sortBlocks[i].Condition == null)
                        throw new NotImplementedException();

                    branch = new IfElse(sortBlocks[i].Condition, CheckSwitch(sortBlocks[i].Condition, sortBlocks[i].Statement), branch);
                }

                result = branch;
            }
            else result = nodes[0].Node;

            return result;
        }

        protected override IAstNode VisitLoop(IConditional conditional, FlowStepLoop<TVertex, TEdge> step)
        {
            mLoopStacks.Push(step);

            // 得到文法头部所有边
            var first = step.Steps[0];
            var edges = first.GetSources().ToArray();

            // 检查下一步
            if (first.StepType == FlowStepType.Loop)
                throw new GraphException<TVertex, TEdge>(
                    $"循环步内不会紧接一个循环步，可能是内部逻辑异常导致。", edges);

            // 单纯的cfg控制流只会创建while文法，do..while与for循环需要在后期修改，需要借助IR信息
            // 文法头部所有可能性,if|if..else|switch|next
            var conds = new IConditional[edges.Length];
            for (var i = 0; i < edges.Length; i++)
                conds[i] = mCtor.CreateObstructiveBranch(edges[i], conditional);

            // 判断循环中是否存在空循环
            var emptyConds = conds.Select((x, i) => (x, i)).Where(x => x.x.Right == null).ToArray();
            if (emptyConds.Length > 0)
            {
                if (emptyConds.Length > 1 || !conds.Any(x => x.Right != null))
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
                var isSameCond = conds.GroupBy(x => new { Source = x.Source, Value = x.Right }).Count() == 1;
                var condition = isSameCond ? conds[0] : throw new NotImplementedException(); //mCtor.CreateObstructiveBranch(edges[0]);

                var stmt = step.Steps[0].Visit(condition, this);
                for (var i = 1; i < step.Steps.Count; i++)
                    stmt = new Concatenation(stmt, step.Steps[i].Visit(null, this));

                if (stmt is Concatenation whileCC)
                {
                    var whileCCLast = whileCC.GetLast();
                    if (whileCCLast.AstNodeType == AstNodeType.Continue)
                    {
                        IAstNode ResetWhileLast(Concatenation ccStmt)
                        {
                            var right = ccStmt.Right;
                            if (right is Concatenation ccRight)
                                right = ResetWhileLast(ccRight);
                            else if (right.AstNodeType == AstNodeType.Continue)
                                return ccStmt.Left;

                            return new Concatenation(ccStmt.Left, right);
                        }

                        stmt = ResetWhileLast(whileCC);
                    }
                }

                IAstNode whileStmt = false ? // conditional != null ?
                    new DoWhile(condition, stmt) as IAstNode :
                    new While(condition, stmt);

                stmt = new Concatenation(whileStmt, mCtor.CreateWaitOne(edges[0]));
                mLoopStacks.Pop();
                return stmt;
            }
            else
            {
                var emptyCond = emptyConds.First();
                var firstIndex = emptyCond.i == 0 ? 1 : 0;
                var isSameCond = conds.Select((x, i) => (x, i)).Where(x => x.i != emptyCond.i).GroupBy(x => new { Source = x.x.Source, Value = x.x.Right }).Count() == 1;
                var condition = isSameCond ? conds[firstIndex] : throw new NotImplementedException(); //mCtor.CreateObstructiveBranch(edges[0]);

                var stmt = step.Steps[firstIndex].Visit(condition, this);
                for (var i = firstIndex + 1; i < step.Steps.Count; i++)
                    if (i != emptyCond.i)
                        stmt = new Concatenation(stmt, step.Steps[i].Visit(null, this));

                stmt = new Concatenation(new DoWhile(condition, stmt), mCtor.CreateWaitOne(edges[0]));

                var emptyStmt = step.Steps[emptyCond.i].Visit(null, this);
                if (emptyStmt == null || emptyStmt is Empty)
                    stmt = new If(condition, stmt);
                else
                    stmt = new IfElse(condition, stmt, emptyStmt);

                return stmt;
            }
        }

        private IConditional MergeSource(IEnumerable<IConditional> conditionals) 
        {
            int GetOrderValue(IAstNode astNode) 
            {
                if (astNode is SourceList sl)
                    return sl.Count;

                return 1;
            }

            // 首先找到不同项
            var diff = conditionals.Where(x => x != null).Select(x => new { Condition = x, OrderValue = GetOrderValue(x.GetDeepSource(false, true)) }).ToArray();
            if (diff.Length > 1)
                return diff.OrderByDescending(x => x.OrderValue).First().Condition;

            return conditionals.First();
        }

        private IConditional MergeSource(IConditional first, IConditional condition, TEdge[] edges, bool reverse)
        {
            // 对条件表达源进行相同合并
            var conditions = new List<IConditional>();
            var illegal = mCtor.GetIllegalValue(edges);
            if (reverse && illegal != null)
            {
                return new Logic(condition.GetDeepSource(), illegal, LogicType.NotEqual);
            }
            else 
            {
                if (condition is Logic binary && binary.Type == LogicType.Or)
                {
                    foreach (var binaryCondition in binary.Expand(LogicType.Or))
                        if (binaryCondition is IConditional cal)
                            conditions.Add(cal);
                        else if (binaryCondition is Parenthese parenthese && parenthese.Node is IConditional parentConditional)
                            conditions.Add(new ParentheseConditional(parentConditional));
                        else
                            throw new GraphException<TVertex, TEdge>($"创建分支条件时产生意外，以下边无法生成有效的分支文法。", edges);
                }
                else conditions.Add(condition);

                // 重新设置判断源
                condition = first ?? conditions[0];
                for (var x = first != null ? 0 : 1; x < conditions.Count; x++)
                {
                    var followCondition = condition.GetDeepSource() == conditions[x].GetDeepSource() ?
                         new Logic(conditions[x].GetDeepSource(), conditions[x].Right, LogicType.Equal) :
                         conditions[x];

                    condition = new Logic(condition, followCondition, LogicType.Or);
                }

                return condition;
            }
        }

        private BranchStruct GetBrachStruct(TEdge edge, IAstNode stmt, bool inFork)
        {
            if (stmt.AstNodeType == AstNodeType.Obstructive)
                return new BranchStruct(stmt as IObstructive);

            var childrens = stmt.GetExpand().ToArray();
            var obsIndex = -1;
            for (var i = 0; i < childrens.Length; i++)
                if (childrens[i] is IObstructive)
                {
                    obsIndex = i;
                    break;
                }

            //
            if (obsIndex == -1 && !inFork)
                return null;

            // 如果不存在中断文法则创建
            if (obsIndex == -1)
            {
                var condition = mCtor.CreateObstructiveBranch(edge, null);
                // 查找连续call
                if (stmt is IObstructive obs)
                    // 如果创建的分支使用了与stmt相同的步则忽略该步骤
                    // 需要一个合理的机制判断它是否存在引用
                    // 如果存在引用则保留该步骤并将值右侧如b(a=b)赋值为condition中使用到的右侧值
                    // 如果不存在引用则设置obsIndex=0直接移除该文法 
                    if (obs.Source.Equals(condition.Source))
                    {
                        obsIndex = 0;
                        throw new NotImplementedException();
                    }

                return new BranchStruct(condition, obsIndex, new IAstNode[] { stmt });
            }

            return new BranchStruct(childrens[obsIndex] as IObstructive, obsIndex, childrens);
        }

        private bool IsFrontStatementSame(BranchStruct[] branchs)
        {
            if (branchs.Length < 2)
                return false;

            var sameLength = branchs.Select(x => x.FrontCount).Distinct().ToArray();
            if (sameLength.Length == 1)
            {
                var length = sameLength[0];
                var same = true;
                for (var i = 0; i < length; i++)
                {
                    var first = branchs[0].AstNodes[i];
                    for (var x = 1; x < branchs.Length; x++)
                    {
                        if (!branchs[i].AstNodes[i].Equals(first))
                        {
                            same = false;
                            break;
                        }
                    }

                    if (!same) break;
                }

                return same && branchs[0].FrontCount > 0;
            }

            return false;
        }

        class BranchStruct 
        {
            public BranchStruct(IObstructive obstructive)
            {
                Conditional = obstructive.Condition;
                Reason = obstructive.Reason;
                FrontCount = 0;
                AstNodes = new IAstNode[] { obstructive };
            }

            public BranchStruct(IObstructive obstructive, int frontStmtIndex, IAstNode[] astNodes)
            {
                Conditional = obstructive?.Condition;
                Reason = obstructive?.Reason;
                FrontCount = frontStmtIndex;
                AstNodes = astNodes;
            }

            public BranchStruct(IConditional conditional, int frontStmtIndex, IAstNode[] astNodes)
            {
                Conditional = conditional;
                Reason = null;
                FrontCount = frontStmtIndex;
                AstNodes = astNodes;
            }

            public BranchStruct(IConditional conditional, BranchStruct origin)
            {
                Conditional = conditional;
                Reason = origin.Reason;
                FrontCount = origin.FrontCount;
                AstNodes = origin.AstNodes;
            }

            public IConditional Conditional { get; }

            public IAstNode Reason { get; }

            public int FrontCount { get; }

            public IAstNode[] AstNodes { get; }
        }

        class ParentheseConditional : Parenthese, IConditional
        {
            public ParentheseConditional(IConditional condition)
                : base(condition)
            {
            }

            public IConditional Condition => Node as IConditional;

            public IAstNode Left
            {
                get { return Condition.Left; }
                set { Condition.Left = value; }
            }

            public IAstNode Right => Condition.Right;

            public bool CanMerge => Condition.CanMerge;

            public override AstNodeType AstNodeType => AstNodeType.Parenthese;
        }
    }
}
