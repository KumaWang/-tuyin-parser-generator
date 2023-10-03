using librule.generater;

namespace librule.targets.dot
{
    internal class DotTargetVisitor : ITargetVisitor
    {
        public DotTargetVisitor(TargetBuilder builder)
        {
            TargetBuilder = builder;
        }

        public TargetBuilder TargetBuilder { get; }

        public string Create(ProductionGraph productionGraph, ProductionTable prodctionTable, TokenTableManager manager)
        {
            return "Dot Not Implemented";
        }
    }
}