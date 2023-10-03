using libflow.stmts;
using librule.targets.code;

namespace librule.targets
{
    internal class Debug : Operator, IAstNode
    {
        public Debug(string message)
        {
            Message = message;
        }

        public Debug(string message, IAstNode follow) 
            : this(message)
        {
            Follow = follow;
        }

        bool IAstNode.IsVaild => false;

        public string Message { get; }

        public IAstNode Follow { get; }

        public override IEnumerable<IAstNode> GetChildrens()
        {
            if (Follow != null)
                yield return Follow;
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            if (Follow != null)
                yield return Follow;
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            if (Follow != null)
                yield return Follow;
        }

        public override string ToString(CodeTargetVisitor visitor)
        {
            if (Follow != null)
                return $"//{Message}\n{Follow.Visit(visitor)}";

            return $"//{Message}";
        }
    }
}
