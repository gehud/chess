namespace Chess {
	public readonly struct Piece {
		public static Piece Empty => new(PieceType.None, PieceColor.None);

		public PieceType Type => (PieceType)(representation & 0b111);

		public PieceColor Color => (PieceColor)((representation & 0b11000) >> 3);

		public bool IsEmpty => this == Empty;

		private readonly byte representation;

		public Piece(PieceType type, PieceColor color) {
			representation = (byte)((byte)type | ((byte)color << 3));
		}

		public static bool operator==(Piece left, Piece right) {
			return left.representation == right.representation;
		}

		public static bool operator!=(Piece left, Piece right) {
			return left.representation != right.representation;
		}

		public override bool Equals(object @object) {
			if (@object is not Piece piece)
				return false;

			return representation == piece.representation;
		}

		public override int GetHashCode() {
			return representation;
		}

		public override string ToString() {
			return $"Type: {Type}, Color: {Color}";
		}
	}
}