using System.Collections.Generic;

namespace libflow.stmts
{
    public class Index : Expression
    {
        public Index(IAstNode source, IAstNode value)
        {
            Source = source;
            Value = value;
        }

        public IAstNode Source { get; }

        public IAstNode Value { get; }

        public override AstNodeType AstNodeType => AstNodeType.Index;

        public override IEnumerable<IAstNode> GetChildrens()
        {
            yield return Source;
            yield return Value;
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            yield return Value;
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            yield return Source;
        }
    }
}
