using GiGraph.Dot.Entities.Graphs;
using GiGraph.Dot.Types.Colors;
using GiGraph.Dot.Types.Styling;
using libgraph;
using System.Drawing;

namespace test_bench
{
    abstract class DebugGraphTest : TestBase<DebugVertex, DebugEdge>
    {
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
                dot.Edges.Add(left, right, e =>
                {
                    e.Label = edge.Descrption.Replace("\0", "ε");
                    if (edge.Flags.HasFlag(EdgeFlags.SpecialPoint))
                        e.Style.LineStyle = DotLineStyle.Tapered;

                    if (edge.Descrption.Contains("missing"))
                        e.Style.LineStyle = DotLineStyle.Dotted;

                    if (ex.Edges.Contains(edge))
                        e.Color = new DotColor(Color.Red);
                });
            }
            return dot;
        }
    }
}
