using libfsm;
using librule.generater;
using librule.productions;
using librule.targets.code;
using librule.targets.dot;
using librule.utils;
using System.Text;

namespace librule.targets
{
    class TargetBuilder
    {
        private DSL dsl;
        private ProductionGraph productionGraph;
        private ProductionTable prodctionTable;
        private TokenTableManager tokenManager;

        public TargetBuilder(DSL dsl, ProductionGraph productionGraph, ProductionTable productionTable, TokenTableManager tokenManager)
        {
            this.dsl = dsl;
            this.productionGraph = productionGraph;
            this.prodctionTable = productionTable;
            this.tokenManager = tokenManager;
        }

        public Lexicon Lexicon => dsl.Lexicon;

        public ProductionTable ProductionTable => prodctionTable;

        internal string Create() 
        {
            var format = dsl.GetOption<string>("parser.target", "csharp");
            ITargetVisitor visitor = format switch
            {
                "csharp" => new CSharpTargetVisitor(this),
                "dot" => new DotTargetVisitor(this),
                _ => throw new FAException($"参数'%parser.target'的值'{dsl.GetOption("parser.target")}'所对应的生成器还未实现。")
            };

            // 返回生成的数据
            return visitor.Create(productionGraph, prodctionTable, tokenManager);
        }

        internal string GetOption(string optionName)
        {
            return dsl.GetOption(optionName);
        }

        internal string GetOption(string optionName, string @default)
        {
            return dsl.GetOption(optionName, @default);
        }

        internal T GetOption<T>(string optionName, string @default = null)
        {
            return dsl.GetOption<T>(optionName, @default);
        }

        internal IEnumerable<string> GetImports() 
        {
            return dsl.GetImports();
        }

        internal IEnumerable<string> GetFormals() 
        {
            return dsl.GetFormals();
        }

        internal string GetTokenType(ushort token) 
        {
            return dsl.GetParameterType(token);
        }

        internal (string Type, string Name) GetRuleFromOriginState(ushort state)
        {
            // 获取一条点原始点的边
            var edge = prodctionTable.Transitions.Where(x => x.Left == state && x.Right != 0).SelectMany(x => productionGraph.Edges.Where(y =>
                    y.Source.Index == x.SourceLeft &&
                    y.Target.Index == x.SourceRight)).FirstOrDefault();

            if (edge == null)
                return (dsl.GetRefer(dsl.GetOption("parser.entry")).ReturnType, $"{Settings.FUNCTION_HEADER}{state}");

            if (edge.Source.Index == 1)
                return (dsl.GetRefer(dsl.GetOption("parser.entry")).ReturnType, $"{Settings.MAIN_ENTRY_NAME}");

            var refer = dsl.GetRefer(edge.FromProduction);
            if (!edge.IsEntry)
                return (refer.ReturnType, $"{Settings.FUNCTION_HEADER}{state}");

            return (refer.ReturnType, refer.Name);
        }

        internal IEnumerable<Token> GetRuleUsingTokens(ushort state) 
        {
            // 获取一条点原始点的边
            var edge = prodctionTable.Transitions.Where(x => x.Left == state && x.Right != 0).SelectMany(x => productionGraph.Edges.Where(y =>
                    y.Source.Index == x.SourceLeft &&
                    y.Target.Index == x.SourceRight)).FirstOrDefault();

            var rule = dsl.GetRule(edge.FromProduction ?? String.Empty);
            if (rule == null)
                return Array.Empty<Token>();

            var references = rule.GetReferences();

            return references.Select(x => dsl.GetToken(x)).Where(x => x != null);
        }

        internal static string Simple(DSL dsl, DSLRule[] rules)
        {
            var sb = new StringBuilder();
            // 插入规则代码
            foreach (var rule in rules)
            {
                rule.Print(dsl, sb);
                sb.AppendLine();
            }

            return new CCodeFormatter().Print(sb.ToString());
        }
    }
}
