using libgraph;
using System.Collections.Generic;

namespace libflow
{
    public class FlowStepConcatenation<TVertex, TEdge> : FlowStep<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        private FlowStep<TVertex, TEdge> left;
        private FlowStep<TVertex, TEdge> right;

        public FlowStepConcatenation(FlowStep<TVertex, TEdge> left, FlowStep<TVertex, TEdge> right)
        {
            this.left = left;
            this.right = right;
        }

        public override FlowStepType StepType => FlowStepType.Concatenation;

        public FlowStep<TVertex, TEdge> Left => left;

        public FlowStep<TVertex, TEdge> Right => right;

        public override IEnumerable<TEdge> GetSources()
        {
            return Left.GetSources();
        }

        public override IEnumerable<TEdge> GetTargets()
        {
            return Right.GetTargets();
        }

        public override IEnumerable<FlowStep<TVertex, TEdge>> GetChildrens()
        {
            yield return Left;
            yield return Right;
        }

    }
}
