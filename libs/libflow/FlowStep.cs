using System.Collections.Generic;
using System.Linq;
using libgraph;

namespace libflow
{
    public abstract class FlowStep<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        public abstract FlowStepType StepType { get; }

        public string Descrption { get; internal set; }

        public abstract IEnumerable<TEdge> GetSources();

        public abstract IEnumerable<TEdge> GetTargets();

        public abstract IEnumerable<FlowStep<TVertex, TEdge>> GetChildrens();

        public virtual IEnumerable<FlowStep<TVertex, TEdge>> Walk()
        {
            yield return this;
            foreach (var child in GetChildrens())
                foreach (var substmt in child.Walk())
                    yield return substmt;
        }

        public virtual IEnumerable<FlowStep<TVertex, TEdge>> Next()
        {
            var first = GetChildrens().FirstOrDefault();
            if (first != null)
            {
                yield return first;
                foreach (var child in first.Next())
                    yield return child;
            }
        }


        public virtual TResult Visit<TResult>(FlowStepVisitor<TVertex, TEdge, TResult> visitor) => visitor.Visit(this);

        public virtual TResult Visit<TResult, TSender>(TSender sender, FlowStepVisitor<TVertex, TEdge, TResult, TSender> visitor) => visitor.Visit(sender, this);

        public static FlowStep<TVertex, TEdge> From(IList<FlowStep<TVertex, TEdge>> steps)
        {
            var first = steps.First(x => x != null);
            for (var i = steps.IndexOf(first) + 1; i < steps.Count; i++)
                if (steps[i] != null)
                    first = new FlowStepConcatenation<TVertex, TEdge>(first, steps[i]);

            return first;
        }

        public override string ToString()
        {
            return Descrption ?? ToString(false);
        }

        public string ToString(bool horizontalOutput)
        {
            return Descrption = Visit(new FlowStepDescrptionBuilder<TVertex, TEdge>(horizontalOutput));
        }
    }
}
