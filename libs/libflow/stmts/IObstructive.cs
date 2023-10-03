namespace libflow.stmts
{
    public interface IObstructive : IConditional
    {
        IConditional Condition { get; set; }

        IAstNode Reason { get; set; }

        bool IConditional.CanMerge => Condition.CanMerge;

        int IConditional.ConditionalCount => Condition.ConditionalCount;

        IAstNode IConditional.Left 
        {
            get { return Condition.Left; }
            set { Condition.Left = value; }
        }

        IAstNode IConditional.Right => Condition.Right; 
    }
}
