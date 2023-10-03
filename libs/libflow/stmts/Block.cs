using System.Collections.Generic;
using System.Linq;

namespace libflow.stmts
{
    public class Block : Statement
    {
        public Block() 
        {
            Statements = new List<IAstNode>();
        }

        public Block(IEnumerable<IAstNode> statements)
        {
            Statements = statements.ToList();
        }

        public Block(params IAstNode[] statements)
        {
            Statements = statements.ToList();
        }

        public List<IAstNode> Statements { get; }

        public override AstNodeType AstNodeType => AstNodeType.Block;

        public override IEnumerable<IAstNode> GetChildrens()
        {
            return Statements;
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            if (Statements.Count > 0)
                yield return Statements[^1];
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            if (Statements.Count > 0)
                yield return Statements[0];
        }
    }
}
