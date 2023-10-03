namespace Tuitor.packages.richtext.format
{
    struct ParseError
    {
        public ParseError(ParseErrorLevel level, int id, SourceSpan sourceSpan, string message)
        {
            Level = level;
            Id = id;
            SourceSpan = sourceSpan;
            Message = message;
        }

        public ParseErrorLevel Level { get; }

        public int Id { get; }

        public SourceSpan SourceSpan { get; }

        public string Message { get; }
    }
}
