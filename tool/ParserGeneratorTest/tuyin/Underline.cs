namespace Tuitor.packages.richtext.format.parsers.markdown;

internal class Underline : MarkdownItem
{
    private MarkdownItem loc2;
    private SourceSpan sourceSpan;
    private int paddingLeft;
    private int paddingRight;

    public Underline(MarkdownItem loc2, Match loc1)
    {
        this.loc2 = loc2;
        this.paddingLeft = 1;
        this.paddingRight = 0;
        this.sourceSpan = new SourceSpan(loc1.SourceSpan.Start, loc2.GetSourceSpan().End);
    }

    public Underline(MarkdownItem loc2, Match loc1, Match loc3)
    {
        this.loc2 = loc2;
        this.paddingLeft = 2;
        this.paddingRight = 2;
        this.sourceSpan = new SourceSpan(loc1.SourceSpan.Start, loc3.SourceSpan.End);
    }

    public override SourceSpan GetSourceSpan() => this.sourceSpan;

   

}
