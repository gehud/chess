using Unity.Collections;
using Unity.Jobs;

namespace Chess
{
    public struct HelperStateJob : IJob
    {
        [ReadOnly]
        public Board Board;
        [ReadOnly]
        public Color MoveColor;
        [WriteOnly]
        public NativeReference<Square> AlliedKingSquare;

        public void Execute()
        {
            for (var square = Square.Zero; square < Board.Area; square++)
            {
                var piece = Board[square];

                if (piece.Figure == Figure.King)
                {
                    if (piece.Color == MoveColor)
                    {
                        AlliedKingSquare.Value = square;
                        return;
                    }
                }
            }
        }
    }
}
