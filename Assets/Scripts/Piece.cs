namespace Chess {
	public static class Piece {
		public const int NONE = 0;
		public const int PAWN = 1;
		public const int KNIGHT = 2;
		public const int BISHOP = 3;
		public const int ROOK = 4;
		public const int QUEEN = 5;
		public const int KING = 6;
		public const int WHITE = 8;
		public const int BLACK = 16;

		public static bool IsWhite(int piece) {
			return (piece & WHITE) != 0;
		}
	}
}