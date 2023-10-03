using libflow;
using libflow.stmts;
using librule.generater;
using librule.utils;
using System.Data;
using System.Text;
using Boolean = libflow.stmts.Boolean;
using Index = libflow.stmts.Index;

namespace librule.targets.code
{
    class CSharpTargetVisitor : CodeTargetVisitor
    {
        public const string LABEL_HEADER = "LB";
        private bool mInsertState;
        private bool mGenerateOnMatch;
        private bool mInTokenVisit = false;
        private Dictionary<int, string> mTokenSwitchs;

        public CSharpTargetVisitor(TargetBuilder builder)
            : base(builder)
        {
            mInsertState = builder.GetOption<bool>("parser.state", "true");
            mGenerateOnMatch = builder.GetOption<bool>("token.callback", "true");
            mTokenSwitchs = new Dictionary<int, string>();
        }

        public override AstCreateFlags CreateFlags => AstCreateFlags.Default;

        public override CodeFormatter CodeFormatter { get; } = new CCodeFormatter();

        protected override string Imports(IEnumerable<string> imports)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine($"using {Settings.COLOR_TYPE}=System.Drawing.Color;");

            foreach (var import in imports)
                sb.AppendLine($"using {import};");

            return sb.ToString();
        }

        protected internal override IAstNode CreateObstructiveReason(GraphEdge<TargetMetadatas> edge, IConditional condition)
        {
            string GetLiteral(string val)
            {
                var sb = new StringBuilder();
                for (var i = 0; i < val.Length; i++)
                {
                    var c = val[i];
                    switch (c)
                    {
                        case '\\': sb.Append("\\\\"); break;
                        case '\"': sb.Append("\\\""); break;
                        case '}': sb.Append("}}"); break;
                        case '{': sb.Append("{{"); break;
                        default: sb.Append(c); break;
                    }
                }

                return sb.ToString();
            }

            if (mInTokenVisit)
            {
                var stmt =  Concatenation.From(
                        new Literal($"{Settings.INDEX_LITERAL}--;"),
                        new Goto(new Label(0)));

                stmt = new Concatenation(new Literal("{"), new Concatenation(stmt, new Literal("}")));
                return stmt;
            }
            else 
            {
                string GetTokenName(ProductionTokenTable table, ushort index)
                {
                    // 如果index是0?
                    var token = table.TokenTable.Lexicon.Tokens[table.TokenTable.HasTokenConverter ? table.TokenTable.GetTokenIndex(index) : index];
                    return GetLiteral(token.Description);
                }

                // 基础信息
                var table = GetStateTable(edge.Source.Index);

                // 后续文法 
                var getTokenName = Settings.GET_MATCH_NAME_LITERAL;
                if (table.TokenTable.HasTokenConverter)
                    getTokenName = $"{Settings.GET_MATCH_NAME_LITERAL}_{table.Name}";

                var getTokenLiteral = !table.TokenTable.HasTokenConverter ?
                    $"{getTokenName}({Settings.READ_STEP_LITERAL}.Token)" :
                    $"{getTokenName}[{Settings.READ_STEP_LITERAL}.Token]";

                var report = CreateReportError(condition.GetDeepSource(true), $"$\"应匹配'{GetTokenName(table, edge.GetInput().GetChars(ushort.MaxValue).First())}'时扫描到了一个'{{{getTokenLiteral}}}'。\"");
                //var @return = new Return(rule.Type == Settings.VOID_TYPE ? null : new Literal(Settings.RESULT_LITERAL));
                return Concatenation.From(new Literal("{"), report, new Colon(), new Literal("}"));
            }
        }

        protected internal override IAstNode CreateInsertState(ushort state)
        {
            return InsertState(state.ToString());
        }

        private IAstNode CreateReportError(IAstNode source, string message)
        {
            var args = new List<IAstNode>();
            args.Add(new Member(source, "SourceSpan"));
            args.Add(new Literal(message));
            args.AddRange(UserFormals);
            return new Call(Settings.REPORT_LITERAL, false, args);
        }

        private IAstNode InsertState(string state)
        {
            var args = new List<IAstNode>();
            args.Add(new Literal(Settings.INDEX_LITERAL));
            args.Add(new Literal(state));
            args.AddRange(UserFormals);
            return Concatenation.From(new Call(Settings.STATE_LITERAL, false, args), new Colon());
        }

        private IAstNode InsertState(string index, string state)
        {
            var args = new List<IAstNode>();
            args.Add(new Literal(index));
            args.Add(new Literal(state));
            args.AddRange(UserFormals);
            return Concatenation.From(new Call(Settings.STATE_LITERAL, false, args), new Colon());
        }


        private IAstNode GetState() 
        {
            var args = new List<IAstNode>();
            args.Add(new Literal(Settings.INDEX_LITERAL));
            args.AddRange(UserFormals);
            return Concatenation.From(new Call($"Get{Settings.STATE_LITERAL}", false, args), new Colon());
        }

        private string GetIfConditionContext(IConditional cond)
        {
            return cond.Visit(this);
        }

        private string GetWhileConditionContext(IConditional cond)
        {
            return cond == null ? "true" : cond.Visit(this);
        }

        protected override string CreateGetFollowWords(ProductionTable production)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"public (string Literal,string Snippet)[] {Settings.GET_FOLLOW_WORDS}(ushort state)");
            sb.AppendLine("{");
            sb.AppendLine("switch(state)");
            sb.AppendLine("{");

            var dict = new Dictionary<ushort, string>();
            foreach (var tran in production.Transitions) 
            {
                var tokenTable = GetStateTable(tran.Left);
                var convert = tokenTable.TokenTable.HasTokenConverter;
                var tokens = tran.Input.Chars.Where(x => x != 0).
                    Select(x => tokenTable.TokenTable.Lexicon.Tokens[convert ? tokenTable.TokenTable.GetTokenIndex(x) : x]).
                    Where(x => x.Index != production.Graph.Lexicon.Missing.Index && x.Index != production.Graph.Lexicon.Eos.Index).
                    ToArray();

                var clears = tokens.Where(x => x.IsClearly).ToArray();
                if (clears.Length > 0)
                    dict[tran.Left] = string.Join(",", clears.Select(x => $"(\"{GetLiteral(x.ClearString)}\",{(x.SnippetString == null ? "null" : GetLiteral(x.SnippetString))})"));
            }

            var groups = dict.GroupBy(x => x.Value);
            foreach (var group in groups) 
            {
                foreach (var @case in group)
                    sb.AppendLine($"case {@case.Key}:");

                sb.AppendLine($"return new (string,string)[] {{{group.Key}}};");
            }

            sb.AppendLine("default:");
            sb.AppendLine("return new (string,string)[0];");
            sb.AppendLine("}");
            sb.AppendLine("}");

            return sb.ToString();
        }

        protected internal override bool ContainsTokenSwitch(int hash)
        {
            return mTokenSwitchs.ContainsKey(hash);
        }

        protected internal override void CreateTokenSwitch(string switchName, int hash, int[] arr)
        {
            var arrType = arr.Max() switch
            {
                int n when n < byte.MaxValue => "byte",
                int n when n < ushort.MaxValue => "ushort",
                int n when n < int.MaxValue => "int",
                _ => "long"
            };

            mTokenSwitchs.Add(hash, $"private {arrType}[] {switchName}=new {arrType}[]{{{string.Join(",", arr.Select(x => x.ToString()))}}};\n");
        }

        protected override string CreateTokenTable(TokenTableManager manager)
        {
            mInTokenVisit = true;

            string CreateTokenTableCode(bool hasSkipTable, ProductionTokenTable table) 
            {
                var sb = new StringBuilder();
                var ctor = new TokenConstructor(this, TargetBuilder);
                var scanner = TokenScanner.Scan(ctor, table);
                foreach (Function func in FlowAnalyzer<GraphState<TargetMetadatas>, GraphEdge<TargetMetadatas>>.GetAsts(scanner.TargetGraph, ctor))
                    sb.AppendLine(func.Body.Visit(this));

                IAstNode tokenStmt = new Literal(Settings.TOKEN_LITERAL);
                if (table.TokenTable.HasTokenConverter)
                    tokenStmt = new Index(new Literal($"{Settings.GET_REAL_TOKEN_LITERAL}_{table.Name}"), tokenStmt);

                var onMatchStmt = new If(
                    new Logic(new Literal(Settings.TOKEN_LITERAL), new Literal("0"), LogicType.NotEqual),
                    Concatenation.From(new Call(
                            Settings.ONMATCH,
                            true,
                            UserFormals,
                            new Literal(Settings.START_INDEX_LITERAL),
                            new Literal(Settings.INDEX_LITERAL),
                            tokenStmt),
                        new Colon())).Visit(this);

                var locals = new List<(string Type, string Name)>
                {
                    ("ushort", $"{Settings.TOKEN_LITERAL}=0"),
                    ("int", $"{Settings.START_INDEX_LITERAL}={Settings.INDEX_LITERAL}"),
                    ("ushort", $"{Settings.INPUT_VARIABLE_LITERAL}=0")
                };

                if (!table.IsSkip)
                    locals.Add((Settings.MATCH_LITERAL, "match=default"));

                var localsGroup = locals.GroupBy(x => x.Type);
                var local = string.Join(string.Empty, localsGroup.Select(y => $"{y.Key} {string.Join(",", y.Select(x => $"{x.Name}"))};"));
                var formal = string.Join(",", UserFormals.Select(x => $"{x.Type} {x.Name}"));

                if (table.IsSkip)
                {
                    var end =                         
                        // 置换状态
                        (mInsertState ? $"{InsertState(Settings.START_INDEX_LITERAL, "lastState").Visit(this)}\n" : string.Empty) +
                        // 着色
                        (mGenerateOnMatch ? onMatchStmt : string.Empty);

                    var continueSkip =
                         new If(new Logic(new Literal(Settings.TOKEN_LITERAL), new Number(0), LogicType.NotEqual),
                            Concatenation.From(
                                new Call(manager.SkipTableName, false, UserFormals), 
                                new Colon())).Visit(this);

                    return
                        // Skip()
                        $"protected unsafe virtual void {manager.SkipTableName}({formal})\n" +
                        $"{{\n" +
                        local + "\n" +
                        // 填入token扫描方法
                        (mInsertState ? $"var lastState={GetState().Visit(this)}\n" : string.Empty) +
                        (mInsertState ? $"{CreateInsertState(0).Visit(this)}\n" : string.Empty) +
                        $"{sb}\n" +
                        $"{LABEL_HEADER}_0:{(string.IsNullOrWhiteSpace(end) ? ";" : string.Empty)}\n" +
                        end +
                        continueSkip + "\n" +
                        $"}}\n";
                }
                else
                {
                    var baseName = $"{Settings.MATCH_LITERAL}_{table.Name}";
                    var skipCtx = hasSkipTable ? $"{new Call(manager.SkipTableName, true, UserFormals).Visit(this)};\n" : string.Empty;
                    var singleTokenNames = string.Empty;
                    var getTokenName = Settings.GET_MATCH_NAME_LITERAL;
                    if (table.TokenTable.HasTokenConverter)
                    {
                        singleTokenNames = CreateTableTokenNameCode(table) + CreateTableTokenConverterCode(table);
                        getTokenName = $"{getTokenName}_{table.Name}";
                    }

                    var matchFormals = string.IsNullOrWhiteSpace(formal) ?
                        "bool close" : $"bool close,{formal}";

                    var header = string.Join("|", table.SupportTokens.Select(x => manager.Lexicon.Tokens[x].Description));
                    header = header.Replace(";", ((int)';').ToString());

                    return
                        $"#region {header}\n" +
                        singleTokenNames +
                        // TryMatch()
                        $"protected unsafe virtual Match {baseName}({matchFormals})\n" +
                        $"{{\n" +
                        $"if(!close){skipCtx}" +
                        local + "\n" +
                        // 填入token扫描方法
                        $"{sb}\n" +
                        $"{LABEL_HEADER}_0:\n" +
                        string.Join(string.Empty, table.TokenTable.Graph.Callback.Select(x => x.Invoke(this))) + "\n" +
                        $"match=new {Settings.MATCH_LITERAL}({Settings.TOKEN_LITERAL},{Settings.START_INDEX_LITERAL},{Settings.INDEX_LITERAL});\n" +
                        // 着色
                        (mGenerateOnMatch ? onMatchStmt : string.Empty) +
                        $"return match;\n" +
                        $"}}\n" +
                        "#endregion\n";
                }
            }

            string CreateTableTokenNameCode(ProductionTokenTable table) 
            {
                var tokens = new Dictionary<ushort, string>();
                foreach (var index in table.TokenTable.GetTokens())
                {
                    var token = table.TokenTable.Lexicon.Tokens[table.TokenTable.HasTokenConverter ? table.TokenTable.GetTokenIndex(index) : index];
                    tokens.Add(index, $"{{{index},\"{GetLiteral(token.Description)}\"}}");
                }

                if(!tokens.ContainsKey(0))
                    tokens.Add(0, $"{{{0},\"{GetLiteral(Settings.UNKNOWN)}\"}}");

                var items = string.Join(",", tokens.OrderBy(x => x.Key).Select(x => x.Value));
                return $"private Dictionary<ushort,string> {Settings.GET_MATCH_NAME_LITERAL}_{table.Name}=new(){{{items}}};\n";
            }

            string CreateTableTokenConverterCode(ProductionTokenTable table) 
            {
                var tokens = new Dictionary<ushort, string>();
                foreach (var index in table.TokenTable.GetTokens())
                {
                    var token = table.TokenTable.Lexicon.Tokens[table.TokenTable.HasTokenConverter ? table.TokenTable.GetTokenIndex(index) : index];
                    tokens.Add(index, $"{{{index},{token.Index}}}");
                }

                if (!tokens.ContainsKey(0))
                    tokens.Add(0, $"{{{0},{0}}}");

                var items = string.Join(",", tokens.OrderBy(x => x.Key).Select(x => x.Value));
                return $"private Dictionary<ushort,ushort> {Settings.GET_REAL_TOKEN_LITERAL}_{table.Name}=new(){{{items}}};\n";
            }

            string CreateLexiconTokenNameCode(Lexicon lexicon)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"public string {Settings.GET_MATCH_NAME_LITERAL}(ushort token)\n{{");
                sb.AppendLine("switch(token)\n{\n");
                foreach (var token in lexicon.Tokens)
                {
                    if (token.InProcessing || token.IsSkip)
                    {
                        sb.AppendLine($"case {token.Index}:");
                        sb.AppendLine($"return \"{GetLiteral(token.Description)}\";");
                    }
                }
                sb.AppendLine("default:");
                sb.AppendLine("throw new ArgumentException($\"字典中不包含Token为'{token}'的令牌。\");");
                sb.AppendLine("}\n}");

                sb.AppendLine($"public bool {Settings.IS_SKIP_TOKEN_LITERAL}(ushort token)\n{{");
                sb.AppendLine("switch(token)\n{\n");
                foreach (var token in lexicon.Tokens)
                {
                    if (token.InProcessing || token.IsSkip)
                    {
                        sb.AppendLine($"case {token.Index}:");
                        sb.AppendLine($"return {token.IsSkip.ToString().ToLower()};");
                    }
                }
                sb.AppendLine("default:");
                sb.AppendLine("throw new ArgumentException($\"字典中不包含Token为'{token}'的令牌。\");");
                sb.AppendLine("}\n}");

                return sb.ToString();
            }

            var sb = string.Join(",", manager.Lexicon.Tokens.Where(x => x.Color.HasValue).Select(x => $"{{{x.Index},{Settings.COLOR_TYPE}.FromArgb({x.Color.Value.A},{x.Color.Value.R},{x.Color.Value.G},{x.Color.Value.B})}}"));
            var colors = $"private readonly Dictionary<ushort,{Settings.COLOR_TYPE}> {Settings.COLORS_FIELD_LITERAL}=new(){{{sb}}};\n";
            var result = new StringBuilder();
            var first =
                   // string mInput
                   $"private unsafe char* {Settings.INPUT_LITERAL};\n" +
                   // int mLength
                   $"private int {Settings.LENGTH_LITERAL};\n" +
                   // int mIndex
                   $"private int {Settings.INDEX_LITERAL};\n" +
                   // tokens
                   "{TOKENS_BYTE_LITERALS}\n" +
                   // 颜色信息
                   colors;
 
            var hasSkipTable = manager.Any(x => x.Value.IsSkip);
            foreach (var tokenTable in manager.Values)
                result.Append(CreateTokenTableCode(hasSkipTable, tokenTable));

            // GetColor
            result.AppendLine($"public {Settings.COLOR_TYPE} {Settings.GET_MATCH_COLOR_LITERAL}(ushort token) => {Settings.COLORS_FIELD_LITERAL}.ContainsKey(token)?{Settings.COLORS_FIELD_LITERAL}[token]:{Settings.COLOR_TYPE}.Transparent;");
            result.AppendLine(CreateLexiconTokenNameCode(manager.Lexicon));
            result.Insert(0, first.Replace("{TOKENS_BYTE_LITERALS}", string.Join("\n", mTokenSwitchs.Select(x => x.Value))));

            mInTokenVisit = false;
            return result.ToString();
        }

        protected override string VisitFunction(Function stmt)
        {
            var rule = TargetBuilder.GetRuleFromOriginState(stmt.EntryPoint);
            var result = rule.Type == Settings.VOID_TYPE ? "return;\n" : $"return {Settings.RESULT_LITERAL};\n";
            var ctor = GetFunctionConstructor(rule.Name);

            if (ctor.EntryPoint != stmt.EntryPoint)
                throw new NotImplementedException();

            var bodyCode = stmt.Body.Visit(this);
            var locals = ctor.Locals.ToList();

            if (rule.Type != Settings.VOID_TYPE)
                locals.Add(new FunctionVariable($"{Settings.RESULT_LITERAL}", rule.Type));

            var localsGroup = locals.GroupBy(x => x.Type);
            var local = string.Join(string.Empty, localsGroup.Select(y => $"{y.Key} {string.Join(",", y.Select(x => $"{x.Name}=default"))};"));

            // 整理本地变量
            var code = string.Empty;
            if (rule.Name == Settings.MAIN_ENTRY_NAME)
            {
                var formals = new List<(string Type, string Name)>
                {
                    ("char*", "input"),
                    ("int", "length")
                };
                formals.AddRange(UserFormals.Select(x => (x.Type, x.Name)));
                var formal = string.Join(",", formals.Select(y => $"{y.Type} {y.Name}"));

                code = $"protected unsafe virtual {rule.Type} {rule.Name}({formal})\n" +
                    $"{{\n" +
                    $"{Settings.INDEX_LITERAL}=0;{Settings.INPUT_LITERAL}=input;{Settings.LENGTH_LITERAL}=length;{Settings.MATCH_LITERAL} {Settings.READ_STEP_LITERAL}=default;\n" +
                    local + "\n" + "\n" + bodyCode + result +
                    $"}}\n";

                if (code.IndexOf(Settings.READ_STEP_LITERAL) == code.LastIndexOf(Settings.READ_STEP_LITERAL))
                    code = code.Replace($"{Settings.MATCH_LITERAL} {Settings.READ_STEP_LITERAL}=default;", string.Empty);
            }
            else
            {
                var formals = ctor.Args.Select(x => (x.Type, x.Name)).ToList();
                formals.AddRange(UserFormals.Select(x => (x.Type, x.Name)));
                formals.Add((Settings.MATCH_LITERAL, $"{Settings.READ_STEP_LITERAL}=default"));
                var formal = string.Join(",", formals.Select(y => $"{y.Type} {y.Name}"));

                // 根据入口点反射到类型
                code = $"protected unsafe virtual {rule.Type} {rule.Name}({formal})\n" +
                    $"{{\n" +
                    local + "\n" + bodyCode + result +
                    $"}}\n";
            }

            return code;
        }

        protected override string VisitConditional(Conditional stmt)
        {
            return $"{stmt.Condition.Visit(this)}?{stmt.Consequent.Visit(this)}:{stmt.Alternate.Visit(this)}";
        }

        protected override string VisitMember(Member stmt)
        {
            return $"{stmt.Source.Visit(this)}.{stmt.MemberName}";
        }

        protected override string VisitIndex(Index stmt)
        {
            return $"{stmt.Source.Visit(this)}[{stmt.Value.Visit(this)}]";
        }

        protected override string VisitBlock(Block stmt)
        {
            return $"{{\n{string.Join("\n", stmt.Statements.Select(x => x.Visit(this)))}\n}}\n";
        }

        protected override string VisitEmpty(Empty stmt)
        {
            return string.Empty;
        }

        protected override string VisitBreak(Break stmt)
        {
            return "break;";
        }

        protected override string VisitCall(Call stmt)
        {
            return $"{stmt.FunctionName}({string.Join(",", stmt.Formals.Select(x => x.Visit(this)))})";
        }

        protected override string VisitConcatenation(Concatenation stmt)
        {
            return $"{stmt.Left.Visit(this)}{stmt.Right.Visit(this)}";
        }

        protected override string VisitContinue(Continue stmt)
        {
            return "continue;";
        }

        protected override string VisitLabel(DefineLabel stmt)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < stmt.Labels.Count; i++)
                if (stmt.Labels[i].Index != 0 || !mInTokenVisit)
                    sb.Append($"{LABEL_HEADER}_{stmt.Labels[i].Index}:");

            return sb.ToString();
        }

        protected override string VisitGoto(Goto stmt)
        {
            return $"goto {LABEL_HEADER}_{stmt.Label.Index};";
        }

        protected override string VisitObstructive(IObstructive stmt)
        {
            var cond = GetIfConditionContext(stmt.Condition);
            return $"if(!({cond})){stmt.Reason.Visit(this)}";
        }

        protected override string VisitIf(If stmt)
        {
            var cond = GetIfConditionContext(stmt.Condition);
            return $"if({cond})\n" +
                $"{{\n" +
                $"{stmt.Consequent.Visit(this)}\n" +
                $"}}\n";
        }

        protected override string VisitIfElse(IfElse stmt)
        {
            if (stmt.Alternate.AstNodeType == AstNodeType.If)
            {
                var cond = GetIfConditionContext(stmt.Condition);
                return $"if({cond})\n" +
                   $"{{\n" +
                   $"{stmt.Consequent.Visit(this)}\n" +
                   $"}}\n" +
                   $"else " + $"{stmt.Alternate.Visit(this)}\n";
            }
            else
            {
                var cond = GetIfConditionContext(stmt.Condition);
                return $"if({cond})\n" +
                       $"{{\n" +
                       $"{stmt.Consequent.Visit(this)}\n" +
                       $"}}\n" +
                       $"else\n{{\n" +
                       $"{stmt.Alternate.Visit(this)}\n" +
                       "\n}\n";
            }
        }

        protected override string VisitSwitch(Switch stmt)
        {
            var dict = new Dictionary<IAstNode, string>();
            string getBody(IAstNode node) 
            {
                if (!dict.ContainsKey(node))
                    dict[node] = node.Visit(this);

                return dict[node];
            }

            StringBuilder sb = new StringBuilder();
            var groups = stmt.Cases.Select(x => new { Condition = x.Condition, Body = getBody(x.Body) }).GroupBy(x => x.Body.GetHashCode()).ToArray();
            if (groups.Length == 1)
            {
                return groups.First().First().Body;
            }
            else
            {
                sb.AppendLine($"switch({stmt.Condition.Visit(this)})");
                sb.AppendLine("{");
                foreach (var group in groups)
                {
                    var isDefaultGroup = group.Any(x => x.Condition == null);
                    if (isDefaultGroup)
                        sb.AppendLine("default:");
                    else
                        foreach (var c in group)
                            sb.AppendLine($"case {c.Condition.Visit(this)}:");

                    var bodyCode = group.First().Body;
                    sb.AppendLine(bodyCode);
                }

                sb.AppendLine("}");
                return sb.ToString();
            }
        }

        protected override string VisitWhile(While stmt)
        {
            return $"while({GetWhileConditionContext(stmt.Condition)})\n" +
                     $"{{\n" +
                     stmt.Body.Visit(this) + "\n" +
                     $"}}\n";
        }

        protected override string VisitDoWhile(DoWhile stmt)
        {
            return $"do\n" +
                $"{{\n" +
                stmt.Body.Visit(this) + "\n" +
                $"}}\n" +
                $"while({GetWhileConditionContext(stmt.Condition)});\n";
        }

        protected override string VisitAssign(Assign stmt)
        {
            return $"{stmt.Left.Visit(this)}={stmt.Right.Visit(this)}";
        }

        protected override string VisitBinary(Logic stmt)
        {
            var oper = stmt.Type switch
            {
                LogicType.Equal => "==",
                LogicType.NotEqual => "!=",
                LogicType.And => "&&",
                LogicType.Or => "||",
                LogicType.LessThan => "<",
                LogicType.GreaterThan => ">",
                LogicType.LessEqual => "<=",
                LogicType.GreaterEqual => ">=",
                _ => throw new NotImplementedException()
            };

            return $"{stmt.Left.Visit(this)}{oper}{stmt.Right.Visit(this)}";
        }

        protected override string VisitArithmetic(Arithmetic stmt)
        {
            var oper = stmt.Type switch
            {
                ArithmeticType.Add => "+",
                ArithmeticType.Sub => "-",
                ArithmeticType.Mul => "*",
                ArithmeticType.Div => "/",
                ArithmeticType.Xor => "^",
                ArithmeticType.Rem => "%",
                ArithmeticType.And => "&",
                ArithmeticType.Or => "|",
                _ => throw new NotImplementedException()
            };

            return $"{stmt.Left.Visit(this)}{oper}{stmt.Right.Visit(this)}";
        }

        protected override string VisitPostfix(Postfix stmt)
        {
            var oper = stmt.PostfixType switch
            {
                PostfixType.Increment => "++",
                PostfixType.Decrement => "--",
                _ => throw new NotImplementedException()
            };

            return $"{stmt.Source.Visit(this)}{oper}";
        }

        protected override string VisitValue(Value stmt)
        {
            return $"{stmt.Left.Visit(this)}";
        }

        protected override string VisitBoolean(Boolean stmt)
        {
            return stmt.Value ? "true" : "false" ;
        }

        protected override string VisitNumber(Number stmt)
        {
            return stmt.Value.ToString();
        }

        protected override string VisitReturn(Return stmt)
        {
            if (mInTokenVisit)
            {
                return $"return {stmt.ReturnValue.Visit(this)};";
            }
            else
            {
                if (stmt.ReturnValue == null)
                    return "return;";

                return $"{Settings.RESULT_LITERAL}={stmt.ReturnValue.Visit(this)};";
            }
        }

        protected override string StartClass(string className)
        {
            return $"partial class {className}\n{{";
        }

        protected override string EndClass()
        {
            return "}";
        }

        protected override string StartNamespace(string @namespace)
        {
            return $"namespace {@namespace};";
        }

        private string GetLiteral(string val)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < val.Length; i++)
            {
                var c = val[i];
                switch (c)
                {
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }

            return sb.ToString();
        }
    }
}
