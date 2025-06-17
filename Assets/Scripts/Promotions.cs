using System;

namespace Chess
{
    [Flags]
    public enum Promotions
    {
        Queen = 1 << 0,
        Rook = 1 << 1,
        Bishop = 1 << 2,
        Knight = 1 << 3,
    }
}
