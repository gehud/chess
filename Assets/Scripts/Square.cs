namespace Chess
{
    public readonly struct Square
    {
        public readonly bool IsEmpty => Piece == Piece.None;

        public static Square Empty => new(0);

        public readonly Piece Piece => (Piece)(value & 0b111);

        public readonly Color Color => (Color)((value & 0b1000) >> 3);

        private readonly int value;

        private Square(int value)
        {
            this.value = value;
        }

        public Square(Piece piece, Color color)
        {
            value = (int)piece | ((int)color << 3);
        }

        public static bool operator ==(Square left, Square right)
        {
            return left.value == right.value;
        }

        public static bool operator !=(Square left, Square right)
        {
            return left.value != right.value;
        }

        public override readonly bool Equals(object @object)
        {
            if (@object is not Square square)
            {
                return false;
            }

            return value == square.value;
        }

        public override readonly int GetHashCode()
        {
            return value;
        }

        public override readonly string ToString()
        {
            return IsEmpty ? "Empty" : $"Piece: {Piece}, Color: {Color}";
        }
    }
}
