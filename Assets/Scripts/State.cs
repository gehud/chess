namespace Chess
{
    public struct State
    {

        public bool WhiteCastlingKingside;
        public bool BlackCastlingKingside;
        public bool WhiteCastlingQueenside;
        public bool BlackCastlingQueenside;
        public Square DoubleMovePawnSquare;
        public int ImmutableMoveCount;
        public int NextMoveIndex;

        public Color AlliedColor;
        public Color EnemyColor;
        
        public Move Move;

        public Piece CapturedPiece;
        public Square AlliedKingSquare;
        public Square EnemyKingSquare;
    }
}
