namespace librule.targets
{
    interface IFunctionConstructor
    {
        ushort EntryPoint { get; }

        string FunctionName { get; }

        string ReturnType { get; }

        IEnumerable<FunctionVariable> Args { get; }

        IEnumerable<FunctionVariable> Locals { get; }
    }
}
