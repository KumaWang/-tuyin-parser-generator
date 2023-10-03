using libflow.steps;
using libflow.stmts;
using libgraph;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace libflow
{
    public static class FlowAnalyzer<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        private static readonly List<AnalysisStep<TVertex, TEdge>> sSteps = new List<AnalysisStep<TVertex, TEdge>>()
        {
            new ConvertDirectedGraph2UpdownFigure<TVertex, TEdge>(),
            new SetOwnerLayer<TVertex, TEdge>(),
            new Updown2Cyclic<TVertex, TEdge>()
        };

        public static FlowPath<TVertex, TEdge> GetPath(FlowFigure<TVertex, TEdge> figure, GraphModel<TVertex, TEdge> model)
        {
            for (var i = 0; i < sSteps.Count; i++)
                figure = sSteps[i].Run(figure, model);

            var step = CommonTools<TVertex, TEdge>.CreateGraphStep(figure);
            if (step == null)
                return null;

            return new FlowPath<TVertex, TEdge>(step, figure);
        }

        public static IList<FlowPath<TVertex, TEdge>> GetPaths(IGraph<TVertex, TEdge> graph)
        {
            // 创建连接数据集
            var model = new GraphModel<TVertex, TEdge>(graph);

            // 查找入口点
            var figures = CommonTools<TVertex, TEdge>.FindFigures(graph, model).Select(x => new FlowFigure<TVertex, TEdge>(x, null)).ToArray();
            var paths = new ConcurrentBag<FlowPath<TVertex, TEdge>>();

            // 执行步
            for (var i = 0; i < figures.Length; i++)
                paths.Add(GetPath(figures[i], model));

            return paths.Where(x => x?.Step != null).ToArray();
        }

        public static Function GetAst(FlowPath<TVertex, TEdge> path, AstConstructor<TVertex, TEdge> ctor) 
        {
            // 每次重新创建一个避免多线程出错
            var converter = new FlowStepConverter<TVertex, TEdge>(ctor);
            // 需要确定如何得到Function文法的Formals
            var stmt = converter.Create(path);
            // 扫描出所有被应用的Label
            var gotos = stmt.Walk().Where(x => x is Goto).Cast<Goto>().Select(x => x.Label).ToHashSet();
            // 对未使用的label进行清理
            foreach (var label in stmt.Walk().Where(x => x is DefineLabel).Cast<DefineLabel>())
            {
                for (var i = 0; i < label.Labels.Count; i++)
                {
                    if (!gotos.Contains(label.Labels[i]))
                    {
                        label.Labels.RemoveAt(i);
                        i--;
                    }
                }
            }

            // 返回函数
            return new Function(path.Step.GetSources().First().Source.Index, stmt);
        }

        public static IEnumerable<IAstNode> GetAsts(IGraph<TVertex, TEdge> graph, AstConstructor<TVertex, TEdge> ctor)
        {
            var asts = new ConcurrentBag<Function>();
            var paths = GetPaths(graph);
            for (var i = 0; i < paths.Count; i++)
                asts.Add(GetAst(paths[i], ctor));

            return asts.OrderBy(x => x.EntryPoint).ToArray();
        }

        public static FlowReport<TVertex, TEdge> GetReport(FlowFigure<TVertex, TEdge> figure, GraphModel<TVertex, TEdge> model, AstConstructor<TVertex, TEdge> ctor) 
        {
            var path = GetPath(figure, model);
            if (path == null)
                return null;

            var ast = GetAst(path, ctor);
            return new FlowReport<TVertex, TEdge>(ast, figure);
        }

        public static IEnumerable<FlowReport<TVertex, TEdge>> GetReports(IGraph<TVertex, TEdge> graph, AstConstructor<TVertex, TEdge> ctor) 
        {
            // 创建连接数据集
            var model = new GraphModel<TVertex, TEdge>(graph);

            // 查找入口点
            var figures = CommonTools<TVertex, TEdge>.FindFigures(graph, model).Select(x => new FlowFigure<TVertex, TEdge>(x, null)).ToArray();
            var reports = new ConcurrentBag<FlowReport<TVertex, TEdge>>();

            // 执行步
            for (var i = 0; i < reports.Count; i++)
                reports.Add(GetReport(figures[i], model, ctor));

            return reports.OrderBy(x => x.Figure.GraphFigure.EntryPoint).Where(x => x != null).ToArray();
        }
    }
}
