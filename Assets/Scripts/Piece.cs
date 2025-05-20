namespace Chess
{
    public readonly struct Piece
    {
        public readonly bool IsEmpty => Figure == Figure.None;

        public static Piece Empty => new(0);

        public readonly Figure Figure => (Figure)(value & 0b111);

        public readonly Color Color => (Color)((value & 0b1000) >> 3);

        private readonly int value;

        private Piece(int value)
        {
            this.value = value;
        }

        public Piece(Figure figure, Color color)
        {
            value = (int)figure | ((int)color << 3);
        }

        public static bool operator ==(Piece left, Piece right)
        {
            return left.value == right.value;
        }

        public static bool operator !=(Piece left, Piece right)
        {
            return left.value != right.value;
        }

        public override readonly bool Equals(object @object)
        {
            if (@object is not Piece piece)
            {
                return false;
            }

            return value == piece.value;
        }

        public override readonly int GetHashCode()
        {
            return value;
        }

        public override readonly string ToString()
        {
            return IsEmpty ? "Empty" : $"Figure: {Figure}, Color: {Color}";
        }
    }
}
