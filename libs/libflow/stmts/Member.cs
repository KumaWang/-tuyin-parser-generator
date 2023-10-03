using System.Collections.Generic;

namespace libflow.stmts
{
    public class Member : Expression
    {
        public Member(IAstNode source, string memberName)
        {
            Source = source;
            MemberName = memberName;
        }

        public IAstNode Source { get; }

        public string MemberName { get; }

        public override AstNodeType AstNodeType => AstNodeType.Member;

        public override IEnumerable<IAstNode> GetChildrens()
        {
            yield return Source;
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            yield return Source;
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            yield return Source;
        }
    }
}
