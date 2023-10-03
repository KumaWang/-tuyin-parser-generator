using librule.generater;

namespace librule.productions
{
    class EosProduction<TAction> : ProductionBase<TAction>
    {
        public override ProductionType ProductionType => ProductionType.Eos;

        public Token Token { get; }

        public EosProduction(Token token)
        {
            Token = token;
        }

        internal override IEnumerable<Token> GetTokens(Production<TAction> self)
        {
            yield return Token;
        }

        internal override IGraphEdgeStep<TMetadata> InternalCreate<TMetadata>(GraphFigure<TMetadata, TAction> figure, IGraphEdgeStep<TMetadata> last, IGraphEdgeStep<TMetadata> entry)
        {
            // 连接到figure
            var end = figure.State();
            foreach (var start in entry.End)
            {
                var edge = figure.Edge(start, end, new GraphEdgeValue(Token.Index), figure.GraphBox.GetDefaultMetadata());
                edge.Descrption = "ε";
                edge.Target = figure.GraphBox.Exit;
            }
            return new NextStep<TMetadata>(entry.End, end);
        }

        public override IEnumerable<ProductionBase<TAction>> GetChildrens()
        {
            return new ProductionBase<TAction>[0];
        }

        public override string ToString()
        {
            return Token.Description ?? Token.Expression.GetDescrption();
        }

        public override int GetCompuateHashCode()
        {
            return Token.Expression.GetCompuateHashCode();
        }
    }
}
