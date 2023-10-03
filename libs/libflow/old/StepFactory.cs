using libgraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace libcfg
{
    public static class StepFactory
    {
        public static IList<GraphStep<TVertex, TEdge>> GetSteps<TVertex, TEdge>(this IGraph<TVertex, TEdge> graph)
            where TEdge : IEdge<TVertex>
            where TVertex : IVertex
        {
            // 整理所有边，并根据源节点和目标节点进行分组
            var sourceEdges = graph.Edges.GroupBy(x => x.Source.Index).ToDictionary(x => x.Key, x => x.ToArray());
            var targetEdges = graph.Edges.GroupBy(x => x.Target.Index).ToDictionary(x => x.Key, x => x.ToArray());

            // 存储流线的起始节点，终止节点和分叉节点
            var lineStartMaps = new Dictionary<ushort, FlowLine<TVertex, TEdge>[]>();
            var lineForkMaps = new Dictionary<ushort, IList<ForkLine<TVertex, TEdge>>>();

            // 获取一个状态（在这里是节点）的所有终止节点和分叉节点
            IList<FlowLine<TVertex, TEdge>> GetTargets(ushort state) => lineStartMaps.ContainsKey(state) ? lineStartMaps[state] : new FlowLine<TVertex, TEdge>[0];
            IList<ForkLine<TVertex, TEdge>> GetForks(ushort state) => lineForkMaps.ContainsKey(state) ? lineForkMaps[state] : new ForkLine<TVertex, TEdge>[0];

            // 对入口点（entry）和一些有特殊标记的边进行处理
            // 生成起始节点列表（entries）
            // 使用BFS对图的每一层顺序遍历从而找到入口点
            var maxState = graph.VerticesCount + 1;
            var entries = new List<ushort>();
            var entryVisitor = new bool[maxState];

            // 搜索以 state 为起点的所有节点，包括 state 节点本身
            void BFSFindSubset(ushort state)
            {
                // 如果已经访问过，直接返回
                if (entryVisitor[state])
                    return;

                // 创建队列，用于存放待访问节点
                var entryQueue = new Queue<ushort>();
                entryQueue.Enqueue(state);

                // 如果该入口点未被访问过，则将当前节点加入结果集合
                entries.Add(state);

                // 开始查找子图
                while (entryQueue.Count > 0)
                {
                    // 取出队列的头部节点，作为当前节点访问
                    var entry = entryQueue.Dequeue();

                    // 如果已经访问过，跳过此次循环
                    if (entryVisitor[entry])
                        continue;

                    // 标记为已访问
                    entryVisitor[entry] = true;

                    // 如果存在从当前节点出发的有向边
                    if (sourceEdges.ContainsKey(entry))
                    {
                        // 获取该边集，并遍历其中的所有边
                        var rights = sourceEdges[entry];

                        for (var i = 0; i < rights.Length; i++)
                        {
                            var right = rights[i];

                            // 如果该边指向的节点有子集节点，则递归访问其子集节点
                            if (right.Subset != null)
                                BFSFindSubset(right.Subset.Index);

                            // 将该边指向的节点加入待访问队列
                            entryQueue.Enqueue(right.Target.Index);
                        }
                    }
                }
            }

            // 从状态1开始查找入口点
            BFSFindSubset(1);

            // 查找未使用的入口点
            foreach (var edge in graph.Edges.Where(x => !targetEdges.ContainsKey(x.Source.Index)))
                BFSFindSubset(edge.Source.Index);

            // 生成终止节点列表（points）
            bool IsPoint(ushort state) => //state != 0 &&
                sourceEdges.TryGetValue(state, out var sEdges) && sEdges.Length > 1 ||
                targetEdges.TryGetValue(state, out var tEdges) && tEdges.Length > 1;

            var points = graph.Vertices.Select(x => x.Index).Where(IsPoint).Union(entries).ToHashSet();

            // 生成起始节点对应的流线（lineStartMaps）和终止节点对应的流线（lineEndMaps）
            foreach (var point in points)
            {
                if (!sourceEdges.ContainsKey(point))
                    continue;

                var edges = sourceEdges[point];
                var lines = edges.Select(x => new FlowLine<TVertex, TEdge>(x)).ToArray();

                lineStartMaps[point] = lines;

                // 进行路径爬取
                for (var i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    while (!points.Contains(line.End.Target.Index))
                    {
                        // 如果目标到了结尾时退出查找
                        if (!sourceEdges.ContainsKey(line.End.Target.Index))
                            break;

                        var right = sourceEdges[line.End.Target.Index];
                        Debug.Assert(right.Length == 1);
                        line.Add(right[0]);
                    }
                }
            }

            // 组建forkline,通过来源一致,归点一致得到
            // TODO:思考是否需要构建包含关系比如1-2-3和1-3是一组,然后1-4和(1-2-|1-)3-4是否为一组呢?
            // 因为上述BFS爬取算法是根据终结点进行的判断,而3的左侧包含2和1是一个终结点,所以在BFS部分已经将1-2-3和1-3设置为停止部分,不会导致1-4和(1-2-|1-)3-4设置为一组
            // 如果后续算法不需要考虑间接子孙关系则不需要考虑包含关系,而这里进行forkline划分可以只是为了加速后续算法访问速度
            var loops = new List<LoopLine<TVertex, TEdge>>();
            var loopQueue = new Queue<LoopLine<TVertex, TEdge>>();
            var loopsVisitor = new HashSet<ForkLine<TVertex, TEdge>>();
            foreach (var lines in lineStartMaps.Values.SelectMany(x => x).GroupBy(x => new { Start = x.Start.Source.Index, End = x.End.Target.Index }))
            {
                if (!lineForkMaps.ContainsKey(lines.Key.Start))
                    lineForkMaps[lines.Key.Start] = new List<ForkLine<TVertex, TEdge>>();

                var forkLine = new ForkLine<TVertex, TEdge>(lines.Key.Start, lines.Key.End);
                forkLine.AddRange(lines);
                lineForkMaps[lines.Key.Start].Add(forkLine);

                if (forkLine.Source == forkLine.Target)
                {
                    loops.Add(new LoopLine<TVertex, TEdge>() { forkLine });
                    loopsVisitor.Add(forkLine);
                }
            }

            // 查找所有循环，创建一个 Stack 对象，用于存储转换后的 LoopLine 对象
            // 遍历所有步骤分支入口
            for (var entryIndex = 0; entryIndex < entries.Count; entryIndex++)
            {
                // 获取当前步骤分支入口指向的所有步骤分支线
                var currentForks = lineForkMaps[entries[entryIndex]];
                // 遍历当前入口指向的所有步骤分支线
                for (var lineIndex = 0; lineIndex < currentForks.Count; lineIndex++)
                {
                    // 创建当前步骤分支线对应的 LoopLine 对象,并压入栈给后续算法使用
                    var currentLoop = new LoopLine<TVertex, TEdge>();
                    var currentFork = currentForks[lineIndex];
                    if (loopsVisitor.Contains(currentFork))
                        continue;

                    currentLoop.Add(currentFork);
                    loopQueue.Enqueue(currentLoop);

                    var visitForkLines = new HashSet<ForkLine<TVertex, TEdge>>();
                    visitForkLines.Add(currentFork);

                    while (loopQueue.Count > 0)
                    {
                        currentLoop = loopQueue.Dequeue();
                        currentFork = currentLoop[^1];
                        var currentForkRights = GetForks(currentFork.Target);

                        if (currentForkRights.Count > 1)
                        {
                            // 如果存在多个分叉线，则将其分裂成多个循环
                            for (var i = 0; i < currentForkRights.Count; i++)
                            {
                                var nextLoop = new LoopLine<TVertex, TEdge>(currentLoop);
                                var right = currentForkRights[i];
                                if (!loopsVisitor.Contains(right))
                                {
                                    nextLoop.Add(right);

                                    if (!nextLoop.CheckLoop())
                                        loopQueue.Enqueue(nextLoop);
                                    else
                                        loops.Add(nextLoop);

                                    visitForkLines.Add(right);
                                }
                            }
                        }
                        else if (currentForkRights.Count == 1)
                        {
                            var right = currentForkRights[0];
                            if (!loopsVisitor.Contains(right))
                            {
                                currentLoop.Add(right);
                                if (currentLoop.CheckLoop())
                                    loops.Add(currentLoop);
                                else
                                    loopQueue.Enqueue(currentLoop);

                                visitForkLines.Add(right);
                            }
                        }
                    }

                    foreach (var forkLine in visitForkLines)
                        loopsVisitor.Add(forkLine);
                }
            }

            // 1.1判断循环交集，以此来取消构建循环,必须保证loops尾部一定是大循环才能确保该段代码正确
            // 最快的信息处理方式是，隐式数据（不需要处理）
            // TODO:需要更多测试,1-2交叉3-1交叉,那么在i==3时无法和1进行交叉判断,产生错误
            // 在后续代码2.1中实现实现了一个用于在包含循环的图结构中寻找循环的唯一父级节点的算法
            var sameLoops = new Dictionary<int, HashSet<LoopLine<TVertex, TEdge>>>();
            var relationEdges = new List<(LoopLine<TVertex, TEdge>, LoopLine<TVertex, TEdge>)>();
            var combinations = GetCombinations(loops.OrderByDescending(x => x.Count).ToArray());
            for (var i = 0; i < combinations.Count; i++)
            {
                var combination = combinations[i];
                var otherLoop = combination.Item1;
                var currentLoop = combination.Item2;
                var checkCount = 0;
                for (var y = 0; y < currentLoop.Count; y++)
                    if (otherLoop.Contains(currentLoop[y]))
                        checkCount++;
                    else if (currentLoop[y].Source == currentLoop[y].Target)
                        if (otherLoop.Any(x => x.Source == currentLoop[y].Source))
                            checkCount++;

                // 如果存在相交边则进入判断，否则它们是两个互不相干的循环
                if (checkCount > 0)
                {
                    // 检查是否被另一条边完全包含
                    if (checkCount == currentLoop.Count - 1 + currentLoop.Count(x => x.Source == x.Target))
                    {
                        // 如果长度一致代表这两个循环是从不同状态点进入但是同一个循环，则应移除一个
                        if (otherLoop.Count == currentLoop.Count)
                        {
                            var nums = otherLoop.OrderBy(x => x.Source).Select(x => x.Source).ToArray();
                            var hash = nums[0].GetHashCode();
                            for (var y = 1; y < nums.Length; y++)
                                hash = HashCode.Combine(nums[y]);

                            if (!sameLoops.ContainsKey(hash))
                                sameLoops[hash] = new HashSet<LoopLine<TVertex, TEdge>>();

                            sameLoops[hash].Add(otherLoop);
                            sameLoops[hash].Add(currentLoop);
                        }
                        // 如果长度不同则设置它们之间的包含关系
                        else
                        {
                            relationEdges.Add((currentLoop, otherLoop));
                        }
                    }
                    else
                    {
                        // 如果没有被完全包含在另一个循环内则代表是交叉循环
                        // 这里一定存在交集，将循环设置为跳跃线（IsJump = true）
                        // 并将其中的流线设置为上行线（LineType = StepLineType.Up）
                        currentLoop.IsJump = true;
                        foreach (FlowLine<TVertex, TEdge> flowLine in currentLoop[^1])
                            flowLine.LineType = StepLineType.Up;
                    }
                }
            }

            // 1.2根据循环重新整理流线
            // 1.将循环形成一个ForkLine，从并连接退出线(Up)
            // 2.将之前整理的ForkLine所有进入循环线的组成一组新的ForkLine
            // 3.如果大于1条ForkLine进入循环线查找到进入点最短路径
            // 4.保持最短路径循环,并移除其他循环
            // 5.其他长路径进入点，添加步至最短循环入口点
            foreach (var group in sameLoops) 
            {
                // 整理所有循环入口点
                var sameLoopEntries = new bool[entryVisitor.Length];
                foreach (var entry in group.Value.Select(x => x[^1].Target))
                    sameLoopEntries[entry] = true;

                // 查找最短路径循环(此最短代表其他入口点需要额外移进到目标循环的最短步数)
                // 所以在这里不需要处理相同长度情况，如果出现相同长度，则使用任意同一长度其他循环需要移进的步骤都是一致的
                LoopLine<TVertex, TEdge> shortLoop = null;
                var shortLength = int.MinValue;
                foreach (var loop in group.Value) 
                {
                    // 向左查找另一个入口点
                    for(var i = 0; i < loop.Count; i++) 
                    {
                        var line = loop[i];
                        if (sameLoopEntries[line.Source]) 
                        {
                            // 如果该点是入口点则记录信息
                            if (i > shortLength)
                            {
                                shortLength = i;
                                shortLoop = loop;
                            }
                        }
                    }
                }

                // 对除最短循环外的循环添加移进步骤
                foreach (var loop in group.Value)
                {
                    if (loop != shortLoop) 
                    {
                        var loopStart = loop[0];
                        // 移动flowline,首先获得进入循环的ForkLines
                        var loopRequests = lineForkMaps.SelectMany(x => x.Value).Where(x => x.Target == loop[0].Source && !loop.Contains(x)).ToArray();
                        foreach (var loopRequest in loopRequests)
                        {
                            var forkLineMap = lineForkMaps[loopRequest.Source];

                            // 移除该forkline并指定新的位置，以避免被后续错误使用
                            forkLineMap.Remove(loopRequest);

                            // 因为存在移进，所以一定需要改变目标点
                            var targetLine = new ForkLine<TVertex, TEdge>(loopRequest.Source, (ushort)maxState++);
                            for (var i = 0; i < loopRequest.Count; i++)
                            {
                                var flowLine = loopRequest[i] as FlowLine<TVertex, TEdge>;
                                targetLine.Add(new FlowLine<TVertex, TEdge>(targetLine.Source, targetLine.Target, flowLine));
                            }
                            forkLineMap.Add(targetLine);

                            // 循环移进,一直循环右侧访问并添加，直到遇到循环点则退出移进
                            var source = targetLine.Target;
                            for (var i = 0; i < loop.Count; i++)
                            {
                                var line = loop[i];
                                var toShort = sameLoopEntries[line.Target];
                                var target = toShort ? shortLoop.Source : (ushort)maxState++;

                                if (!lineForkMaps.ContainsKey(source))
                                    lineForkMaps[source] = new List<ForkLine<TVertex, TEdge>>();

                                var shiftLine = new ForkLine<TVertex, TEdge>(source, target);
                                for (var x = 0; x < line.Count; x++)
                                {
                                    var flowLine = line[x] as FlowLine<TVertex, TEdge>;
                                    shiftLine.Add(new FlowLine<TVertex, TEdge>(shiftLine.Source, shiftLine.Target, flowLine));
                                }
                                lineForkMaps[source].Add(shiftLine);

                                if (toShort)
                                    break;

                                source = target;
                            }
                        }

                        // 移除该循环
                        loops.Remove(loop);
                    }
                }
            }

            // 2.1:BFS遍历取得最后结尾根据长度来确定父子关系
            // 首先，定义一个字典"relationLefts"，它的键是节点编号
            // 值是该节点作为左侧指向其他节点的边所连接的右侧节点编号的数组
            // "relationEdges"是描述所有边的元组集合
            var relationLefts = relationEdges.GroupBy(x => x.Item1).ToDictionary(x => x.Key, x => x.Select(y => y.Item2).ToArray());
            for (var i = 0; i < loops.Count; i++)
            {
                var loop = loops[i];

                // 如果当前节点没有循环边，则忽略
                if (!relationLefts.ContainsKey(loop)) continue;

                var relationLines = new List<List<LoopLine<TVertex, TEdge>>>();

                // 对于该节点的所有循环边，以起始点为初始路径，进行深度优先遍历，并找到所有可能的父子路径
                foreach (var initLine in relationLefts[loop].Select(x => new List<LoopLine<TVertex, TEdge>>() { x }))
                {
                    var relationStacks = new Stack<List<LoopLine<TVertex, TEdge>>>();
                    var relationVisitor = new HashSet<LoopLine<TVertex, TEdge>>(loops.Count + 1);
                    relationStacks.Push(initLine);
                    while (relationStacks.Count > 0)
                    {
                        var relationLine = relationStacks.Pop();
                        var relationPoint = relationLine[^1]; // 取"relationLine"的最后一个节点
                        if (relationVisitor.Contains(relationPoint))
                            throw new GraphException<TVertex, TEdge>("不可能包含重复循环父级", loops[i].GetEdges().Where(x => x.Target.Index == relationPoint.Target).ToArray());

                        relationVisitor.Add(relationPoint);
                        if (relationLefts.ContainsKey(relationPoint))
                        {
                            var relationForks = relationLefts[relationPoint];
                            if (relationForks.Length == 1)
                            {
                                // 如果当前节点只有一条出边，直接将该边加入路径中，并继续遍历
                                relationLine.Add(relationForks[0]);
                                relationStacks.Push(relationLine);
                            }
                            else
                            {
                                // 如果有多条出边，则将每条出边与之前路径合并，并将合并后的路径压入栈中
                                for (var x = 0; x < relationForks.Length; x++)
                                {
                                    var newLine = new List<LoopLine<TVertex, TEdge>>(relationLine);
                                    newLine.Add(relationForks[x]);
                                    relationStacks.Push(newLine);
                                }
                            }
                        }
                        else
                        {
                            // 如果当前节点没有出边，则已找到该节点的一条完整父子路径
                            relationLines.Add(relationLine);
                        }
                    }
                }

                // 在所有可能的父子路径中，找到最长的一条（因为父子关系必须是完整的），并确定其唯一起点作为这个节点的父级节点
                var maxRelationLines = relationLines.GroupBy(x => x.Count).OrderByDescending(x => x.Key).First().Select(x => x).ToArray();
                var first = maxRelationLines[0][0];
                if (maxRelationLines.Length > 1)
                    // 如果存在多条最长路径，则必须要都起始点相同，否则表示无法确定唯一父级节点（这种情况应该属于图结构定义错误，抛出异常）
                    for (var y = 1; y < maxRelationLines.Length; y++)
                        if (first != maxRelationLines[y][0])
                        {
                            var firstEdges = first.GetEdges();
                            var followEdges = maxRelationLines[y][0].GetEdges();
                            if (firstEdges.SequenceEqual(followEdges))
                                continue;

                            var edges = new TEdge[2];
                            edges[0] = firstEdges.First();
                            edges[1] = followEdges.First();

                            var sb = new StringBuilder();
                            sb.AppendLine(CreateGraphStep(BlockLine<TVertex, TEdge>.FromLoop(first)).Visit(new StepDescrptionBuilder<TVertex, TEdge>(true)));
                            sb.AppendLine(CreateGraphStep(BlockLine<TVertex, TEdge>.FromLoop(maxRelationLines[y][0])).Visit(new StepDescrptionBuilder<TVertex, TEdge>(true)));
                            throw new GraphException<TVertex, TEdge>("无法确定嵌套循环关系,因为它们存在多个长度相等父级来源:\n" + sb.ToString().Substring(0, sb.Length - 2), edges);
                        }

                // 将此节点的唯一父级节点设置为起始节点
                loops[i].Parent = first;
            }

            // 根据loop.Parent等信息将lines构建为steplines
            // 首先根据loop.Parent将loop排序
            var loopIndexMaping = new Dictionary<LoopLine<TVertex, TEdge>, int>();

            // 初始化排序字典
            for (int i = 0; i < loops.Count; i++)
                if (!loops[i].IsJump)
                    loopIndexMaping.Add(loops[i], -1);

            // 遍历所有ForkLine，将From所对应的索引存入字典
            for (int i = 0; i < loops.Count; i++)
            {
                var loop = loops[i];
                if (!loop.IsJump)
                {
                    var parent = loop.Parent;
                    if (parent != null)
                    {
                        int index = loops.IndexOf(parent);
                        if (index >= 0)
                        {
                            loopIndexMaping[loops[i]] = index + (loops.Any(x => x.Source == loop[^1].Source && x != loop) ? 1 : 0);
                        }
                    }
                }
            }
            loops = loops.Where(x => !x.IsJump).OrderByDescending(f => loopIndexMaping[f]).ToList();

            // 得到所有未在loop中使用的forks并创建StepLine
            var nestedLoopStepLines = new Dictionary<ushort, List<IStepLine<TVertex, TEdge>>>();
            var endlessStepLines = lineForkMaps.SelectMany(x => x.Value).Except(loops.SelectMany(x => x)).GroupBy(x => x.Source).ToDictionary(x => x.Key, x => x.Cast<IStepLine<TVertex, TEdge>>().ToList());

            // 获得嵌套步骤边
            IEnumerable<IStepLine<TVertex, TEdge>> GetNestedStepLines(ushort state)
            {
                if (nestedLoopStepLines.ContainsKey(state))
                    foreach (var stepLine in nestedLoopStepLines[state])
                        yield return stepLine;

                if (endlessStepLines.ContainsKey(state))
                    foreach (var stepLine in endlessStepLines[state])
                        yield return stepLine;
            }


            // 创建loop的StepLine
            // 注意:在这一步中ForkLine将被重组,重组后的ForkLine.Target属性值将会失效,其中不同的FlowLine的End.Index可能会不一样
            // 在后续算法中继续使用ForkLine.Target将会造成致命错误
            var existStepLines = new Dictionary<ushort, IStepLine<TVertex, TEdge>>();
            var loopsExits = new Dictionary<IStepLine<TVertex, TEdge>, List<FlowLine<TVertex, TEdge>>>();
            for (var i = 0; i < loops.Count; i++)
            {
                var loop = loops[i];
                var block = new BlockLine<TVertex, TEdge>(StepLineType.Loop);

                loopsExits[block] = new List<FlowLine<TVertex, TEdge>>();

                for (var x = 0; x < loop.Count; x++)
                {
                    var stepLine = loop[x] as IStepLine<TVertex, TEdge>;
                    var resultLine = stepLine;

                    // 寻找到嵌套循环步和嵌套终结步
                    if (stepLine.Source != stepLine.Target && loop.Parent == null)
                    {
                        var nestedStepLines = GetNestedStepLines(stepLine.Source).ToArray();
                        if (nestedStepLines.Length > 0)
                        {
                            var forkLineItems = new List<IStepLine<TVertex, TEdge>>();

                            for (var y = 0; y < nestedStepLines.Length; y++)
                            {
                                var nestedStepLine = nestedStepLines[y];

                                // 如果是Fork则重新整理分支
                                if (nestedStepLine.LineType == StepLineType.Fork)
                                    forkLineItems.AddRange(nestedStepLine as IStepLineCollection<TVertex, TEdge>);
                                else
                                    forkLineItems.Add(nestedStepLine);

                                // 当处理完嵌套步时则从集合中移除
                                var stepLineCollection = nestedStepLine.LineType == StepLineType.Loop ?
                                    nestedLoopStepLines :
                                    endlessStepLines;

                                if (stepLineCollection.ContainsKey(nestedStepLine.Source) && stepLineCollection[nestedStepLine.Source].Remove(nestedStepLine))
                                    if (stepLineCollection[nestedStepLine.Source].Count == 0)
                                        stepLineCollection.Remove(nestedStepLine.Source);
                            }

                            // 设置Flow步为退出循环
                            ForkLine<TVertex, TEdge> frontLine = new ForkLine<TVertex, TEdge>(stepLine.Source, stepLine.Source);
                            for (var y = 0; y < forkLineItems.Count; y++)
                            {
                                var forkLineItem = forkLineItems[y];
                                if (forkLineItem is FlowLine<TVertex, TEdge> flowLine)
                                {
                                    flowLine.LineType = StepLineType.Down;
                                    loopsExits[block].Add(flowLine);
                                }
                                else if (forkLineItem.LineType == StepLineType.Loop) 
                                {
                                    // 重置嵌套循环内部属性
                                    var loopLine = forkLineItems[y] as BlockLine<TVertex, TEdge>;
                                    if (loopLine[^1].Source == loopLine.Source)
                                    {
                                        frontLine.Add(forkLineItem);
                                        forkLineItems.RemoveAt(y);
                                        y--;
                                        continue;
                                    }

                                    loopLine.LineType = StepLineType.Flow;
                                    foreach (FlowLine<TVertex, TEdge> loopFlowLine in loopLine[^1] as ForkLine<TVertex, TEdge>)
                                        loopFlowLine.LineType = StepLineType.Up;

                                    // 找到相同线全部移除
                                    loopLine.RemoveAll(x => loop.Contains(x));
                                    if (loopLine.Source != stepLine.Source)
                                    {
                                        forkLineItems.RemoveAt(y);
                                        y--;

                                        if (loopLine.Count > 0)
                                        {
                                            if (!endlessStepLines.ContainsKey(loopLine.Source))
                                                endlessStepLines.Add(loopLine.Source, new List<IStepLine<TVertex, TEdge>>());

                                            endlessStepLines[loopLine.Source].Add(loopLine);
                                        }
                                    }
                                }
                            }

                            forkLineItems.Add(resultLine);

                            // 向前读取
                            var lastLine = block.Count > 0 ? block[^1] as ForkLine<TVertex, TEdge> : null;
                            if (lastLine != null && lastLine.Select(x => x.Target).Distinct().Count() > 1)
                            {
                                var newLastLine = new ForkLine<TVertex, TEdge>(lastLine.Source, lastLine.Target);
                                var lastLineGroups = lastLine.GroupBy(x => x.Target);
                                foreach (var group in lastLineGroups) 
                                {
                                    var target = group.First().Target;
                                    var newBlockLine = new BlockLine<TVertex, TEdge>(StepLineType.Flow);
                                    newBlockLine.AddRange(new ForkLine<TVertex, TEdge>(group.Key, target, group));

                                    var connectItems = new List<IStepLine<TVertex, TEdge>>();
                                    for (var z = 0; z < forkLineItems.Count; z++)
                                    {
                                        var forkLineItem = forkLineItems[z];
                                        if (forkLineItem.Source == target)
                                        {
                                            connectItems.Add(forkLineItem);
                                            forkLineItems.RemoveAt(z);
                                            z--;
                                        }
                                    }

                                    if (connectItems.Count > 0)
                                    {
                                        var connectTargets = connectItems.SelectMany(x => x.GetLines()).
                                            Where(x => x.LineType != StepLineType.Up).
                                            Select(x => x.Target).Distinct().ToArray();

                                        if (connectTargets.Length != 1)
                                            throw new GraphException<TVertex, TEdge>("无法确定分支源",
                                                connectItems.SelectMany(x => x.GetEdges()).Distinct().ToArray());

                                        var nextLineItem = new ForkLine<TVertex, TEdge>(group.Key, connectTargets.First(), connectItems);
                                        newBlockLine.Add(nextLineItem);
                                    }

                                    newLastLine.Add(newBlockLine);
                                }

                                block[^1] = newLastLine;
                            }

                            if (forkLineItems.Count > 0)
                            {
                                var sources = forkLineItems.Select(x => x.Source).Distinct().ToArray();
                                if (sources.Length != 1)
                                    throw new GraphException<TVertex, TEdge>("无法确定分支源",
                                        forkLineItems.SelectMany(x => x.GetEdges()).Distinct().ToArray());

                                var targets = forkLineItems.SelectMany(x => x.GetLines()).
                                    Where(x => x.LineType == StepLineType.Flow).
                                    Select(x => x.Target).Distinct().ToArray();

                                resultLine = new ForkLine<TVertex, TEdge>(sources.First(), targets.Length == 0 || targets.Length > 1 ? (ushort)0 : targets.First(), forkLineItems);
                                if (frontLine.Count > 0)
                                {
                                    var blockLine = new BlockLine<TVertex, TEdge>(StepLineType.Flow);
                                    blockLine.Add(frontLine);
                                    blockLine.Add(resultLine);
                                    resultLine = blockLine;
                                }
                            }
                            else resultLine = null;
                        }
                    }

                    if (resultLine != null)
                        block.Add(resultLine);
                }

                if (loop.Parent != null)
                {
                    if (!nestedLoopStepLines.ContainsKey(loop.Source))
                        nestedLoopStepLines[loop.Source] = new List<IStepLine<TVertex, TEdge>>();

                    nestedLoopStepLines[loop.Source].Add(block);
                }
                else
                {
                    if (existStepLines.ContainsKey(block.Source))
                    {
                        var currFlow = new BlockLine<TVertex, TEdge>(StepLineType.Flow);
                        currFlow.AddRange(block);

                        var existBlock = existStepLines[block.Source] as BlockLine<TVertex, TEdge>;
                        if (existBlock[0] is BlockLine<TVertex, TEdge> firstBlock && firstBlock.LineType == StepLineType.Fork)
                        {
                            firstBlock.Add(currFlow);
                        }
                        else 
                        {
                            var forkBlock = new ForkLine<TVertex, TEdge>(existBlock.Source, 0);
                            var lastFlow = new BlockLine<TVertex, TEdge>(StepLineType.Flow);
                            lastFlow.AddRange(existBlock);

                            forkBlock.Add(lastFlow);
                            forkBlock.Add(currFlow);

                            existBlock.Clear();
                            existBlock.Add(forkBlock);
                        }
                    }
                    else
                    {
                        existStepLines.Add(block.Source, block);
                    }
                }
            }

            // 将所有连接至loop中的endlessStepLines Target改为loop点
            foreach (var endlessStepLine in endlessStepLines) 
            {
                var endlessStepLineList = endlessStepLine.Value;
                for (var i = 0; i < endlessStepLineList.Count; i++) 
                {
                    var el = endlessStepLineList[i];
                    var loop = loops.FirstOrDefault(x => x.Any(y => y.Source == el.Target));
                    if (loop != null && el.Target != loop.Source) 
                    {
                        if (el is ForkLine<TVertex, TEdge> forkLine)
                        {
                            var newForkLine = new ForkLine<TVertex, TEdge>(el.Source, loop.Target);
                            newForkLine.AddRange(forkLine);
                            endlessStepLineList[i] = newForkLine;
                        }
                        else 
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
            }

            // 对步骤进行分层并取回入口点StepLine
            IEnumerable<IStepLine<TVertex, TEdge>> BFSGetRights(ushort target, bool convertLoops)
            {
                var stepsLines = endlessStepLines.ContainsKey(target) ? endlessStepLines[target].ToList() : new List<IStepLine<TVertex, TEdge>>();
                if (existStepLines.ContainsKey(target))
                    stepsLines.Add(existStepLines[target]);

                if (convertLoops)
                {
                    for (var i = 0; i < stepsLines.Count; i++)
                    {
                        var stepLine = stepsLines[i];
                        if (stepLine is BlockLine<TVertex, TEdge> blockLine && blockLine.LineType == StepLineType.Loop)
                        {
                            stepsLines.RemoveAt(i);
                            i--;

                            // 如果是循环则选取退出点作为后续
                            if (loopsExits.ContainsKey(blockLine))
                                foreach (var exit in loopsExits[blockLine].Select(x => x.Target).Distinct())
                                    foreach (var sub in BFSGetRights(exit, convertLoops))
                                        stepsLines.Add(sub);
                        }
                    }
                }

                return stepsLines;
            }

            // 存储每个节点的入度
            var inDegree = new Dictionary<ushort, int>();
            var degressLayers = new Dictionary<ushort, BFSLayer<TVertex, TEdge>>();
            void BFSDegress(ushort state, HashSet<IStepLine<TVertex, TEdge>> visitor) 
            {
                foreach (var next in BFSGetRights(state, true))
                {
                    if (visitor.Contains(next))
                        return;

                    visitor.Add(next);

                    if (!inDegree.ContainsKey(next.Target))
                        inDegree[next.Target] = 0;

                    inDegree[next.Target]++;

                    BFSDegress(next.Target, visitor);
                }
            }

            BFSLayer<TVertex, TEdge> SwapLayer(BFSLayer<TVertex, TEdge> parent, BFSLayer<TVertex, TEdge> current, bool converLayer)
            {
                if (converLayer && parent.LineType == StepLineType.Fork)
                {
                    var topLayer = new BFSLayer<TVertex, TEdge>(parent.Parent, StepLineType.Flow);

                    var parentIndex = parent.Parent.Lines.IndexOf(parent);
                    parent.Parent.Lines.RemoveAt(parentIndex);
                    parent.Parent.Lines.Insert(parentIndex, topLayer);

                    parent.Parent = topLayer;
                    topLayer.Lines.Add(parent);
                    parent = topLayer;
                }

                parent.Lines.Add(current);
                return current;
            }

            BFSLayer<TVertex, TEdge> BFSLayout(ushort state, BFSLayer<TVertex, TEdge> parent)
            {
                var rights = BFSGetRights(state, false).Where(x => !parent.Contains(x)).ToArray();
                if (rights.Length > 1)
                    parent = SwapLayer(parent, new BFSLayer<TVertex, TEdge>(parent, StepLineType.Fork), false);

                foreach (var right in rights)
                {
                    var layer = rights.Length < 2 ? parent : SwapLayer(parent, new BFSLayer<TVertex, TEdge>(parent, StepLineType.Flow), false);
                    layer.Lines.Add(new BFSStepLine<TVertex, TEdge>(right));

                    if (inDegree.ContainsKey(right.Target))
                    {
                        // 标记储存层
                        if (!degressLayers.ContainsKey(right.Target))
                            degressLayers.Add(right.Target, parent);
                        else if (degressLayers[right.Target].Level > parent.Level)
                            degressLayers[right.Target] = parent;

                        inDegree[right.Target]--;
                        if (inDegree[right.Target] == 0)
                        {
                            inDegree.Remove(right.Target);
                            parent.Outs.Add(right);

                            var nextLayer = BFSLayout(right.Target, SwapLayer(degressLayers[right.Target], new BFSLayer<TVertex, TEdge>(parent, StepLineType.Flow), true));
                            foreach (var layerNext in nextLayer.Outs)
                                BFSLayout(layerNext.Target, nextLayer.Parent);
                        }
                    }
                    else BFSLayout(right.Target, layer);
                }

                return parent;
            }

            var stepLines = new List<GraphStep<TVertex, TEdge>>();
            for (int i = 0; i < entries.Count; i++)
            {
                BFSDegress(entries[i], new HashSet<IStepLine<TVertex, TEdge>>());

                foreach (var remove1 in inDegree.Where(x => x.Value < 2).ToArray())
                    inDegree.Remove(remove1.Key);

                var layer = new BFSLayer<TVertex, TEdge>(null, StepLineType.Flow);
                BFSLayout(entries[i], layer);

                while (layer.Parent != null)
                    layer = layer.Parent;

                if(layer.Check()) stepLines.Add(CreateGraphStep(layer));
            }

            // 将steplines还原成步并优化其构造
            return stepLines;
        }

        abstract class BFSLine<TVertex, TEdge>
           where TEdge : IEdge<TVertex>
           where TVertex : IVertex
        {
        }

        class BFSLayer<TVertex, TEdge> : BFSLine<TVertex, TEdge>
            where TEdge : IEdge<TVertex>
            where TVertex : IVertex
        {
            public BFSLayer(BFSLayer<TVertex, TEdge> parent, StepLineType lineType) 
            {
                Parent = parent;
                LineType = lineType;
                Lines = new List<BFSLine<TVertex, TEdge>>();
                Outs = new List<IStepLine<TVertex, TEdge>>();
            }

            public int Level 
            {
                get 
                {
                    var level = 0;
                    var curr = this;
                    while (curr != null) 
                    {
                        level++;
                        curr = curr.Parent;
                    }

                    return level;
                }
            }

            public BFSLayer<TVertex, TEdge> Parent { get; set; }

            public StepLineType LineType { get; }

            public IList<BFSLine<TVertex, TEdge>> Lines { get; }

            public IList<IStepLine<TVertex, TEdge>> Outs { get; }

            public bool Contains(IStepLine<TVertex, TEdge> stepLine) 
            {
                var result = false;
                for (var i = 0; i < Lines.Count; i++) 
                {
                    var line = Lines[i];
                    if (line is BFSStepLine<TVertex, TEdge> line2)
                    {
                        if (line2.StepLine == stepLine)
                        {
                            result = true;
                            break;
                        }
                    }
                }

                return result;
            }

            public bool Check() 
            {
                for (var i = 0; i < Lines.Count; i++) 
                {
                    var line = Lines[i];
                    if (line is BFSLayer<TVertex, TEdge> line2)
                    {
                        if (!line2.Check()) 
                        {
                            Lines.RemoveAt(i);
                            i--;
                        }
                    }
                }

                return Lines.Count != 0;
            }
        }

        class BFSStepLine<TVertex, TEdge> : BFSLine<TVertex, TEdge>
            where TEdge : IEdge<TVertex>
            where TVertex : IVertex
        {
            public BFSStepLine(IStepLine<TVertex, TEdge> stepLine)
            {
                StepLine = stepLine;
            }

            public IStepLine<TVertex, TEdge> StepLine { get; }
        }

        static GraphStep<TVertex, TEdge> CreateGraphStep<TVertex, TEdge>(BFSLine<TVertex, TEdge> bfsLine)
            where TEdge : IEdge<TVertex>
            where TVertex : IVertex
        {
            GraphStep<TVertex, TEdge> graphStep = null;
            switch (bfsLine) 
            {
                case BFSLayer<TVertex, TEdge> layer:
                    var steps = new GraphStep<TVertex, TEdge>[layer.Lines.Count];
                    for (var i = 0; i < layer.Lines.Count; i++)
                        steps[i] = CreateGraphStep(layer.Lines[i]);

                    switch (layer.LineType) 
                    {
                        case StepLineType.Fork:
                            graphStep = steps.Length == 0 ? null : (steps.Length == 1 ? steps[0] : new StepFork<TVertex, TEdge>(steps));
                            break;
                        case StepLineType.Flow:
                            graphStep = GraphStep<TVertex, TEdge>.From(steps);
                            break;
                    }
                    break;
                case BFSStepLine<TVertex, TEdge> line:
                    graphStep = CreateGraphStep(line.StepLine);
                    break;
            }

            return graphStep;
        }

        static GraphStep<TVertex, TEdge> CreateGraphStep<TVertex, TEdge>(IStepLine<TVertex, TEdge> stepLine)
            where TEdge : IEdge<TVertex>
            where TVertex : IVertex
        {
            GraphStep<TVertex, TEdge> graphStep = null;

            // 判断步骤类型
            switch (stepLine)
            {
                case FlowLine<TVertex, TEdge> fl:
                    {
                        var flowSteps = new StepNext<TVertex, TEdge>[fl.Count];

                        // 遍历流程线中的每个步骤
                        for (var y = 0; y < fl.Count; y++)
                            flowSteps[y] = new StepNext<TVertex, TEdge>(fl[y]);

                        // 根据流程线的类型，添加 UpwardStep 或 DownwardStep
                        switch (fl.LineType)
                        {
                            case StepLineType.Up:
                                flowSteps[^1] = new StepUpward<TVertex, TEdge>(flowSteps[^1].Edge);
                                break;
                            case StepLineType.Down:
                                flowSteps[0] = new StepDownward<TVertex, TEdge>(flowSteps[0].Edge);
                                break;
                        }

                        graphStep = GraphStep<TVertex, TEdge>.From(flowSteps);
                    }
                    break;
                // 处理 ForkLine 步骤
                case ForkLine<TVertex, TEdge> fk:
                    {
                        if (fk.Count == 1)
                        {
                            graphStep = CreateGraphStep(fk[0]);
                        }
                        else
                        {
                            // 初始化 GraphStep 数组
                            var forkSteps = new GraphStep<TVertex, TEdge>[fk.Count];

                            // 遍历 ForkLine 中的每个流程线
                            for (var x = 0; x < fk.Count; x++)
                                forkSteps[x] = CreateGraphStep(fk[x]);

                            // 如果 GraphStep 数组长度为 1，则直接返回该 GraphStep 对象，否则返回 ForkStep 对象
                            graphStep = forkSteps.Length == 0 ? null : (forkSteps.Length == 1 ? forkSteps[0] : new StepFork<TVertex, TEdge>(forkSteps));
                        }
                    }
                    break;

                // 处理 BlockLine 步骤
                case BlockLine<TVertex, TEdge> bl:
                    // 将 BlockLine 中的所有子步骤转换为 GraphStep 对象，然后返回一个 LoopStep 对象
                    {
                        switch (bl.LineType)
                        {
                            case StepLineType.Flow:
                                graphStep = GraphStep<TVertex, TEdge>.From(bl.Select(CreateGraphStep).ToArray());
                                break;
                            case StepLineType.Loop:
                                graphStep = new StepLoop<TVertex, TEdge>(bl.Select(CreateGraphStep).ToArray());
                                break;
                            default:
                                throw new NotImplementedException("内部错误");
                        }
                    }
                    break;
            }

            // 返回转换后的 GraphStep 对象
            return graphStep;
        }

        static List<Tuple<T, T>> GetCombinations<T>(IList<T> elements)
        {
            List<Tuple<T, T>> combinations = new List<Tuple<T, T>>();
            for (int i = 0; i < elements.Count - 1; i++)
                for (int j = i + 1; j < elements.Count; j++)
                    combinations.Add(new Tuple<T, T>(elements[i], elements[j]));

            return combinations;
        }

        enum StepLineType
        {
            Flow,
            Up,
            Down,
            Fork,
            Loop
        }

        interface IStepLine<TVertex, TEdge> : IEdgeLine<TVertex, TEdge>
            where TEdge : IEdge<TVertex>
            where TVertex : IVertex
        {
            ushort Source { get; }

            ushort Target { get; }

            StepLineType LineType { get; }

            IEnumerable<IStepLine<TVertex, TEdge>> GetLines();
        }

        interface IStepLineCollection<TVertex, TEdge> : IList<IStepLine<TVertex, TEdge>>
            where TEdge : IEdge<TVertex>
            where TVertex : IVertex
        {
        }

        interface IEdgeLine<TVertex, TEdge>
            where TEdge : IEdge<TVertex>
            where TVertex : IVertex
        {
            IEnumerable<TEdge> GetEdges();
        }

        class BlockLine<TVertex, TEdge> : List<IStepLine<TVertex, TEdge>>, IStepLine<TVertex, TEdge>, IStepLineCollection<TVertex, TEdge>
            where TEdge : IEdge<TVertex>
            where TVertex : IVertex
        {
            public BlockLine(StepLineType type)
            {
                LineType = type;
            }

            public StepLineType LineType { get; set; }

            public ushort Source => this[0].Source;

            public ushort Target => this[^1].Target;

            public IEnumerable<TEdge> GetEdges()
            {
                return this.SelectMany(x => x.GetEdges());
            }

            public IEnumerable<IStepLine<TVertex, TEdge>> GetLines()
            {
                foreach (var item in this)
                    foreach (var sub in item.GetLines())
                        yield return sub;
            }

            public static BlockLine<TVertex, TEdge> FromLoop(LoopLine<TVertex, TEdge> loopLine)
            {
                BlockLine<TVertex, TEdge> block = new BlockLine<TVertex, TEdge>(StepLineType.Loop);
                block.AddRange(loopLine);
                return block;
            }
        }

        class FlowLine<TVertex, TEdge> : List<TEdge>, IStepLine<TVertex, TEdge>
            where TEdge : IEdge<TVertex>
            where TVertex : IVertex
        {
            private readonly ushort? mStart;
            private readonly ushort? mEnd;

            public FlowLine(TEdge edge)
            {
                Add(edge);
            }

            public FlowLine(ushort start, ushort end, IEnumerable<TEdge> edges) 
            {
                mStart = start;
                mEnd = end;
                AddRange(edges);
            }

            public TEdge Start => this[0];

            public TEdge End => this[^1];

            public StepLineType LineType { get; set; }

            public ushort Source => mStart ?? Start.Source.Index;

            public ushort Target => mEnd ?? End.Target.Index;

            public bool ContainsPoint(ushort point)
            {
                for (var i = 0; i < Count; i++)
                {
                    var edge = this[i];
                    if (edge.Source.Index == point ||
                        edge.Target.Index == point)
                        return true;
                }

                return false;
            }

            public IEnumerable<TEdge> GetEdges()
            {
                return this;
            }

            public IEnumerable<IStepLine<TVertex, TEdge>> GetLines()
            {
                yield return this;
            }

            public override string ToString()
            {
                return $"{Start} {End}";
            }
        }

        class ForkLine<TVertex, TEdge> : List<IStepLine<TVertex, TEdge>>, IStepLine<TVertex, TEdge>, IStepLineCollection<TVertex, TEdge>
            where TEdge : IEdge<TVertex>
            where TVertex : IVertex
        {
            public ForkLine(ushort source, ushort target, IEnumerable<IStepLine<TVertex, TEdge>> steps)
            {
                Source = source;
                Target = target;
                AddRange(steps);
            }

            public ForkLine(ushort source, ushort target)
            {
                Source = source;
                Target = target;
            }

            public ushort Source { get; }

            public ushort Target { get; }

            public StepLineType LineType => StepLineType.Fork;

            public IEnumerable<TEdge> GetEdges()
            {
                return this.SelectMany(x => x.GetEdges());
            }

            public IEnumerable<IStepLine<TVertex, TEdge>> GetLines()
            {
                foreach (var item in this)
                    foreach (var sub in item.GetLines())
                        yield return sub;
            }
        }

        class LoopLine<TVertex, TEdge> : List<ForkLine<TVertex, TEdge>>, IEdgeLine<TVertex, TEdge>
            where TEdge : IEdge<TVertex>
            where TVertex : IVertex
        {
            public LoopLine()
            {
            }

            public LoopLine(LoopLine<TVertex, TEdge> parent)
            {
                AddRange(parent);
            }

            public bool IsJump { get; set; }

            public LoopLine<TVertex, TEdge> Parent { get; set; }

            public ushort Source => this[0].Source;

            public ushort Target => this[^1].Target;

            public int FindPointLineIndex(ushort point)
            {
                for (var i = 0; i < Count; i++)
                {
                    var line = this[i];
                    if (line.Source == point)
                        return i;
                }

                return -1;
            }

            public bool CheckLoop()
            {
                var last = this[Count - 1];
                var index = FindPointLineIndex(last.Target);
                if (index != -1)
                {
                    for (var i = 0; i < index; i++)
                        RemoveAt(0);

                    return true;
                }

                return false;
            }

            public IEnumerable<TEdge> GetEdges()
            {
                return this.SelectMany(x => x.GetEdges());
            }

            public override string ToString()
            {
                return $"({Source}->{this[^1].Source})";
            }
        }
    }
}
