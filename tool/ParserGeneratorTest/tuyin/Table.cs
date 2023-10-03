using System.Collections;

namespace Tuitor.packages.richtext.format.parsers.markdown;
internal class Table : MarkdownItem, IEnumerable<TableRow>
{
    private readonly List<TableRow> mList = new List<TableRow>();

    public int Count => mList.Count;

    public TableRow this[int index]
    {
        get { return mList[index]; }
    }

    public Table(TableRow item)
    {
        mList.Add(item);
    }

    public Table Add(TableRow item)
    {
        mList.Add(item);
        return this;
    }

    public override SourceSpan GetSourceSpan()
    {
        if (mList.Count == 1)
            return mList[0].GetSourceSpan();

        return mList[0].GetSourceSpan().Combine(mList[^1].GetSourceSpan());
    }

    public IEnumerator<TableRow> GetEnumerator()
    {
        return mList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
