namespace Tuitor.packages.richtext.format.parsers.markdown;
internal class OrderedListItem : MarkdownItem
{
    private Match number;
    private MarkdownItem content;

    public OrderedListItem(Match loc1, MarkdownItem loc10)
    {
        this.number = loc1;
        this.content = loc10;
    }

    public override SourceSpan GetSourceSpan()
    {
        return new SourceSpan(number.SourceSpan.Start, content.GetSourceSpan().End);
    }
}
