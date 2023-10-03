using libgraph;

namespace librule.generater
{
    public class GraphState<TMetadata> : IVertex
    {
        public GraphStateFlags Flags { get; internal set; }

        public ushort Index { get; }

        public virtual string Descrption => Index.ToString();

        public virtual string DisplayName => Descrption;

        internal GraphState(ushort index, GraphStateFlags flags)
        {
            Index = index;
            Flags = flags;
        }
    }
}
