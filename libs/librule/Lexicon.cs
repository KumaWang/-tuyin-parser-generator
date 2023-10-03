using librule.expressions;
using librule.generater;
using librule.utils;
using System.Collections;

namespace librule
{
    public class Lexicon
    {
        private ushort mIndex;
        private TokenDictionary mTokens;
        private Dictionary<string, ClarityToken> mClarities;

        public Token Eos { get; }

        public Token Missing { get; }

        public IReadOnlyList<Token> Tokens => mTokens;

        internal Lexicon()
        {
            mTokens = new TokenDictionary();
            mClarities = new Dictionary<string, ClarityToken>();

            Missing = DefineToken(mIndex++, new EmptyExpression<TableAction>(), "missing");
            Eos = DefineToken(mIndex++, RegularExpression<TableAction>.Symbol('\0'), "ε");
        }

        internal ClarityToken Clarity(IEnumerable<Token> tokens) 
        {
            var clarityName = tokens.GetUniqueName();
            if (!mClarities.ContainsKey(clarityName))
            {
                var clarityToken = new ClarityToken(mIndex++, tokens.ToArray(), string.Join("|", tokens.Select(x => x.Description)));
                mClarities[clarityName] = clarityToken;
                mTokens.Add(clarityToken);
            }

            return mClarities[clarityName];
        }

        internal Token DefineToken(RegularExpression<TableAction> regex)
        {
            return DefineToken(regex, null, null);
        }

        internal Token DefineToken(RegularExpression<TableAction> regex, string description)
        {
            return DefineToken(regex, description, null);
        }

        internal Token DefineToken(RegularExpression<TableAction> regex, TokenColor color)
        {
            return DefineToken(regex, null, color);
        }

        internal Token DefineToken(RegularExpression<TableAction> regex, string description, TokenColor? color)
        {
            return DefineToken(mIndex++, regex, description, color);
        }

        internal Token DefineToken(ushort index, RegularExpression<TableAction> regex, string description) 
        {
            return DefineToken(index, regex, description, null);
        }

        internal Token DefineToken(ushort index, RegularExpression<TableAction> regex, string description, TokenColor? color)
        {
            var token = new Token(index, regex, description, color);
            mTokens.Add(token);
            return token;
        }

        class TokenDictionary : IReadOnlyList<Token>
        {
            private readonly Dictionary<int, Token> mTokens = new Dictionary<int, Token>();

            public int Count => mTokens.Count;

            public Token this[int key] => mTokens[key];

            internal void Add(Token token) => mTokens.Add(token.Index, token);

            public IEnumerator<Token> GetEnumerator()
            {
                return mTokens.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
