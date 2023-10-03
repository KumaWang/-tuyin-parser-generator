using libgraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libflow
{
    public class FlowNode<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        public FlowNode()
        {
            Nodes = new FlowNodeCollection<TVertex, TEdge>(this);
        }

        public FlowNode(FlowLine<TVertex, TEdge> from)
            : this()
        {
            From = from;
        }

        public FlowLine<TVertex, TEdge> From { get; internal set; }

        public FlowNode<TVertex, TEdge> Parent { get; internal set; }

        public FlowNodeCollection<TVertex, TEdge> Nodes { get; }

        public IEnumerable<FlowNode<TVertex, TEdge>> Walk() 
        {
            yield return this;
            foreach(var subNode in Nodes.SelectMany(x => x.Walk()))
                yield return subNode;
        }

        public IEnumerable<FlowNode<TVertex, TEdge>> Where(Func<FlowNode<TVertex, TEdge>, bool> predicate)
        {
            if (predicate(this)) yield return this;
            foreach (var next in Nodes.SelectMany(x => x.Where(predicate)))
                yield return next;
        }

        public IEnumerable<FlowNode<TVertex, TEdge>> GetEnds()
        {
            if (Nodes.Count > 0)
            {
                foreach (var next in Nodes.SelectMany(x => x.GetEnds()))
                    yield return next;
            }
            else yield return this;
        }
    }
}
