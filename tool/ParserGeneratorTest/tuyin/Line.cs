using Tuitor.controls.notelib;

namespace Tuitor.packages.richtext.format.parsers.markdown;

internal class Line : MarkdownItem
{
    private Match loc1;

    public Line(Match loc1)
    {
        this.loc1 = loc1;
    }

    public override SourceSpan GetSourceSpan() => loc1.SourceSpan;


    public override Style GetStyle(TextEngine engine)
    {
        return new LineStyle();
    }

    class LineStyle : Style 
    {
        public override int GetDisplayHeight(TextViewRenderer renderer)
        {
            throw new NotImplementedException();
        }
    }
}
