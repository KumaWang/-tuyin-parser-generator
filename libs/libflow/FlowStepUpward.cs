using libgraph;
using System;
using System.Collections.Generic;

namespace libflow
{
    public class FlowStepUpward<TVertex, TEdge> : FlowStepGoto<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        public FlowStepUpward(TEdge edge)
            : base(edge)
        {
        }

        public override FlowStepType StepType => FlowStepType.Upward;

        public override IEnumerable<FlowStep<TVertex, TEdge>> GetChildrens()
        {
            return Array.Empty<FlowStep<TVertex, TEdge>>();
        }
    }
}
