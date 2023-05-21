namespace Chess {
	public readonly struct Piece {
		public enum Types {
			None = 0,
			Pawn = 1,
			Knight = 2,
			Bishop = 3,
			Rook = 4,
			Queen = 5,
			King = 6,
		}

		public enum Colors {
			None = 0,
			White = 8,
			Black = 16,
		}

		public static Piece Empty => new(Types.None, Colors.None);

		public Types Type {
			get => (Types)(representation & 0b111);
		}

		public Colors Color {
			get => (Colors)(representation & 0b11000);
		}

		public bool IsSliding => Type switch {
			Types.Bishop => true,
			Types.Rook => true,
			Types.Queen => true,
			_ => false
		};

		private readonly int representation;

		public Piece(Types type, Colors color) {
			representation = (int)type | (int)color;
		}

		public static bool operator==(Piece left, Piece right) {
			return left.representation == right.representation;	
		}

		public static bool operator!=(Piece left, Piece right) {
			return left.representation != right.representation;
		}

		public override bool Equals(object obj) {
			if (obj is not Piece piece)
				return false;

			return this.representation == piece.representation;
		}

		public override int GetHashCode() {
			return representation;
		}

		public override string ToString() {
			return $"Type: {Type}, Color: {Color}";
		}
	}
}