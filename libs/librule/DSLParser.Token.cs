using librule.expressions;
using librule.generater;
using librule.utils;

namespace librule
{
    partial class DSLParser
    {
        private int left_ph_count = 0;

        private DSLTokenItem ParseTokenOr()
        {
            var tokenNode = ParseTokenToken();
            if (tokenNode == null)
                if (ParseLiteral(TokenType.OR, out var orToken))
                    return new LiteralTokenItem(orToken);
                else
                    return null;

            while (ParseLiteral(TokenType.OR, out var orToken))
            {
                var nextTokenNode = ParseTokenToken();
                if (nextTokenNode == null)
                    return new OrTokenItem(tokenNode, new LiteralTokenItem(orToken));

                tokenNode = new OrTokenItem(tokenNode, nextTokenNode);
            }

            return tokenNode;
        }

        private DSLTokenItem ParseTokenToken()
        {
            var tokenPostNode = ParseTokenPost();
            if (tokenPostNode == null)
                return null;

            while (true)
            {
                var nextTokenPostNode = ParseTokenPost();
                if (nextTokenPostNode == null)
                    break;

                tokenPostNode = new ConcatenationTokenItem(tokenPostNode, nextTokenPostNode);
            }

            return tokenPostNode;
        }

        private DSLTokenItem ParseTokenPost()
        {
            var tokenPrimNode = ParseTokenPrim();
            if (tokenPrimNode == null)
            {
                DSLLiteral postToken = null;
                if (ParseLiteral(TokenType.OPTIONAL, out postToken) ||
                    ParseLiteral(TokenType.MANY, out postToken) ||
                    ParseLiteral(TokenType.MANY1, out postToken) ||
                    ParseLiteral(TokenType.LEFT_BK, out postToken))
                    tokenPrimNode = new LiteralTokenItem(postToken);
            }

            while (true)
                if (ParseLiteral(TokenType.OPTIONAL))
                    tokenPrimNode = new OptionalTokenItem(tokenPrimNode);
                else if (ParseLiteral(TokenType.MANY))
                    tokenPrimNode = new ManyTokenItem(tokenPrimNode);
                else if (ParseLiteral(TokenType.MANY1))
                    tokenPrimNode = new Many1TokenItem(tokenPrimNode);
                else if (ParseLiteral(TokenType.LEFT_BK, out var leftToken))
                {
                    if (!ParseLiteral(TokenType.RUS, out var rusToken))
                        tokenPrimNode = new ConcatenationTokenItem(tokenPrimNode, new LiteralTokenItem(leftToken));
                    else
                    {
                        tokenPrimNode = new ActionTokenItem(tokenPrimNode, rusToken);
                        if (!ParseLiteral(TokenType.RIGHT_BK))
                            return null;
                    }
                }
                else break;

            if (ParseLiteral(TokenType.METADATA, out var metaItem))
                tokenPrimNode = new MetadataTokenItem(tokenPrimNode, metaItem);

            return tokenPrimNode;
        }

        private DSLTokenItem ParseTokenPrim()
        {
            if (ParseLiteral(TokenType.LEFT_PH, out var leftToken))
            {
                left_ph_count++;
                var tokenOrNode = ParseTokenOr();
                left_ph_count--;

                if (tokenOrNode == null)
                    return new LiteralTokenItem(leftToken);

                if (!ParseLiteral(TokenType.RIGHT_PH))
                    return null;

                return tokenOrNode;
            }

            var primToken = ParsePrevious() ?? ParseAnyToken() ?? ParseToToken() ?? ParseExceptToken();
            if (primToken == null)
                if ((left_ph_count == 0 && ParseLiteral(TokenType.RIGHT_PH, out var rightToken)) || ParseLiteral(TokenType.RIGHT_BK, out rightToken))
                    primToken = new LiteralTokenItem(rightToken);

            if (primToken != null)
                if (ParseLiteral(TokenType.METADATA, out var metaItem))
                    primToken = new MetadataTokenItem(primToken, metaItem);

            return primToken;
        }

        private DSLTokenItem ParseToToken() 
        {
            var first = ParseLiteral(TokenType.CHS, out var startChsToken);
            if (!first)
            {
                first = ParseLiteral(TokenType.TOKEN_CHS, out startChsToken);
                if (startChsToken != null)
                {
                    var token = Tokens.FirstOrDefault(x => x?.Name == startChsToken.Value);
                    if (token != null)
                        return new ReferenceTokenItem(token);
                }
            }

            if (ParseLiteral(TokenType.TO, out var toToken))
            {
                if (!first)
                    return new LiteralTokenItem(toToken);
                else if (ParseLiteral(TokenType.CHS, out var endChsToken) || ParseLiteral(TokenType.TOKEN_CHS, out endChsToken))
                    return new ToTokenItem(startChsToken, endChsToken);
                else
                    return null;
            }
            else if (ParseLiteral(TokenType.RANGE, out var rangeToken))
            {
                if (!first)
                    return new LiteralTokenItem(rangeToken);
                else if (ParseLiteral(TokenType.CHS, out var endChsToken) || ParseLiteral(TokenType.TOKEN_CHS, out endChsToken))
                    return new RangeTokenItem(startChsToken, endChsToken);
                else
                    return null;
            }
            else
            {
                if (startChsToken == null)
                    return null;

                if (startChsToken.Value.Length == 0)
                    return new EmptyTokenItem(startChsToken);

                return new LiteralTokenItem(startChsToken);
            }
        }

        private DSLTokenItem ParsePrevious()
        {
            if (ParseLiteral(TokenType.TO3, out var to3Token))
                if (ParseLiteral(TokenType.CHS, out var endChsToken) || ParseLiteral(TokenType.TOKEN_CHS, out endChsToken))
                {
                    if (endChsToken.Value.Length > 1)
                        throw new NotImplementedException("Previous表达式停止字不能超过1个字符。");

                    return new PreviousTokenItem(to3Token, endChsToken);
                }

            return null;
        }

        private DSLTokenItem ParseAnyToken()
        {
            if (ParseLiteral(TokenType.ANY, out var anyToken))
                return new AnyTokenItem(anyToken);

            return null;
        }

        private DSLTokenItem ParseExceptToken()
        {
            if (ParseLiteral(TokenType.XOR, out var xorToken))
                if (ParseLiteral(TokenType.CHS, out var chsToken) || ParseLiteral(TokenType.TOKEN_CHS, out chsToken))
                {
                    if (chsToken.Value.Length == 1)
                        return new ExceptTokenItem(chsToken);
                    else
                        return new UntilTokenItem(chsToken);
                }
                else return new LiteralTokenItem(xorToken);

            return null;
        }

        private DSLTokenItem ParseRangeToken()
        {
            var first = ParseLiteral(TokenType.CHS, out var startChsToken);
            if (!first)
            {
                first = ParseLiteral(TokenType.TOKEN_CHS, out startChsToken);
                if (startChsToken != null)
                {
                    var token = Tokens.FirstOrDefault(x => x?.Name == startChsToken.Value);
                    if (token != null)
                        return new ReferenceTokenItem(token);
                }
            }

            if (ParseLiteral(TokenType.RANGE, out var rangeToken))
            {
                if (!first)
                    return new LiteralTokenItem(rangeToken);
                else if (ParseLiteral(TokenType.CHS, out var endChsToken) || ParseLiteral(TokenType.TOKEN_CHS, out endChsToken))
                    return new RangeTokenItem(startChsToken, endChsToken);
                else
                    return null;
            }
            else
            {
                if (startChsToken == null)
                    return null;

                if(startChsToken.Value.Length == 0)
                    return new EmptyTokenItem(startChsToken);

                return new LiteralTokenItem(startChsToken);
            }
        }

        class ActionTokenItem : DSLTokenItem
        {
            private DSLTokenItem nt0_s;
            private DSLLiteral nt1_s;

            public ActionTokenItem(DSLTokenItem nt0_s, DSLLiteral nt1_s)
            {
                this.nt0_s = nt0_s;
                this.nt1_s = nt1_s;
            }

            public override RegularExpression<TableAction> Create(DSL dsl)
            {
                return new ActionExpression<TableAction>(nt0_s.Create(dsl), new TableAction(nt1_s.Value, 0, 0));
            }
        }

        class AnyTokenItem : DSLTokenItem
        {
            private DSLLiteral nt1_s;

            public AnyTokenItem(DSLLiteral nt1_s)
            {
                this.nt1_s = nt1_s;
            }

            public override RegularExpression<TableAction> Create(DSL dsl)
            {
                return new PositionExpression<TableAction>(RegularExpression<TableAction>.Any(), nt1_s.Location);
            }
        }

        class ConcatenationTokenItem : DSLTokenItem
        {
            private DSLTokenItem tmp_7_i;
            private DSLTokenItem nt1_s;

            public ConcatenationTokenItem(DSLTokenItem tmp_7_i, DSLTokenItem nt1_s)
            {
                this.tmp_7_i = tmp_7_i;
                this.nt1_s = nt1_s;
            }

            public override RegularExpression<TableAction> Create(DSL dsl)
            {
                return tmp_7_i.Create(dsl) > nt1_s.Create(dsl);
            }
        }

        class EmptyTokenItem : DSLTokenItem 
        {
            private DSLLiteral nt1_s;

            public EmptyTokenItem(DSLLiteral nt1_s)
            {
                this.nt1_s = nt1_s;
            }

            public override RegularExpression<TableAction> Create(DSL dsl)
            {
                return new PositionExpression<TableAction>(new EmptyExpression<TableAction>(), nt1_s.Location);
            }
        }

        class ExceptTokenItem : DSLTokenItem
        {
            private DSLLiteral nt1_s;

            public ExceptTokenItem(DSLLiteral nt1_s)
            {
                this.nt1_s = nt1_s;
            }

            public override RegularExpression<TableAction> Create(DSL dsl)
            {
                return new PositionExpression<TableAction>(RegularExpression<TableAction>.Except(nt1_s.Value.ToArray()), nt1_s.Location);
            }
        }

        class LiteralTokenItem : DSLTokenItem
        {
            private DSLLiteral nt1_s;

            public LiteralTokenItem(DSLLiteral nt1_s)
            {
                this.nt1_s = nt1_s;
            }

            public override RegularExpression<TableAction> Create(DSL dsl)
            {
                return new PositionExpression<TableAction>(RegularExpression<TableAction>.Literal(nt1_s.Value), nt1_s.Location);
            }
        }

        class Many1TokenItem : DSLTokenItem
        {
            private DSLTokenItem tmp_8_i;

            public Many1TokenItem(DSLTokenItem tmp_8_i)
            {
                this.tmp_8_i = tmp_8_i;
            }

            public override RegularExpression<TableAction> Create(DSL dsl)
            {
                return tmp_8_i.Create(dsl).Many1();
            }
        }

        class ManyTokenItem : DSLTokenItem
        {
            private DSLTokenItem tmp_8_i;

            public ManyTokenItem(DSLTokenItem tmp_8_i)
            {
                this.tmp_8_i = tmp_8_i;
            }

            public override RegularExpression<TableAction> Create(DSL dsl)
            {
                return tmp_8_i.Create(dsl).Many();
            }
        }

        class ToTokenItem : DSLTokenItem
        {
            private DSLLiteral startChsToken;
            private DSLLiteral endChsToken;

            public ToTokenItem(DSLLiteral startChsToken, DSLLiteral endChsToken)
            {
                this.startChsToken = startChsToken;
                this.endChsToken = endChsToken;
            }

            public override RegularExpression<TableAction> Create(DSL dsl)
            {
                var first = RegularExpression<TableAction>.Literal(startChsToken.Value);
                first = new ConcatenationExpression<TableAction>(first, RegularExpression<TableAction>.Until(endChsToken.Value));
                return first;
            }
        }

        class RangeTokenItem : DSLTokenItem
        {
            private DSLLiteral nt1_s;
            private DSLLiteral nt3_s;

            public RangeTokenItem(DSLLiteral nt1_s, DSLLiteral nt3_s)
            {
                this.nt1_s = nt1_s;
                this.nt3_s = nt3_s;
            }

            public override RegularExpression<TableAction> Create(DSL dsl)
            {
                RegularExpression<TableAction> expr = null;

                if (nt1_s.Value.Length > 1)
                    expr = RegularExpression<TableAction>.Literal(new string(nt1_s.Value.SelectRange(0, nt1_s.Value.Length - 1).ToArray()));

                var range =  RegularExpression<TableAction>.Range(nt1_s.Value.Last(), nt3_s.Value.First());
                expr = expr == null ? range : new ConcatenationExpression<TableAction>(expr, range);

                if (nt3_s.Value.Length > 1)
                    expr = new ConcatenationExpression<TableAction>(expr, RegularExpression<TableAction>.Literal(new string(nt3_s.Value.SelectRange(1, nt1_s.Value.Length).ToArray())));

                return new PositionExpression<TableAction>(expr, nt1_s.Location);
            }
        }

        class OrTokenItem : DSLTokenItem
        {
            private DSLTokenItem tmp_6_i;
            private DSLTokenItem nt2_s;

            public OrTokenItem(DSLTokenItem tmp_6_i, DSLTokenItem nt2_s)
            {
                this.tmp_6_i = tmp_6_i;
                this.nt2_s = nt2_s;
            }

            public override RegularExpression<TableAction> Create(DSL dsl)
            {
                return tmp_6_i.Create(dsl) | nt2_s.Create(dsl);
            }
        }

        class OptionalTokenItem : DSLTokenItem
        {
            private DSLTokenItem tmp_8_i;

            public OptionalTokenItem(DSLTokenItem tmp_8_i)
            {
                this.tmp_8_i = tmp_8_i;
            }

            public override RegularExpression<TableAction> Create(DSL dsl)
            {
                return tmp_8_i.Create(dsl).Optional();
            }
        }

        class UntilTokenItem : DSLTokenItem
        {
            private DSLLiteral nt1_s;

            public UntilTokenItem(DSLLiteral nt1_s)
            {
                this.nt1_s = nt1_s;
            }

            public override RegularExpression<TableAction> Create(DSL dsl)
            {
                return new PositionExpression<TableAction>(RegularExpression<TableAction>.Until(nt1_s.Value), nt1_s.Location);
            }
        }

        class PreviousTokenItem : DSLTokenItem 
        {
            private DSLLiteral self;
            private DSLLiteral stop;

            public PreviousTokenItem(DSLLiteral self, DSLLiteral stop)
            {
                this.self = self;
                this.stop = stop;
            }

            public override RegularExpression<TableAction> Create(DSL dsl)
            {
                var loc = self.Location;
                if (stop != null)
                    loc = new SourceLocation(loc.Line, loc.Start, stop.Location.End);

                return new PositionExpression<TableAction>(new PreviousExpression<TableAction>(stop == null || stop.Value.Length < 1 ? '\0' : stop.Value[0]), loc);
            }
        }

        class ReferenceTokenItem : DSLTokenItem 
        {
            private DSLToken token;

            public ReferenceTokenItem(DSLToken token)
            {
                this.token = token;
            }

            public override RegularExpression<TableAction> Create(DSL dsl)
            {
                return token.GetRegularExpression(dsl);
            }
        }

        class MetadataTokenItem : DSLTokenItem
        {
            private DSLTokenItem item;
            private DSLLiteral meta;

            public MetadataTokenItem(DSLTokenItem item, DSLLiteral backMetaItem)
            {
                this.item = item;
                this.meta = backMetaItem;
            }

            public override RegularExpression<TableAction> Create(DSL dsl)
            {
                var formals = string.Join(",", dsl.GetFormals().Select((x, i) => (x, i)).Select(x => $"{Settings.FORMAL_HEADER_LITERAL}{x.i}"));
                if (!string.IsNullOrWhiteSpace(formals))
                    formals = "," + formals;

                var action = new TableAction($"{Settings.METADATA_LITERAL}({Settings.INDEX_LITERAL},\"{meta.Value}\"{formals})", 0, 0);
                return new ActionExpression<TableAction>(item.Create(dsl), action);
            }
        }
    }
}
