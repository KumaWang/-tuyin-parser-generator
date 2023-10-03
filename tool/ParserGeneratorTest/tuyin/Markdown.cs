namespace Tuitor.packages.richtext.format.parsers.markdown;

internal class Markdown : List<MarkdownItem>
{
    public Markdown() 
    {
    }

    public Markdown(MarkdownItem item) 
    {
        base.Add(item);
    }

    public new Markdown Add(MarkdownItem item) 
    {
        base.Add(item);
        return this;
    }
}
