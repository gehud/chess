using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;

namespace Chess {
    public class Board : IEnumerable<Piece>, IDisposable {
        public const int Size = 8;
        public const int Area = Size * Size;

        public Piece this[int index] {
            get => squares[index];
            set => squares[index] = value;
        }

        private NativeArray<Piece> squares;

        private NativeArray<int> moveLimits;

        private NativeArray<int> squareOffsets;

        public int GetMoveLimit(int squareIndex, int directionIndex) {
            return moveLimits[squareIndex + directionIndex * Area];
        }

        public int GetSquareOffset(int directionIndex) {
            return squareOffsets[directionIndex];
        }

        public int GetSquareOffset(Direction direction) {
            return GetSquareOffset((int)direction);
        }

        public Board(Allocator allocator) {
            squares = new NativeArray<Piece>(Area, allocator);

            moveLimits = new NativeArray<int>(Area * 8, allocator, NativeArrayOptions.UninitializedMemory);
            var job = MoveLimitsComputationJob.Create(moveLimits);
            job.Schedule().Complete();

            squareOffsets = new NativeArray<int>(8, allocator, NativeArrayOptions.UninitializedMemory);
            squareOffsets[0] = 8;
            squareOffsets[1] = -8;
            squareOffsets[2] = 1;
            squareOffsets[3] = -1;
            squareOffsets[4] = 7;
            squareOffsets[5] = -7;
            squareOffsets[6] = 9;
            squareOffsets[7] = -9;
        }

        public static int GetFile(int squareIndex) {
            return squareIndex % Size;
        }

        public static int GetRank(int squareIndex) {
            return squareIndex / Size;
        }

        public IEnumerator<Piece> GetEnumerator() {
            return squares.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return squares.GetEnumerator();
        }

        public void Dispose() {
            squares.Dispose();
            squareOffsets.Dispose();
            moveLimits.Dispose();
        }
    }
}
