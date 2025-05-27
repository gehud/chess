using System;
using System.Text;
using Unity.Mathematics;

namespace Chess
{
    public struct Bitboard : IEquatable<Bitboard>
    {
        public static Bitboard All => new(ulong.MaxValue);
        public static Bitboard Empty => new(ulong.MinValue);

        public static Bitboard FileA => new(0x101010101010101);
        public static Bitboard FileH => FileA << 7;

        public static Bitboard Rank1 => new(0b11111111);
        public static Bitboard Rank2 => Rank1 << 8;
        public static Bitboard Rank3 => Rank2 << 8;
        public static Bitboard Rank4 => Rank3 << 8;
        public static Bitboard Rank5 => Rank4 << 8;
        public static Bitboard Rank6 => Rank5 << 8;
        public static Bitboard Rank7 => Rank6 << 8;
        public static Bitboard Rank8 => Rank7 << 8;

        public static Bitboard WhiteKingsideMask => Empty.With(Square.F1).With(Square.G1);
        public static Bitboard BlackKingsideMask => Empty.With(Square.F8).With(Square.G8);

        public static Bitboard WhiteQueensideMask2 => Empty.With(Square.D1).With(Square.C1);
        public static Bitboard BlackQueensideMask2 => Empty.With(Square.D8).With(Square.C8);

        public static Bitboard WhiteQueensideMask => WhiteQueensideMask2 | Empty.With(Square.B1);
        public static Bitboard BlackQueensideMask => BlackQueensideMask2 | Empty.With(Square.B8);

        public readonly bool IsEmpty => value == 0ul;

        private ulong value;

        public Bitboard(ulong value)
        {
            this.value = value;
        }

        public Square Pop()
        {
            var i = math.tzcnt(value);
            value &= value - 1ul;
            return new Square(i);
        }

        public readonly Bitboard Shifted(int shift) => shift > 0 ? new Bitboard(value << shift) : new Bitboard(value >> -shift);

        public readonly bool Contains(Square square) => (value & (1ul << square.Index)) != 0;

        public readonly bool Contains(int file, int rank) => Contains(new Square(file, rank));

        public readonly bool Contains(SquareName name) => Contains(new Square(name));

        public void Include(Square square) => value |= 1ul << square.Index;

        public void Include(int file, int rank) => Include(new Square(file, rank));

        public readonly Bitboard With(Square square)
        {
            var with = this;
            with.Include(square);
            return with;
        }

        public readonly Bitboard With(SquareName name) => With(new Square(name));

        public void Exclude(Square square) => value &= ~(1ul << square.Index);

        public void Exclude(int file, int rank) => Exclude(new Square(file, rank));

        public readonly Bitboard Without(Square square)
        {
            var without = this;
            without.Exclude(square);
            return without;
        }

        public void Toggle(Square square) => value ^= 1ul << square.Index;

        public void Toggle(int file, int rank) => Toggle(new Square(file, rank));

        public readonly Bitboard Toggled(Square square)
        {
            var toggled = this;
            toggled.Toggle(square);
            return toggled;
        }

        public void Union(Bitboard other) => this |= other;

        public readonly bool Equals(Bitboard other)
        {
            return value == other.value;
        }

        public override readonly bool Equals(object other)
        {
            if (other is not Bitboard bitboard)
            {
                return false;
            }

            return Equals(bitboard);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(value);
        }

        public static bool operator ==(Bitboard left, Bitboard right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Bitboard left, Bitboard right)
        {
            return !left.Equals(right);
        }

        public static Bitboard operator |(Bitboard left, Bitboard right)
        {
            return new(left.value | right.value);
        }

        public static Bitboard operator |(Bitboard left, Square right)
        {
            return left.With(right);
        }

        public static Bitboard operator &(Bitboard left, Bitboard right)
        {
            return new(left.value & right.value);
        }

        public static Bitboard operator ^(Bitboard left, Bitboard right)
        {
            return new(left.value ^ right.value);
        }

        public static Bitboard operator ^(Bitboard left, Square right)
        {
            return left.Toggled(right);
        }

        public static Bitboard operator ~(Bitboard bitboard)
        {
            return new(~bitboard.value);
        }

        public static Bitboard operator >>(Bitboard bitboard, int shift)
        {
            return new(bitboard.value >> shift);
        }

        public static Bitboard operator <<(Bitboard bitboard, int shift)
        {
            return new(bitboard.value << shift);
        }

        public static Bitboard operator *(Bitboard left, Bitboard right)
        {
            return new(left.value * right.value);
        }

        public static Bitboard operator -(Bitboard left, Bitboard right)
        {
            return new(left.value - right.value);
        }

        public static explicit operator ulong(Bitboard bitboard)
        {
            return bitboard.value;
        }

        public static explicit operator Bitboard(ulong value)
        {
            return new(value);
        }

#if DEBUG
        public override readonly string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("  +---------------+");
            
            for (var rank = Square.MaxComponent; rank >= Square.MinComponent; rank--)
            {
                builder.Append((rank + 1) + " | ");

                for (var file = Square.MinComponent; file <= Square.MaxComponent; file++)
                {
                    builder.Append(Contains(file, rank) ? "1  " : ".  ");
                }

                builder.AppendLine("|");
            }

            builder.AppendLine("  +---------------+");
            builder.AppendLine("    a b c d e f g h");

            return builder.ToString();
        }
#endif
    }
}
