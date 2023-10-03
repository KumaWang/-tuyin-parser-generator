using libgraph;
using System.Collections.Generic;
using System.Linq;

namespace libflow.steps
{
    /// <summary>
    /// 将有向图转换为可用的updown结构
    /// </summary>
    class ConvertDirectedGraph2UpdownFigure<TVertex, TEdge> : AnalysisStep<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        protected internal override FlowFigure<TVertex, TEdge> Run(FlowFigure<TVertex, TEdge> figure, GraphModel<TVertex, TEdge> model)
        {
            IEnumerable<FlowLine<TVertex, TEdge>> GetRights(ushort state) => model.GetRights(state).Select(x => new FlowLine<TVertex, TEdge>(x, FlowLineType.Next));

            var layers = new List<FlowLayer<TVertex, TEdge>>();
            var visitor = new Dictionary<FlowLine<TVertex, TEdge>, FlowNode<TVertex, TEdge>>();

            var states = new Dictionary<ushort, FlowLayer<TVertex, TEdge>>() { { figure.GraphFigure.EntryPoint, null } };
            while (states.Count > 0)
            {
                var nextStates = new Dictionary<ushort, FlowLayer<TVertex, TEdge>>();
                foreach (var state in states)
                {
                    var layer = new FlowLayer<TVertex, TEdge>(figure, (state.Value?.Level ?? 0) + 1, state.Key, state.Value);
                    var nodes = new Queue<(FlowNode<TVertex, TEdge> Parent, FlowNode<TVertex, TEdge> Current)>();
                    foreach (var updownLine in GetRights(state.Key))
                        nodes.Enqueue((layer.Tree, new FlowNode<TVertex, TEdge>(updownLine)));

                    // 开始查找子图
                    while (nodes.Count > 0)
                    {
                        // 取出队列的头部节点，作为当前节点访问
                        var currStep = nodes.Dequeue();

                        // 如果已经访问过，跳过此次循环
                        if (visitor.ContainsKey(currStep.Current.From))
                            continue;

                        // 标记为已访问
                        visitor.Add(currStep.Current.From, currStep.Current);

                        // 添加到上一层中
                        currStep.Parent.Nodes.Add(currStep.Current);

                        // 继续向后访问
                        var rightSteps = GetRights(currStep.Current.From.Edge.Target.Index);
                        foreach (var nextStep in rightSteps)
                        {
                            if (figure.GraphFigure.InDeeps[nextStep.Edge.Source.Index] > 1)
                            {
                                var nextLayer = layer;

                                // 如果当前点包含上一层则查找到这2层对应的上级层
                                if (nextStates.ContainsKey(nextStep.Edge.Source.Index) && nextStates[nextStep.Edge.Source.Index] != layer)
                                    if ((nextLayer = CommonTools<TVertex, TEdge>.FindLayerRoot(layer, nextStates[nextStep.Edge.Source.Index])) == null)
                                        throw new GraphException<TVertex, TEdge>("内部异常,流分析不允许由不同的入口点指向相同状态点。", nextStep.Edge);

                                // 加入下一层访问
                                nextStates[nextStep.Edge.Source.Index] = nextLayer;

                                // 设置关系,用于层排序
                                if (!figure.GraphFigure.Relations.ContainsKey(layer.State))
                                    figure.GraphFigure.Relations[layer.State] = new HashSet<ushort>();

                                figure.GraphFigure.Relations[layer.State].Add(nextStep.Edge.Source.Index);
                            }
                            else
                            {
                                // 继续加入队列
                                nodes.Enqueue((currStep.Current, new FlowNode<TVertex, TEdge>(nextStep)));
                            }
                        }
                    }

                    if (layer.Tree.Nodes.Count > 0)
                        layers.Add(layer);
                }

                states = nextStates;
            }

            // 排序层
            CommonTools<TVertex, TEdge>.SortLayers(layers);

            // 处理未连接到某层的结构
            var layerTargets = layers.Select((x, i) => (x, i)).ToDictionary(x => x.x.State, x => x.i);
            layerTargets[0] = int.MaxValue;
            for (var i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];
                var ends = layer.Tree.GetEnds().ToArray();

                for (var j = 0; j < ends.Length; j++)
                {
                    var end = ends[j];

                    // 如果end.Target连接到某一层则一定代表需要转跳层
                    if (layerTargets.ContainsKey(end.From.Edge.Target.Index))
                    {
                        var targetIndex = layerTargets[end.From.Edge.Target.Index];
                        // i + 1 代表当前层的下一层索引,如果结尾直接连接到下一层则不需要Goto过去
                        if (i + 1 != targetIndex)
                        {
                            // 因为代码生成时用到的层级最终时根据layers索引排布的
                            // 所以这里可以直接比较layer在layers中的序列位置
                            end.From = new FlowLine<TVertex, TEdge>(end.From.Edge, i < targetIndex ? FlowLineType.Downward : FlowLineType.Upward);
                        }
                    }
                }
            }

            return new FlowFigure<TVertex, TEdge>(figure.GraphFigure, layers);
        }
    }
}
