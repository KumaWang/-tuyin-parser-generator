using libfsm;
using librule.expressions;
using librule.generater;
using librule.productions;
using System.Text;

namespace librule
{
    class DSLLiteral 
    {
        public DSLLiteral(string value, SourceLocation sourceSpan)
        {
            Value = value;
            Location = sourceSpan;
        }

        public string Value { get; }

        public SourceLocation Location { get; }
    }

    enum DSLReferType
    {
        Expression,
        Colour,
        Format,
        Indent,
        Production
    }

    abstract class DSLRefer
    {
        public DSLRefer(string name, int line)
        {
            Name = name;
            Line = line;
        }

        public string Name { get; }

        public int Line { get; }

        public abstract DSLReferType Type { get; }
    }

    class DSLExpressionRefer : DSLRefer
    {
        public DSLExpressionRefer(string name, int line, string color, RegularExpression<TableAction> exp)
            : base(name, line)
        {
            Expression = exp;
            Color = color;
        }

        public override DSLReferType Type => DSLReferType.Expression;

        public string Color { get; }

        public RegularExpression<TableAction> Expression { get; }
    }

    class DSLProductionRefer : DSLRefer
    {
        public DSLProductionRefer(string name, string type, int line, ProductionBase<TableAction> production)
            : base(name, line)
        {
            ReturnType = type;
            Production = production;
        }

        public override DSLReferType Type => DSLReferType.Production;

        public string ReturnType { get; }

        public ProductionBase<TableAction> Production { get; set; }
    }

    abstract class DSLTokenItem
    {
        public abstract RegularExpression<TableAction> Create(DSL dsl);
    }

    internal class DSLToken
    {
        private DSLLiteral nt1_s;
        private DSLTokenItem nt2_s;
        private DSLLiteral nt4_s;
        private bool completable;
        private DSLLiteral snippet;
        private RegularExpression<TableAction> expr;

        public DSLToken(DSLLiteral nt1_s, DSLTokenItem nt2_s, DSLLiteral nt4_s, bool completable, DSLLiteral snippet)
        {
            this.nt1_s = nt1_s;
            this.nt2_s = nt2_s;
            this.nt4_s = nt4_s;
            this.completable = completable;
            this.snippet = snippet;
        }

        public string Name => nt1_s.Value;

        public int Line => nt1_s.Location.Line;

        public string Color => nt4_s?.Value;

        public string Snippent => snippet?.Value;

        public bool Completable => completable;

        internal RegularExpression<TableAction> GetRegularExpression(DSL dsl) => expr ?? (expr = nt2_s.Create(dsl));

        internal DSLRefer Create(DSL dsl)
        {
            return new DSLExpressionRefer(nt1_s.Value, nt1_s.Location.Line, Color, GetRegularExpression(dsl));
        }
    }

    internal class DSLOption
    {
        private DSLLiteral nt1_s;

        public DSLOption(DSLLiteral nt1_s)
        {
            this.nt1_s = nt1_s;
        }

        public DSLLiteral Context => nt1_s;
    }

    abstract class DSLRuleItem
    {
        public bool IsEndRuleItem { get; set; }

        public ushort Token { get; internal set; }

        public abstract void Initialize();

        public virtual string GetDefineName(DSL dsl) => throw new NotImplementedException();

        public abstract ProductionBase<TableAction> Create(DSL dsl, DSLRule rule);

        public abstract void Print(DSL dsl, StringBuilder sb, bool isFormal);

        public abstract IEnumerable<string> GetReferences();
    }

    internal class DSLRule
    {
        private DSLLiteral id;
        private DSLLiteral type;
        private DSLRuleItem nt2_s;

        public string Name => id.Value;

        public string Type => type.Value;

        public int Line => id.Location.Line;

        public DSLRule(DSLLiteral id, DSLLiteral type, DSLRuleItem nt2_s)
        {
            this.id = id;
            this.type = type;
            this.nt2_s = nt2_s;
            this.nt2_s.Initialize();
        }

        internal DSLRefer Create(DSL dsl)
        {
            return new DSLProductionRefer(Name, Type, Line, nt2_s.Create(dsl, this));
        }

        internal void Print(DSL dsl, StringBuilder sb)
        {
            sb.AppendLine($"protected virtual {Type} {Name}()\n{{\n{Type} result=default;\n");
            nt2_s.Print(dsl, sb, false);
            sb.AppendLine($"return result;\n}}\n");
        }

        internal IEnumerable<string> GetReferences() => new string[] { Name }.Union(nt2_s.GetReferences());
    }

    partial class DSLParser
    {
        private int _index;

        private string input;
        private int index 
        {
            get { return _index; }
            set 
            {
                if(_index != value) 
                {
                    if (value > _index)
                    {
                        for (var i = _index; i < value; i++) 
                        {
                            var c = input[i];
                            if (c == '\n')
                                line++;
                        }

                    }
                    else if (value < _index) 
                    {
                        for (var i = _index - 1; i >= value; i--)
                        {
                            var c = input[i];
                            if (c == '\n')
                                line--;
                        }
                    }

                    _index = value;
                }
            }
        }
        private int line;

        public List<DSLToken> Tokens { get; private set; }

        public List<DSLRule> Rules { get; private set; }

        public List<DSLOption> Opations { get; private set; }

        public DSL Parse(string input) 
        {
            this.input = input.Replace("\r", string.Empty) + "\0";
            this.index = 0;
            this.line = 1;

            Opations = new List<DSLOption>();
            Tokens = new List<DSLToken>();
            Rules = new List<DSLRule>();
            var dsl = new DSL(Opations, Tokens, Rules);
            dsl.Init(this.input);

            while (!ParseLiteral(TokenType.EOS)) 
            {
                Skip();

                if (ParseLiteral(TokenType.OPTION, out var literal))
                {
                    Opations.Add(new DSLOption(literal));
                }
                else if (ParseLiteral(TokenType.RULE))
                {
                    if (Skip() || !ParseLiteral(TokenType.ID, out var id))
                        throw new FAException($"{dsl.GetErrorLine(new SourceLocation(line, index - 1, index))}\n未能匹配'ID'。");

                    if (Skip() || !ParseLiteral(TokenType.TYPE, out var type))
                        throw new FAException($"{dsl.GetErrorLine(new SourceLocation(line, index - 1, index))}\n未能匹配'Type'。");

                    if (Skip() || !ParseLiteral(TokenType.COLON) || Skip())
                        throw new FAException($"{dsl.GetErrorLine(new SourceLocation(line, index - 1, index))}\n未能匹配':'。");

                    var orItem = ParseRuleOr();
                    if (orItem == null)
                        throw new FAException($"{dsl.GetErrorLine(new SourceLocation(line, index - 1, index))}\n未能匹配任意字符。");

                    if (Skip() || !ParseLiteral(TokenType.SEM))
                        throw new FAException($"{dsl.GetErrorLine(new SourceLocation(line, index - 1, index))}\n未能匹配';'。");

                    Rules.Add(new DSLRule(id, type, orItem));
                }
                else
                {

                    DSLTokenItem tokenOrNode = null;
                    string exMsg = "未能匹配任意字符。";
                    try
                    {
                        tokenOrNode = ParseTokenOr();
                    }
                    catch (Exception ex)
                    {
                        exMsg = ex.Message;
                    }

                    if (tokenOrNode == null)
                        throw new FAException($"{dsl.GetErrorLine(new SourceLocation(line, index - 1, index))}\n{exMsg}");

                    Skip();
                    if (ParseLiteral(TokenType.XOR))
                    {
                        Skip();
                        var completable = ParseLiteral(TokenType.NOT);
                        Skip();

                        if (!ParseLiteral(TokenType.ID, out var id))
                            id = new DSLLiteral(null, new SourceLocation(line, index - 1, index));

                        Skip();
                        ParseLiteral(TokenType.HEX_COLOR, out var colorItem);
                        Skip();

                        DSLLiteral snippetItem = null;
                        var hasSnippet = 
                            ParseLiteral(TokenType.LEFT_BK) &&
                            ParseLiteral(TokenType.RUS, out snippetItem) && 
                            ParseLiteral(TokenType.RIGHT_BK);

                        Tokens.Add(new DSLToken(id, tokenOrNode, colorItem, completable, snippetItem));
                    }
                    else
                    {
                        Skip();
                        ParseLiteral(TokenType.HEX_COLOR, out var colorItem);

                        Tokens.Add(new DSLToken(new DSLLiteral(null, new SourceLocation(line, index - 1, index)), tokenOrNode, colorItem, false, null));
                    }
                }

                Skip();
            }

  
            return dsl;
        }

        private bool Skip() 
        {
            while (ParseLiteral(TokenType.SKIP)) ;
            return false;
        }

        protected int Read()
        {
            if (index >= input.Length)
                return -1;

            return input[index++];
        }

        private bool ParseLiteral(TokenType type)
        {
            DSLLiteral literal = null;
            return ParseLiteral(type, out literal);
        }

        private bool ParseLiteral(TokenType type, out DSLLiteral literal)
        {
            literal = GetLiteral(type);
            if (literal == null)
                return false;

            return true;
        }

        private DSLLiteral GetLiteral(TokenType type)
        {
            var start = index;
            var l = line;

            string literal = null;
            switch (type)
            {
                case TokenType.NOT:
                    literal = "!";
                    break;
                case TokenType.TO:
                    literal = "..";
                    break;
                case TokenType.TO3:
                    literal = "...";
                    break;
                case TokenType.LEFT_BK:
                    literal = "{";
                    break;
                case TokenType.RIGHT_BK:
                    literal = "}";
                    break;
                case TokenType.OR:
                    literal = "|";
                    break;
                case TokenType.OPTIONAL:
                    literal = "?";
                    break;
                case TokenType.MANY:
                    literal = "*";
                    break;
                case TokenType.MANY1:
                    literal = "+";
                    break;
                case TokenType.ANY:
                    literal = ".";
                    break;
                case TokenType.RANGE:
                    literal = "-";
                    break;
                case TokenType.LEFT_PH:
                    literal = "(";
                    break;
                case TokenType.RIGHT_PH:
                    literal = ")";
                    break;
                case TokenType.EOS:
                    {
                        var c = Read();
                        if (c == -1)
                            return new DSLLiteral("\0", new SourceLocation(l, this.input.Length, this.input.Length));

                        if (c == '\0')
                            return new DSLLiteral("\0", new SourceLocation(l, this.input.Length - 1, this.input.Length));

                        index--;
                        return null;
                    }
                case TokenType.XOR:
                    literal = "^";
                    break;
                case TokenType.SEM:
                    literal = ";";
                    break;
                case TokenType.COLON:
                    literal = ":";
                    break;
                case TokenType.RULE:
                    literal = "rule";
                    break;
                case TokenType.METADATA:
                    {
                        var sb = new StringBuilder();
                        var first = Read();
                        if (first == -1) throw new Exception("EOF in string");

                        if (first != '[')
                        {
                            index = start;
                            return null;
                        }

                        // 遇到标记符停止
                        int count = 1;
                        for (; ; )
                        {
                            int ch = Read();
                            if (ch == -1) throw new Exception("EOF in string");

                            if (ch == '[')
                                count++;

                            else if (ch == ']')
                            {
                                if (--count == 0)
                                    break;
                            }

                            sb.Append((char)ch);
                        }

                        return new DSLLiteral(sb.ToString(), new SourceLocation(l, start, index - 1));
                    }
                case TokenType.HEX_COLOR:
                    {
                        var sb = new StringBuilder();
                        var first = Read();
                        if (first == -1) throw new Exception("EOF in string");

                        if (first != '#')
                        {
                            index = start;
                            return null;
                        }
                        sb.Append((char)first);

                        const string vailds = "0123456789ABCDEFabcdef";

                        // 遇到标记符停止
                        for (; ; )
                        {
                            var ch = Read();
                            if (ch == -1) throw new Exception("EOF in string");

                            var c = (char)ch;
                            if (!vailds.Contains(c))
                            {
                                index--;
                                break;
                            }

                            sb.Append(c);

                            if (sb.Length >= 8)
                                break;
                        }

                        // 检查长度
                        switch (sb.Length) 
                        {
                            case 4:
                            case 7:
                            case 9:
                                break;
                            default:
                                throw new Exception("非法长度");
                        }

                        return new DSLLiteral(sb.ToString(), new SourceLocation(l, start, index - 1));
                    }
                case TokenType.DESCRPTION:
                    {
                        var sb = new StringBuilder();
                        var first = Read();
                        if (first == -1) throw new Exception("EOF in string");

                        if (first != '#')
                        {
                            index = start;
                            return null;
                        }
                        sb.Append((char)first);

                        // 遇到标记符停止
                        for (; ; )
                        {
                            var ch = Read();
                            if (ch == -1) throw new Exception("EOF in string");

                            var c = (char)ch;
                            if (c == '\n')
                                break;

                            sb.Append(c);
                        }

                        return new DSLLiteral(sb.ToString(), new SourceLocation(l, start, index - 1));
                    }
                case TokenType.ID:
                    {
                        const string firstFlags = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_";
                        const string flags = firstFlags + "0123456789";

                        var sb = new StringBuilder();
                        var first = Read();
                        if (first == -1) throw new Exception("EOF in string");

                        var firstC = (char)first;
                        if (!firstFlags.Contains(firstC))
                        {
                            index = start;
                            return null;
                        }
                        sb.Append(firstC);

                        // 遇到标记符停止
                        for (; ; )
                        {
                            var ch = Read();
                            if (ch == -1) throw new Exception("EOF in string");

                            var c = (char)ch;
                            if (!flags.Contains(c))
                            {
                                index--;
                                break;
                            }

                            sb.Append(c);
                        }

                        return new DSLLiteral(sb.ToString(), new SourceLocation(l, start, index));
                    }
                case TokenType.OPTION:
                    {
                        var sb = new StringBuilder();
                        var first = Read();
                        if (first == -1) throw new Exception("EOF in string");

                        if (first != '%') 
                        {
                            index = start;
                            return null;
                        }
                        sb.Append((char)first);

                        // 遇到标记符停止
                        for (; ; )
                        {
                            var ch = Read();
                            if (ch == -1) throw new Exception("EOF in string");

                            var c = (char)ch;
                            if (c == '\n')
                                break;

                            sb.Append(c);
                        }

                        return new DSLLiteral(sb.ToString(), new SourceLocation(l, start, index - 1));
                    }
                case TokenType.TYPE:
                    {
                        var sb = new StringBuilder();
                        var first = Read();
                        if (first != '<')
                        {
                            index = start;
                            return new DSLLiteral(Settings.VOID_TYPE, new SourceLocation(line, start, index));
                        }

                        int count = 1;
                        for (; ; )
                        {
                            int ch = Read();
                            if (ch == -1) throw new Exception("EOF in string");

                            if (ch == '<')
                                count++;

                            if (ch == '>')
                            {
                                if (--count == 0)
                                    break;
                            }

                            sb.Append((char)ch);
                        }

                        return new DSLLiteral(sb.ToString(), new SourceLocation(l, start, index));
                    }
                case TokenType.CHS:
                    {
                        var sb = new StringBuilder();
                        var first = Read();
                        if (first == -1) throw new Exception("EOF in string");

                        if (first != '\'' && first != '"')
                        {
                            index = start;
                            return null;
                        }

                        int last = 0;
                        for (; ; )
                        {
                            int ch = Read();
                            if (ch == -1) throw new Exception("EOF in string");

                            if (last != '\\' && ch == first)
                                break;

                            if (last == '\\')
                            {
                                sb.Append(System.Text.RegularExpressions.Regex.Unescape($"\\{(char)ch}")[0]);
                                last = 0;
                            }
                            else
                            {
                                if (ch != '\\') sb.Append((char)ch);
                                last = ch;
                            }
                        }

                        return new DSLLiteral(sb.ToString(), new SourceLocation(l, start, index));
                    }
                case TokenType.TOKEN_CHS:
                    {
                        var flags = " \n\t\b{}()|*+-?$^'";

                        var sb = new StringBuilder();
                        int last = 0;
                        for (; ; )
                        {
                            int ch = Read();
                            if (ch == -1) throw new Exception("EOF in string");

                            var c = (char)ch;
                            if (last == '\\')
                            {
                                sb.Append(System.Text.RegularExpressions.Regex.Unescape($"\\{(char)ch}")[0]);
                                last = 0;
                            }
                            else
                            {
                                if (flags.Contains(c))
                                {
                                    index--;
                                    break;
                                }

                                if (c != '\\') sb.Append(c);
                                last = ch;
                            }
                        }

                        if (sb.Length <= 0)
                            return null;

                        return new DSLLiteral(sb.ToString(), new SourceLocation(l, start, index));
                    }
                case TokenType.RUS:
                    {
                        var sb = new StringBuilder();
                        int count = 1;
                        for (; ; )
                        {
                            int ch = Read();
                            if (ch == -1) throw new Exception("EOF in string");

                            if (ch == '{')
                                count++;

                            if (ch == '}')
                            {
                                if (--count == 0)
                                {
                                    index--;
                                    break;
                                }
                            }

                            sb.Append((char)ch);
                        }

                        return new DSLLiteral(sb.ToString(), new SourceLocation(l, start, index));
                    }
                case TokenType.RUS_LINE:
                    {
                        var flags = "\n";

                        var sb = new StringBuilder();
                        int last = 0;
                        for (; ; )
                        {
                            int ch = Read();
                            if (ch == -1) throw new Exception("EOF in string");

                            var c = (char)ch;
                            if (last == '\\')
                            {
                                sb.Append(System.Text.RegularExpressions.Regex.Unescape($"\\{(char)ch}")[0]);
                                last = 0;
                            }
                            else
                            {
                                if (flags.Contains(c))
                                {
                                    index--;
                                    break;
                                }

                                if (c != '\\') sb.Append(c);
                                last = ch;
                            }
                        }

                        if (sb.Length <= 0)
                            return null;

                        return new DSLLiteral(sb.ToString(), new SourceLocation(l, start, index));
                    }
                case TokenType.SKIP:
                    {
                        var mode = 0;
                        var first = Read();
                        if (first == -1) throw new Exception("EOF in comment");

                        switch (first) 
                        {
                            //case '#':
                            //    mode = 1;
                            //    break;
                            case '/':
                                var follow = Read();
                                if (follow == -1) throw new Exception("EOF in comment");

                                switch (follow) 
                                {
                                    case '/':
                                        mode = 1;
                                        break;
                                    case '*':
                                        mode = 2;
                                        break;
                                    default:
                                        index = start;
                                        break;
                                }

                                break;
                            default:
                                index = start;
                                break;
                        }

                        switch (mode) 
                        {
                            case 0:
                                {
                                    const string flags = " \n\t\b";
                                    int last = start;
                                    for (; ; )
                                    {
                                        var ch = Read();
                                        if (ch == -1) throw new Exception("EOF in string");

                                        var c = (char)ch;
                                        if (!flags.Contains(c))
                                        {
                                            index = last;
                                            break;
                                        }

                                        last = index;
                                    }
                                }
                                break;
                            case 1:
                                {
                                    int last = 0;
                                    for (; ; )
                                    {
                                        var ch = Read();
                                        if (ch == -1) throw new Exception("EOF in string");

                                        var c = (char)ch;
                                        if (last != '\\' && c == '\n')
                                            break;

                                        last = ch;
                                    }
                                }
                                break;
                            case 2:
                                for (; ; )
                                {
                                    var next = Read();
                                    if (next == -1) throw new Exception("EOF in comment");

                                    if (Read() == '*' && Read() == '/')
                                        break;
                                }

                                break;
                        }

                        return start == index ? null : new DSLLiteral(null, default);

                    }
            }

            if (index + literal.Length > input.Length || !input.Substring(index, literal.Length).Equals(literal))
                return null;

            var result = new DSLLiteral(literal, new SourceLocation(line, index, index + literal.Length));
            for (var i = 0; i < literal.Length; i++)
                Read();

            return result;
        }

        enum TokenType
        {
            OR,
            OPTIONAL,
            MANY,
            MANY1,
            CHS,
            TOKEN_CHS,
            ANY,
            TO,
            TO3,
            RANGE,
            RUS,
            RUS_LINE,
            LEFT_PH,
            RIGHT_PH,
            LEFT_BK,
            RIGHT_BK,
            EOS,
            SKIP,
            XOR,
            ID,
            OPTION,
            TYPE,
            SEM,
            COLON,
            RULE,
            HEX_COLOR,
            DESCRPTION,
            METADATA,
            NOT
        }
    }
}
