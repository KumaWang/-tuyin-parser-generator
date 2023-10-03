using System.Collections;

namespace Tuitor.packages.richtext.format.parsers.markdown;
internal class TableRow : MarkdownItem, IEnumerable<TableRowItem>
{
    private readonly List<TableRowItem> mList = new List<TableRowItem>();

    public int Count => mList.Count;

    public TableRowItem this[int index]
    {
        get { return mList[index]; }
    }

    public TableRow(TableRowItem item)
    {
        mList.Add(item);
    }

    public TableRow Add(TableRowItem item)
    {
        mList.Add(item);
        return this;
    }

    public override SourceSpan GetSourceSpan()
    {
        if (mList.Count == 1)
            return mList[0].SourceSpan;

        return mList[0].SourceSpan.Combine(mList[^1].SourceSpan);
    }

    public IEnumerator<TableRowItem> GetEnumerator()
    {
        return mList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
