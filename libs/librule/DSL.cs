using libfsm;
using libgraph;
using librule.expressions;
using librule.generater;
using librule.productions;
using librule.targets;
using librule.utils;
using System.Data;
using System.Text;

namespace librule
{
    class DSL
    {
        private List<DSLOption> nt0_s;
        private List<DSLToken> nt1_s;
        private List<DSLRule> nt2_s;
        private Dictionary<string, DSLRule> rules;
        private Dictionary<string, DSLRefer> refers;
        private Dictionary<string, DSLRefer> tokenRefers;
        private Dictionary<string, Token> tokens;
        private Dictionary<ushort, string> parameterType;
        private TwoKeyDictionary<int, string, ushort> parameterToken;
        private Dictionary<string, (SourceLocation Location, string Value)> options;

        internal Lexicon Lexicon { get; }

        internal TextLines Lines { get; set; }

        internal Dictionary<string, DSLRule> Rules => rules;

        internal DSL(List<DSLOption> nt0_s, List<DSLToken> nt1_s, List<DSLRule> nt2_s)
        {
            this.nt0_s = nt0_s;
            this.nt1_s = nt1_s;
            this.nt2_s = nt2_s;

            rules = new Dictionary<string, DSLRule>();
            refers = new Dictionary<string, DSLRefer>();
            tokenRefers = new Dictionary<string, DSLRefer>();
            tokens = new Dictionary<string, Token>();
            parameterType = new Dictionary<ushort, string>();
            parameterToken = new TwoKeyDictionary<int, string, ushort>();
            options = new Dictionary<string, (SourceLocation Location, string Value)>();

            Lexicon = new Lexicon();
        }

        internal void Init(string ctx)
        {
            Lines = new TextLines(ctx);
        }

        internal ushort GetParameterToken(int index, string type)
        {
            if (!parameterToken.ContainsKey(index, type))
            {
                var token = (ushort)(parameterToken.Count + 1);
                parameterToken[index, type] = token;
                parameterType[token] = type;
            }

            return parameterToken[index, type];
        }

        internal string GetParameterType(ushort token)
        {
            return parameterType[token];
        }

        internal TargetBuilder Generate(bool isSimple)
        {
            // 解析选项
            for (var i = 0; i < nt0_s.Count; i++)
            {
                var opt = nt0_s[i];
                var ctx = opt.Context.Value.Trim();
                var slt = ctx.IndexOf('=');
                if (slt == -1 || ctx.StartsWith("%import") || ctx.StartsWith("%formal"))
                {
                    var name = ctx;
                    if (options.ContainsKey(name))
                        throw new FAException($"Line {opt.Context.Location.Line}:已包含了一个名为'{name}'的参数，无法重复定义。");

                    options[name] = (opt.Context.Location, string.Empty);
                }
                else
                {
                    var name = ctx.Substring(1, slt - 1);
                    var value = ctx.Substring(slt + 1);

                    if (options.ContainsKey(name))
                        throw new FAException($"Line {opt.Context.Location.Line}:已包含了一个名为'{name}'的参数，无法重复定义。");

                    options[name] = (new SourceLocation(opt.Context.Location.Line, opt.Context.Location.Start + slt + 2, opt.Context.Location.End), value.Trim());
                }
            }

            // 声明表达式
            for (var i = 0; i < nt1_s.Count; i++)
            {
                if (nt1_s[i].Name == Settings.MAIN_ENTRY_NAME)
                    throw new FAException($"Line {nt1_s[i].Line}:规则名不能以'{Settings.MAIN_ENTRY_NAME}'声明，它被生成器内部作为主要入口占用。");

                AddRefer(nt1_s[i].Create(this));
            }

            // 声明产生式
            for (var i = 0; i < nt2_s.Count; i++)
            {
                var rule = nt2_s[i];
                var name = rule.Name;
                if (name == Settings.MAIN_ENTRY_NAME)
                    throw new FAException($"Line {rule.Line}:规则名不能以'{Settings.MAIN_ENTRY_NAME}'声明，它被生成器内部作为主要入口占用。");

                rules.Add(name, rule);
                refers.Add(name, new DSLProductionRefer(name, rule.Type, rule.Line, null));
            }

            // 创建产生式
            var entryOption = GetOptionWithLine("parser.entry");

            // 获得入口产生式并生成代码
            var main = GetProduction(entryOption.Value, entryOption.Location);

            // 创建产生式快照
            main = new ConcatenationProduction<TableAction>(new ReportProduction<TableAction>(main), new EosProduction<TableAction>(Lexicon.Eos));
            var productionGraph = new ProductionGraph(Lexicon);
            var productionFigure = productionGraph.Figure("Main");
            main.Create(productionFigure, null, new Entry<ProductionMetadata>(productionFigure));

            // 需要在token table冲突解决完成后执行
            productionGraph.Final();

            // 查找DSL距离
            var edgeClosures = productionGraph.Edges.
                Where(x => x.SourceLocation.HasValue).
                OrderBy(x => x.SourceLocation.Value.Start).
                Select(x => new { Start = x.SourceLocation.Value.Start, End = x.SourceLocation.Value.End, Value = x }).ToArray();

            for (var i = 0; i < edgeClosures.Length - 1; i++) 
            {
                var curr = edgeClosures[i];
                var next = edgeClosures[i + 1];

                if (curr.End - next.Start == 0)
                    curr.Value.Flags |= EdgeFlags.Close;
            }
            
            // 创建table，在productionGraph.Final中重置了多义词token后生成table
            var prodctionTable = CreateTable(productionGraph) as ProductionTable;

            // 创建token table
            var tokenManager = new TokenTableManager(this);
            var productionStates = prodctionTable.StateCount;
            var productionTableRights = prodctionTable.Transitions.GroupBy(x => x.Left).ToDictionary(x => x.Key, x => x.ToArray());
            var supportClarity = GetOption<bool>("token.clarity", "false");

            // 令牌创建函数
            TokenTable CreateTokenTable(IEnumerable<Token> tokens, string followTableName)
            {
                // 创建对应的TokenTable
                var tokenGraph = new TokenGraph(supportClarity, Lexicon);
                var tokenFigure = tokenGraph.Figure("Main");

                foreach (var token in tokens.OrderBy(x => x.Expression.RepeatLevel()))
                {
                    // 首先展开token并查找previous表达式
                    if (followTableName != null && token.IsPrevious)
                    {
                        var previous = token.Expression.GetLast().Where(x => x.ExpressionType == RegularExpressionType.Previous).Cast<PreviousExpression<TableAction>>().ToArray();
                        for (var i = 0; i < previous.Length; i++)
                        {
                            previous[i].FollowTableName = followTableName;
                            previous[i].TokenIndex = token.Index;
                        }
                    }

                    tokenGraph.StartCollect();
                    var step = token.Expression.CreateGraphState(tokenFigure, new Entry<TokenMetadata>(tokenFigure), default);
                    var edge = tokenGraph.EndCollect();

                    var ends = new bool[tokenGraph.States.Max(x => x.Index) + 1];
                    foreach (var end in step.End)
                        ends[end.Index] = true;

                    foreach (var end in edge.Where(x => ends[x.Target.Index]))
                        tokenGraph.TokenEdge(end, token.Index);
                }

                // 创建table
                return CreateTable(tokenGraph) as TokenTable;
            }

            // 开始为每个状态点创建token table
            var productionQueue = new Queue<IList<FATransition<ProductionMetadata>>>(productionTableRights.Values);
            while (productionQueue.Count > 0)
            {
                var tableRight = productionQueue.Dequeue();
                var state = tableRight[0].Left;

                // 当右侧边拥有token歧义时进行消除
                // 查找所有组合的tokens,这里是为了得到词法冲突做后续操作
                var tokens = tableRight.SelectMany(x => x.Input.Chars.Select(y => Lexicon.Tokens[y])).Where(x => x != Lexicon.Missing).ToArray();

                // 获得follow tokens
                var follows = tableRight.Select(x => x.Right).Where(productionTableRights.ContainsKey).SelectMany(x => productionTableRights[x]).
                    SelectMany(x => x.Input.Chars.Select(y => Lexicon.Tokens[y])).Where(x => x != Lexicon.Missing).Distinct().Where(
                    x => x.Expression.GetLast().All(y => y.ExpressionType != RegularExpressionType.Previous)).ToArray();

                string followName = null;
                var tableName = tokens.GetUniqueName();
                var previousTokens = tokens.Any(x => x.IsPrevious);
                if (follows.Length > 0 && previousTokens) 
                {
                    followName = follows.GetUniqueName();
                    if (!tokenManager.ContainsKey(followName))
                        tokenManager[followName] = new ProductionTokenTable(
                        CreateTokenTable(follows.Distinct(), null),
                        follows.Select(x => x.Index).ToHashSet(),
                        false,
                        followName);

                    tableName = $"{tableName}_{followName}";
                }

                if (!tokenManager.ContainsKey(tableName))
                    tokenManager[tableName] = new ProductionTokenTable(
                    CreateTokenTable(tokens.Distinct(), followName ?? (previousTokens ? string.Empty : null)),
                    tokens.Select(x => x.Index).ToHashSet(),
                    false,
                    tableName);

                if (tokenManager.ContainsState(state))
                    throw new NotImplementedException();

                tokenManager[state] = tokenManager[tableName];

                continue;

                var tokenTable = tokenManager[tableName].TokenTable;

                // 如果没有冲突则直接略过
                var trans = tableRight.Where(x => x.Input.Chars.Any(y => tokenTable.IsConflictToken(y))).ToArray();
                if (trans.Length == 0)
                    continue;

                // 查询是否可以消除歧义,分支右侧大于1个不包含右侧链接时无法处理多义词，因为没有边可以接收右推元数据
                if (trans.Count(x => !productionTableRights.ContainsKey(x.Right) || productionTableRights[x.Right].Length == 0) > 1)
                    throw ReportDSLCreateException(GetOriginEdges(productionGraph, tableRight), prodctionTable, "无法消除多义词歧义，因为大于1条边没有可以接收元数据的右侧移进连接。");

                // 查询token中所有支持的tokens
                var supportTokens = tokenTable.Transitions.Select(x => x.Metadata.Token).Distinct().ToHashSet();

                for (var i = 0; i < tokenTable.ClarityTokens.Count; i++)
                {
                    // 获得歧义令牌
                    var clarityToken = tokenTable.ClarityTokens[i];

                    // 创建基础数据
                    var target = (ushort)(productionStates++);
                    var list = new List<FATransition<ProductionMetadata>>();
                    FATransition<ProductionMetadata> tokenEdge = default;

                    // 查找到冲突边并链接到源右侧
                    var clarityEdges = trans.Where(x => x.Input.Chars.Any(y => clarityToken.IsConflictToken(y))).ToArray();
                    foreach (var right in clarityEdges)
                    {
                        var token = (ushort)right.Input.Chars[0];

                        ProductionMetadata metadata = default;
                        // 如果edge.Metadata.Token不等于0则代表它会被引用，引用时才需要进行重赋值
                        if (right.Metadata.Token != 0)
                        {
                            if (tokenEdge.Input.Chars != null && tokenEdge.Metadata.Token != right.Metadata.Token)
                                throw ReportDSLCreateException(GetOriginEdges(productionGraph, new FATransition<ProductionMetadata>[] { tokenEdge, right }), prodctionTable, "无法消除多义词歧义，因为以上边使用的参数标记不一致。");

                            tokenEdge = right;
                            metadata = productionGraph.CreateMetadata(null, new TableAction($"${right.Metadata.Token}.ChangeToken({token})", 0, right.Metadata.Token), supportClarity);
                        }
                        else metadata = productionGraph.GetDefaultMetadata();

                        if (productionTableRights.ContainsKey(right.Right))
                        {
                            // 连接回子集
                            var dst = new FATransition<ProductionMetadata>(
                                target, right.Right, right.SourceLeft, right.SourceRight,
                                right.Input, right.Symbol, productionGraph.MergeMetadatas(metadata, right.Metadata));

                            list.Add(dst);
                            prodctionTable.InsertDelay(dst);
                        }
                        else
                        {
                            // 创建空链接
                            var dst = new FATransition<ProductionMetadata>(
                                target, (ushort)(productionStates++), right.SourceLeft, right.SourceRight,
                                prodctionTable.EmptyInput, default, productionGraph.MergeMetadatas(metadata, right.Metadata));

                            list.Add(dst);
                            prodctionTable.InsertDelay(dst);
                        }

                        // 检查边的token是否完全被消除
                        if (!supportTokens.Contains(token))
                            prodctionTable.RemoveDelay(right);
                    }

                    // 填入任务
                    if (list.Count > 0)
                        productionQueue.Enqueue(list);

                    // 添加一条连接来支持扫描到多义词时的情况
                    prodctionTable.InsertDelay(new FATransition<ProductionMetadata>(
                        state, target, clarityEdges[0].SourceLeft, clarityEdges[0].SourceRight,
                        new EdgeInput(clarityToken.Index), default, new ProductionMetadata(productionGraph.GetDefaultMetadata().Value, tokenEdge.Metadata.Token)));
                }
            }

            if (supportClarity)
                prodctionTable.Update();

            // 创建Skip TokenTable
            var skippingTokens = Lexicon.Tokens.Where(x => x.IsSkip).ToArray();
            if (skippingTokens.Length > 0)
            {
                var tokenTable = CreateTokenTable(skippingTokens, null);
                tokenManager[tokenManager.SkipTableName] = new ProductionTokenTable(
                    tokenTable, skippingTokens.Select(x => x.Index).ToHashSet(), true, tokenManager.SkipTableName);
            }

            // 将dsl翻译成源代码
            return new TargetBuilder(this, productionGraph, prodctionTable, tokenManager);
        }

        private static TokenColor HexToColor(string hexValue)
        {
            if (hexValue.StartsWith("#"))
            {
                hexValue = hexValue.Substring(1);
            }

            int length = hexValue.Length;

            if (length == 3 || length == 6 || length == 8)
            {
                if (int.TryParse(hexValue, System.Globalization.NumberStyles.HexNumber, null, out int hex))
                {
                    if (length == 3)
                    {
                        int red = (hex >> 8 & 0xF) * 17;
                        int green = (hex >> 4 & 0xF) * 17;
                        int blue = (hex & 0xF) * 17;
                        return new TokenColor(red, green, blue); // Color.FromArgb(red, green, blue);
                    }
                    else if (length == 6)
                    {
                        int red = (hex >> 16) & 0xFF;
                        int green = (hex >> 8) & 0xFF;
                        int blue = hex & 0xFF;
                        return new TokenColor(red, green, blue);
                    }
                    else if (length == 8)
                    {
                        int alpha = (hex >> 24) & 0xFF;
                        int red = (hex >> 16) & 0xFF;
                        int green = (hex >> 8) & 0xFF;
                        int blue = hex & 0xFF;
                        return new TokenColor(red, green, blue, alpha);
                    }
                }
            }

            throw new ArgumentException("Invalid hex value");
        }

        private void AddRefer(DSLRefer refer)
        {
            if (refer.Type == DSLReferType.Expression)
            {
                var expRef = refer as DSLExpressionRefer;
                var color = string.IsNullOrWhiteSpace(expRef.Color) ? null : new TokenColor?(HexToColor(expRef.Color));
                var token = Lexicon.DefineToken(expRef.Expression, refer.Name ?? Settings.SKIP_NAME, color);

                if (refer.Name != null)
                {
                    refer = new DSLProductionRefer(refer.Name, Settings.MATCH_LITERAL, refer.Line, token.AsTerminal());
                    if (token.ClearString != null)
                    {
                        tokens[token.ClearString] = token;
                        tokenRefers[token.ClearString] = refer;
                    }
                }
                else
                {
                    refer = null;
                    token.IsSkip = true;
                }
            }

            if (refer != null)
            {
                if (refers.ContainsKey(refer.Name))
                {
                    var r = refers[refer.Name];
                    if (r.Type == DSLReferType.Production)
                    {
                        var r2 = r as DSLProductionRefer;
                        if (r2.Production.ProductionType == ProductionType.Recursive)
                        {
                            var r3 = r2.Production as Production<TableAction>;
                            if (r3.Rule == null)
                            {
                                r3.Rule = new ReportProduction<TableAction>((refer as DSLProductionRefer).Production);
                                return;
                            }
                        }
                    }

                    throw new NotSupportedException($"Line {refer.Line}:规则'{refer.Name}'重复定义");
                }
                else
                {
                    refers[refer.Name] = refer;
                }
            }
        }

        internal ProductionBase<TableAction> GetProduction(string name, SourceLocation loc)
        {
            if (!refers.ContainsKey(name))
                throw new FAException($"{GetErrorLine(loc)}\n产生式'{name}'不存在，请检查拼写后在重新生成。");

            var refer = refers[name] as DSLProductionRefer;
            if (refer.Production == null)
            {
                refer.Production = new Production<TableAction>(name);
                AddRefer(rules[name].Create(this));
            }
            return refer.Production;
        }

        internal string GetOption(string name)
        {
            return GetOption(name, null);
        }

        internal string GetOption(string name, string @default = null)
        {
            return GetOptionWithLine(name, @default).Value;
        }

        internal (SourceLocation Location, string Value) GetOptionWithLine(string name, string @default = null)
        {
            if (!options.ContainsKey(name))
                if (@default == null)
                    throw new FAException($"输入流未包含合法选项,请在流头部检查'%{name}'选项是否存在后在重新生成。");
                else
                    return (default, @default);

            return options[name];
        }

        internal T GetOption<T>(string name, string @default = null)
        {
            var type = typeof(T);
            var option = GetOptionWithLine(name, @default);
            try
            {
                return (T)Convert.ChangeType(option.Value, type);
            }
            catch
            {
                throw new FAException($"{GetErrorLine(option.Location)}\n选项值'{option.Value}'非法,请检值是否是一个有效的{type.Name}类型后在重新生成。");
            }
        }

        internal IEnumerable<string> GetImports() 
        {
            return options.Where(x => string.IsNullOrWhiteSpace(x.Value.Value)).Where(x => x.Key.StartsWith("%import")).Select(x => x.Key.Substring(8));
        }

        internal IEnumerable<string> GetFormals()
        {
            return options.Where(x => string.IsNullOrWhiteSpace(x.Value.Value)).Where(x => x.Key.StartsWith("%formal")).Select(x => x.Key.Substring(8));
        }

        internal DSLRule GetRule(string name)
        {
            if (!rules.ContainsKey(name))
                return null;

            return rules[name];
        }

        internal DSLProductionRefer GetRefer(string name)
        {
            if (!refers.ContainsKey(name))
                return null;

            return refers[name] as DSLProductionRefer;
        }

        internal Token GetToken(string ctx)
        {
            if (!tokens.ContainsKey(ctx))
            {
                if (refers.ContainsKey(ctx))
                {
                    var refer = refers[ctx] as DSLProductionRefer;
                    if (refer.Production is Terminal<TableAction> terminal)
                        return terminal.Token;
                }

                return null;
            }

            return tokens[ctx];
        }

        internal Token GetToken(string ctx, int line)
        {
            if (!tokens.ContainsKey(ctx))
            {
                if (!GetOption<bool>("token.auto", "false"))
                {
                    throw new NotSupportedException($"Line {line}:未定义字面意为'{ctx}'的Token");
                }
                else
                {
                    var name = $"Auto Token {line} {Guid.NewGuid()}";
                    var token = Lexicon.DefineToken(RegularExpression<TableAction>.Literal(ctx), ctx);
                    var refer = new DSLProductionRefer(name, Settings.MATCH_LITERAL, line, token.AsTerminal());
                    tokens[token.ClearString] = token;
                    tokenRefers[token.ClearString] = refer;
                    refers[name] = refer;
                }
            }

            return tokens[ctx];
        }

        internal DSLRefer GetTokenRefer(string ctx, int line)
        {
            if (!tokenRefers.ContainsKey(ctx))
                throw new NotSupportedException($"Line {line}:未定义字面意为'{ctx}'的Token");

            return tokenRefers[ctx];
        }

        internal GraphTable<T> CreateTable<T>(IProductionGraph<T> graph)
        {
            GraphTable<T> table = graph.Tabulate();
            try
            {
                var inline = GetOption<int>("parser.inline", "-1");
                var timeout = GetOption<int>("parser.timeout", "5000");

                var flags = FATableFlags.None;
                flags |= GetOption<bool>("parser.keep", "false") ? FATableFlags.KeepSubset : FATableFlags.None;
                flags |= GetOption<bool>("parser.minimize", "true") ? FATableFlags.Minimize : FATableFlags.None;
                flags |= GetOption<bool>("parser.nfa", "false") ? FATableFlags.None : FATableFlags.ConflictResolution;

                return table.Generate(inline, timeout, flags);
            }
            catch (FAException ex)
            {
                switch (ex)
                {
                    case LeftRecursionOverflowException<T> ex1:
                        throw ReportDSLCreateException(GetOriginEdges(graph, ex1.Transition), table, "创建图失败,存在未能解决的左递归符号。"); 
                    case ConflictException<T> ex2:
                        throw ReportDSLCreateException(GetOriginEdges(graph, ex2.Transitions), table, "解决冲突失败,可能是由于规则完全一致或元数据不同导致。");
                    case SymbolConflictException<T> ex3:
                        throw ReportDSLCreateException(GetOriginEdges(graph, ex3.Groups.SelectMany(x => x.Transitions)), table, "解决冲突失败,未解决符号冲突。");
                    case MetadataConflictException<T> ex4:
                        throw ReportDSLCreateException(GetOriginEdges(graph, ex4.Groups.SelectMany(x => x.Transitions)), table, "解决冲突失败,未解决元数据冲突。");
                    case DiscontinuityException<T> ex5:
                        throw ReportDSLCreateException(GetOriginEdges(graph, ex5.Transition), table, "去符号化失败，某一条空边不存在后续连接。");    
                    case TimeoutException<T> ex6:
                        throw ReportDSLCreateException(GetOriginEdges(graph, ex6.Transitions), table, "生成表失败，通常是由声明的Token规则产生了NP(停机问题)导致解决冲突超时引发,请简化声明式或提高parser.timeout上限。");
                }

                throw new DSLException<T>("位置错误", table);
            }
        }

        private DSLException<T> ReportDSLCreateException<T>(IEnumerable<GraphEdge<T>> edges, GraphTable<T> table, string descrption)
        {
            var locs = edges.Where(x => x.SourceLocation.HasValue).Select(x => x.SourceLocation.Value).Distinct().OrderBy(x => x.Line).ToArray();
            var sb = new StringBuilder();
            for (var i = 0; i < locs.Length; i++)
                sb.AppendLine(GetErrorLine(locs[i]));

            return new DSLException<T>($"{sb}\n{descrption}", table, edges.ToArray());
        }

        private IEnumerable<GraphEdge<T>> GetOriginEdges<T>(IProductionGraph<T> graph, params FATransition<T>[] trans)
        {
            return GetOriginEdges(graph, trans as IEnumerable<FATransition<T>>);
        }

        private IEnumerable<GraphEdge<T>> GetOriginEdges<T>(IProductionGraph<T> graph, IEnumerable<FATransition<T>> trans)
        {
            var result = trans.SelectMany(x => graph.Edges.Where(y =>
                    y.Source.Index == x.SourceLeft &&
                    y.Target.Index == x.SourceRight &&
                    (y.GetInput().Equals(x.Input) || x.Input.Chars[0].Equals((char)Lexicon.Missing.Index)))).ToArray();

            if(result.Length == 0)
                result = trans.SelectMany(x => graph.Edges.Where(y =>
                    y.Source.Index == x.SourceLeft &&
                    y.Target.Index == x.SourceRight)).ToArray();

            return result;
        }

        internal string GetErrorLine(SourceLocation loc)
        {
            var sb = new StringBuilder();
            var header = $"Line {loc.Line}:";
            var line = Lines[loc.Line - 1];
            var tmp = $"{header}{line.Context.Replace('\t', ' ')}";
            sb.AppendLine(tmp);
            sb.AppendLine("~".PadLeft(loc.Length, '~').PadLeft(loc.Length + header.Length + loc.Start - line.Start));

            return sb.ToString();
        }

        internal class TextLines
        {
            private TextLine[] mLines;

            public TextLine this[int line] => mLines[line];

            public int Count => mLines.Length;

            public TextLines(string ctx)
            {
                mLines = ctx.Split('\n').Select(x => new TextLine(x, 0, 0)).ToArray();
                var start = 0;
                for (var i = 0; i < mLines.Length; i++)
                {
                    var line = mLines[i];
                    mLines[i] = new TextLine(line.Context, start, i);
                    start = start + line.Context.Length + 1;
                }
            }

            public struct TextLine
            {
                internal TextLine(string context, int start, int line)
                {
                    Context = context;
                    Start = start;
                    Line = line;
                }

                internal string Context { get; }

                internal int Start { get; }

                internal int Line { get; }
            }
        }
    }
}