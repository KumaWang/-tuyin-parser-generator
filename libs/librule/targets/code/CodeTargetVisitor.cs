using libflow;
using libflow.stmts;
using librule.generater;
using System.Text;

namespace librule.targets.code
{
    abstract class CodeTargetVisitor : AstNodeVisitor<string>, ITargetVisitor
    {
        private Dictionary<string, IFunctionConstructor> mFuncs;
        private TokenTableManager mTokenTableManager;

        public CodeTargetVisitor(TargetBuilder builder)
        {
            Debug = builder.GetOption<bool>("parser.debug", "false");
            TargetBuilder = builder;
            UserFormals = TargetBuilder.GetFormals().Select((x, i) => new Formal($"{Settings.FORMAL_HEADER_LITERAL}{i}", x)).ToArray();
        }

        public bool HasSkipTable => mTokenTableManager.Any(x => x.Value.IsSkip);

        public string SkipTableName => mTokenTableManager.SkipTableName;

        public abstract AstCreateFlags CreateFlags { get; }

        public bool Debug { get; }

        public TargetBuilder TargetBuilder { get; }

        public abstract CodeFormatter CodeFormatter { get; }

        public Formal[] UserFormals { get; }

        public string Create(ProductionGraph productionGraph, ProductionTable prodctionTable, TokenTableManager manager)
        {
            mTokenTableManager = manager;
            var preview = TargetBuilder.GetOption<bool>("parser.state", "true");
            var flags = preview ? CreateFlags | AstCreateFlags.InsertState : CreateFlags;

            // 根据graph将所有元数据参数进行替换
            var ctor = new CodeConstructor(this, TargetBuilder, flags);
            var scanResult = MetadataScanner.Scan(ctor, productionGraph, prodctionTable, manager);
            mFuncs = scanResult.FunctionConstructors.ToDictionary(x => x.FunctionName);

            // 生成代码
            var tmp = new StringBuilder();
            tmp.AppendLine(Imports(TargetBuilder.GetImports()));
            tmp.AppendLine(StartNamespace(TargetBuilder.GetOption("parser.namespace", $"{TargetBuilder.GetOption("parser.entry")}Namsespace")));
            tmp.AppendLine(StartClass(TargetBuilder.GetOption("parser.class", $"{TargetBuilder.GetOption("parser.entry")}Parser")));
            tmp.AppendLine(CreateTokenTable(manager));
            if (preview) tmp.AppendLine(CreateGetFollowWords(prodctionTable));
            foreach (var stmt in FlowAnalyzer<GraphState<TargetMetadatas>, GraphEdge<TargetMetadatas>>.GetAsts(scanResult.TargetGraph, ctor))
                tmp.AppendLine(stmt.Visit(this));
            tmp.Append(EndClass());
            tmp.AppendLine(EndNamespace());

            return CodeFormatter.Print(tmp.ToString());
        }

        protected virtual string CreateGetFollowWords(ProductionTable table)
        {
            return string.Empty;
        }

        protected internal abstract bool ContainsTokenSwitch(int hash);

        protected internal abstract void CreateTokenSwitch(string switchName, int hash, int[] arr);

        protected abstract string CreateTokenTable(TokenTableManager manager);

        protected internal abstract IAstNode CreateObstructiveReason(GraphEdge<TargetMetadatas> edge, IConditional condition);

        protected virtual string Imports(IEnumerable<string> imports) 
        {
            return string.Empty;
        }

        protected abstract string StartClass(string className);

        protected abstract string EndClass();

        protected virtual string StartNamespace(string @namespace) 
        {
            return string.Empty;
        }

        protected virtual string EndNamespace() 
        {
            return string.Empty;
        }

        protected override string VisitParenthese(Parenthese stmt)
        {
            return $"({stmt.Node.Visit(this)})";
        }

        protected override string VisitExternal(IAstNode stmt)
        {
            return (stmt as Operator).ToString(this);
        }

        protected IFunctionConstructor GetFunctionConstructor(string name) => mFuncs[name];

        internal string GetStateTableName(ushort state) => mTokenTableManager[state].Name;

        internal ProductionTokenTable GetStateTable(ushort state) => mTokenTableManager[state];

        protected internal abstract IAstNode CreateInsertState(ushort state);
    }
}
