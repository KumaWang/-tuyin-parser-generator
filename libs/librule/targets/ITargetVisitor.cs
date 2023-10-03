using librule.generater;

namespace librule.targets
{
    interface ITargetVisitor
    {
        TargetBuilder TargetBuilder { get; }

        string Create(ProductionGraph productionGraph, ProductionTable prodctionTable, TokenTableManager manager);
    }
}
