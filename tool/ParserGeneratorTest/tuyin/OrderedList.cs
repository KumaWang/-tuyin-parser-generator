using System.Collections;

namespace Tuitor.packages.richtext.format.parsers.markdown;
internal class OrderedList : MarkdownItem, IEnumerable<OrderedListItem>
{
    private readonly List<OrderedListItem> mList = new List<OrderedListItem>();

    public int Count => mList.Count;

    public OrderedListItem this[int index] 
    {
        get { return mList[index]; }
    }

    public OrderedList(OrderedListItem item) 
    {
        mList.Add(item);
    }

    public OrderedList Add(OrderedListItem item) 
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

    public IEnumerator<OrderedListItem> GetEnumerator()
    {
        return mList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
