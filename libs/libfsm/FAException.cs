using libgraph;
using System;
using System.Collections.Generic;

namespace libfsm
{
    [Serializable]
    public class FAException : Exception
    {
        public FAException() { }
        public FAException(string message) : base(message) { }
        public FAException(string message, Exception inner) : base(message, inner) { }
        protected FAException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public class DiscontinuityException<T> : FAException 
    {
        public FATransition<T> Transition { get; }

        public DiscontinuityException(FATransition<T> transition)
        {
            Transition = transition;
        }
    }

    public class LeftRecursionOverflowException<T> : FAException 
    {
        public FATransition<T> Transition { get; }

        public LeftRecursionOverflowException(FATransition<T> transition) 
        {
            Transition = transition;
        }
    }

    public class TimeoutException<T> : FAException 
    {
        public IList<FATransition<T>> Transitions { get; }

        public TimeoutException(IList<FATransition<T>> transitions)
        {
            Transitions = transitions;
        }
    }

    public class ConflictException<T> : FAException 
    {
        public IList<FATransition<T>> Transitions { get; }

        public ConflictException(IList<FATransition<T>> transitions)
        {
            Transitions = transitions;
        }
    }

    public class SymbolConflictException<T> : FAException
    {
        public SymbolConflictException(IList<SymbolGroup<T>> groups)
        {
            Groups = groups;
        }

        public IList<SymbolGroup<T>> Groups { get; }
    }

    public class MetadataConflictException<T> : FAException
    {
        public MetadataConflictException(IList<MetadataGroup<T>> groups)
        {
            Groups = groups;
        }

        public IList<MetadataGroup<T>> Groups { get; }
    }
}
