using librule.expressions;
using librule.generater;
using System.Diagnostics;

namespace librule
{
    class ClarityToken : Token
    {
        internal ClarityToken(ushort index, IList<Token> tokens, string descrption) 
            : base(index, new SymbolExpression<TableAction>('\b'), descrption, null)
        {
            Debug.Assert(!tokens.Any(x => x is ClarityToken), "不能包含歧义Token");
            Tokens = tokens;
        }

        public IList<Token> Tokens { get; }

        public override IEnumerable<Token> Expands()
        {
            return Tokens;
        }

        internal bool IsConflictToken(ushort token)
        {
            return Tokens.Any(x => x.Index == token);
        }
    }
}
