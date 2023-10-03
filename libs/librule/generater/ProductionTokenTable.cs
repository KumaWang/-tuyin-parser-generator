namespace librule.generater
{
    record ProductionTokenTable
    {
        public ProductionTokenTable(TokenTable tokenTable, HashSet<ushort> supportTokens, bool isSkip, string name)
        {
            TokenTable = tokenTable;
            SupportTokens = supportTokens;
            IsSkip = isSkip;
            Name = name;
        }

        public TokenTable TokenTable { get; }

        public HashSet<ushort> SupportTokens { get; }

        public bool IsSkip { get; }

        public string Name { get; }
    }
}
