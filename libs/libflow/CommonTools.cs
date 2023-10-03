using libgraph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace libflow
{
    static class CommonTools<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        private readonly static FlowLayerComparer sFlowLayerComparer = new FlowLayerComparer();

        /// <summary>
        /// 创建figure的step
        /// </summary>
        public static FlowStep<TVertex, TEdge> CreateGraphStep(FlowFigure<TVertex, TEdge> figure)
        {
            if (figure?.Layers?.Count == 0)
                return null;

            var graphStep = CreateGraphStep(figure.Layers[0].Tree);
            for (var i = 1; i < figure.Layers.Count; i++)
                graphStep = new FlowStepConcatenation<TVertex, TEdge>(graphStep, CreateGraphStep(figure.Layers[i].Tree));

            return graphStep;
        }

        /// <summary>
        /// 创建node的step
        /// </summary>
        public static FlowStep<TVertex, TEdge> CreateGraphStep(FlowNode<TVertex, TEdge> node)
        {
            var subSteps = new List<FlowStep<TVertex, TEdge>>();
            foreach (var subnode in node.Nodes)
                subSteps.Add(CreateGraphStep(subnode));

            var graphStep = subSteps.Count == 0 ? null : subSteps.Count == 1 ? subSteps[0] : new FlowStepFork<TVertex, TEdge>(subSteps);
            if (node.From.Edge != null)
            {
                FlowStep<TVertex, TEdge> flowStep = node.From.LineType switch
                {
                    FlowLineType.Next => new FlowStepNext<TVertex, TEdge>(node.From.Edge),
                    FlowLineType.Upward => new FlowStepUpward<TVertex, TEdge>(node.From.Edge),
                    FlowLineType.Downward => new FlowStepDownward<TVertex, TEdge>(node.From.Edge),
                    _ => throw new NotImplementedException()
                };

                graphStep = graphStep == null ? flowStep : new FlowStepConcatenation<TVertex, TEdge>(flowStep, graphStep);
            }

            return graphStep;
        }

        /// <summary>
        /// 按执行顺序查询图的所有子图
        /// </summary>
        public static GraphFigure<TVertex, TEdge>[] FindFigures(IGraph<TVertex, TEdge> graph, GraphModel<TVertex, TEdge> model)
        {
            // 对入口点（entry）和一些有特殊标记的边进行处理
            // 生成起始节点列表（entries）
            // 使用BFS对图的每一层顺序遍历从而找到入口点
            var figures = new List<GraphFigure<TVertex, TEdge>>();
            var visitor = new bool[model.MaxState];

            // 搜索以 state 为起点的所有节点，包括 state 节点本身
            GraphFigure<TVertex, TEdge> BFSFindSubset(ushort state)
            {
                var figure = new GraphFigure<TVertex, TEdge>(state);

                // 创建队列，用于存放待访问节点
                var entryQueue = new Queue<ushort>();
                entryQueue.Enqueue(state);

                // 开始查找子图
                while (entryQueue.Count > 0)
                {
                    // 取出队列的头部节点，作为当前节点访问
                    var entry = entryQueue.Dequeue();

                    if (!figure.InDeeps.ContainsKey(entry))
                        figure.InDeeps[entry] = 1;

                    // 如果已经访问过，跳过此次循环
                    if (visitor[entry])
                    {
                        figure.InDeeps[entry]++;
                        continue;
                    }

                    // 标记为已访问
                    visitor[entry] = true;

                    // 获取该边集，并遍历其中的所有边
                    var rights = model.GetRights(entry);
                    for (var i = 0; i < rights.Count; i++)
                    {
                        var right = rights[i];

                        // 将该边指向的节点加入待访问队列
                        entryQueue.Enqueue(right.Target.Index);
                    }
                }

                // 减去入口点入度
                figure.InDeeps[state]--;

                return figure;
            }

            // 查找未使用的入口点
            foreach (var edge in graph.Vertices.Where(x => model.GetLefts(x.Index).Count == 0))
                figures.Add(BFSFindSubset(edge.Index));

            return figures.ToArray();
        }

        /// <summary>
        /// 快速找到从后向前的第一个相同项
        /// </summary>
        public static FlowLayer<TVertex, TEdge> FindLayerRoot(FlowLayer<TVertex, TEdge> layer1, FlowLayer<TVertex, TEdge> layer2)
        {
            var visitor = new HashSet<FlowLayer<TVertex, TEdge>>();
            var layer = layer1;
            while (layer != null)
            {
                visitor.Add(layer);
                layer = layer.Parent;
            }

            layer = layer2;
            while (layer != null)
            {
                if (visitor.Contains(layer))
                    return layer;

                layer = layer.Parent;
            }

            return null;
        }

        /// <summary>
        /// 查询多组层的根
        /// </summary>
        public static FlowNode<TVertex, TEdge> FindNodeRoot(FlowNode<TVertex, TEdge>[] nodes)
        {
            var hashs = new Dictionary<FlowNode<TVertex, TEdge>, int>();
            var computes = nodes.ToArray();
            while (computes.Any(x => x != null))
            {
                for (var i = 0; i < computes.Length; i++)
                {
                    var compute = computes[i];
                    if (compute == null)
                        continue;

                    if (!hashs.ContainsKey(compute))
                        hashs[compute] = 0;

                    hashs[compute]++;
                    if (hashs[compute] >= nodes.Length)
                        return compute;

                    computes[i] = compute?.Parent;
                }
            }

            return null;
        }

        /// <summary>
        /// 对层进行排序
        /// </summary>
        public static void SortLayers(List<FlowLayer<TVertex, TEdge>> layers)
        {
            layers.Sort(sFlowLayerComparer);
        }

        /// <summary>
        /// 对层进行排序
        /// </summary>
        public static IEnumerable<FlowLayer<TVertex, TEdge>> SortLayers(IEnumerable<FlowLayer<TVertex, TEdge>> layers)
        {
            return layers.OrderBy(x => x, sFlowLayerComparer);
        }

        class FlowLayerComparer : IComparer<FlowLayer<TVertex, TEdge>>
        {
            public int Compare(FlowLayer<TVertex, TEdge> x, FlowLayer<TVertex, TEdge> y)
            {
                if (x == y) return 0;
                if (x.Figure.GraphFigure.IsConnect(x, y)) return -1;
                if (y.Figure.GraphFigure.IsConnect(y, x)) return 1;
                return x.Level.CompareTo(y.Level);
            }
        }
    }
}
