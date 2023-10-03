using librule.generater;

namespace librule.expressions
{
    class PositionExpression<TAction> : RegularExpression<TAction>
    {
        internal override RegularExpressionType ExpressionType => RegularExpressionType.Position;

        private RegularExpression<TAction> exp;
        private SourceLocation loc;

        public PositionExpression(RegularExpression<TAction> exp, SourceLocation loc) 
        {
            this.exp = exp;
            this.loc = loc;
        }

        public RegularExpression<TAction> Expression => exp;

        public SourceLocation Location => loc;

        public override IEnumerable<RegularExpression<TAction>> GetLast()
        {
            return exp.GetLast();
        }

        public override RegularExpression<TAction> ExtractExclusionExpression()
        {
            return new PositionExpression<TAction>(exp.ExtractExclusionExpression(), loc);
        }

        public override string GetDescrption()
        {
            return exp.GetDescrption();
        }

        internal override IGraphEdgeStep<TMetadata> InternalCreate<TMetadata>(GraphFigure<TMetadata, TAction> figure, IGraphEdgeStep<TMetadata> step, TMetadata metadata)
        {
            figure.GraphBox.StartCollect();
            var result = exp.CreateGraphState(figure, step, metadata);
            var edge = figure.GraphBox.EndCollect();

            var rights = edge.Where(x => result.End.Contains(x.Target)).ToArray();
            foreach (var right in rights)
                if (!right.SourceLocation.HasValue)
                    right.SourceLocation = loc;

            return result;
        }

        protected internal override RegularExpression<TAction> Merge(RegularExpression<TAction> right)
        {
            RegularExpression<TAction> merge = null;
            if (right is PositionExpression<TAction> pos)
                merge = exp.Merge(pos.exp);
            else
                merge = exp.Merge(right);

            if (merge != null)
                merge = new PositionExpression<TAction>(merge, loc);

            return merge;
        }

        internal override string GetClearString()
        {
            return exp.GetClearString();
        }

        internal override int GetCompuateHashCode()
        {
            return exp.GetCompuateHashCode();
        }

        internal override int GetMaxLength()
        {
            return exp.GetMaxLength();
        }

        internal override int GetMinLength()
        {
            return exp.GetMinLength();
        }

        internal override int RepeatLevel()
        {
            return exp.RepeatLevel();
        }
    }
}
