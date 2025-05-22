namespace Chess
{
    public readonly struct Square
    {
        public static Square Zero => new(0);

        public readonly bool IsValid => index >= 0 && index < Board.Area;

        public readonly int File => index % Board.Size;

        public readonly int Rank => index / Board.Size;

        private readonly int index;

        private static readonly char[] fileNotations =
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'
        };

        public Square(int index)
        {
            this.index = index;
        }

        public Square(int file, int rank)
        {
            index = rank * Board.Size + file;
        }

        public Square Translated(in Board board, in Direction direction, int distance = 1)
        {
            return board.GetTranslatedSquare(this, direction, distance);
        }

        public int GetBorderDistance(in Board board, in Direction direction)
        {
            return board.GetBorderDistance(this, direction);
        }

        public static implicit operator int(Square square)
        {
            return square.index;
        }

        public static implicit operator Square(int index)
        {
            return new Square(index);
        }

        public override readonly string ToString()
        {
            return $"{fileNotations[File]}{Rank + 1}";
        }
    }
}
