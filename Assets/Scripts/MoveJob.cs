using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Chess
{
    public struct MoveJob : IJobFor
    {
        [ReadOnly]
        public State State;
        [ReadOnly]
        public Board Board;
        [ReadOnly]
        public NativeArray<Bitboard> Pinned;
        [WriteOnly]
        public NativeList<Move> Moves;

        void IJobFor.Execute(int index)
        {
            var square = (Square)index;
            var piece = Board[square];

            if (piece.IsEmpty || piece.Color != State.MoveColor)
            {
                return;
            }

            switch (piece.Figure)
            {
                case Figure.Pawn:
                    GeneratePawnMoves(square);
                    break;
                case Figure.Knight:
                    GenerateKnightMoves(square);
                    break;
                case Figure.Bishop:
                    GenerateSlidingMoves(square, Direction.NorthWest, Direction.SouthWest);
                    break;
                case Figure.Rook:
                    GenerateSlidingMoves(square, Direction.North, Direction.West);
                    break;
                case Figure.Queen:
                    GenerateSlidingMoves(square, Direction.North, Direction.SouthWest);
                    break;
                case Figure.King:
                    GenerateKingMoves(square);
                    break;
            }
        }

        private void GeneratePawnMoves(in Square square)
        {
            var doubleMoveRank = -1;
            var forwardDirection = Direction.None;

            switch (State.MoveColor)
            {
                case Color.White:
                    doubleMoveRank = 1;
                    forwardDirection = Direction.North;
                    break;
                case Color.Black:
                    doubleMoveRank = 6;
                    forwardDirection = Direction.South;
                    break;
            }

            var moveDistance = square.Rank == doubleMoveRank ? 2 : 1;

            for (var i = 1; i <= math.min(Board.GetBorderDistance(square, forwardDirection), moveDistance); i++)
            {
                var targetSquare = Board.GetTranslatedSquare(square, forwardDirection, i);
                var targetPiece = Board[targetSquare];

                if (targetPiece != Piece.Empty)
                {
                    break;
                }

                if (!TryAddPawnPromotion(square, targetSquare))
                {
                    Moves.Add(new Move(square, targetSquare));
                }
            }
        }

        private bool TryAddPawnPromotion(Square fromSquare, Square toSquare)
        {
            var rank = toSquare.Rank;

            if ((rank != 0 || State.MoveColor != Color.Black) && 
                (rank != Board.Size - 1 || State.MoveColor != Color.White))
            {
                return false;
            }

            Moves.Add(new Move(fromSquare, toSquare, MoveFlags.QueenPromotion));
            Moves.Add(new Move(fromSquare, toSquare, MoveFlags.RookPromotion));
            Moves.Add(new Move(fromSquare, toSquare, MoveFlags.KnightPromotion));
            Moves.Add(new Move(fromSquare, toSquare, MoveFlags.BishopPromotion));

            return true;
        }

        private void GenerateKnightMove(Square from, Square to)
        {
            var targetPiece = Board[to];
            if (targetPiece.IsEmpty || targetPiece.Color != State.MoveColor)
            {
                Moves.Add(new Move(from, to));
            }
        }

        private void GenerateKnightMoves(Square square)
        {
            int file = square.File;
            int rank = square.Rank;

            if (file > 1 && rank > 0)
            {
                var targetSquare = square
                    .Translated(Board, Direction.West, 2)
                    .Translated(Board, Direction.South);

                GenerateKnightMove(square, targetSquare);
            }

            if (file > 0 && rank > 1)
            {
                var targetSquare = square
                    .Translated(Board, Direction.South, 2)
                    .Translated(Board, Direction.West);

                GenerateKnightMove(square, targetSquare);
            }

            if (file > 0 && rank < 6)
            {
                var targetSquare = square
                    .Translated(Board, Direction.North, 2)
                    .Translated(Board, Direction.West);

                GenerateKnightMove(square, targetSquare);
            }

            if (file > 1 && rank < 7)
            {
                var targetSquare = square
                    .Translated(Board, Direction.West, 2)
                    .Translated(Board, Direction.North);

                GenerateKnightMove(square, targetSquare);
            }

            if (file < 7 && rank < 6)
            {
                var targetSquare = square
                    .Translated(Board, Direction.North, 2)
                    .Translated(Board, Direction.East);

                GenerateKnightMove(square, targetSquare);
            }

            if (file < 6 && rank < 7)
            {
                var targetSquare = square
                    .Translated(Board, Direction.East, 2)
                    .Translated(Board, Direction.North);

                GenerateKnightMove(square, targetSquare);
            }

            if (file < 6 && rank > 0)
            {
                var targetSquare = square
                    .Translated(Board, Direction.East, 2)
                    .Translated(Board, Direction.South);

                GenerateKnightMove(square, targetSquare);
            }

            if (file < 7 && rank > 1)
            {
                var targetSquare = square
                    .Translated(Board, Direction.South, 2)
                    .Translated(Board, Direction.East);

                GenerateKnightMove(square, targetSquare);
            }
        }

        private void GenerateSlidingMoves(Square square, Direction startDirection, Direction endDirection)
        {
            for (var direction = startDirection; direction <= endDirection; direction++)
            {
                var distance = square.GetBorderDistance(Board, direction);

                for (var i = 1; i <= distance; i++)
                {
                    var targetSquare = square.Translated(Board, direction, i);
                    var targetPiece = Board[targetSquare];

                    if (targetPiece.IsEmpty)
                    {
                        Moves.Add(new Move(square, targetSquare));
                    }
                    else
                    {
                        if (targetPiece.Color != State.MoveColor)
                        {
                            Moves.Add(new Move(square, targetSquare));
                        }

                        break;
                    }
                }
            }
        }

        private void GenerateKingMoves(Square square)
        {
            for (var direction = Direction.North; direction <= Direction.SouthWest; direction++)
            {
                if (square.GetBorderDistance(Board, direction) == 0)
                {
                    continue;
                }

                var targetSquare = square.Translated(Board, direction);
                var targetPiece = Board[targetSquare];

                if (targetPiece.IsEmpty)
                {
                    Moves.Add(new Move(square, targetSquare));
                }
                else
                {
                    if (targetPiece.Color != State.MoveColor)
                    {
                        Moves.Add(new Move(square, targetSquare));
                    }
                }
            }
        }
    }
}
