using libgraph;

namespace librule.generater
{
    partial class GraphFigure<TMetadata, TAction> : GraphState<TMetadata>, IProductionGraph<TMetadata>
    {
        private List<GraphEdge<TMetadata>> mEdges;
        private List<GraphState<TMetadata>> mStates;

        public GraphBox<TMetadata, TAction> GraphBox { get; }

        public override string DisplayName { get; }

        public IReadOnlyList<GraphEdge<TMetadata>> Edges => mEdges;

        public IReadOnlyList<GraphState<TMetadata>> States => mStates;

        public GraphState<TMetadata> Main => this;

        public GraphState<TMetadata> Exit { get; }

        IEnumerable<GraphState<TMetadata>> IGraph<GraphState<TMetadata>, GraphEdge<TMetadata>>.Vertices => States;

        public Lexicon Lexicon => GraphBox.Lexicon;

        internal GraphFigure(ushort index, GraphBox<TMetadata, TAction> box, string figureName)
            : base(index, GraphStateFlags.Figure)
        {
            GraphBox = box;
            DisplayName = figureName;

            mEdges = new List<GraphEdge<TMetadata>>();
            mStates = new List<GraphState<TMetadata>>();

            Exit = GraphBox.Exit;
        }

        internal GraphFigure(ushort index, GraphBox<TMetadata, TAction> box, string figureName, GraphState<TMetadata> exit)
            : this(index, box, figureName)
        {
            Exit = exit;
        }

        public GraphState<TMetadata> State()
        {
            var state = GraphBox.State();
            mStates.Add(state);
            return state;
        }

        public GraphState<TMetadata> Empty()
        {
            var state = GraphBox.Empty();
            mStates.Add(state);
            return state;
        }

        public GraphEdge<TMetadata> Edge(GraphState<TMetadata> left, GraphState<TMetadata> right, GraphState<TMetadata> subset, GraphEdgeValue value, TMetadata metadata)
        {
            var edge = GraphBox.Edge(left, right, subset, value, metadata);
            edge.Descrption = value.ToString();
            mEdges.Add(edge);
            return edge;
        }

        public GraphEdge<TMetadata> Edge(GraphState<TMetadata> left, GraphState<TMetadata> right, GraphEdgeValue value, TMetadata metadata)
        {
            return Edge(left, right, null, value, metadata);
        }

        public void Remove(GraphEdge<TMetadata> edge)
        {
            GraphBox.Remove(edge);
            mEdges.Remove(edge);
        }

        public int GetMetadataCompreValue(TMetadata metadata)
        {
            return GraphBox.GetMetadataCompreValue(metadata);
        }

        public TMetadata GetDefaultMetadata()
        {
            return GraphBox.GetDefaultMetadata();
        }

        public IEnumerable<GraphEdge<TMetadata>> GetLefts(GraphState<TMetadata> state)
        {
            return mEdges.AsParallel().Where(x => x.Target == state);
        }

        public IEnumerable<GraphEdge<TMetadata>> GetRights(GraphState<TMetadata> state)
        {
            return mEdges.AsParallel().Where(x => x.Source == state);
        }

        public short GetActionNumber(params TAction[] actions)
        {
            return GraphBox.GetActionNumber(actions);
        }

        public IList<TAction> GetAction(short num)
        {
            return GraphBox.GetAction(num);
        }

        public GraphTable<TMetadata> Tabulate()
        {
            return GraphBox.Tabulate(this);
        }

        public GraphEdgeValue GetMissValue()
        {
            return GraphBox.GetMissValue();
        }
    }
}
