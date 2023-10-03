namespace libflow.stmts
{
    public interface IArithmetic : IAstNode
    {
        IAstNode Left { get; }

        IAstNode Right { get; }
    }
}
