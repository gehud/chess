using System;

namespace Chess
{
    [Flags]
    public enum MoveFlags
    {
        None = 0,
        QueenPromotion = 1 << 0,
        RookPromotion = 1 << 1,
        BishopPromotion = 1 << 2,
        KnightPromotion = 1 << 3,
        Promotion = QueenPromotion | RookPromotion | BishopPromotion | KnightPromotion,
        WhiteEnPassant = 1 << 4,
        BlackEnPassant = 1 << 5,
        EnPassant = WhiteEnPassant | BlackEnPassant,
    }
}
