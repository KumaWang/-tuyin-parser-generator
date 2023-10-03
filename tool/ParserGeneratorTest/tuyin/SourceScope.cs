namespace Tuitor.packages.richtext.format
{
    struct SourceScope
    {
        public SourceScope(SourceSpan sourceSpan, string display)
        {
            SourceSpan = sourceSpan;
            Display = display;
        }

        public SourceSpan SourceSpan { get; }

        public string Display { get; }
    }
}
