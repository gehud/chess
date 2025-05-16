using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Chess
{
    public struct Board : IDisposable
    {
        public const int Size = 8;
        public const int Area = Size * Size;

        public Square this[Coordinate coordinate]
        {
            get => squares[coordinate];
            set => squares[coordinate] = value;
        }

        public Square this[int file, int rank]
        {
            get => this[new Coordinate(file, rank)];
            set => this[new Coordinate(file, rank)] = value;
        }

        private NativeArray<Square> squares;
        private NativeArray<int> moveLimits;

        public Board(Allocator allocator)
        {
            squares = new NativeArray<Square>(Area, allocator, NativeArrayOptions.UninitializedMemory);

            moveLimits = new NativeArray<int>
            (
                Area * (int)Direction.SouthWest,
                allocator,
                NativeArrayOptions.UninitializedMemory
            );

            for (var coordinate = Coordinate.Zero; coordinate < Area; coordinate++)
            {
                int file = coordinate.File;
                int rank = coordinate.Rank;

                int northSquareCount = Size - 1 - rank;
                int southSquareCount = rank;
                int eastSquareCount = Size - 1 - file;
                int westSquareCount = file;

                moveLimits[coordinate + Area * 0] = northSquareCount;
                moveLimits[coordinate + Area * 1] = southSquareCount;
                moveLimits[coordinate + Area * 2] = eastSquareCount;
                moveLimits[coordinate + Area * 3] = westSquareCount;
                moveLimits[coordinate + Area * 4] = math.min(northSquareCount, westSquareCount);
                moveLimits[coordinate + Area * 5] = math.min(southSquareCount, eastSquareCount);
                moveLimits[coordinate + Area * 6] = math.min(northSquareCount, eastSquareCount);
                moveLimits[coordinate + Area * 7] = math.min(southSquareCount, westSquareCount);
            }
        }

        public void Dispose()
        {
            squares.Dispose();
            moveLimits.Dispose();
        }
    }
}
