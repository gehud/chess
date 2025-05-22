namespace Chess
{
    public struct State
    {
        public Color MoveColor;
        public bool WhiteCastlingKingside;
        public bool BlackCastlingKingside;
        public bool WhiteCastlingQueenside;
        public bool BlackCastlingQueenside;
        public Square DoubleMovePawnSquare;
        public int ImmutableMoveCount;
        public int NextMoveIndex;

        public Move Move;
        public Piece CapturedPiece;
        public Square AlliedKingSquare;
        public Square EnemyKingSquare;
    }
}
