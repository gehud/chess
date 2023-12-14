using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Chess {
    [BurstCompile]
    public struct MoveLimitsComputationJob : IJob {
        [WriteOnly]
        private NativeArray<int> moveLimits;

        public static MoveLimitsComputationJob Create(NativeArray<int> moveLimits) {
            return new MoveLimitsComputationJob {
                moveLimits = moveLimits
            };
        }

        public void Execute() {
            for (int squareIndex = 0; squareIndex < Board.Area; squareIndex++) {
                int file = Board.GetFile(squareIndex);
                int rank = Board.GetRank(squareIndex);

                int northSquareCount = Board.Size - 1 - rank;
                int southSquareCount = rank;
                int eastSquareCount = Board.Size - 1 - file;
                int westSquareCount = file;

                moveLimits[squareIndex + Board.Area * 0] = northSquareCount;
                moveLimits[squareIndex + Board.Area * 1] = southSquareCount;
                moveLimits[squareIndex + Board.Area * 2] = eastSquareCount;
                moveLimits[squareIndex + Board.Area * 3] = westSquareCount;
                moveLimits[squareIndex + Board.Area * 4] = math.min(northSquareCount, westSquareCount);
                moveLimits[squareIndex + Board.Area * 5] = math.min(southSquareCount, eastSquareCount);
                moveLimits[squareIndex + Board.Area * 6] = math.min(northSquareCount, eastSquareCount);
                moveLimits[squareIndex + Board.Area * 7] = math.min(southSquareCount, westSquareCount);
            }
        }
    }
}