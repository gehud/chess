using System;

namespace Chess
{
    public readonly struct Move : IEquatable<Move>
    {
        public static Move Invalid => new(Square.A1, Square.A1);

        public readonly bool IsValid => From != To;

        public readonly Square From;
        public readonly Square To;
        public readonly MoveFlags Flags;

        public Move(Square from, Square to, MoveFlags flags = MoveFlags.None)
        {
            From = from;
            To = to;
            Flags = flags;
        }

        private string FormatFlags()
        {
            if ((Flags & MoveFlags.QueenPromotion) != MoveFlags.None)
            {
                return "q";
            }
            else if ((Flags & MoveFlags.RookPromotion) != MoveFlags.None)
            {
                return "r";
            }
            else if ((Flags & MoveFlags.BishopPromotion) != MoveFlags.None)
            {
                return "b";
            }
            else if ((Flags & MoveFlags.KnightPromotion) != MoveFlags.None)
            {
                return "n";
            }

            return string.Empty;
        }

        public override string ToString()
        {
            return $"{From}{To}{FormatFlags()}";
        }

        public bool Equals(Move other)
        {
            return From == other.From && To == other.To && Flags == other.Flags;
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
            return HashCode.Combine(From, To, Flags);
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
