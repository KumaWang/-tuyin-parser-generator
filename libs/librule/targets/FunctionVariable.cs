namespace librule.targets
{
    record FunctionVariable
    {
        public FunctionVariable(string name, string type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }

        public string Type { get; set; }
    }
}
