using System;

namespace libfsm
{
    [Flags]
    public enum FASymbolType
    {
        None        = 0,
        Request     = 1,
        Report      = 2,
        Close       = 4
    }
}
