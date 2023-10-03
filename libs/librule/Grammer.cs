using librule.generater;
using librule.productions;

namespace librule
{
    static class Grammer
    {
        public static ProductionBase<TableAction> AsTerminal(this Token token)
        {
            return new Terminal<TableAction>(token);
        }


        public static ProductionBase<TableAction> Many(this Token token)
        {
            return token.AsTerminal().Many(default(Token));
        }

        public static ProductionBase<TableAction> Many(this ProductionBase<TableAction> production)
        {
            return production.Many(default(Token));
        }

        public static ProductionBase<TableAction> Many(this Token token, Token seperator)
        {
            return token.Many(seperator?.AsTerminal());
        }

        public static ProductionBase<TableAction> Many(this Token token, ProductionBase<TableAction> separator)
        {
            return token.AsTerminal().Many(separator);
        }

        public static ProductionBase<TableAction> Many(this ProductionBase<TableAction> production, Token seperator)
        {
            return production.Many(seperator?.AsTerminal());
        }

        public static ProductionBase<TableAction> Many(this ProductionBase<TableAction> production, ProductionBase<TableAction> separator)
        {
            var many = new Production<TableAction>(production.ProductionName + "*");
            if (separator != null)
            {
                many.Rule = new ConcatenationProduction<TableAction>(production, many.PrefixedBy(separator).Optional());
            }
            else
            {
                many.Rule = new ConcatenationProduction<TableAction>(production, many.Optional());
            }

            return many.Optional();
        }

        public static ProductionBase<TableAction> Many1(this Token token)
        {
            return token.Many1(default(Token));
        }

        public static ProductionBase<TableAction> Many1(this Token token, Token seperator)
        {
            return token.AsTerminal().Many1(seperator?.AsTerminal());
        }

        public static ProductionBase<TableAction> Many1(this Token token, ProductionBase<TableAction> separator)
        {
            return token.AsTerminal().Many1(separator);
        }

        public static ProductionBase<TableAction> Many1(this ProductionBase<TableAction> production)
        {
            return production.Many1(default(Token));
        }

        public static ProductionBase<TableAction> Many1(this ProductionBase<TableAction> production, Token seperator)
        {
            return production.Many1(seperator?.AsTerminal());
        }

        public static ProductionBase<TableAction> Many1(this ProductionBase<TableAction> production, ProductionBase<TableAction> separator)
        {
            if (separator != null)
            {
                return new ConcatenationProduction<TableAction>(production, production.PrefixedBy(separator).Many());
            }
            else
            {
                return new ConcatenationProduction<TableAction>(production, production.Many());
            }
        }

        public static ProductionBase<TableAction> Close(this Token token, ProductionBase<TableAction> right)
        {
            return token.AsTerminal().Close(right);
        }

        public static ProductionBase<TableAction> Close(this ProductionBase<TableAction> left, ProductionBase<TableAction> right)
        {
            return new ConcatenationProduction<TableAction>(left, right);
        }

        public static ProductionBase<TableAction> Close(this ProductionBase<TableAction> left, Token right)
        {
            return new ConcatenationProduction<TableAction>(left, right.AsTerminal());
        }

        public static ProductionBase<TableAction> Optional(this Token token)
        {
            return token.AsTerminal().Optional();
        }

        public static ProductionBase<TableAction> Optional(this ProductionBase<TableAction> production)
        {
            if (production.ProductionType == ProductionType.Empty)
                return production;

            return new EmptyProduction<TableAction>() | production;
        }

        public static ProductionBase<TableAction> PrefixedBy(this Token token, ProductionBase<TableAction> prefix)
        {
            return token.AsTerminal().PrefixedBy(prefix);
        }

        public static ProductionBase<TableAction> PrefixedBy(this ProductionBase<TableAction> production, ProductionBase<TableAction> prefix)
        {
            return new ConcatenationProduction<TableAction>(prefix, production);
        }

    }
}
