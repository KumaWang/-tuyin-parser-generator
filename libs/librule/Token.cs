using librule.expressions;
using librule.generater;

namespace librule
{
    public class Token : IComparable<Token>, IComparable
    {
        public ushort Index
        {
            get;
        }

        internal RegularExpression<TableAction> Expression
        {
            get;
        }

        public bool IsClearly
        {
            get { return ClearString != null; }
        }

        public bool Completable
        {
            get;
            private set;
        }

        public string ClearString
        {
            get;
        }

        public string SnippetString
        {
            get;
            private set;
        }

        public string Description
        {
            get;
        }

        public int MinCount
        {
            get;
        }

        public int MaxCount
        {
            get;
        }

        public bool IsComment
        {
            get;
            private set;
        }

        public TokenColor? Color
        {
            get;
        }

        internal bool IsSkip
        {
            get;
            set;
        }

        internal bool InProcessing
        {
            get;
            set;
        }

        internal bool IsPrevious 
        {
            get;
        }

        internal Token(ushort index, RegularExpression<TableAction> regex, string descrption, TokenColor? color)
        {
            Index = index;
            Expression = regex;
            Description = descrption ?? Expression.GetDescrption();
            Color = color;
            ClearString = regex.GetClearString();
            MinCount = regex.GetMinLength();
            MaxCount = regex.GetMaxLength();

            IsPrevious = Expression.GetLast().Any(x => x.ExpressionType == RegularExpressionType.Previous);
        }

        public int CompareTo(Token other)
        {
            return Index.CompareTo(other.Index);
        }

        public int CompareTo(object obj)
        {
            if (!(obj is Token))
                return 0;

            return CompareTo(obj as Token);
        }

        public override string ToString()
        {
            return Description;
        }

        public Token Complete()
        {
            Completable = true;
            return this;
        }

        public Token Snippet(string snippet)
        {
            Completable = true;
            SnippetString = snippet;
            return this;
        }

        public Token MarkComment()
        {
            IsComment = true;
            return this;
        }

        public virtual IEnumerable<Token> Expands() 
        {
            yield return this;
        }
    }
}
