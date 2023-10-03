namespace Tuitor.packages.richtext.format.parsers.markdown;
internal class Reference : MarkdownItem
{
    private MarkdownLiteral loc2;
    private SourceSpan sourceSpan;

    public Reference(MarkdownLiteral loc2, Match loc1)
    {
        this.loc2 = loc2;
        this.sourceSpan = new SourceSpan(loc1.SourceSpan.Start, loc2.GetSourceSpan().End);
    }

    public override SourceSpan GetSourceSpan() => sourceSpan;
}
