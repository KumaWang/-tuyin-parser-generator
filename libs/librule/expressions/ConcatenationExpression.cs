using librule.generater;

namespace librule.expressions
{
    class ConcatenationExpression<TAction> : RegularExpression<TAction>
    {
        internal override RegularExpressionType ExpressionType => RegularExpressionType.Concatenation;

        public List<RegularExpression<TAction>> Expressions { get; }

        public ConcatenationExpression(RegularExpression<TAction> left, RegularExpression<TAction> right)
        {
            Expressions = new List<RegularExpression<TAction>>();
            Expressions.Add(left);
            Expressions.Add(right);
        }

        public ConcatenationExpression(IEnumerable<RegularExpression<TAction>> regexs)
        {
            Expressions = new List<RegularExpression<TAction>>(regexs);
        }

        public override IEnumerable<RegularExpression<TAction>> GetLast()
        {
            return Expressions[^1].GetLast();
        }

        public override RegularExpression<TAction> ExtractExclusionExpression()
        {
            return new ConcatenationExpression<TAction>(Expressions.Select(x => x.ExtractExclusionExpression()));
        }

        public override string GetDescrption()
        {
            return string.Join(string.Empty, Expressions.Select(X => X.GetDescrption()));
        }

        internal override int GetMaxLength()
        {
            return Expressions.Sum(X => X.GetMaxLength());
        }

        internal override int GetMinLength()
        {
            return Expressions.Sum(X => X.GetMinLength());
        }

        internal override int RepeatLevel()
        {
            return Expressions.Sum(x => x.RepeatLevel());
        }

        internal override IGraphEdgeStep<TMetadata> InternalCreate<TMetadata>(GraphFigure<TMetadata, TAction> figure, IGraphEdgeStep<TMetadata> step, TMetadata metadata)
        {
            var expr = new IGraphEdgeStep<TMetadata>[Expressions.Count];
            var curr = step;
            for (var i = 0; i < Expressions.Count; i++)
            {
                var exp = Expressions[i];
                curr = exp.CreateGraphState(figure, curr, metadata);
                expr[i] = curr;
            }

            return new NextStep<TMetadata>(expr[0].Start, expr[expr.Length - 1].End);
        }

        internal override string GetClearString()
        {
            var str = string.Empty;
            for (var i = 0; i < Expressions.Count; i++)
            {
                var exp = Expressions[i];
                var expStr = exp.GetClearString();
                if (expStr == null)
                    return null;

                str = str + expStr;
            }

            return str;
        }

        internal override int GetCompuateHashCode()
        {
            var hash = Expressions[0].GetCompuateHashCode();
            for (var i = 1; i < Expressions.Count; i++)
                hash = hash ^ 201 ^ Expressions[i].GetCompuateHashCode();

            return hash;
        }
    }
}
