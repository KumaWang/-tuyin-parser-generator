using System.Collections;

namespace Tuitor.packages.richtext.format.parsers.markdown;
internal class UnorderedList : MarkdownItem, IEnumerable<UnorderedListItem>
{
    private readonly List<UnorderedListItem> mList = new List<UnorderedListItem>();

    public int Count => mList.Count;

    public UnorderedListItem this[int index]
    {
        get { return mList[index]; }
    }

    public UnorderedList(UnorderedListItem item)
    {
        mList.Add(item);
    }

    public UnorderedList Add(UnorderedListItem item)
    {
        mList.Add(item);
        return this;
    }

    public override SourceSpan GetSourceSpan()
    {
        if (mList.Count == 1)
            return mList[0].GetSourceSpan();

        var first = mList[0].GetSourceSpan();
        var last = mList[^1].GetSourceSpan();
        return new SourceSpan(first.Start, last.End);
    }
    public IEnumerator<UnorderedListItem> GetEnumerator()
    {
        return mList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

}
