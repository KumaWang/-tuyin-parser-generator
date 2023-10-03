namespace librule.generater
{
    class TokenTableManager : Dictionary<string, ProductionTokenTable>
    {
        private Dictionary<int, ProductionTokenTable> states;
        private DSL dsl;

        public ProductionTokenTable this[ushort state] 
        {
            get { return states[state]; }
            set { states[state] = value; }
        }

        public TokenTableManager(DSL dsl) 
        {
            this.dsl = dsl;
            this.states = new Dictionary<int, ProductionTokenTable>();

            var skipNameIndex = 1;
            var skipTableName = Settings.SKIP_NAME;
            while (dsl.GetRefer(skipTableName) != null)
                skipTableName = $"{Settings.SKIP_NAME}{skipNameIndex++}";
         
            SkipTableName = skipTableName;
        }

        public string SkipTableName { get; }

        public Lexicon Lexicon => dsl.Lexicon;

        public bool ContainsState(int state) 
        {
            return states.ContainsKey(state);
        }
    }
}
