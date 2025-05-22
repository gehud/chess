namespace Chess
{
    public struct Bitboard
    {
        public readonly bool IsEmpty => value == 0;

        private ulong value;

        public readonly bool Contains(Square square) => ((value >> square) & 1) != 0;

        public readonly bool Get(int file, int rank) => Contains(new Square(file, rank));

        public void Include(Square square) => value |= 1ul << square;

        public void Include(int file, int rank) => Include(new Square(file, rank));

        public void Exclude(Square square) => value ^= 1ul << square;

        public void Exclude(int file, int rank) => Exclude(new Square(file, rank));

        public void Union(Bitboard other) => value |= other.value;
    }
}
