using GiGraph.Dot.Entities.Graphs;
using GiGraph.Dot.Extensions;
using GiGraph.Dot.Types.Styling;
using libgraph;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

static class Helper
{
    [DllImport("shell32.dll")]
    public static extern int FindExecutable(string lpFile, string lpDirectory, [Out] StringBuilder lpResult);

    public static void OpenImage(string imagePath)
    {
        Process[] processes = Process.GetProcessesByName("mspaint");
        foreach (Process cp in processes)
            if (cp.MainWindowTitle.Contains(Path.GetFileName(imagePath))) //检查进程名称是否为 "X - 画图"
                cp.Kill();

        var exePathReturnValue = new StringBuilder();
        FindExecutable(Path.GetFileName(imagePath), Path.GetDirectoryName(imagePath), exePathReturnValue);
        var exePath = exePathReturnValue.ToString();
        var arguments = "\"" + imagePath + "\"";

        // Handle cases where the default application is photoviewer.dll.
        if (Path.GetFileName(exePath).Equals("photoviewer.dll", StringComparison.InvariantCultureIgnoreCase))
        {
            arguments = "\"" + exePath + "\", ImageView_Fullscreen " + imagePath;
            exePath = "rundll32";
        }

        var process = new Process();
        process.StartInfo.FileName = exePath;
        process.StartInfo.Arguments = arguments;

        process.Start();
    }

    public static void ShowGraph(DotGraph graph)
    {
        string fileName = $"{"test"}.gv";

        graph.SaveToFile(fileName);

        var workroot = Path.GetDirectoryName(new FileInfo(fileName).FullName);
        var output = $"{Path.GetFileNameWithoutExtension(fileName)}.png";
        Process create = new Process();
        create.StartInfo.FileName = "dot";
        create.StartInfo.Arguments = $"-Tpng {Path.GetFileName(fileName)} -o {output}";
        create.StartInfo.WorkingDirectory = workroot;
        create.Start();
        create.WaitForExit();
        Helper.OpenImage($"{workroot}\\{output}");
    }

    public static void CreateGraph(string name, DotGraph graph)
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

        File.Delete(fileName);
    }

    public static DotGraph CreateDotGraph(IGraph<DebugVertex, DebugEdge> graph)
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

    public static string GetThisFilePath([CallerFilePath] string path = null) => path;
}
