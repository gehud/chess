namespace Chess
{
    public struct State
    {
        public Color MoveColor;
        public bool WhiteCastlingKingside;
        public bool BlackCastlingKingside;
        public bool WhiteCastlingQueenside;
        public bool BlackCastlingQueenside;
        public Square EnPassantTargetSquare;
        public int ImmutableMoveCount;
        public int NextMoveIndex;

        public Square KingSquare;
        public Bitboard StraightSlidingPinning;
        public Bitboard DiagonalSlidingPinning;
    }
}
