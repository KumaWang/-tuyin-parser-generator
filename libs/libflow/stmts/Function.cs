using System.Collections.Generic;

namespace libflow.stmts
{
    public class Function : Statement
    {
        public Function(ushort state, IAstNode stmt)
        {
            EntryPoint = state;
            Body = stmt;
        }

        public override AstNodeType AstNodeType => AstNodeType.Function;

        public ushort EntryPoint { get; }

        public IAstNode Body { get; }

        public override IEnumerable<IAstNode> GetChildrens()
        {
            yield return Body;
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            yield return Body;
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            yield return Body;
        }
    }
}
