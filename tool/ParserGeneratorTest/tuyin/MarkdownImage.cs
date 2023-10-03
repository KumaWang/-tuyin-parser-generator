namespace Tuitor.packages.richtext.format.parsers.markdown;

class MarkdownImage : MarkdownItem
{
    private Link loc10;
    private SourceSpan sourceSpan;

    public MarkdownImage(Link loc10, Match loc1)
    {
        this.loc10 = loc10;
        this.sourceSpan = new SourceSpan(loc1.SourceSpan.Start, loc10.GetSourceSpan().End);
    }

    public override SourceSpan GetSourceSpan() => sourceSpan;
}
