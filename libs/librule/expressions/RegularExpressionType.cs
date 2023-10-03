namespace librule.expressions
{
    enum RegularExpressionType
    {
        Symbol,
        Empty,
        CharSet,
        Or,
        Repeat,
        Concatenation,
        Except,
        Action,
        Position,
        Previous
    }
}
