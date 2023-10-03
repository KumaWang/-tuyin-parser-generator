using libgraph;
using System.Collections.ObjectModel;

namespace libflow
{
    public class FlowNodeCollection<TVertex, TEdge> : Collection<FlowNode<TVertex, TEdge>>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        private FlowNode<TVertex, TEdge> node;

        public FlowNodeCollection(FlowNode<TVertex, TEdge> node)
        {
            this.node = node;
        }

        protected override void InsertItem(int index, FlowNode<TVertex, TEdge> item)
        {
            item.Parent = node;
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            this[index].Parent = null;
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, FlowNode<TVertex, TEdge> item)
        {
            item.Parent = node;
            base.SetItem(index, item);
        }
    }
}
