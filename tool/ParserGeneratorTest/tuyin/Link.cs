namespace Tuitor.packages.richtext.format.parsers.markdown;
internal class Link : MarkdownItem
{
    private MarkdownLiteral url;
    private MarkdownLiteral display;
    private SourceSpan sourceSpan;

    public Link(MarkdownLiteral url, Match loc1, Match loc3) 
    {
        this.url = url;
        this.display = url;
        this.sourceSpan = new SourceSpan(loc1.SourceSpan.Start, loc3.SourceSpan.End);
    }

    public Link(MarkdownLiteral url, MarkdownLiteral display, Match loc1, Match loc3)
    {
        this.url = url;
        this.display = display;
        this.sourceSpan = new SourceSpan(loc1.SourceSpan.Start, loc3.SourceSpan.End);
    }

    public string Descrption => display?.Context ?? url.Context;

    public string Source => url.Context;

    public override SourceSpan GetSourceSpan() => sourceSpan;
}
