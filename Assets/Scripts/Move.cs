namespace Chess
{
    public readonly struct Move
    {
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
    }
}
