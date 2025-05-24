namespace Chess
{
    public struct State
    {
        public Figure capturedPieceType;
        public int enPassantFile;
        public int castlingRights;
        public int fiftyMoveCounter;
        public ulong zobristKey;

        public const int ClearWhiteKingsideMask = 0b1110;
        public const int ClearWhiteQueensideMask = 0b1101;
        public const int ClearBlackKingsideMask = 0b1011;
        public const int ClearBlackQueensideMask = 0b0111;

        public bool HasKingsideCastleRight(bool isWhite)
        {
            int mask = isWhite ? 1 : 4;
            return (castlingRights & mask) != 0;
        }

        public bool HasQueensideCastleRight(bool isWhite)
        {
            int mask = isWhite ? 2 : 8;
            return (castlingRights & mask) != 0;
        }
    }
}
