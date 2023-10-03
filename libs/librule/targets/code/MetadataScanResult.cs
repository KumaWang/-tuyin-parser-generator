namespace librule.targets.code
{
    record MetadataScanResult
    {
        public MetadataScanResult(TargetGraph targetGraph, IEnumerable<IFunctionConstructor> functionConstructors)
        {
            TargetGraph = targetGraph;
            FunctionConstructors = functionConstructors;
        }

        public TargetGraph TargetGraph { get; }

        public IEnumerable<IFunctionConstructor> FunctionConstructors { get; }
    }
}
