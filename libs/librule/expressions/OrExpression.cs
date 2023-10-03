using librule.generater;

namespace librule.expressions
{
    class OrExpression<TAction> : RegularExpression<TAction>
    {
        private bool mHasEmpty;

        internal override RegularExpressionType ExpressionType => RegularExpressionType.Or;

        public List<RegularExpression<TAction>> Forks { get; }

        public OrExpression(RegularExpression<TAction> left, RegularExpression<TAction> right)
        {
            Forks = Simplify(left, right);

            var empties = Forks.Where(X => X.ExpressionType == RegularExpressionType.Empty).ToArray();
            Forks.RemoveAll(X => empties.Contains(X));

            mHasEmpty = empties.Length > 0;
            if (mHasEmpty)
            {
                var empty = empties.FirstOrDefault();
                if (empty != null)
                {
                    Forks.Add(empty);
                }
            }
        }

        public override IEnumerable<RegularExpression<TAction>> GetLast()
        {
            return Forks.SelectMany(x => x.GetLast());
        }

        public override string GetDescrption()
        {
            return string.Join("|", Forks.Select(X => X.GetDescrption()));
        }

        internal override int GetMaxLength()
        {
            return Forks.Max(X => X.GetMaxLength());
        }

        internal override int GetMinLength()
        {
            return Forks.Max(X => X.GetMinLength());
        }

        internal override string GetClearString()
        {
            return null;
        }

        internal override int RepeatLevel()
        {
            return Forks.Max(x => x.RepeatLevel());
        }

        internal override IGraphEdgeStep<TMetadata> InternalCreate<TMetadata>(GraphFigure<TMetadata, TAction> figure, IGraphEdgeStep<TMetadata> step, TMetadata metadata)
        {
            return new ConnectStep<TMetadata>(Forks.Select(x => x.CreateGraphState(figure, step, metadata)).ToArray());
        }

        private List<RegularExpression<TAction>> Simplify(params RegularExpression<TAction>[] exps)
        {
            var prods = new List<RegularExpression<TAction>>();
            for (var i = 0; i < exps.Length; i++)
            {
                var prod = exps[i];
                if (prod.ExpressionType == RegularExpressionType.Or)
                {
                    prods.AddRange((prod as OrExpression<TAction>).Forks);
                }
                else
                {
                    prods.Add(prod);
                }
            }

            return prods;
        }

        internal override int GetCompuateHashCode()
        {
            var hash = Forks[0].GetCompuateHashCode();
            for (var i = 1; i < Forks.Count; i++)
                hash = hash ^ Forks[i].GetCompuateHashCode();

            return hash;
        }

        public override RegularExpression<TAction> ExtractExclusionExpression()
        {
            var exp = Forks[0].ExtractExclusionExpression();
            for (var i = 1; i < Forks.Count; i++)
                exp |= Forks[i].ExtractExclusionExpression();

            return exp;
        }
    }
}
