namespace Tuitor.packages.richtext.format.parsers.markdown;
internal class NumberTitle : MarkdownItem
{
    private int level;
    private MarkdownLiteral loc2;
    private SourceSpan sourceSpan;

    public NumberTitle(Match v, MarkdownLiteral loc2)
    {
        this.level = 1;
        this.loc2 = loc2;
        this.sourceSpan = new SourceSpan(v.SourceSpan.Start, loc2.GetSourceSpan().End);
    }

    public NumberTitle AddLevel(Match match) 
    {
        level++;
        this.sourceSpan = new SourceSpan(match.SourceSpan.Start, this.sourceSpan.End);
        return this;
    }

    public override SourceSpan GetSourceSpan() => sourceSpan;
}
