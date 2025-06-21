using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Chess
{
    public struct PieceSquareTables : IDisposable
    {
        public NativeArray<int>.ReadOnly Pawns => pawns.AsReadOnly();
        public NativeArray<int>.ReadOnly PawnsEnd => pawnsEnd.AsReadOnly();
        public NativeArray<int>.ReadOnly Knights => knights.AsReadOnly();
        public NativeArray<int>.ReadOnly Bishops => bishops.AsReadOnly();
        public NativeArray<int>.ReadOnly Rooks => rooks.AsReadOnly();
        public NativeArray<int>.ReadOnly Queens => queens.AsReadOnly();
        public NativeArray<int>.ReadOnly KingStart => kingStart.AsReadOnly();
        public NativeArray<int>.ReadOnly KingEnd => kingEnd.AsReadOnly();

        private NativeArray<int> pawns;
        private NativeArray<int> pawnsEnd;
        private NativeArray<int> knights;
        private NativeArray<int> bishops;
        private NativeArray<int> rooks;
        private NativeArray<int> queens;
        private NativeArray<int> kingStart;
        private NativeArray<int> kingEnd;

        private NativeArray<NativeArray<int>> tables;

        public static int Read(NativeArray<int>.ReadOnly table, Square square, Color color)
        {
            if (color == Color.White)
            {
                var file = square.File;
                var rank = square.Rank;
                rank = Board.Size - rank - 1;
                square = new(file, rank);
            }

            return table[square.Index];
        }

        public int Read(Piece piece, Square square)
        {
            return tables[piece.Index][square.Index];
        }

        public PieceSquareTables(Allocator allocator)
        {
            pawns = new
            (
                new int[Board.Area]
                {
                    0,   0,   0,   0,   0,   0,   0,   0,
                    50,  50,  50,  50,  50,  50,  50,  50,
                    10,  10,  20,  30,  30,  20,  10,  10,
                    5,   5,  10,  25,  25,  10,   5,   5,
                    0,   0,   0,  20,  20,   0,   0,   0,
                    5,  -5, -10,   0,   0, -10,  -5,   5,
                    5,  10,  10, -20, -20,  10,  10,   5,
                    0,   0,   0,   0,   0,   0,   0,   0
                },
                allocator
            );

            pawnsEnd = new
            (
                new int[Board.Area]
                {
                    0,   0,   0,   0,   0,   0,   0,   0,
                    80,  80,  80,  80,  80,  80,  80,  80,
                    50,  50,  50,  50,  50,  50,  50,  50,
                    30,  30,  30,  30,  30,  30,  30,  30,
                    20,  20,  20,  20,  20,  20,  20,  20,
                    10,  10,  10,  10,  10,  10,  10,  10,
                    10,  10,  10,  10,  10,  10,  10,  10,
                    0,   0,   0,   0,   0,   0,   0,   0
                },
                allocator
            );

            knights = new
            (
                new int[Board.Area]
                {
                    -50,-40,-30,-30,-30,-30,-40,-50,
                    -40,-20,  0,  0,  0,  0,-20,-40,
                    -30,  0, 10, 15, 15, 10,  0,-30,
                    -30,  5, 15, 20, 20, 15,  5,-30,
                    -30,  0, 15, 20, 20, 15,  0,-30,
                    -30,  5, 10, 15, 15, 10,  5,-30,
                    -40,-20,  0,  5,  5,  0,-20,-40,
                    -50,-40,-30,-30,-30,-30,-40,-50,
                },
                allocator
            );

            bishops = new
            (
                new int[Board.Area]
                {
                    -20,-10,-10,-10,-10,-10,-10,-20,
                    -10,  0,  0,  0,  0,  0,  0,-10,
                    -10,  0,  5, 10, 10,  5,  0,-10,
                    -10,  5,  5, 10, 10,  5,  5,-10,
                    -10,  0, 10, 10, 10, 10,  0,-10,
                    -10, 10, 10, 10, 10, 10, 10,-10,
                    -10,  5,  0,  0,  0,  0,  5,-10,
                    -20,-10,-10,-10,-10,-10,-10,-20,
                },
                allocator
            );

            rooks = new
            (
                new int[Board.Area]
                {
                    0,  0,  0,  0,  0,  0,  0,  0,
                    5, 10, 10, 10, 10, 10, 10,  5,
                    -5,  0,  0,  0,  0,  0,  0, -5,
                    -5,  0,  0,  0,  0,  0,  0, -5,
                    -5,  0,  0,  0,  0,  0,  0, -5,
                    -5,  0,  0,  0,  0,  0,  0, -5,
                    -5,  0,  0,  0,  0,  0,  0, -5,
                    0,  0,  0,  5,  5,  0,  0,  0
                },
                allocator
            );

            queens = new
            (
                new int[Board.Area]
                {
                    -20,-10,-10, -5, -5,-10,-10,-20,
                    -10,  0,  0,  0,  0,  0,  0,-10,
                    -10,  0,  5,  5,  5,  5,  0,-10,
                    -5,  0,  5,  5,  5,  5,  0, -5,
                    0,  0,  5,  5,  5,  5,  0, -5,
                    -10,  5,  5,  5,  5,  5,  0,-10,
                    -10,  0,  5,  0,  0,  0,  0,-10,
                    -20,-10,-10, -5, -5,-10,-10,-20
                },
                allocator
            );

            kingStart = new
            (
                new int[Board.Area]
                {
                    -80, -70, -70, -70, -70, -70, -70, -80,
                    -60, -60, -60, -60, -60, -60, -60, -60,
                    -40, -50, -50, -60, -60, -50, -50, -40,
                    -30, -40, -40, -50, -50, -40, -40, -30,
                    -20, -30, -30, -40, -40, -30, -30, -20,
                    -10, -20, -20, -20, -20, -20, -20, -10,
                    20, 20, -5, -5, -5, -5, 20, 20,
                    20, 30, 10, 0, 0, 10, 30, 20
                },
                allocator
            );

            kingEnd = new
            (
                new int[Board.Area]
                {
                    -20, -10, -10, -10, -10, -10, -10, -20,
                    -5, 0, 5, 5, 5, 5, 0, -5,
                    -10, -5, 20, 30, 30, 20, -5, -10,
                    -15, -10, 35, 45, 45, 35, -10, -15,
                    -20, -15, 30, 40, 40, 30, -15, -20,
                    -25, -20, 20, 25, 25, 20, -20, -25,
                    -30, -25, 0, 0, 0, 0, -25, -30,
                    -50, -30, -30, -30, -30, -30, -30, -50
                },
                allocator
            );

            tables = new(Piece.MaxIndex + 1, allocator);

            tables[new Piece(Figure.Pawn, Color.White).Index] = pawns;
            tables[new Piece(Figure.Knight, Color.White).Index] = knights;
            tables[new Piece(Figure.Bishop, Color.White).Index] = bishops;
            tables[new Piece(Figure.Rook, Color.White).Index] = rooks;
            tables[new Piece(Figure.Queen, Color.White).Index] = queens;

            tables[new Piece(Figure.Pawn, Color.Black).Index] = CreateFlippedTable(pawns, allocator);
            tables[new Piece(Figure.Knight, Color.Black).Index] = CreateFlippedTable(knights, allocator);
            tables[new Piece(Figure.Bishop, Color.Black).Index] = CreateFlippedTable(bishops, allocator);
            tables[new Piece(Figure.Rook, Color.Black).Index] = CreateFlippedTable(rooks, allocator);
            tables[new Piece(Figure.Queen, Color.Black).Index] = CreateFlippedTable(queens, allocator);
        }

        private static NativeArray<int> CreateFlippedTable(NativeArray<int> table, Allocator allocator)
        {
            var flipped = new NativeArray<int>(table.Length, allocator);

            for (var i = 0; i < table.Length; i++)
            {
                var square = new Square(i);
                var coordinate = square.Coordinate;
                var flippedCoordinate = new int2(coordinate.x, Board.Size - coordinate.y - 1);
                flipped[new Square(flippedCoordinate.x, flippedCoordinate.y).Index] = table[i];
            }

            return flipped;
        }

        public void Dispose()
        {
            pawnsEnd.Dispose();

            kingStart.Dispose();
            kingEnd.Dispose();

            foreach (var table in tables)
            {
                if (table.IsCreated)
                {
                    table.Dispose();
                }
            }

            tables.Dispose();
        }
    }
}
