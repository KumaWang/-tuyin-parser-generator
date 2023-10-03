using libgraph;
using System.Collections.Generic;

namespace libflow
{
    public class FlowStepLoop<TVertex, TEdge> : FlowStep<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        public FlowStepLoop(IList<FlowStep<TVertex, TEdge>> steps)
        {
            Steps = steps;
        }

        public override FlowStepType StepType => FlowStepType.Loop;

        public IList<FlowStep<TVertex, TEdge>> Steps { get; }

        public override IEnumerable<TEdge> GetSources()
        {
            return Steps[0].GetSources();
        }

        public override IEnumerable<TEdge> GetTargets()
        {
            return Steps[^1].GetTargets();
        }

        public override IEnumerable<FlowStep<TVertex, TEdge>> GetChildrens()
        {
            return Steps;
        }
    }
}
