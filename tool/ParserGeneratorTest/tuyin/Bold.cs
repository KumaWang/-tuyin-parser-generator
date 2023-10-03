namespace Tuitor.packages.richtext.format.parsers.markdown;

internal class Bold : MarkdownItem
{
    private MarkdownLiteral loc2;
    private SourceSpan sourceSpan;

    public Bold(MarkdownLiteral loc2, Match loc1, Match loc3)
    {
        this.loc2 = loc2;
        this.sourceSpan = new SourceSpan(loc1.SourceSpan.Start, loc3.SourceSpan.End);
    }

    public override SourceSpan GetSourceSpan() => sourceSpan;
}
