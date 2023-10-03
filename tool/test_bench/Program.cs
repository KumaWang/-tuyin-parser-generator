// See https://aka.ms/new-console-template for more information
using GiGraph.Dot.Entities.Graphs;
using GiGraph.Dot.Extensions;
using GiGraph.Dot.Types.Styling;
using libgraph;
using librule;
using System.Diagnostics;
using test_bench;

DotGraph CreateDotGraph(IGraph<DebugVertex, DebugEdge> graph, GraphException<DebugVertex, DebugEdge> ex)
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

void ShowGraph(string name, DotGraph graph)
{
    string fileName = $"{name}.gv";

    graph.SaveToFile(fileName);

    var workroot = Path.GetDirectoryName(new FileInfo(fileName).FullName);
    var output = $"{Path.GetFileNameWithoutExtension(fileName)}.png";
    Process create = new Process();
    create.StartInfo.FileName = "dot";
    create.StartInfo.Arguments = $"-Tpng {Path.GetFileName(fileName)} -o {output}";
    create.StartInfo.WorkingDirectory = workroot;
    create.Start();
    create.WaitForExit();
    TestHelper.OpenImage($"{workroot}\\{output}");

    File.Delete(fileName);
}



/*
var index = 0;
foreach (var step in ModelGenerator.Generate(File.ReadAllText("G:\\项目\\compiler\\tool\\test_bench\\test2.gram"), true)) 
{
    ShowGraph($"step{index++}", CreateDotGraph(step, null));
}
*/

File.WriteAllText("G:\\a.project\\compiler\\tool\\test_bench\\test2.cs", ModelGenerator.Generate(File.ReadAllText("G:\\a.project\\compiler\\tool\\test_bench\\test2.gram"), false, out var graph));
ShowGraph($"aaa", CreateDotGraph(graph, null));
return;

var tests = new List<ITest>()
{
    new DSLTest(),
    new FixedTest(),
    new RandomTest()
};

for (var i = 0; i < tests.Count; i++)
    tests[i].Run();
