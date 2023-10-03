using System.Collections.Generic;

namespace libfsm
{
    public sealed class SymbolGroup<T>
    {
        public SymbolGroup(FASymbol symbol, IList<FATransition<T>> transitions)
        {
            Symbol = symbol;
            Transitions = transitions;
        }

        public FASymbol Symbol { get; }

        public IList<FATransition<T>> Transitions { get; }
    }
}
