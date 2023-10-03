namespace Tuitor.packages.richtext.format.parsers.markdown;
internal class TableRowItem
{
    private MarkdownItem loc3;

    public TableRowItem(MarkdownItem loc3)
    {
        this.loc3 = loc3;
    }

    public SourceSpan SourceSpan => loc3.GetSourceSpan();
}
