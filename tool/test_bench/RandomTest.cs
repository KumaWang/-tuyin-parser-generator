using libgraph;

namespace test_bench
{
    internal class RandomTest : DebugGraphTest
    {
        public override string TestName => "RANDOM";

        protected override IEnumerable<IGraph<DebugVertex, DebugEdge>> GetGraphs()
        {
            for (var i = 0; i < 10; i++)
                yield return CreateRandomGraph(15, 2, 2, 2);
        }

        static DebugGraph CreateRandomGraph(int verticesCount, int groupCount, int connectionsInGroup, int connectionsBetweenGroups)
        {
            Dictionary<DebugVertex, List<DebugVertex>> neighbors = new Dictionary<DebugVertex, List<DebugVertex>>();
            List<DebugVertex> vertices = new List<DebugVertex>();
            var result = new List<DebugEdge>();
            for (int i = 0; i < verticesCount; i++)
                vertices.Add(new DebugVertex((ushort)i, i.ToString(), VertexFlags.None));

            List<DebugVertex> GetNeighbors(DebugVertex vertex)
            {
                if (!neighbors.ContainsKey(vertex))
                    neighbors[vertex] = new List<DebugVertex>();

                return neighbors[vertex];
            }

            Random random = new Random();
            int interval = (verticesCount / groupCount);
            int random1 = 0;
            int random2 = 0;
            for (int i = 0; i < groupCount; i++)
            {
                //Console.WriteLine("Group " + i);
                for (int j = 0; j < interval; j++)
                {
                    random2 = i * interval + j;
                    while (GetNeighbors(vertices[random2]).Count < connectionsInGroup)
                    {
                        random1 = random.Next(i * interval, i * interval + interval);

                        if (!GetNeighbors(vertices[random2]).Contains(vertices[random1]) && (random2) != random1)
                        {
                            result.Add(new DebugEdge(EdgeFlags.None, "", vertices[random2], vertices[random1], null));

                            GetNeighbors(vertices[random2]).Add(vertices.First(x => x.Index == random1));
                            GetNeighbors(vertices[random1]).Add(vertices.First(x => x.Index == random2));
                            //Console.WriteLine("Connecting vertice " + vertices[random2].Id + "  =>  " + vertices[random1].Id);
                        }

                    }
                }
            }
            List<int> groupUsed = new List<int>();
            int group1 = random.Next(0, groupCount);

            List<Tuple<int, int>> connectedVectors = new List<Tuple<int, int>>();
            while (groupUsed.Count != groupCount)
            {
                int group2 = random.Next(0, groupCount);
                if (group1 != group2 && !groupUsed.Contains(group1))
                {
                    //Console.WriteLine("Connecting group " + group1 + " with group " + group2);

                    groupUsed.Add(group1);

                    for (int j = 0; j < connectionsBetweenGroups; j++)
                    {
                        random1 = random.Next(group1 * interval, group1 * interval + interval);
                        random2 = random.Next(group2 * interval, group2 * interval + interval);
                        bool isRepeated = connectedVectors.Any(v => (v.Item1 == random1 && v.Item2 == random2) || (v.Item1 == random2 && v.Item2 == random1));
                        if (isRepeated)
                        {
                            //Console.WriteLine("Skipping this attempt " + random1 + " => " + random2 + ". Repeat detected!");
                            j--;
                            continue;
                        }
                        //Console.WriteLine("Connecting vertice " + vertices[random1].Id + "  =>  " + vertices[random2].Id);
                        result.Add(new DebugEdge(EdgeFlags.None, "", vertices[random1], vertices[random2], null));

                        connectedVectors.Add(new Tuple<int, int>(random1, random2));

                        GetNeighbors(vertices[random1]).Add(vertices.First(x => x.Index == random2));
                        GetNeighbors(vertices[random2]).Add(vertices.First(x => x.Index == random1));
                    }
                }
                group1 = group2;
            }
            //Check if any left empty
            var emptyVertices = vertices.FindAll(vertex => GetNeighbors(vertex).Count == 0);
            if (emptyVertices.Count != 0)
            {
                foreach (var vertex in emptyVertices)
                {
                    var neighbour = vertices.FirstOrDefault(x => GetNeighbors(x).Count > 5);
                    if (neighbour != null)
                    {
                        result.Add(new DebugEdge(EdgeFlags.None, "", vertex, neighbour, null));
                        GetNeighbors(vertices[vertex.Index]).Add(vertices.First(x => x.Index == random2));
                        GetNeighbors(vertices[neighbour.Index]).Add(vertices.First(x => x.Index == vertex.Index));
                    }
                }
            }

            var graph = new DebugGraph("");
            foreach (var edge in result)
                graph.AddEdge(edge);

            return graph;
        }
    }
}
