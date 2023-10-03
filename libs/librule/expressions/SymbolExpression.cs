using librule.generater;

namespace librule.expressions
{
    class SymbolExpression<TAction> : RegularExpression<TAction>
    {
        private string mClearString;

        internal override RegularExpressionType ExpressionType => RegularExpressionType.Symbol;

        public GraphEdgeValue Token { get; }

        public SymbolExpression(char c)
        {
            Token = new GraphEdgeValue(c);
            mClearString = c.ToString();
        }

        protected internal override RegularExpression<TAction> Merge(RegularExpression<TAction> right)
        {
            if (right.ExpressionType == RegularExpressionType.CharSet)
                return new CharSetExpression<TAction>(Token.Merge((right as CharSetExpression<TAction>).Token));

            if (right.ExpressionType == RegularExpressionType.Symbol)
                return new CharSetExpression<TAction>(Token.Merge((right as SymbolExpression<TAction>).Token));

            return base.Merge(right);
        }

        public override RegularExpression<TAction> ExtractExclusionExpression()
        {
            return this;
        }

        public override string GetDescrption()
        {
            return Token.ToString();
        }

        internal override int GetMinLength()
        {
            return 1;
        }

        internal override int GetMaxLength()
        {
            return 1;
        }

        internal override int RepeatLevel()
        {
            return 1;
        }

        internal override IGraphEdgeStep<TMetadata> InternalCreate<TMetadata>(GraphFigure<TMetadata, TAction> figure, IGraphEdgeStep<TMetadata> step, TMetadata metadata)
        {
            var next = figure.State();
            for (var i = 0; i < step.End.Length; i++)
                figure.Edge(step.End[i], next, Token, metadata);

            return new NextStep<TMetadata>(step.End, next);
        }

        internal override string GetClearString()
        {
            return mClearString;
        }

        internal override int GetCompuateHashCode()
        {
            return Token.GetHashCode();
        }
    }
}
