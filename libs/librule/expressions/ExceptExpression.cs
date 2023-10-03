using librule.generater;
using librule.utils;

namespace librule.expressions
{
    class ExceptExpression<TAction> : RegularExpression<TAction>
    {
        private static readonly GraphEdgeValue sEos = new GraphEdgeValue('\0');

        internal override RegularExpressionType ExpressionType => RegularExpressionType.Except;

        public GraphEdgeValue[] Values { get; }

        public ExceptExpression(string literal)
            : this(literal.ToCharArray(), null)
        {
        }

        public ExceptExpression(string literal, IEnumerable<char> with)
            : this(literal.ToCharArray(), with)
        {
        }

        public ExceptExpression(char[] chars, IEnumerable<char> with)
        {
            if (with != null)
            {
                Values = chars.Distinct().Select(x => new GraphEdgeValue(false, with.Concat(new char[] { x }).Distinct())).ToArray();
            }
            else
            {
                Values = chars.Distinct().Select(x => new GraphEdgeValue(x)).ToArray();
            }
        }

        public override RegularExpression<TAction> ExtractExclusionExpression()
        {
            RegularExpression<TAction> exp = new SymbolExpression<TAction>(Values[0].Chars[0]);
            for (var i = 1; i < Values.Length; i++)
                exp = new ConcatenationExpression<TAction>(exp, new SymbolExpression<TAction>(Values[i].Chars[0]));

            return exp;
        }

        public override string GetDescrption()
        {
            return string.Join(string.Empty, Values.Select(x => x.ToString()));
        }

        internal override IGraphEdgeStep<TMetadata> InternalCreate<TMetadata>(GraphFigure<TMetadata, TAction> figure, IGraphEdgeStep<TMetadata> step, TMetadata metadata)
        {
            var edges = new List<GraphEdge<TMetadata>>();
            var last = step.End.ToArray();

            if (last.Contains(figure.Main))
            {
                var ends = new List<GraphEdge<TMetadata>>();

                for (var i = 0; i < Values.Length; i++)
                {
                    var temp = new GraphState<TMetadata>[1];
                    temp[0] = figure.State();

                    last.Do(x =>
                    {
                        edges.Add(figure.Edge(x, temp[0], Values[i], metadata));
                    });

                    if (i == 0)
                        ends.AddRange(edges);

                    last = temp;
                }

                foreach (var edge in edges.Distinct().ToArray())
                {
                    // 创建相反token
                    var token = edge.Value.Reverse().Eliminate(sEos);

                    // 连接到终点
                    foreach (var end in ends)
                    {
                        var state = figure.State();
                        figure.Edge(edge.Source, state, token, edge.Metadata);
                        figure.Edge(state, state, end.Value.Reverse().Eliminate(sEos), edge.Metadata);
                        figure.Edge(state, end.Target, end.Value, edge.Metadata);
                    }
                }
            }
            else
            {
                for (var i = 0; i < Values.Length; i++)
                {
                    var temp = new GraphState<TMetadata>[1];
                    temp[0] = figure.State();

                    last.Do(x =>
                    {
                        edges.Add(figure.Edge(x, temp[0], Values[i], metadata));
                    });

                    last = temp;
                }

                foreach (var edge in edges.Distinct().ToArray())
                {
                    // 创建相反token
                    var token = edge.Value.Reverse().Eliminate(sEos);

                    // 连接到终点
                    foreach (var end in step.End)
                    {
                        figure.Edge(edge.Source, end, token, edge.Metadata);
                    }
                }
            }

            return new NextStep<TMetadata>(step.End, last);
        }

        internal override int GetMaxLength()
        {
            return Values.Length;
        }

        internal override int GetMinLength()
        {
            return Values.Length;
        }

        internal override int RepeatLevel()
        {
            return Values.Length;
        }

        internal override string GetClearString()
        {
            var str = string.Empty;
            for (var i = 0; i < Values.Length; i++)
            {
                var token = Values[i];
                var chars = token.GetChars(char.MaxValue);
                if (chars.OverCount(1))
                {
                    return null;
                }

                try
                {
                    str = str + chars.First().ToString();
                }
                catch
                {
                    return null;
                }
            }
            return str;
        }

        internal override int GetCompuateHashCode()
        {
            var hash = Values[0].GetHashCode();
            for (var i = 1; i < Values.Length; i++)
                hash = hash ^ 202 ^ Values[i].GetHashCode();

            return hash;
        }
    }
}
