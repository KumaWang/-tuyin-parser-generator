using System;
using System.Collections.Generic;

namespace libgraph
{
    public struct DebugEdge : IEdge<DebugVertex>
    {
        public string Descrption { get; }

        public DebugVertex Source { get; }

        public DebugVertex Target { get; }

        public DebugVertex Subset { get; }

        public EdgeFlags Flags { get; }

        public string Tips { get; }

        public DebugEdge(EdgeFlags flags, string descrption, DebugVertex source, DebugVertex target, DebugVertex subset) 
            : this(flags, descrption, source, target, subset, null)
        {

        }

        public DebugEdge(EdgeFlags flags, string descrption, DebugVertex source, DebugVertex target, DebugVertex subset, string tips)
        {
            Flags = flags;
            Descrption = descrption;
            Source = source; 
            Target = target;
            Subset = subset;
            Tips = tips;
        }

        public EdgeInput GetInput()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            return obj is DebugEdge edge &&
                   EqualityComparer<int>.Default.Equals(Source?.Index ?? 0, edge.Source?.Index ?? 0) &&
                   EqualityComparer<int>.Default.Equals(Target?.Index ?? 0, edge.Target?.Index ?? 0) &&
                   Flags == edge.Flags;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Source?.Index ?? 0, Target?.Index ?? 0, Flags);
        }

        public override string ToString()
        {
            return $"{Source?.Index ?? 0}->{Target?.Index ?? 0}";
        }
    }
}
