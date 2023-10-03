namespace Tuitor.packages.richtext.format.parsers.markdown;

class UnorderedListItem : MarkdownItem
{
    private MarkdownItem loc12;
    private SourceSpan sourceSpan;

    public UnorderedListItem(Match loc, MarkdownItem loc12)
    {
        this.loc12 = loc12;
        this.sourceSpan = new SourceSpan(loc.SourceSpan.Start, loc12.GetSourceSpan().End);
    }

    public override SourceSpan GetSourceSpan() => sourceSpan;

}
