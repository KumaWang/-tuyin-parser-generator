using System;

namespace libgraph
{
    [Flags]
    public enum EdgeFlags : ushort
    {
        None            = 0,
        Optional        = 1,
        SpecialPoint    = 2,
        AnyPoint        = 3,
        Close            = 4
    }
}
