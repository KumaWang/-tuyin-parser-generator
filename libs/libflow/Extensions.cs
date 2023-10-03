using libflow.stmts;
using System.Collections.Generic;
using System.Linq;

namespace libflow
{
    public static class Extensions
    {
        public static IEnumerable<IAstNode> Walk(this IAstNode stmt)
        {
            yield return stmt;
            foreach (var child in stmt.GetChildrens())
                foreach (var substmt in Walk(child))
                    yield return substmt;
        }

        public static IEnumerable<IAstNode> Next(this IAstNode stmt)
        {
            var first = stmt.GetChildrens().FirstOrDefault(x => x.IsVaild);
            if (first != null)
            {
                yield return first;
                foreach (var child in first.Next())
                    yield return child;
            }
        }

        public static IEnumerable<IAstNode> Ends(this IAstNode stmt)
        {
            var childs = stmt.GetEnds().ToArray();
            if (childs.Length == 0)
                yield return stmt;

            foreach (var end in childs.SelectMany(x => x.Ends()))
                yield return end;
        }

    }
}
