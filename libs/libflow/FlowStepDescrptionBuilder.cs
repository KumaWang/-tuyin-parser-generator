using System;
using System.Linq;
using libgraph;

namespace libflow
{
    class FlowStepDescrptionBuilder<TVertex, TEdge> : FlowStepVisitor<TVertex, TEdge, string>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        private OutputMode mMode;

        public FlowStepDescrptionBuilder(bool horizontalOutput)
        {
            HorizontalOutput = horizontalOutput;
        }

        public bool HorizontalOutput { get; set; }

        public override string Visit(FlowStep<TVertex, TEdge> step)
        {
            var result = base.Visit(step);
            step.Descrption = result;
            return result;
        }

        protected override string VisitConcatenation(FlowStepConcatenation<TVertex, TEdge> step)
        {
            var left = step.Left.Visit(this);
            var right = step.Right.Visit(this);
            return left + right;
        }

        protected override string VisitFork(FlowStepFork<TVertex, TEdge> step)
        {
            if (HorizontalOutput)
                return $"[{string.Join("|", step.Steps.Select(x => x.Visit(this)))}]";

            for (var i = 0; i < step.Steps.Count; i++)
            {
                mMode = OutputMode.First;
                step.Steps[i].Visit(this);
            }

            return string.Empty;
        }

        protected override string VisitLoop(FlowStepLoop<TVertex, TEdge> step)
        {
            if (HorizontalOutput)
            {
                var tmp = step.Steps[0].Visit(this);
                for (var i = 1; i < step.Steps.Count; i++)
                    tmp = tmp + step.Steps[i].Visit(this);

                return $"({tmp})";
            }
            else
            {
                return $"({step.Steps[^1].Visit(this)})";
            }
        }

        protected override string VisitNext(FlowStepNext<TVertex, TEdge> step)
        {
            return VisitEdgeStep(step);
        }

        protected override string VisitUpward(FlowStepUpward<TVertex, TEdge> step)
        {
            return "up" + VisitEdgeStep(step);
        }

        protected override string VisitDownward(FlowStepDownward<TVertex, TEdge> step)
        {
            return "dw" + VisitEdgeStep(step);
        }

        private string VisitEdgeStep(FlowStepNext<TVertex, TEdge> step)
        {
            switch (mMode)
            {
                case OutputMode.First:
                    mMode = OutputMode.Second;
                    return $"{step.Edge.Source.Index}->{step.Edge.Target.Index}";
                case OutputMode.Second:
                    return $"->{step.Edge.Target.Index}";
            }

            throw new NotImplementedException();
        }

        enum OutputMode
        {
            First,
            Second
        }
    }
}
