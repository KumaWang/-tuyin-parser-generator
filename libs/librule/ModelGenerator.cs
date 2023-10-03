using libfsm;
using libgraph;
using librule.generater;
using librule.targets;
using System.Collections.Concurrent;
using System.Data;
using System.Reflection;

namespace librule
{
    public static class ModelGenerator
    {
        public static string Generate(string ctx)
        {
            return Generate(ctx, out var graph);
        }

        public static string Generate(string ctx, out DebugGraph debugGraph)
        {
            return Generate(ctx, false, out debugGraph);
        }

        public static string Generate(string ctx, bool isSimple, out DebugGraph debugGraph)
        {
            debugGraph = null;
            try
            {
                var dsl = new DSLParser().Parse(ctx);
                var builder = dsl.Generate(isSimple);
                var productionSRC = isSimple ? TargetBuilder.Simple(dsl,
                    dsl.Rules[dsl.GetOption("parser.entry")].GetReferences().Where(dsl.Rules.ContainsKey).Select(x => dsl.Rules[x]).ToArray()) :
                    builder.Create();

                debugGraph = CreateDebugGraph(builder.ProductionTable);
                return productionSRC ?? "unknown";
            }
            catch (DSLException<TokenMetadata> ex)
            {
                Console.WriteLine(ex.Message);
                debugGraph = CreateDebugGraph(ex.Table);
            }
            catch (FAException ex) 
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        private static DebugGraph CreateDebugGraph<TMetadata>(GraphTable<TMetadata> table)
        {
            //return CreateDebugGraph(productionGraph);

            // 创建debug graph
            var graph = new DebugGraph("debug");
            var states = new ConcurrentDictionary<int, DebugVertex>();
            var edges = new ConcurrentBag<DebugEdge>();

            void CheckStates(ushort left, ushort right, ushort subset)
            {
                if (!states.ContainsKey(left))
                    states[left] = new DebugVertex(left, left.ToString(), VertexFlags.None);

                if (!states.ContainsKey(right))
                    states[right] = new DebugVertex(right, right.ToString(), VertexFlags.None);

                if (subset != 0 && !states.ContainsKey(subset))
                    states[subset] = new DebugVertex(subset, subset.ToString(), VertexFlags.None);
            }

            foreach (var tran in table.Transitions)
            {
                CheckStates(tran.Left, tran.Right, tran.Symbol.Value);
     
                var input = table is TokenTable ? tran.Input.ToString() : table.Graph.Lexicon.Tokens[tran.Input.Chars[0]].Description;
                var header = tran.Symbol.Type.HasFlag(FASymbolType.Request) ? $"jump->{tran.Symbol.Value}" : tran.Input.Chars[0] == table.Graph.Lexicon.Missing.Index ? "miss" : $"match->{input}";
                var ctx = table.GetMetadataDescrption(tran.Metadata, header);
                var edge = new DebugEdge(
                    tran.Symbol.Type.HasFlag(FASymbolType.Report) ? EdgeFlags.SpecialPoint : EdgeFlags.None,
                    ctx,
                    states[tran.Left],
                    states[tran.Right],
                    tran.Symbol.Value == 0 ? null : states[tran.Symbol.Value]);

                edges.Add(edge);
            }

            foreach (var edge in edges)
                graph.AddEdge(edge);

            return graph;
        }
    }
}
