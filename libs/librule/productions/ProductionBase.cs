using librule.generater;

namespace librule.productions
{
    abstract class ProductionBase<TAction>
    {
        internal static ProductionBase<TAction>[] EMPTY_CHILDRENS = new ProductionBase<TAction>[0];

        internal abstract IEnumerable<Token> GetTokens(Production<TAction> self);

        public virtual string ProductionName { get; set; }

        public abstract ProductionType ProductionType { get; }

        public IGraphEdgeStep<TMetadata> Create<TMetadata>(GraphFigure<TMetadata, TAction> figure, IGraphEdgeStep<TMetadata> last, IGraphEdgeStep<TMetadata> entry) where TMetadata : struct
        {
            return InternalCreate(figure, last, entry);
        }

        internal abstract IGraphEdgeStep<TMetadata> InternalCreate<TMetadata>(GraphFigure<TMetadata, TAction> figure, IGraphEdgeStep<TMetadata> last, IGraphEdgeStep<TMetadata> entry) where TMetadata : struct;

        public abstract IEnumerable<ProductionBase<TAction>> GetChildrens();

        public static ProductionBase<TAction> operator |(ProductionBase<TAction> p1, ProductionBase<TAction> p2)
        {
            return new OrProduction<TAction>(p1, p2);
        }

        public abstract int GetCompuateHashCode();
    }
}
