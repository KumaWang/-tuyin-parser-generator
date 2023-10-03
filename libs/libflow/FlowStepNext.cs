using libgraph;
using System;
using System.Collections.Generic;

namespace libflow
{
    public class FlowStepNext<TVertex, TEdge> : FlowStep<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        public FlowStepNext(TEdge edge)
        {
            Edge = edge;
        }

        public override FlowStepType StepType => FlowStepType.Next;

        public TEdge Edge { get; }

        public override IEnumerable<TEdge> GetSources()
        {
            yield return Edge;
        }

        public override IEnumerable<TEdge> GetTargets()
        {
            yield return Edge;
        }

        public override IEnumerable<FlowStep<TVertex, TEdge>> GetChildrens()
        {
            return Array.Empty<FlowStep<TVertex, TEdge>>();
        }
    }
}
