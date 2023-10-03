namespace librule.targets.code
{
    class CodeFormatterBlock
    {
        public CodeFormatterBlock(string start,int space, int token, params CodeFormatterBlockEnd[] ends)
        {
            Token = token;
            Start = start;
            Spcae = space;
            Ends = ends;
        }

        public int Token { get; }

        public string Start { get; }

        public CodeFormatterBlockEnd[] Ends { get; }

        public int Spcae { get; }
    }
}
