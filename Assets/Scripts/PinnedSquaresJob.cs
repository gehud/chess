using Unity.Collections;
using Unity.Jobs;

namespace Chess
{
    public struct PinnedSquaresJob : IJob
    {
        [ReadOnly]
        public Board Board;
        [ReadOnly]
        public State State;
        [WriteOnly]
        public NativeArray<Bitboard> Pinned;

        public void Execute()
        {
            for (var direction = Direction.North; direction <= Direction.SouthWest; direction++)
            {
                Bitboard pinned = default;

                var borderDistance = Board.GetBorderDistance(State.KingSquare, direction);

                for (var i = 1; i <= borderDistance; i++)
                {
                    var targetSquare = Board.GetTranslatedSquare(State.KingSquare, direction, i);

                    if (IsPinning(targetSquare, direction))
                    {
                        for (var j = 1; j <= i; j++)
                        {
                            var pinnedSquare = Board.GetTranslatedSquare(State.KingSquare, direction, j);
                            pinned[pinnedSquare] = true;
                        }
                    }
                }

                Pinned[(int)(direction - 1)] = pinned;
            }
        }

        private readonly bool IsPinning(Square square, Direction direction)
        {
            return direction >= Direction.North 
                && direction <= Direction.West 
                && State.StraightSlidingPinning[square] 
                || direction >= Direction.NorthWest 
                && direction <= Direction.SouthWest 
                && State.DiagonalSlidingPinning[square];
        }
    }
}
