namespace Chess
{
    public struct Bitboard
    {
        public bool this[Square square]
        {
            readonly get => ((value >> square) & 1) != 0;
            set
            {
                if (value)
                {
                    this.value |= 1ul << square;
                }
                else
                {
                    this.value ^= 1ul << square;
                }
            }
        }

        public bool this[int file, int rank]
        {
            readonly get => this[new Square(file, rank)];
            set => this[new Square(file, rank)] = value;
        }

        private ulong value;
    }
}
