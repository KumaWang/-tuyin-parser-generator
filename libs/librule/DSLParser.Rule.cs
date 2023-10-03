using libfsm;
using librule.generater;
using librule.productions;
using System.Text;

namespace librule
{
    partial class DSLParser
    {
        private DSLRuleItem ParseRuleOr()
        {
            //rule_or<RuleItem>
	        //  : rule_or '|' rule_action       ^ new OrRuleItem($1, $3)
            //  | rule_action                   ^$1
            //  ;

            var first = ParseRuleAction();
            if (first == null)
                return null;

            Skip();
            while (ParseLiteral(TokenType.OR))
            {
                Skip();
                var follow = ParseRuleAction();
                if (follow == null)
                    break;

                first = new OrRuleItem(first, follow);
                Skip();
            }

            return first;
        }

        private DSLRuleItem ParseRuleAction()
        {
            //rule_action<RuleItem>
            //  : { RUS }                       ^ new ActionRuleItem(new EmptyRuleItem(), $2)
            //  | ^ RUS_LINE                    ^ new ActionRuleItem(new EmptyRuleItem(), $1)
            //  | rule_rule { RUS }             ^ new ActionRuleItem($1, $3)
            //  | rule_rule ^ RUS_LINE          ^ new ActionRuleItem($1, $3)
            //  | rule_rule                     ^$1
            //  ;

            var first = ParseRuleRule();

            Skip();

            if (ParseLiteral(TokenType.XOR)) 
            {
                if (!ParseLiteral(TokenType.RUS_LINE, out var rusItem))
                    return null;

                if (first == null)
                    return new ActionRuleItem(new EmptyRuleItem(), rusItem);
                else
                    return new ActionRuleItem(first, rusItem);
            }
            else if (ParseLiteral(TokenType.LEFT_BK))
            {
                if (!ParseLiteral(TokenType.RUS, out var rusItem) || !ParseLiteral(TokenType.RIGHT_BK))
                    return null;

                if (first == null)
                    return new ActionRuleItem(new EmptyRuleItem(), rusItem);
                else
                    return new ActionRuleItem(first, rusItem);
            }

            return first ?? new EmptyRuleItem();
        }

        private DSLRuleItem ParseRuleRule()
        {
            //rule_rule<RuleItem>
            //  : rule_rule rule_prim           ^new ConcatenationRuleItem($1, $2)
            //  | rule_prim                     ^$1
            //  ;

            var first = ParseRulePrim();
            if (first == null)
                return null;

            while (true)
            {
                Skip();
                var follow = ParseRulePrim();
                if (follow == null)
                    break;

                first = new ConcatenationRuleItem(first, follow);
            }

            return first;
        }

        private DSLRuleItem ParseRulePrim()
        {
            //rule_prim<RuleItem>
            //  : ID                            ^ new ReferRuleItem($1)
            //  | CHS                           ^ new CharRuleItem($1)
            //  ;

            DSLRuleItem item = null;
            if (ParseLiteral(TokenType.ID, out var idItem))
                item = new ReferRuleItem(idItem);

            else if (ParseLiteral(TokenType.CHS, out var chsItem))
                item = new CharRuleItem(chsItem);

            while (ParseLiteral(TokenType.METADATA, out var metaItem))
                item = new MetadataRuleItem(item, metaItem);

            return item;
        }

        class ActionRuleItem : DSLRuleItem
        {
            private DSLRuleItem tmp_4_i;
            private DSLLiteral nt1_s;
            private bool[] formals;
            private string action;

            public ActionRuleItem(DSLRuleItem tmp_4_i, DSLLiteral nt1_s)
            {
                this.tmp_4_i = tmp_4_i;
                this.nt1_s = nt1_s;
            }

            public override void Initialize()
            {
                tmp_4_i.Initialize();
            }

            public override ProductionBase<TableAction> Create(DSL dsl, DSLRule rule)
            {
                var production = tmp_4_i.Create(dsl, rule);
                var flow = GetProductionFlow(production).ToArray();
                var rules = GetRuleItemFlow(tmp_4_i).ToArray();
                var visitor = new bool[flow.Length];

                if (flow.Length != rules.Length)
                    throw new NotImplementedException("Production创建异常。");

                //if (nt1_s.Value.Trim()
                formals = new bool[flow.Length];

                /*
                var match = Regex.Match(nt1_s.Value.Trim(), @"\$(\w+)$");
                if (match.Success)
                {
                    int x = int.Parse(match.Groups[1].Value);
                    if (x == flow.Length)
                    {
                        action = action.Trim();

                        var first = flow[0];
                        for (var i = 1; i < flow.Length; i++)
                            first = new ConcatenationProduction<TableAction>(first, flow[i]);

                        return new ActionProduction<TableAction>(first, new TableAction(action, 0, 0));
                    }
                }
                */

                var numMode = false;
                var numIndex = 0;
                var lastIndex = 0;
                action = string.Empty;
                for (var i = 0; i < nt1_s.Value.Length; i++)
                {
                    var c = nt1_s.Value[i];
                    var l = i == nt1_s.Value.Length - 1 && char.IsDigit(c);
                    if (numMode)
                    {
                        if (char.IsDigit(c) && !l)
                        {
                        }
                        else
                        {
                            numMode = false;
                            var numLen = i - numIndex - (l ? 0 : 1);
                            if (numLen != 0)
                            {
                                var numStr = nt1_s.Value.Substring(numIndex + 1, numLen);
                                var num = int.Parse(numStr) - 1;
                                if (num < 0 || num >= flow.Length)
                                    throw new FAException($"Line {nt1_s.Location.Line}:在规则'{rule.Name}'中'${numStr}'超出参数索引，索引从1开始，请检查后重新生成。");

                                // 将索引倒叙
                                var parameterType = dsl.GetRefer(rules[num].GetDefineName(dsl)).ReturnType;
                                if (parameterType == Settings.VOID_TYPE)
                                    throw new FAException($"Line {nt1_s.Location.Line}:在规则'{rule.Name}'中参数'${numStr}'不可标记，因为'{rules[num].GetDefineName(dsl)}'是一个'{Settings.VOID_TYPE}'类型，它不存在返回值。");

                                var parameterToken = dsl.GetParameterToken(num, parameterType);
                                var parameterName = $"${parameterToken}";

                                formals[num] = true;
                                if (!visitor[num])
                                {
                                    flow[num] = new ParameterProduction<TableAction>(flow[num], parameterToken);
                                    rules[num].Token = parameterToken;
                                    visitor[num] = true;
                                }

                                var tmpLen = numIndex - lastIndex;
                                var tmp = tmpLen > 0 ? nt1_s.Value.Substring(lastIndex, tmpLen) : string.Empty;
                                action = action + tmp + parameterName;
                                lastIndex = l ? i + 1 : i;
                            }
                            else
                            {
                                if (rule.Type == Settings.VOID_TYPE)
                                    throw new FAException($"Line {nt1_s.Location.Line}:在规则'{rule.Name}'中不可声明返回标记，因为它是一个'{Settings.VOID_TYPE}'类型，不存在返回值。");
                            }
                        }
                    }
                    else
                    {
                        if (c == '$')
                        {
                            numMode = true;
                            numIndex = i;
                        }
                    }
                }

                var lastLen = nt1_s.Value.Length - lastIndex;
                if (lastLen > 0)
                    action = action + nt1_s.Value.Substring(lastIndex, lastLen);

                var first = flow[0];
                for (var i = 1; i < flow.Length; i++)
                    first = new ConcatenationProduction<TableAction>(first, flow[i]);

                return new ActionProduction<TableAction>(first, new TableAction(action, 0, 0));
            }

            public override void Print(DSL dsl, StringBuilder sb, bool isFormal)
            {
                var flows = GetRuleItemFlow(tmp_4_i).ToArray();
                for (var i = 0; i < flows.Length; i++)
                {
                    var flow = flows[i];
                    if (formals[i])
                        sb.Append($"{flow.Token}=");

                    flow.Print(dsl, sb, formals[i]);
                }

                sb.AppendLine($"result={action};");
            }

            public override IEnumerable<string> GetReferences()
            {
                return tmp_4_i.GetReferences();
            }

            private IEnumerable<DSLRuleItem> GetRuleItemFlow(DSLRuleItem item)
            {
                if (item is ConcatenationRuleItem c)
                {
                    foreach (var first in GetRuleItemFlow(c.Left).Concat(GetRuleItemFlow(c.Right)))
                        yield return first;
                }
                else
                {
                    yield return item;
                }
            }

            private IEnumerable<ProductionBase<TableAction>> GetProductionFlow(ProductionBase<TableAction> production)
            {
                if (production.ProductionType == ProductionType.Concatenation)
                {
                    var conncat = production as ConcatenationProduction<TableAction>;
                    foreach (var first in conncat.GetChildrens())
                        foreach (var follow in GetProductionFlow(first))
                            yield return follow;
                }
                else
                {
                    yield return production;
                }
            }
        }

        class CharRuleItem : DSLRuleItem
        {
            private DSLLiteral nt1_s;

            public CharRuleItem(DSLLiteral nt1_s)
            {
                this.nt1_s = nt1_s;
            }

            public override void Initialize()
            {
            }

            public override string GetDefineName(DSL dsl)
            {
                return dsl.GetTokenRefer(nt1_s.Value, nt1_s.Location.Line).Name;
            }

            public override ProductionBase<TableAction> Create(DSL dsl, DSLRule rule)
            {
                return new PositionProduction<TableAction>(dsl.GetToken(nt1_s.Value, nt1_s.Location.Line).AsTerminal(), nt1_s.Location);
            }

            public override void Print(DSL dsl, StringBuilder sb, bool isFormal)
            {
                var refer = dsl.GetTokenRefer(nt1_s.Value, nt1_s.Location.Line);
                sb.AppendLine($"{(isFormal ? "Match" : "Check")}({refer.Name});");
            }

            public override IEnumerable<string> GetReferences()
            {
                return new string[0];
            }
        }

        class ConcatenationRuleItem : DSLRuleItem
        {
            private DSLRuleItem tmp_3_i;
            private DSLRuleItem nt1_s;

            public ConcatenationRuleItem(DSLRuleItem tmp_3_i, DSLRuleItem nt1_s)
            {
                this.tmp_3_i = tmp_3_i;
                this.nt1_s = nt1_s;
            }

            public override void Initialize()
            {
                GetRuleItemFlow(this).Last().IsEndRuleItem = true;
            }

            internal DSLRuleItem Left => tmp_3_i;

            internal DSLRuleItem Right => nt1_s;

            public override ProductionBase<TableAction> Create(DSL dsl, DSLRule rule)
            {
                return new ConcatenationProduction<TableAction>(tmp_3_i.Create(dsl, rule), nt1_s.Create(dsl, rule));
            }

            public override void Print(DSL dsl, StringBuilder sb, bool isFormal)
            {
                tmp_3_i.Print(dsl, sb, isFormal);
                nt1_s.Print(dsl, sb, isFormal);
            }

            public override IEnumerable<string> GetReferences()
            {
                return tmp_3_i.GetReferences().Union(nt1_s.GetReferences());
            }

            private static IEnumerable<DSLRuleItem> GetRuleItemFlow(DSLRuleItem item)
            {
                if (item is ConcatenationRuleItem orItem)
                {
                    foreach (var first in GetRuleItemFlow(orItem.tmp_3_i).Concat(GetRuleItemFlow(orItem.nt1_s)))
                        yield return first;
                }
                else
                {
                    yield return item;
                }
            }
        }

        class EmptyRuleItem : DSLRuleItem
        {
            public override void Initialize()
            {

            }

            public override ProductionBase<TableAction> Create(DSL dsl, DSLRule rule)
            {
                return new EmptyProduction<TableAction>();
            }

            public override void Print(DSL dsl, StringBuilder sb, bool isFormal)
            {
            }

            public override IEnumerable<string> GetReferences()
            {
                return new string[0];
            }
        }

        class OrRuleItem : DSLRuleItem
        {
            private DSLRuleItem tmp_2_i;
            private DSLRuleItem nt2_s;

            public OrRuleItem(DSLRuleItem tmp_2_i, DSLRuleItem nt2_s)
            {
                this.tmp_2_i = tmp_2_i;
                this.nt2_s = nt2_s;
            }

            public override void Initialize()
            {
                tmp_2_i.Initialize();
                nt2_s.Initialize();
            }

            public override ProductionBase<TableAction> Create(DSL dsl, DSLRule rule)
            {
                return new OrProduction<TableAction>(tmp_2_i.Create(dsl, rule), nt2_s.Create(dsl, rule));
            }

            public override void Print(DSL dsl, StringBuilder sb, bool isFormal)
            {
                var flows = GetRuleItemFlow(this).ToArray();
                sb.AppendLine($"switch(Next())\n{{");
                for (var i = 0; i < flows.Length; i++)
                {
                    var flow = flows[i];
                    sb.AppendLine($"case {i}:");
                    flow.Print(dsl, sb, isFormal);
                    sb.AppendLine("break;");
                }
                sb.AppendLine("}");
            }

            public override IEnumerable<string> GetReferences()
            {
                return tmp_2_i.GetReferences().Union(nt2_s.GetReferences());
            }

            private static IEnumerable<DSLRuleItem> GetRuleItemFlow(DSLRuleItem item)
            {
                if (item is OrRuleItem orItem)
                {
                    foreach (var first in GetRuleItemFlow(orItem.tmp_2_i).Concat(GetRuleItemFlow(orItem.nt2_s)))
                        yield return first;
                }
                else
                {
                    yield return item;
                }
            }
        }

        class ReferRuleItem : DSLRuleItem
        {
            private DSLLiteral nt1_s;

            public ReferRuleItem(DSLLiteral nt1_s)
            {
                this.nt1_s = nt1_s;
            }

            public string ReferenceName => nt1_s.Value;

            public SourceLocation SourceLocation => nt1_s.Location;

            public override void Initialize()
            {
            }

            public override string GetDefineName(DSL dsl)
            {
                return ReferenceName;
            }

            public override ProductionBase<TableAction> Create(DSL dsl, DSLRule rule)
            {
                var loc = SourceLocation;
                return new PositionProduction<TableAction>(dsl.GetProduction(nt1_s.Value, loc), loc);
            }

            public override void Print(DSL dsl, StringBuilder sb, bool isFormal)
            {
                var production = dsl.GetProduction(nt1_s.Value, SourceLocation);
                if (production.ProductionType == ProductionType.Terminal)
                    sb.AppendLine($"{(isFormal ? "Match" : "Check")}({nt1_s.Value});");
                else
                    sb.AppendLine($"{nt1_s.Value}();");
            }

            public override IEnumerable<string> GetReferences()
            {
                yield return nt1_s.Value;
            }
        }

        class MetadataRuleItem : DSLRuleItem
        {
            private DSLRuleItem item;
            private DSLLiteral meta;

            public MetadataRuleItem(DSLRuleItem item, DSLLiteral backMetaItem)
            {
                this.item = item;
                this.meta = backMetaItem;
            }

            public override ProductionBase<TableAction> Create(DSL dsl, DSLRule rule)
            {
                var formals = string.Join(",", dsl.GetFormals().Select((x, i) => (x, i)).Select(x => $"{Settings.FORMAL_HEADER_LITERAL}{x.i}"));
                if (!string.IsNullOrWhiteSpace(formals))
                    formals = "," + formals;

                return new MetadataProduction<TableAction>(item.Create(dsl, rule), new TableAction($"{Settings.METADATA_LITERAL}({Settings.INDEX_LITERAL},\"{meta.Value}\"{formals})", 0, 0));
            }

            public override string GetDefineName(DSL dsl)
            {
                return item.GetDefineName(dsl);
            }

            public override IEnumerable<string> GetReferences()
            {
                return item.GetReferences();
            }

            public override void Initialize()
            {
                item.Initialize();
            }

            public override void Print(DSL dsl, StringBuilder sb, bool isFormal)
            {
                item.Print(dsl, sb, isFormal);
                sb.Remove(sb.Length - 2, 1);
                sb.AppendLine($"[{meta.Value}]");
            }
        }
    }
}
