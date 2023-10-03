using System;
using libgraph;

namespace libflow
{
    public abstract class FlowStepVisitor<TVertex, TEdge, TResult>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        public virtual TResult Visit(FlowStep<TVertex, TEdge> step)
        {
            switch (step.StepType)
            {
                case FlowStepType.Fork: return VisitFork(step as FlowStepFork<TVertex, TEdge>);
                case FlowStepType.Upward: return VisitUpward(step as FlowStepUpward<TVertex, TEdge>);
                case FlowStepType.Downward: return VisitDownward(step as FlowStepDownward<TVertex, TEdge>);
                case FlowStepType.Next: return VisitNext(step as FlowStepNext<TVertex, TEdge>);
                case FlowStepType.Loop: return VisitLoop(step as FlowStepLoop<TVertex, TEdge>);
                case FlowStepType.Concatenation: return VisitConcatenation(step as FlowStepConcatenation<TVertex, TEdge>);
            }

            throw new NotImplementedException();
        }

        protected virtual TResult VisitDownward(FlowStepDownward<TVertex, TEdge> step)
        {
            throw new NotImplementedException();
        }

        protected virtual TResult VisitConcatenation(FlowStepConcatenation<TVertex, TEdge> step)
        {
            throw new NotImplementedException();
        }

        protected virtual TResult VisitLoop(FlowStepLoop<TVertex, TEdge> step)
        {
            throw new NotImplementedException();
        }

        protected virtual TResult VisitNext(FlowStepNext<TVertex, TEdge> step)
        {
            throw new NotImplementedException();
        }

        protected virtual TResult VisitUpward(FlowStepUpward<TVertex, TEdge> step)
        {
            throw new NotImplementedException();
        }

        protected virtual TResult VisitFork(FlowStepFork<TVertex, TEdge> step)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class FlowStepVisitor<TVertex, TEdge, TResult, TSender>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        public virtual TResult Visit(TSender sender, FlowStep<TVertex, TEdge> step)
        {
            switch (step.StepType)
            {
                case FlowStepType.Fork: return VisitFork(sender, step as FlowStepFork<TVertex, TEdge>);
                case FlowStepType.Upward: return VisitUpward(sender, step as FlowStepUpward<TVertex, TEdge>);
                case FlowStepType.Downward: return VisitDownward(sender, step as FlowStepDownward<TVertex, TEdge>);
                case FlowStepType.Next: return VisitNext(sender, step as FlowStepNext<TVertex, TEdge>);
                case FlowStepType.Loop: return VisitLoop(sender, step as FlowStepLoop<TVertex, TEdge>);
                case FlowStepType.Concatenation: return VisitConcatenation(sender, step as FlowStepConcatenation<TVertex, TEdge>);
            }

            throw new NotImplementedException();
        }

        protected virtual TResult VisitDownward(TSender sender, FlowStepDownward<TVertex, TEdge> step)
        {
            throw new NotImplementedException();
        }

        protected virtual TResult VisitConcatenation(TSender sender, FlowStepConcatenation<TVertex, TEdge> step)
        {
            throw new NotImplementedException();
        }

        protected virtual TResult VisitLoop(TSender sender, FlowStepLoop<TVertex, TEdge> step)
        {
            throw new NotImplementedException();
        }

        protected virtual TResult VisitNext(TSender sender, FlowStepNext<TVertex, TEdge> step)
        {
            throw new NotImplementedException();
        }

        protected virtual TResult VisitUpward(TSender sender, FlowStepUpward<TVertex, TEdge> step)
        {
            throw new NotImplementedException();
        }

        protected virtual TResult VisitFork(TSender sender, FlowStepFork<TVertex, TEdge> step)
        {
            throw new NotImplementedException();
        }
    }
}
