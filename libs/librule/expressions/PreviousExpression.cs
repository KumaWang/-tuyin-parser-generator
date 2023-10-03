using libflow.stmts;
using librule.generater;
using librule.targets.code;
using System.Text;

namespace librule.expressions
{
    class PreviousExpression<TAction> : RegularExpression<TAction>
    {
        private char stopChar;
        private SymbolExpression<TAction> exp;

        public PreviousExpression(char stopChar)
        {
            this.stopChar = stopChar;

            exp = new SymbolExpression<TAction>(stopChar);
        }

        internal string FollowTableName { get; set; }

        internal ushort TokenIndex 
        { 
            get; 
            set;
        }

        internal override RegularExpressionType ExpressionType => RegularExpressionType.Previous;


        public override string GetDescrption()
        {
            return $"...{stopChar}";
        }

        public override RegularExpression<TAction> ExtractExclusionExpression()
        {
            return new SymbolExpression<TAction>(stopChar);
        }

        internal override IGraphEdgeStep<TMetadata> InternalCreate<TMetadata>(GraphFigure<TMetadata, TAction> figure, IGraphEdgeStep<TMetadata> step, TMetadata metadata)
        {
            if (FollowTableName != null)
                figure.GraphBox.CreateCodeCallback((sender, visitor) =>
                {
                    var opations = sender as CallbackSender;
                    var tableName = $"{Settings.MATCH_LITERAL}_{opations.Name}";

                    var sb = new StringBuilder();
                    sb.AppendLine($"if({Settings.TOKEN_LITERAL}==0)");
                    sb.AppendLine("{");
                    sb.AppendLine($"{Settings.INDEX_LITERAL}={Settings.START_INDEX_LITERAL};");
                    sb.AppendLine($"while(true)");
                    sb.AppendLine("{");

                    if (!string.IsNullOrWhiteSpace(opations.Name))
                    {
                        if (stopChar == 0)
                        {
                            sb.AppendLine($"if({Settings.INPUT_LITERAL}[{Settings.INDEX_LITERAL}]!={(int)stopChar})");
                        }
                        else
                        {
                            sb.AppendLine($"if((c={Settings.INPUT_LITERAL}[{Settings.INDEX_LITERAL}])!={(int)stopChar}&&c!='\\0')");
                        }
                        sb.AppendLine("{");
                        sb.AppendLine($"match={new Call(tableName, false, visitor.UserFormals, new Literal("false")).Visit(visitor)};");

                        sb.AppendLine($"if(match!=0)");
                        sb.AppendLine("{");
                        sb.AppendLine($"{Settings.INDEX_LITERAL}=match.SourceSpan.Start;");
                        sb.AppendLine($"{Settings.TOKEN_LITERAL}={opations.Index};");
                        sb.AppendLine("break;");
                        sb.AppendLine("}");

                        sb.AppendLine("else");
                        sb.AppendLine("{");
                        sb.AppendLine($"{Settings.INDEX_LITERAL}++;");
                        sb.AppendLine("}");

                        sb.AppendLine("}");


                        sb.AppendLine("else");
                        sb.AppendLine("{");
                        sb.AppendLine($"{Settings.TOKEN_LITERAL}=(ushort)(c=='\\0'?0:{opations.Index});");
                        sb.AppendLine("break;");
                        sb.AppendLine("}");
                        sb.AppendLine("}");
                        sb.AppendLine("}");
                    }
                    else 
                    {
                        if (stopChar == 0)
                        {
                            sb.AppendLine($"if({Settings.INPUT_LITERAL}[{Settings.INDEX_LITERAL}]!={(int)stopChar})");
                        }
                        else
                        {
                            sb.AppendLine($"if((c={Settings.INPUT_LITERAL}[{Settings.INDEX_LITERAL}])!={(int)stopChar}&&c!='\\0')");
                        }
                        sb.AppendLine("{");
                        sb.AppendLine($"{Settings.INDEX_LITERAL}++;");
                        sb.AppendLine("}");
                        sb.AppendLine("else");
                        sb.AppendLine("{");
                        sb.AppendLine("break;");
                        sb.AppendLine("}");

                        sb.AppendLine("}");
                        sb.AppendLine($"{Settings.TOKEN_LITERAL}={opations.Index};");
                        sb.AppendLine("}");
                    }

                    return sb.ToString();

                }, new CallbackSender(FollowTableName, TokenIndex));

            FollowTableName = null;
            TokenIndex = 999;
            return step;
        }

        internal override string GetClearString()
        {
            return string.Empty;
        }

        internal override int GetCompuateHashCode()
        {
            return int.MaxValue - exp.GetCompuateHashCode();
        }

        internal override int GetMaxLength()
        {
            return exp.GetMaxLength();
        }

        internal override int GetMinLength()
        {
            return exp.GetMinLength();
        }

        internal override int RepeatLevel()
        {
            return exp.RepeatLevel();
        }

        class CallbackSender 
        {
            public CallbackSender(string name, int index)
            {
                Name = name;
                Index = index;
            }

            public string Name { get; }

            public int Index { get; }
        }
    }
}
