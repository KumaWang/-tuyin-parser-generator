using librule.generater;

namespace librule.expressions
{
    class EmptyExpression<TAction> : RegularExpression<TAction>
    {
        internal override RegularExpressionType ExpressionType => RegularExpressionType.Empty;

        public EmptyExpression()
        {
        }

        public override string GetDescrption()
        {
            return string.Empty;
        }

        internal override int GetMaxLength()
        {
            return 0;
        }

        internal override int GetMinLength()
        {
            return 0;
        }

        internal override int RepeatLevel()
        {
            return 0;
        }

        internal override IGraphEdgeStep<TMetadata> InternalCreate<TMetadata>(GraphFigure<TMetadata, TAction> figure, IGraphEdgeStep<TMetadata> step, TMetadata metadata)
        {
            return step;
        }

        internal override string GetClearString()
        {
            return null;
        }

        internal override int GetCompuateHashCode()
        {
            return 0;
        }

        public override RegularExpression<TAction> ExtractExclusionExpression()
        {
            return this;
        }
    }
}
