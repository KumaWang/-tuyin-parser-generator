using libgraph;

namespace test_bench
{
    internal class FixedTest : DebugGraphTest
    {
        public override string TestName => "FIXED";

        protected override IEnumerable<IGraph<DebugVertex, DebugEdge>> GetGraphs()
        {
            DebugVertex v1 = new DebugVertex(1, "1", VertexFlags.None);
            DebugVertex v2 = new DebugVertex(2, "2", VertexFlags.None);
            DebugVertex v3 = new DebugVertex(3, "3", VertexFlags.None);
            DebugVertex v4 = new DebugVertex(4, "4", VertexFlags.None);
            DebugVertex v5 = new DebugVertex(5, "5", VertexFlags.None);
            DebugVertex v6 = new DebugVertex(6, "6", VertexFlags.None);
            DebugVertex v7 = new DebugVertex(7, "7", VertexFlags.None);
            DebugVertex v8 = new DebugVertex(8, "8", VertexFlags.None);
            DebugVertex v9 = new DebugVertex(9, "9", VertexFlags.None);
            DebugVertex v10 = new DebugVertex(10, "10", VertexFlags.None);
            DebugVertex v11 = new DebugVertex(11, "11", VertexFlags.None);
            DebugVertex v12 = new DebugVertex(12, "12", VertexFlags.None);

            DebugGraph test_loop = new DebugGraph("");
            test_loop.AddEdge(new DebugEdge(EdgeFlags.None, "", v1, v2, null));
            test_loop.AddEdge(new DebugEdge(EdgeFlags.None, "", v2, v1, null));
            test_loop.AddEdge(new DebugEdge(EdgeFlags.None, "", v1, v3, null));
            test_loop.AddEdge(new DebugEdge(EdgeFlags.None, "", v3, v4, null));
            test_loop.AddEdge(new DebugEdge(EdgeFlags.None, "", v4, v1, null));
            yield return test_loop;

            DebugGraph test1 = new DebugGraph("");
            test1.AddEdge(new DebugEdge(EdgeFlags.None, "", v1, v2, null));
            test1.AddEdge(new DebugEdge(EdgeFlags.None, "", v2, v3, null));
            test1.AddEdge(new DebugEdge(EdgeFlags.None, "", v3, v4, null));
            test1.AddEdge(new DebugEdge(EdgeFlags.None, "", v4, v5, null));
            test1.AddEdge(new DebugEdge(EdgeFlags.None, "", v5, v6, null));
            test1.AddEdge(new DebugEdge(EdgeFlags.None, "", v6, v7, null));
            test1.AddEdge(new DebugEdge(EdgeFlags.None, "", v7, v8, null));

            test1.AddEdge(new DebugEdge(EdgeFlags.None, "", v6, v1, null));
            test1.AddEdge(new DebugEdge(EdgeFlags.None, "", v5, v3, null));
            test1.AddEdge(new DebugEdge(EdgeFlags.None, "", v4, v2, null));
            test1.AddEdge(new DebugEdge(EdgeFlags.None, "", v4, v9, null));
            test1.AddEdge(new DebugEdge(EdgeFlags.None, "", v9, v12, null));
            test1.AddEdge(new DebugEdge(EdgeFlags.None, "", v5, v10, null));
            test1.AddEdge(new DebugEdge(EdgeFlags.None, "", v10, v11, null));
            test1.AddEdge(new DebugEdge(EdgeFlags.None, "", v11, v6, null));
            yield return test1;


            DebugGraph test2 = new DebugGraph("");
            test2.AddEdge(new DebugEdge(EdgeFlags.None, "", v1, v2, null));
            test2.AddEdge(new DebugEdge(EdgeFlags.None, "", v2, v3, null));
            test2.AddEdge(new DebugEdge(EdgeFlags.None, "", v3, v4, null));
            test2.AddEdge(new DebugEdge(EdgeFlags.None, "", v4, v5, null));
            test2.AddEdge(new DebugEdge(EdgeFlags.None, "", v5, v6, null));
            test2.AddEdge(new DebugEdge(EdgeFlags.None, "", v6, v7, null));

            test2.AddEdge(new DebugEdge(EdgeFlags.None, "", v2, v1, null));
            test2.AddEdge(new DebugEdge(EdgeFlags.None, "", v4, v3, null));
            test2.AddEdge(new DebugEdge(EdgeFlags.None, "", v6, v3, null));
            yield return test2;

            DebugGraph test3 = new DebugGraph("");
            test3.AddEdge(new DebugEdge(EdgeFlags.None, "", v1, v2, null));
            test3.AddEdge(new DebugEdge(EdgeFlags.None, "", v2, v3, null));
            test3.AddEdge(new DebugEdge(EdgeFlags.None, "", v3, v4, null));

            test3.AddEdge(new DebugEdge(EdgeFlags.None, "", v1, v3, null));
            test3.AddEdge(new DebugEdge(EdgeFlags.None, "", v1, v8, null));

            test3.AddEdge(new DebugEdge(EdgeFlags.None, "", v4, v5, null));
            test3.AddEdge(new DebugEdge(EdgeFlags.None, "", v5, v6, null));

            test3.AddEdge(new DebugEdge(EdgeFlags.None, "", v4, v7, null));
            yield return test3;
        }
    }
}
