using librule.generater;
using System.Globalization;

namespace librule.expressions
{
    abstract class RegularExpression<TAction>
    {
        private static readonly HashSet<UnicodeCategory> sLettersCategories = new HashSet<UnicodeCategory>()
        {
            UnicodeCategory.LetterNumber,
            UnicodeCategory.LowercaseLetter,
            UnicodeCategory.ModifierLetter,
            UnicodeCategory.OtherLetter,
            UnicodeCategory.TitlecaseLetter,
            UnicodeCategory.UppercaseLetter
        };

        private static readonly string sExceptString = string.Empty;

        static RegularExpression()
        {
            var chars = new List<char>();
            for (var i = char.MinValue; i < char.MaxValue; i++)
            {
                if (!sLettersCategories.Contains(char.GetUnicodeCategory(i)))
                {
                    chars.Add(i);
                }
            }
            sExceptString = new string(chars.ToArray());
        }

        internal bool IsOptional { get; set; }

        internal abstract RegularExpressionType ExpressionType { get; }

        internal abstract int GetCompuateHashCode();

        internal abstract int GetMinLength();

        internal abstract int GetMaxLength();

        internal abstract string GetClearString();

        internal IGraphEdgeStep<TMetadata> CreateGraphState<TMetadata>(GraphFigure<TMetadata, TAction> figure, IGraphEdgeStep<TMetadata> step, TMetadata metadata)
        {
            return InternalCreate(figure, step, metadata);
        }

        internal abstract IGraphEdgeStep<TMetadata> InternalCreate<TMetadata>(GraphFigure<TMetadata, TAction> figure, IGraphEdgeStep<TMetadata> step, TMetadata metadata);

        public virtual IEnumerable<RegularExpression<TAction>> GetLast() 
        {
            yield return this;
        }

        public abstract RegularExpression<TAction> ExtractExclusionExpression();

        public abstract string GetDescrption();

        public RegularExpression<TAction> Many()
        {
            if (ExpressionType == RegularExpressionType.Repeat)
                return this;

            return new RepeatExpression<TAction>(this).Optional();
        }

        public RegularExpression<TAction> Many1()
        {
            return this > Many();
        }

        public RegularExpression<TAction> Optional()
        {
            IsOptional = true;

            return new EmptyExpression<TAction>() | this;
        }

        internal abstract int RepeatLevel();

        public static RegularExpression<TAction> Any()
        {
            return new CharSetExpression<TAction>(new GraphEdgeValue(true, '\0'));
        }

        public static RegularExpression<TAction> Symbol(char c)
        {
            return new SymbolExpression<TAction>(c);
        }

        public static RegularExpression<TAction> CharSet(string literal)
        {
            return new CharSetExpression<TAction>(literal.ToCharArray());
        }

        public static RegularExpression<TAction> CharSet(IEnumerable<char> set)
        {
            return new CharSetExpression<TAction>(set.ToArray());
        }

        public static RegularExpression<TAction> Literal(string literal)
        {
            if (string.IsNullOrEmpty(literal))
                throw new NotImplementedException();

            if (literal.Length == 1)
                return new SymbolExpression<TAction>(literal[0]);

            var symbols = new SymbolExpression<TAction>[literal.Length];
            for (var i = 0; i < literal.Length; i++)
                symbols[i] = new SymbolExpression<TAction>(literal[i]);

            return new ConcatenationExpression<TAction>(symbols);
        }

        public static RegularExpression<TAction> Range(char start, char end)
        {
            var chars = new char[end - start + 1];
            for (var i = start; i <= end; i++)
                chars[i - start] = i;

            return new CharSetExpression<TAction>(chars);
        }

        public static RegularExpression<TAction> Except(params char[] symbol)
        {
            return new CharSetExpression<TAction>(new GraphEdgeValue(true, symbol.Concat(new char[] { '\0' })));
        }

        public static RegularExpression<TAction> Until(string literal)
        {
            return new ExceptExpression<TAction>(literal);
        }

        public static RegularExpression<TAction> operator |(RegularExpression<TAction> left, RegularExpression<TAction> right)
        {
            var combineExp = left.Merge(right);
            if (combineExp != null)
                return combineExp;

            return new OrExpression<TAction>(left, right);
        }

        protected internal virtual RegularExpression<TAction> Merge(RegularExpression<TAction> right)
        {
            return null;
        }

        public static RegularExpression<TAction> operator >(RegularExpression<TAction> left, RegularExpression<TAction> right)
        {
            return new ConcatenationExpression<TAction>(left, right);
        }

        public static RegularExpression<TAction> operator <(RegularExpression<TAction> left, RegularExpression<TAction> right)
        {
            return new ConcatenationExpression<TAction>(right, left);
        }

        public static RegularExpression<TAction> operator |(string left, RegularExpression<TAction> right)
        {
            return new OrExpression<TAction>(Literal(left), right);
        }

        public static RegularExpression<TAction> operator >(string left, RegularExpression<TAction> right)
        {
            return new ConcatenationExpression<TAction>(Literal(left), right);
        }

        public static RegularExpression<TAction> operator <(string left, RegularExpression<TAction> right)
        {
            return new ConcatenationExpression<TAction>(right, Literal(left));
        }

        public static RegularExpression<TAction> operator |(char left, RegularExpression<TAction> right)
        {
            return new OrExpression<TAction>(Symbol(left), right);
        }

        public static RegularExpression<TAction> operator >(char left, RegularExpression<TAction> right)
        {
            return new ConcatenationExpression<TAction>(Symbol(left), right);
        }

        public static RegularExpression<TAction> operator <(char left, RegularExpression<TAction> right)
        {
            return new ConcatenationExpression<TAction>(right, Symbol(left));
        }

        public static RegularExpression<TAction> operator |(RegularExpression<TAction> left, string right)
        {
            return new OrExpression<TAction>(left, Literal(right));
        }

        public static RegularExpression<TAction> operator >(RegularExpression<TAction> left, string right)
        {
            return new ConcatenationExpression<TAction>(left, Literal(right));
        }

        public static RegularExpression<TAction> operator <(RegularExpression<TAction> left, string right)
        {
            return new ConcatenationExpression<TAction>(Literal(right), left);
        }

        public static RegularExpression<TAction> operator |(RegularExpression<TAction> left, char right)
        {
            return new OrExpression<TAction>(left, Symbol(right));
        }

        public static RegularExpression<TAction> operator >(RegularExpression<TAction> left, char right)
        {
            return new ConcatenationExpression<TAction>(left, Symbol(right));
        }

        public static RegularExpression<TAction> operator <(RegularExpression<TAction> left, char right)
        {
            return new ConcatenationExpression<TAction>(Symbol(right), left);
        }

    }
}
