namespace Chess
{
    public readonly struct Coordinate
    {
        public static Coordinate Zero => new(0);

        public readonly bool IsValid => index >= 0 && index < Board.Area;

        public readonly int File => index % Board.Size;

        public readonly int Rank => index / Board.Size;

        private readonly int index;

        private static readonly char[] fileNotations =
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'
        };

        public Coordinate(int index)
        {
            this.index = index;
        }

        public Coordinate(int file, int rank)
        {
            index = rank * Board.Size + file;
        }

        public readonly bool Equals(Coordinate other)
        {
            return index == other.index;
        }

        public static implicit operator int(Coordinate coordinate)
        {
            return coordinate.index;
        }

        public static implicit operator Coordinate(int index)
        {
            return new Coordinate(index);
        }

        public override readonly string ToString()
        {
            return $"{fileNotations[File]}{Rank + 1}";
        }
    }
}
