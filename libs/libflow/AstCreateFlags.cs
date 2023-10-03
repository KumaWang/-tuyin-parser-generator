using System;

namespace libflow
{
    [Flags]
    public enum AstCreateFlags
    {
        Default = 0,
        LimitBreak = 1,
        LimitContinue = 2,
        LimitSwitch = 4,
        LimitDoWhile = 8,
        LimitGotoFront = 16,
        LimitGotoBack = 32,
        LimitIf = 128,
        InsertState = 256,
        LimitGoto = LimitGotoFront | LimitGotoBack,
    }
}
