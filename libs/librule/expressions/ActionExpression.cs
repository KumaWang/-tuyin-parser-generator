using librule.generater;

namespace librule.expressions
{
    internal class ActionExpression<TAction> : RegularExpression<TAction>
    {
        private RegularExpression<TAction> exp;
        private TAction action;

        public ActionExpression(RegularExpression<TAction> exp, TAction action)
        {
            this.exp = exp;
            this.action = action;
        }

        internal override RegularExpressionType ExpressionType => RegularExpressionType.Action;

        public RegularExpression<TAction> Expression => exp;

        public TAction Action => action;

        public override IEnumerable<RegularExpression<TAction>> GetLast()
        {
            return exp.GetLast();
        }

        public override string GetDescrption()
        {
            return $"{{{this.action}}}";
        }

        internal override IGraphEdgeStep<TMetadata> InternalCreate<TMetadata>(GraphFigure<TMetadata, TAction> figure, IGraphEdgeStep<TMetadata> step, TMetadata metadata)
        {
            figure.GraphBox.StartCollect();
            var result = exp.CreateGraphState(figure, step, metadata);
            var edge = figure.GraphBox.EndCollect();

            var rights = edge.Where(x => result.End.Contains(x.Target)).ToArray();
            foreach (var right in rights)
            {
                var meta = figure.GraphBox.CreateMetadata(right, action, IsOptional);
                if (right.Metadata.Equals(figure.GetDefaultMetadata()))
                    right.Metadata = meta;
                else
                {
                    var metas = new TMetadata[2];
                    metas[1] = meta;
                    metas[0] = right.Metadata;
                    right.Metadata = figure.GraphBox.MergeMetadatas(metas);
                }
            }

            return result;
        }

        internal override string GetClearString()
        {
            return string.Empty;
        }

        internal override int GetCompuateHashCode()
        {
            return HashCode.Combine(exp.GetCompuateHashCode(), action.GetHashCode());
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

        public override RegularExpression<TAction> ExtractExclusionExpression()
        {
            return new ActionExpression<TAction>(exp.ExtractExclusionExpression(), Action);
        }
    }
}
