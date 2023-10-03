using libgraph;
using System;
using System.Collections.Generic;

namespace libflow
{
    public class FlowStepDownward<TVertex, TEdge> : FlowStepGoto<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {

        public FlowStepDownward(TEdge edge)
            : base(edge)
        {
        }

        public override FlowStepType StepType => FlowStepType.Downward;

        public override IEnumerable<FlowStep<TVertex, TEdge>> GetChildrens()
        {
            return Array.Empty<FlowStep<TVertex, TEdge>>();
        }
    }
}
