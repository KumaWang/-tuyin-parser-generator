using libflow;
using libflow.stmts;
using libgraph;
using System.Collections.Concurrent;
using Edge = librule.generater.GraphEdge<librule.targets.TargetMetadatas>;
using State = librule.generater.GraphState<librule.targets.TargetMetadatas>;

namespace librule.targets.code
{
    class CodeConstructor : AstConstructor<State, Edge>
    {
        private TargetBuilder mBuilder;
        private CodeTargetVisitor mVisitor;
        private ConcurrentDictionary<int, ushort> mEntryPoints;

        public CodeConstructor(CodeTargetVisitor visitor, TargetBuilder builder, AstCreateFlags createFlags)
        {
            mVisitor = visitor;
            mBuilder = builder;
            CreateFlags = createFlags;
            mEntryPoints = new ConcurrentDictionary<int, ushort>();
        }

        public override AstCreateFlags CreateFlags { get; }

        public TargetBuilder TargetBuilder => mBuilder;

        public CodeTargetVisitor CodeVisitor => mVisitor;

        public override int OverBranchCountToSwitch => 3;

        public override void OnEntry(ushort entry)
        {
            mEntryPoints[Thread.CurrentThread.ManagedThreadId] = entry;
        }

        public override IAstNode CreateStatement(Edge edge, IConditional conditional)
        {
            bool isEmptyStatement = true;
            IAstNode stmt = null;

            if (edge.Metadata.Count > 0)
            {
                var addReturn = edge.Flags.HasFlag(EdgeFlags.SpecialPoint) && mBuilder.GetRuleFromOriginState(edge.Source.Index).Type != Settings.VOID_TYPE;
                var metas = new List<IAstNode>();

                for (var i = 0; i < edge.Metadata.Count; i++)
                {
                    var metadata = edge.Metadata[i];
                    if (!metadata.IsVaild)
                        continue;

                    var addColon = !addReturn || i != edge.Metadata.Count - 1;
                    if (metadata is TargetStatement control)
                        metas.Add(addColon && control.Statement is Expression ? Concatenation.From(control.Statement, new Colon()) : control.Statement);
                    else
                        metas.Add(addColon ? Concatenation.From(new Metadata(metadata), new Colon()) : new Metadata(metadata));
                }

                if (metas.Count > 0 && addReturn)
                    metas[^1] = new Return(metas[^1]);

                stmt = metas.Count > 0 ? metas[0] : null;
                for (var i = 1; i < metas.Count; i++)
                    stmt = new Concatenation(stmt, metas[i]);

                isEmptyStatement = stmt == null;
                if (mVisitor.Debug)
                    stmt = stmt == null ?
                        new Debug($"{edge.Source.Index}->{edge.Target.Index}\n") :
                        new Concatenation(new Debug($"{edge.Source.Index}->{edge.Target.Index}\n"), stmt);
            }

            if (CreateFlags.HasFlag(AstCreateFlags.InsertState) && !isEmptyStatement)
            {
                var insertState = mVisitor.CreateInsertState(edge.Source.Index);
                stmt = stmt == null ?
                    insertState :
                    new Concatenation(insertState, stmt);
            }

            return stmt;
        }

        public override IObstructive CreateObstructiveBranch(Edge edge, IConditional readCondition)
        {
            IConditional GetBranch(IAstNode source, char c) => c == mBuilder.Lexicon.Missing.Index ?
                new Value(source) :
                new Logic(source, new Number(c), LogicType.Equal);

            // 基础信息
            var table = mVisitor.GetStateTable(edge.Source.Index);
            var matchName = $"{Settings.MATCH_LITERAL}_{table.Name}";
            //if (table.TokenTable.HasTokenConverter)
            //    matchName = $"{Settings.MATCH_LITERAL}_{table.Name}";

            // 条件信息
            var close = new Literal(edge.Flags.HasFlag(EdgeFlags.Close).ToString().ToLower());
            var readStep = new Variable(edge.Source.Index, Settings.READ_STEP_LITERAL);
            IAstNode matchStmt = new Call(matchName, false, mVisitor.UserFormals, close);
            if (edge.Source.Index == mEntryPoints[Thread.CurrentThread.ManagedThreadId])
                matchStmt = new Assign(readStep, new Conditional(new Logic(readStep, new Number(0), LogicType.NotEqual), readStep, matchStmt));
            else
                matchStmt = new Assign(readStep, matchStmt);

            matchStmt = new Parenthese(matchStmt);

            var inputs = edge.GetInput().GetChars(mBuilder.Lexicon.Tokens.Count).ToArray();
            if (inputs.Length == 1 && inputs[0] == mBuilder.Lexicon.Missing.Index)
                return null;

            var condition = GetBranch(matchStmt, inputs[0]);
            for (var i = 1; i < inputs.Length; i++)
                condition = new Logic(condition, GetBranch(readStep, inputs[i]), LogicType.Or);

            // 返回分支
            return new Obstructive(condition, mVisitor.CreateObstructiveReason(edge, condition));
        }

        public override IAstNode GetIllegalValue(Edge[] edges)
        {
            return new Number(0);
        }

        public override IAstNode CreateWaitOne(Edge edge)
        {
            return new WaitOne();
        }
    }
}
