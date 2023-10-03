using libgraph;

namespace libflow.steps
{
    internal class SetOwnerLayer<TVertex, TEdge> : AnalysisStep<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        where TVertex : IVertex
    {
        protected internal override FlowFigure<TVertex, TEdge> Run(FlowFigure<TVertex, TEdge> figure, GraphModel<TVertex, TEdge> model)
        {
            for (var i = 0; i < figure.Layers.Count; i++) 
            {
                var layer = figure.Layers[i];
                foreach (var node in layer.Tree.Walk()) 
                {
                    if (node.From.Edge != null) 
                    {
                        figure.GraphFigure.OwnerLayer[node.From.Edge.Source.Index] = layer.State;
                    }
                }
            }

            return figure;
        }
    }
}
