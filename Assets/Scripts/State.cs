namespace Chess
{
    public struct State
    {
        public Figure CapturedFigure;
        public int EnPassantFile;
        public int CastlingRights;
        public int FiftyMoveCounter;
        public ulong ZobristKey;

        public const int ClearWhiteKingsideMask = 0b1110;
        public const int ClearWhiteQueensideMask = 0b1101;
        public const int ClearBlackKingsideMask = 0b1011;
        public const int ClearBlackQueensideMask = 0b0111;

        public readonly bool HasKingsideCastleRight(bool isWhite)
        {
            int mask = isWhite ? 1 : 4;
            return (CastlingRights & mask) != 0;
        }

        public readonly bool HasQueensideCastleRight(bool isWhite)
        {
            int mask = isWhite ? 2 : 8;
            return (CastlingRights & mask) != 0;
        }
    }
}
