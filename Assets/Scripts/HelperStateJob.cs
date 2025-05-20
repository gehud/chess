using Unity.Collections;
using Unity.Jobs;

namespace Chess
{
    public struct HelperStateJob : IJobFor
    {
        [ReadOnly]
        public Board Board;
        [ReadOnly]
        public Color MoveColor;
        [WriteOnly]
        public Square KingSquare;
        [WriteOnly]
        public Bitboard StraightSlidingPinning;
        [WriteOnly]
        public Bitboard DiagonalSlidingPinning;

        public void Execute(int index)
        {
            var square = (Square)index;

            var piece = Board[square];

            if (piece.IsEmpty)
            {
                return;
            }

            if (piece.Color == MoveColor)
            {
                if (piece.Figure == Figure.King)
                {
                    KingSquare = square;
                }
            }
            else
            {
                switch (piece.Figure)
                {
                    case Figure.Bishop | Figure.Queen:
                        DiagonalSlidingPinning[square] = true;
                        break;
                    case Figure.Rook | Figure.Queen:
                        StraightSlidingPinning[square] = true;
                        break;
                }
            }
        }
    }
}
