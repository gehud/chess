using System;

namespace Chess
{
    public readonly struct Piece : IEquatable<Piece>
    {
        public readonly bool IsEmpty => Figure == Figure.None;

        public readonly int Index => (int)Color * 6 + (int)Figure;

        public const int MinIndex = 0;
        public const int MaxIndex = 12;

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
            return left.Equals(right);
        }

        public static bool operator !=(Piece left, Piece right)
        {
            return !left.Equals(right);
        }

        public bool Equals(Piece other)
        {
            return value == other.value;
        }

        public override readonly bool Equals(object other)
        {
            if (other is not Piece piece)
            {
                return false;
            }

            return Equals(piece);
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
