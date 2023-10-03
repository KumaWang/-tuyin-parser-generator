namespace librule.targets.code
{
    class CodeFormatterBlockEnd
    {
        public CodeFormatterBlockEnd(string context, bool immediately, bool pop = false)
        {
            Pop = pop;
            Context = context;
            Immediately = immediately;
        }

        public bool Pop { get; }

        public string Context { get; }

        public bool Immediately { get; }
    }
}
