namespace Chess
{
    public struct State
    {
        public Color MoveColor;
        public bool WhiteCastlingKingside;
        public bool BlackCastlingKingside;
        public bool WhiteCastlingQueenside;
        public bool BlackCastlingQueenside;
        public Coordinate EnPassantTargetCoordinate;
        public int ImmutableMoveCount;
        public int NextMoveIndex;
    }
}
