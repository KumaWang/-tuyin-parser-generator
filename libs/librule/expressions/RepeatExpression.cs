using librule.generater;

namespace librule.expressions
{
    class RepeatExpression<TAction> : RegularExpression<TAction>
    {
        internal override RegularExpressionType ExpressionType => RegularExpressionType.Repeat;

        public RegularExpression<TAction> Expression { get; }

        public RepeatExpression(RegularExpression<TAction> exp)
        {
            Expression = exp;
        }

        public override IEnumerable<RegularExpression<TAction>> GetLast()
        {
            return Expression.GetLast();
        }

        public override RegularExpression<TAction> ExtractExclusionExpression()
        {
            return new RepeatExpression<TAction>(Expression.ExtractExclusionExpression());
        }

        public override string GetDescrption()
        {
            return $"{Expression.GetDescrption()}*";
        }

        internal override int GetMinLength()
        {
            return 0;
        }

        internal override int GetMaxLength()
        {
            return Expression.GetMaxLength();
        }

        internal override int RepeatLevel()
        {
            return Expression.RepeatLevel() + 1000;
        }

        internal override IGraphEdgeStep<TMetadata> InternalCreate<TMetadata>(GraphFigure<TMetadata, TAction> figure, IGraphEdgeStep<TMetadata> step, TMetadata metadata)
        {
            figure.GraphBox.StartCollect();
            var first = Expression.CreateGraphState(figure, step, metadata);
            var edges = figure.GraphBox.EndCollect();

            var starts = edges.Where(x => step.End.Contains(x.Source)).ToArray();
            var ends = edges.Where(x => first.End.Contains(x.Target)).ToArray();
            var group = edges.GroupBy(x => x.Target.Index).ToDictionary(x => x.Key, x => x.ToArray());

            foreach (var end in ends)    
            {
                foreach (var start in FindStarts(group, starts, end))
                {
                    if (start.Source == figure.Main)
                    {
                        var edge = figure.Edge(end.Target, start.Target, start.Value, start.Metadata);
                        edge.Descrption = start.Descrption;
                        edge.SourceLocation = start.SourceLocation;
                        edge.FromProduction = start.FromProduction;
                        edge.IsEntry = start.IsEntry;
                    }
                    else
                    {
                        figure.Remove(end);
                        var edge = figure.Edge(end.Source, start.Source, end.Value, end.Metadata);
                        edge.Descrption = start.Descrption;
                        edge.SourceLocation = start.SourceLocation;
                        edge.FromProduction = start.FromProduction;
                        edge.IsEntry = start.IsEntry;
                    }
                }
            }

            return first;
        }

        private static HashSet<GraphEdge<TMetadata>> FindStarts<TMetadata>(IDictionary<ushort, GraphEdge<TMetadata>[]> edges, GraphEdge<TMetadata>[] starts, GraphEdge<TMetadata> end) 
        {
            // 持续向上搜索
            var visitor = new HashSet<GraphEdge<TMetadata>>();
            var stacks = new Stack<GraphEdge<TMetadata>>();
            var results = new HashSet<GraphEdge<TMetadata>>();
            stacks.Push(end);

            while(stacks.Count > 0) 
            {
                var edge = stacks.Pop();
                if (visitor.Contains(edge))
                    continue;

                visitor.Add(edge);

                if (starts.Contains(edge))
                    results.Add(edge);

                var lefts = edges.ContainsKey(edge.Source.Index) ? edges[edge.Source.Index] : null;
                if (lefts != null)
                    foreach (var left in lefts)
                        stacks.Push(left);
            }

            return results;
        }

        internal override string GetClearString()
        {
            return null;
        }

        internal override int GetCompuateHashCode()
        {
            return Expression.GetCompuateHashCode() ^ 204;
        }
    }
}
