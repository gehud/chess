namespace Chess {
    public readonly struct Piece {
        public static Piece Empty => new(Figure.None, Color.None);

        public Figure Figure => (Figure)(value & 0b111);

        public Color Color => (Color)((value & 0b11000) >> 3);

        public bool IsEmpty => value == 0;

        private readonly byte value;

        public Piece(Figure figure, Color color) {
            value = (byte)((byte)figure | ((byte)color << 3));
        }

        public static bool operator ==(Piece left, Piece right) {
            return left.value == right.value;
        }

        public static bool operator !=(Piece left, Piece right) {
            return left.value != right.value;
        }

        public override bool Equals(object @object) {
            if (@object is not Piece piece) {
                return false;
            }

            return value == piece.value;
        }

        public override int GetHashCode() {
            return value;
        }

        public override string ToString() {
            return $"Figure: {Figure}, Color: {Color}";
        }
    }
}