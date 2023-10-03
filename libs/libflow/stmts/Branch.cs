namespace libflow.stmts
{
    public abstract class Branch : Statement
    {
        public abstract IConditional Condition { get; set; }
    }
}
