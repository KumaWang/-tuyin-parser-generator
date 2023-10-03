namespace libflow.stmts
{
    public interface IConditional : IAstNode
    {
        int ConditionalCount => 1;

        IAstNode Source => Left is IConditional conditional && Left.AstNodeType != AstNodeType.Assign ? conditional.Source : GetParentheseSource(false);

        IAstNode GetDeepSource(bool containsCondition = false, bool longSource = false)
        {
            if (Left is IConditional conditional)
                return conditional.GetDeepSource(containsCondition);

            if (Left is IArithmetic arithmetic)
                return arithmetic.Left;

            if (containsCondition && Left is Conditional condition)
                return condition.Condition.GetDeepSource(containsCondition);

            return GetParentheseSource(true, containsCondition, longSource);
        }

        IAstNode GetParentheseSource(bool containsAssign, bool containsCondition = false, bool longSource = false) 
        {
            if (Left is Parenthese parenthese) 
            {
                if (parenthese.Node is IConditional conditional && (containsAssign || parenthese.Node.AstNodeType != AstNodeType.Assign))
                {
                    var sl = new SourceList();
                    if (longSource && conditional.Right is Parenthese rightParenthese) 
                    {
                        if (rightParenthese.Node is IConditional conditional2 && (containsAssign || rightParenthese.Node.AstNodeType != AstNodeType.Assign))
                        {
                            var left = conditional.GetDeepSource();
                            var right = conditional2.GetDeepSource();

                            if (left is SourceList sl1)
                                sl.AddRange(sl1);
                            else
                                sl.Add(left);

                            if(right is SourceList sl2)
                                sl.AddRange(sl2);
                            else
                                sl.Add(right);

                            return sl;
                        }
                    }

                    return conditional.GetDeepSource();
                }

                if (containsCondition && parenthese.Node is Conditional condition)
                    return condition.Condition.GetParentheseSource(containsAssign);

                return parenthese.Node;
            }

            return Left;
        }

        IAstNode Left { get; set; }

        IAstNode Right { get; }

        bool CanMerge { get; }
    }
}
