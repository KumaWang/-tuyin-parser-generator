using librule.generater;
using librule.utils;

namespace librule.expressions
{
    class CharSetExpression<TAction> : RegularExpression<TAction>
    {
        internal override RegularExpressionType ExpressionType => RegularExpressionType.CharSet;

        public GraphEdgeValue Token { get; }

        public CharSetExpression(bool xor, params char[] chars)
        {
            Token = new GraphEdgeValue(xor, chars);
        }

        public CharSetExpression(params char[] chars)
            : this(false, chars)
        {
        }

        internal CharSetExpression(GraphEdgeValue vector)
        {
            Token = vector;
        }

        protected internal override RegularExpression<TAction> Merge(RegularExpression<TAction> right)
        {
            if (right.ExpressionType == RegularExpressionType.CharSet)
                return new CharSetExpression<TAction>(Token.Merge((right as CharSetExpression<TAction>).Token));

            if (right.ExpressionType == RegularExpressionType.Symbol)
                return new CharSetExpression<TAction>(Token.Merge((right as SymbolExpression<TAction>).Token));

            return base.Merge(right);
        }

        internal override int GetMaxLength()
        {
            return 1;
        }

        internal override int GetMinLength()
        {
            return 1;
        }

        internal override int RepeatLevel()
        {
            return 1;
        }

        public override string GetDescrption()
        {
            return Token.ToString();
        }

        internal override string GetClearString()
        {
            var chars = Token.GetChars(char.MaxValue);
            if (chars.OverCount(1))
            {
                return null;
            }

            try
            {
                return chars.First().ToString();
            }
            catch
            {
                return null;
            }
        }

        internal override IGraphEdgeStep<TMetadata> InternalCreate<TMetadata>(GraphFigure<TMetadata, TAction> figure, IGraphEdgeStep<TMetadata> step, TMetadata metadata)
        {
            var next = figure.State();
            for (var i = 0; i < step.End.Length; i++)
                figure.Edge(step.End[i], next, Token, metadata);

            return new NextStep<TMetadata>(step.End, next);
        }

        internal override int GetCompuateHashCode()
        {
            return Token.GetHashCode();
        }

        public override RegularExpression<TAction> ExtractExclusionExpression()
        {
            return this;
        }
    }
}
