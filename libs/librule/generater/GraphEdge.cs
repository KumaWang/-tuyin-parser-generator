using libgraph;

namespace librule.generater
{
    public class GraphEdge<TMetadata> : IEdge<GraphState<TMetadata>>
    {
        public EdgeFlags Flags { get; internal set; }

        public TMetadata Metadata { get; internal set; }

        public GraphState<TMetadata> Source { get; internal set; }

        public GraphState<TMetadata> Target { get; internal set; }

        public GraphEdgeValue Value { get; internal set; }

        public GraphState<TMetadata> Subset { get; internal set; }

        public string Descrption { get; internal set; }

        internal SourceLocation? SourceLocation { get; set; }

        internal string FromProduction { get; set; }

        internal bool IsEntry { get; set; }

        internal GraphEdge(GraphState<TMetadata> source, GraphState<TMetadata> target, GraphEdgeValue value, TMetadata metadata)
        {
            Source = source;
            Target = target;
            Value = value;
            Metadata = metadata;
        }

        internal GraphEdge(GraphState<TMetadata> source, GraphState<TMetadata> target, GraphState<TMetadata> subset, GraphEdgeValue value, TMetadata metadata)
        {
            Source = source;
            Target = target;
            Subset = subset;
            Value = value;
            Metadata = metadata;
        }

        public bool Contains(char val)
        {
            return Value.Contains(val);
        }

        public EdgeInput GetInput()
        {
            return new EdgeInput(Value.Xor, Value.Chars);
        }

        public override string ToString()
        {
            return $"{Source.Index}->{Target.Index}";
        }
    }
}
