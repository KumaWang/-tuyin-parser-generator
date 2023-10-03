using libfsm;
using libgraph;

namespace librule.generater
{
    class TokenGraph : GraphBox<TokenMetadata, TableAction>
    {
        public TokenGraph(bool clarity, Lexicon lexicon)
        {
            SupportClarity = clarity;
            Lexicon = lexicon;
        }

        public bool SupportClarity { get; }

        public override Lexicon Lexicon { get; }

        public override TokenMetadata CreateMetadata(GraphEdge<TokenMetadata> edge, TableAction action, bool skipToken)
        {
            return new TokenMetadata(GetActionNumber(action), 0);
        }

        public override TokenMetadata MergeMetadatas(IEnumerable<TokenMetadata> metadatas)
        {
            var token = metadatas.First().Token;
            var values = metadatas.Select(x => x.Value);
            var actions = values.SelectMany(GetAction).ToList();
            actions.Sort(TableActionComparer.Instance);
            return new TokenMetadata(GetActionNumber(actions), token);
        }

        public override GraphTable<TokenMetadata> Tabulate()
        {       
            return new TokenTable(this, FATableFlags.ConnectSubset | FATableFlags.Predicate);
        }

        public override GraphTable<TokenMetadata> Tabulate(GraphFigure<TokenMetadata, TableAction> figure)
        {
            var graph = new TokenGraph(SupportClarity, Lexicon);
            var graphFigure = graph.Figure(figure.DisplayName);
            var states = new Dictionary<GraphState<TokenMetadata>, GraphState<TokenMetadata>>();

            GraphState<TokenMetadata> GetState(GraphState<TokenMetadata> state) 
            {
                if (state == null) return null;
                if (state == figure.Main) return graphFigure.Main;
                if (state == figure.Exit) return graphFigure.Exit;

                if (!states.ContainsKey(state))
                    states[state] = graphFigure.State();

                return states[state];
            }

            foreach (var edge in figure.Edges) 
            {
                var left = GetState(edge.Source);
                var right = GetState(edge.Target);
                var subset = GetState(edge.Subset);

                var graphEdge = graphFigure.Edge(left, right, subset, edge.Value, edge.Metadata);
                graphEdge.Flags = edge.Flags;
                graphEdge.Descrption = edge.Descrption;
                graphEdge.SourceLocation = edge.SourceLocation;
                graphEdge.FromProduction = edge.FromProduction;
                graphEdge.IsEntry = edge.IsEntry;
            }

            return new TokenTable(graph, FATableFlags.ConnectSubset | FATableFlags.Predicate);
        }

        public override int GetMetadataCompreValue(TokenMetadata metadata)
        {
            return Lexicon.Tokens[metadata.Token].Expression.RepeatLevel() + (metadata.Token == 0 ? int.MaxValue / 2 : 0);
        }

        public override void TokenEdge(GraphEdge<TokenMetadata> edge, ushort token)
        {
            if (edge.Metadata.Token != 0 && edge.Metadata.Token != token)
                throw new NotImplementedException("内部错误:在构建Graph时不会出现多义词存于一条边的情况。");

            edge.Metadata = new TokenMetadata(edge.Metadata.Value, token);
            edge.Flags |= EdgeFlags.SpecialPoint;
        }

        public override GraphEdgeValue GetMissValue()
        {
            throw new NotImplementedException();
        }
    }
}
