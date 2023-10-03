namespace Tuitor.packages.richtext.format.parsers.markdown;

internal unsafe class MarkdownLiteral : MarkdownItem
{
    private char* input;
    private Match loc1;

    public MarkdownLiteral(char* input, Match loc1)
    {
        this.input = input;
        this.loc1 = loc1;
    }

    public string Context => loc1.GetContent(input);

    public override SourceSpan GetSourceSpan() => loc1.SourceSpan;
}
