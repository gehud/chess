namespace Chess {
	public struct GameState {
		public Move Move;
		public Piece Captured;
		public Move CastlingRook;
		public int TwoSquarePawn;
		public int EnPassantCaptured;
		public int PromotedPawn;
		public bool IsWhiteKingsideCastlingAvaible;
		public bool IsWhiteQueensideCastlingAvaible;
		public bool IsBlackKingsideCastlingAvaible;
		public bool IsBlackQueensideCastlingAvaible;
	}
}
