using GiGraph.Dot.Entities.Graphs;
using GiGraph.Dot.Types.Styling;
using libgraph;
using librule;
using System.Diagnostics;

namespace test_bench
{
    internal class DSLTest : DebugGraphTest
    {
        public override string TestName => "DSL";

        protected override IEnumerable<IGraph<DebugVertex, DebugEdge>> GetGraphs()
        {
            var sw = new Stopwatch();
            sw.Start();
            Console.WriteLine(ModelGenerator.Generate(File.ReadAllText("G:\\项目\\compiler\\tool\\test_bench\\test2.gram"), true, out var dsl));
            sw.Stop();

            Console.WriteLine($"Generate parser ms:{sw.ElapsedMilliseconds}");

            yield return dsl;
            ShowGraph(CreateDotGraph(dsl, null));
        }

        protected override DotGraph CreateDotGraph(IGraph<DebugVertex, DebugEdge> graph, GraphException<DebugVertex, DebugEdge> ex)
        {
            var dot = new DotGraph(directed: true);
            foreach (var vertex in graph.Vertices)
            {
                var label = vertex.Id.ToString();

                var node = dot.Nodes.Add(label);
                node.Label = vertex.Id.ToString();
                node.Shape = vertex.Id.Contains("*") ? GiGraph.Dot.Types.Nodes.DotNodeShape.Box3D : GiGraph.Dot.Types.Nodes.DotNodeShape.Circle;
                node.Style.CornerStyle = DotCornerStyle.Rounded;
                node.Style.FillStyle = GiGraph.Dot.Types.Nodes.DotNodeFillStyle.Radial;
            }

            foreach (var edge in graph.Edges)
            {
                var left = edge.Source.Id;
                var right = edge.Target.Id;
                if (true) // edge.Descrption != "miss\n")
                {
                    dot.Edges.Add(left, right, e =>
                    {
                        e.Label = edge.Descrption.Replace("\0", "ε");
                        if (edge.Flags.HasFlag(EdgeFlags.SpecialPoint))
                            e.Style.LineStyle = DotLineStyle.Tapered;

                        if (edge.Descrption.Contains("miss"))
                            e.Style.LineStyle = DotLineStyle.Dotted;
                    });

                    continue;
                    if (edge.Subset != null && edge.Subset.Index != 0)
                    {
                        dot.Edges.Add(left, edge.Subset.Id, e =>
                        {
                            e.Label = string.Empty;
                            e.Style.LineStyle = DotLineStyle.Tapered;
                        });
                    }
                }
            }
            return dot;
        }
    }


}
