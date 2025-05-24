using System;
using Unity.Collections;

namespace Chess
{
    public struct Bitboard : IEquatable<Bitboard>
    {
        public static Bitboard All => new(ulong.MaxValue);
        public static Bitboard Empty => new(ulong.MinValue);

        public static Bitboard FileA => new(0x101010101010101);
        public static Bitboard FileH => FileA << 7;

        public static Bitboard Rank1 => new((ulong)0b11111111);
        public static Bitboard Rank2 => new(Rank1 << 8);
        public static Bitboard Rank3 => new(Rank2 << 8);
        public static Bitboard Rank4 => new(Rank3 << 8);
        public static Bitboard Rank5 => new(Rank4 << 8);
        public static Bitboard Rank6 => new(Rank5 << 8);
        public static Bitboard Rank7 => new(Rank6 << 8);
        public static Bitboard Rank8 => new(Rank7 << 8);

        public static Bitboard WhiteKingsideMask => 1ul << Square.F1 | 1ul << Square.G1;
        public static Bitboard BlackKingsideMask => 1ul << Square.F8 | 1ul << Square.G8;

        public static Bitboard WhiteQueensideMask2 => 1ul << Square.D1 | 1ul << Square.C1;
        public static Bitboard BlackQueensideMask2 => 1ul << Square.D8 | 1ul << Square.C8;

        public static Bitboard WhiteQueensideMask => WhiteQueensideMask2 | 1ul << Square.B1;
        public static Bitboard BlackQueensideMask => BlackQueensideMask2 | 1ul << Square.B8;

        public readonly bool IsEmpty => value.Value == 0;

        private BitField64 value;

        public Bitboard(ulong value)
        {
            this.value = new(value);
        }

        public Bitboard(Square square)
        {
            value = new(0);
            Include(square);
        }

        public Square Pop()
        {
            var i = value.CountTrailingZeros();
            value = new(value.Value & (value.Value - 1));
            return i;
        }

        public readonly Bitboard Shifted(int shift) => shift > 0 ? new Bitboard(value.Value << shift) : new Bitboard(value.Value >> -shift);

        public readonly bool Contains(Square square) => value.IsSet(square);

        public readonly bool Get(int file, int rank) => Contains(new Square(file, rank));

        public void Include(Square square) => value.SetBits(square, true);

        public void Include(int file, int rank) => Include(new Square(file, rank));

        public void Exclude(Square square) => value.SetBits(square, false);

        public void Exclude(int file, int rank) => Exclude(new Square(file, rank));

        public void Toggle(Square square) => value.SetBits(square, !Contains(square));

        public void Toggle(int file, int rank) => Toggle(new Square(file, rank));

        public void Union(Bitboard other) => this |= other;

        public readonly bool Equals(Bitboard other)
        {
            return value.Value == other.value.Value;
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
            return new(left.value.Value | right.value.Value);
        }

        public static Bitboard operator &(Bitboard left, Bitboard right)
        {
            return new(left.value.Value & right.value.Value);
        }

        public static Bitboard operator ^(Bitboard left, Bitboard right)
        {
            return new(left.value.Value ^ right.value.Value);
        }

        public static Bitboard operator ~(Bitboard bitboard)
        {
            return new(~bitboard.value.Value);
        }

        public static Bitboard operator >>(Bitboard bitboard, int shift)
        {
            return new(bitboard.value.Value >> shift);
        }

        public static Bitboard operator <<(Bitboard bitboard, int shift)
        {
            return new(bitboard.value.Value << shift);
        }

        public static Bitboard operator *(Bitboard left, Bitboard right)
        {
            return new(left.value.Value * right.value.Value);
        }

        public static implicit operator ulong(Bitboard bitboard)
        {
            return bitboard.value.Value;
        }

        public static implicit operator Bitboard(ulong value)
        {
            return new(value);
        }
    }
}
