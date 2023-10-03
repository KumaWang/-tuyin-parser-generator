using GiGraph.Dot.Entities.Graphs;
using GiGraph.Dot.Extensions;
using libflow;
using libgraph;
using System.Diagnostics;

namespace test_bench
{
    abstract class TestBase<TVertex, TEdge> : ITest
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        public abstract string TestName { get; }

        public void Run() 
        {
            var enu = GetGraphs().GetEnumerator();
            var index = 0;
            try
            {
                while (enu.MoveNext())
                {
                    Console.WriteLine($"// === 开始进行{TestName}第{++index}次测试 ===");
                    foreach (var step in FlowAnalyzer<TVertex, TEdge>.GetPaths(enu.Current).Select(x => x.Step))
                        Console.WriteLine(step.ToString(true));

                    Console.WriteLine();
                }
            }
            catch (GraphException<TVertex, TEdge> ex)
            {
                ShowGraph(CreateDotGraph(enu.Current, ex));
                Console.WriteLine(ex.Message);
            }
        }

        public void RunWithoutStep()
        {
            var enu = GetGraphs().GetEnumerator();
            try
            {
                while (enu.MoveNext())
                {
                }
            }
            catch (GraphException<TVertex, TEdge> ex)
            {
                ShowGraph(CreateDotGraph(enu.Current, ex));
                Console.WriteLine(ex.Message);
            }
        }

        protected abstract IEnumerable<IGraph<TVertex, TEdge>> GetGraphs();

        protected abstract DotGraph CreateDotGraph(IGraph<TVertex, TEdge> graph, GraphException<TVertex, TEdge> ex);

        protected void ShowGraph(DotGraph graph) 
        {
            string fileName = $"{TestName}.gv";

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
        }
    }
}
