using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Chess
{
    public struct Board : IDisposable
    {
        public const int Size = 8;
        public const int Area = Size * Size;

        public Piece this[Square coordinate]
        {
            get => pieces[coordinate];
            set => pieces[coordinate] = value;
        }

        public Piece this[int file, int rank]
        {
            get => this[new Square(file, rank)];
            set => this[new Square(file, rank)] = value;
        }

        private NativeArray<Piece> pieces;
        private NativeArray<int> moveLimits;
        private NativeArray<int> squareOffsets;

        public Board(Allocator allocator)
        {
            pieces = new NativeArray<Piece>(Area, allocator, NativeArrayOptions.UninitializedMemory);

            moveLimits = new NativeArray<int>
            (
                Area * 8,
                allocator,
                NativeArrayOptions.UninitializedMemory
            );

            for (var square = Square.Zero; square < Area; square++)
            {
                int file = square.File;
                int rank = square.Rank;

                int northSquareCount = Size - 1 - rank;
                int southSquareCount = rank;
                int eastSquareCount = Size - 1 - file;
                int westSquareCount = file;

                moveLimits[square + Area * 0] = northSquareCount;
                moveLimits[square + Area * 1] = southSquareCount;
                moveLimits[square + Area * 2] = westSquareCount;
                moveLimits[square + Area * 3] = eastSquareCount;
                moveLimits[square + Area * 4] = math.min(northSquareCount, westSquareCount);
                moveLimits[square + Area * 5] = math.min(southSquareCount, eastSquareCount);
                moveLimits[square + Area * 6] = math.min(northSquareCount, eastSquareCount);
                moveLimits[square + Area * 7] = math.min(southSquareCount, westSquareCount);
            }

            squareOffsets = new NativeArray<int>(8, allocator, NativeArrayOptions.UninitializedMemory);
            squareOffsets[0] = 8;
            squareOffsets[1] = -8;
            squareOffsets[2] = -1;
            squareOffsets[3] = 1;
            squareOffsets[4] = 7;
            squareOffsets[5] = -7;
            squareOffsets[6] = 9;
            squareOffsets[7] = -9;
        }

        public int GetBorderDistance(Square square, Direction direction)
        {
            return moveLimits[(int)square + Area * (int)direction];
        }

        public Square GetTranslatedSquare(Square square, Direction direction, int distance = 1)
        {
            return (Square)(square + squareOffsets[(int)direction] * distance);
        }

        public void Dispose()
        {
            pieces.Dispose();
            moveLimits.Dispose();
            squareOffsets.Dispose();
        }
    }
}
