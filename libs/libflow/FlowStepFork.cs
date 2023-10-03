using libgraph;
using System.Collections.Generic;
using System.Linq;

namespace libflow
{
    public class FlowStepFork<TVertex, TEdge> : FlowStep<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        public FlowStepFork(IList<FlowStep<TVertex, TEdge>> steps)
        {
            // 使用展开保证fork在同级
            Steps = Expand(steps);
        }

        public override FlowStepType StepType => FlowStepType.Fork;

        public IList<FlowStep<TVertex, TEdge>> Steps { get; }

        public override IEnumerable<TEdge> GetSources()
        {
            return Steps.SelectMany(x => x.GetSources());
        }

        public override IEnumerable<TEdge> GetTargets()
        {
            return Steps.SelectMany(x => x.GetTargets());
        }

        public override IEnumerable<FlowStep<TVertex, TEdge>> GetChildrens()
        {
            return Steps;
        }

        private IList<FlowStep<TVertex, TEdge>> Expand(IList<FlowStep<TVertex, TEdge>> forks)
        {
            var step = new List<FlowStep<TVertex, TEdge>>();
            for (var i = 0; i < forks.Count; i++)
            {
                var fork = forks[i];
                if (fork.StepType == FlowStepType.Fork)
                    step.AddRange((fork as FlowStepFork<TVertex, TEdge>).Steps);
                else
                    step.Add(fork);
            }

            return step;
        }
    }
}
