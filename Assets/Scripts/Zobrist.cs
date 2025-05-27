using System;
using Unity.Collections;
using Random = Unity.Mathematics.Random;

namespace Chess
{
    public readonly struct Zobrist : IDisposable
    {
        public readonly NativeArray<ulong> Pieces;
        public readonly NativeArray<ulong> CastlingRights;
        public readonly NativeArray<ulong> EnPassantFile;
        public readonly ulong SideToMove;

        public Zobrist(Allocator allocator)
        {
            Pieces = new((Piece.MaxIndex + 1) * 64, allocator);
            CastlingRights = new(16, allocator);
            EnPassantFile = new(9, allocator);
            SideToMove = default;

            var random = new Random(29426028);
            var startIndex = new Piece(Figure.Pawn, Color.White).Index;
            var endIndex = new Piece(Figure.King, Color.Black).Index;

            for (var square = Square.Min.Index; square <= Square.Max.Index; square++)
            {
                for (var piece = startIndex; piece <= endIndex; piece++)
                {
                    Pieces[piece * Board.Area + square] = random.NextULong();
                }
            }

            for (var i = 0; i < CastlingRights.Length; i++)
            {
                CastlingRights[i] = random.NextULong();
            }

            for (var i = 0; i < EnPassantFile.Length; i++)
            {
                EnPassantFile[i] = random.NextULong();
            }

            SideToMove = random.NextULong();
        }

        public ulong CalculateKey(in Board board)
        {
            var key = 0ul;

            for (var square = Square.Min.Index; square <= Square.Max.Index; square++)
            {
                var piece = board[new Square(square)];

                if (!piece.IsEmpty)
                {
                    key ^= Pieces[piece.Index * Board.Area + square];
                }

                key ^= EnPassantFile[board.State.EnPassantFile];

                if (board.AlliedColor == Color.Black)
                {
                    key ^= SideToMove;
                }

                key ^= CastlingRights[board.State.CastlingRights];
            }

            return key;
        }

        public void Dispose()
        {
            Pieces.Dispose();
            CastlingRights.Dispose();
            EnPassantFile.Dispose();
        }
    }
}
