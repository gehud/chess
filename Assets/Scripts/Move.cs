using System;

namespace Chess
{
    public readonly struct Move : IEquatable<Move>
    {
        public static Move Null => new(0);

        public readonly Square From => (Square)(data & 0b0000000000111111);
        public readonly Square To => (Square)((data & 0b0000111111000000) >> 6);
        public readonly MoveFlag Flag => (MoveFlag)(data >> 12);
        public readonly bool IsNull => this == Null;
        public readonly bool IsPromotion => Flag >= MoveFlag.KnightPromotion;

        private readonly ushort data;

        private Move(ushort data)
        {
            this.data = data;
        }

        public Move(Square from, Square to)
        {
            data = (ushort)((int)from | (int)to << 6);
        }

        public Move(Square from, Square to, MoveFlag flag)
        {
            data = (ushort)((int)from | (int)to << 6 | (int)flag << 12);
        }

        private string FormatFlags()
        {
            return Flag switch
            {
                MoveFlag.KnightPromotion => "n",
                MoveFlag.BishopPromotion => "b",
                MoveFlag.RookPromotion => "r",
                MoveFlag.QueenPromotion => "q",
                _ => string.Empty
            };
        }

        public override string ToString()
        {
            return $"{From}{To}{FormatFlags()}";
        }

        public bool Equals(Move other)
        {
            return data == other.data;
        }

        public override bool Equals(object other)
        {
            if (other is not Move move)
            {
                return false;
            }

            return Equals(move);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(data);
        }

        public static bool operator ==(Move left, Move right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Move left, Move right)
        {
            return !left.Equals(right);
        }
    }
}
