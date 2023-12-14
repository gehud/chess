using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Chess {
    [BurstCompile]
    public struct MoveGenerationJob : IJob, IDisposable {
        public int MyKing;
        [ReadOnly]
        private bool isWhiteKingsideCastlingAvaible;
        [ReadOnly]
        private bool isWhiteQueensideCastlingAvaible;
        [ReadOnly]
        private bool isBlackKingsideCastlingAvaible;
        [ReadOnly]
        private bool isBlackQueensideCastlingAvaible;
        [ReadOnly]
        private Color color;
        [ReadOnly]
        private NativeArray<Piece> board;
        [ReadOnly]
        private NativeArray<int> squareOffsets;
        [ReadOnly]
        private NativeArray<int> moveLimits;
        [ReadOnly]
        private int twoSquarePawn;
        [ReadOnly]
        private bool capturesOnly;
        [WriteOnly]
        private NativeList<Move> moves;
        [WriteOnly]
        private NativeList<int> attackSquares;

        public static MoveGenerationJob Create(
            Color color, 
            NativeArray<Piece> board, 
            NativeArray<int> squareOffset, 
            NativeArray<int> moveLimits, 
            int twoSquarePawn, 
            bool isWhiteKingsideCastlingAvaible, 
            bool isWhiteQueensideCastlingAvaible, 
            bool isBlackKingsideCastlingAvaible,
            bool isBlackQueensideCastlingAvaible,
            bool capturesOnly = false) {
            return new MoveGenerationJob {
                color = color,
                board = board,
                squareOffsets = squareOffset,
                moveLimits = moveLimits,
                twoSquarePawn = twoSquarePawn,
                isWhiteKingsideCastlingAvaible = isWhiteKingsideCastlingAvaible,
                isWhiteQueensideCastlingAvaible = isWhiteQueensideCastlingAvaible,
                isBlackKingsideCastlingAvaible = isBlackKingsideCastlingAvaible,
                isBlackQueensideCastlingAvaible = isBlackQueensideCastlingAvaible,
                capturesOnly = capturesOnly,
                moves = new NativeList<Move>(Allocator.TempJob),
                attackSquares = new NativeList<int>(Allocator.TempJob),
            };
        }

        public void Execute() {
            for (int squareIndex = 0; squareIndex < Board.Area; squareIndex++) {
                Select(squareIndex);
            }
        }

        public void Select(int squareIndex) {
            var piece = board[squareIndex];

            if (piece.Color != color) {
                return;
            }

            if (piece == Piece.Empty) {
                return;
            }

            var figure = piece.Figure;
            if (IsSliding(figure)) {
                GenerateSlidingMoves(squareIndex);
            } else if (figure == Figure.Pawn) {
                GeneratePawnMoves(squareIndex);
            } else if (figure == Figure.Knight) {
                GenerateKnightMoves(squareIndex);
            } else if (figure == Figure.King) {
                GenerateKingMoves(squareIndex);
            }
        }

        private readonly bool IsSliding(Figure figure) => (int)figure <= 4;

        private void GenerateSlidingMoves(int squareIndex) {
            var piece = board[squareIndex];
            var figure = piece.Figure;

            var color = piece.Color;
            int startDirectionIndex = figure == Figure.Bishop ? 4 : 0;
            int endDirectionIndex = figure == Figure.Rook ? 4 : 8;

            for (int directionIndex = startDirectionIndex; directionIndex < endDirectionIndex; directionIndex++) {
                int limit = GetMoveLimit(squareIndex, directionIndex);
                for (int i = 0; i < limit; i++) {
                    int targetSquareIndex = squareIndex + squareOffsets[directionIndex] * (i + 1);
                    var targetPiece = board[targetSquareIndex];

                    attackSquares.Add(targetSquareIndex);
                    if (targetPiece.Color == color) {
                        break;
                    }

                    if (!capturesOnly || targetPiece.Color != color) {
                        moves.Add(new Move(squareIndex, targetSquareIndex));
                    }

                    if (targetPiece != Piece.Empty) {
                        if (targetPiece.Figure == Figure.King && targetPiece.Color != color) {
                            for (int j = i + 1; j < limit; j++) {
                                targetSquareIndex = squareIndex + squareOffsets[directionIndex] * (j + 1);
                                targetPiece = board[targetSquareIndex];
                                attackSquares.Add(targetSquareIndex);
                                if (targetPiece != Piece.Empty) {
                                    break;
                                }
                            }
                        }

                        break;
                    }
                }
            }
        }

        bool TryAddPromotionIfNeeded(int fromSquareIndex, int toSquareIndex) {
            var rank = Board.GetRank(toSquareIndex);
            if (rank == 0 || rank == 7) {
                if (!capturesOnly || board[toSquareIndex].Color != color) {
                    moves.Add(new Move(fromSquareIndex, toSquareIndex, MoveFlags.QueenPromotion));
                    moves.Add(new Move(fromSquareIndex, toSquareIndex, MoveFlags.RookPromotion));
                    moves.Add(new Move(fromSquareIndex, toSquareIndex, MoveFlags.KnightPromotion));
                    moves.Add(new Move(fromSquareIndex, toSquareIndex, MoveFlags.BishopPromotion));
                }

                return true;
            }

            return false;
        }

        private int GetSquareOffset(Direction direction) {
            return squareOffsets[(int)direction];
        }

        private int GetMoveLimit(int squareIndex, int directionIndex) {
            return moveLimits[squareIndex + directionIndex * Board.Area];
        }

        private void GeneratePawnMoves(int squareIndex) {
            var color = board[squareIndex].Color;

            bool isInVertivalBounds = color == Color.White ?
                Board.GetRank(squareIndex) < 7 : Board.GetRank(squareIndex) > 0;

            if (isInVertivalBounds && Board.GetFile(squareIndex) > 0) {
                var leftDiagonalDirection = color == Color.White ?
                    Direction.NorthWest : Direction.SouthWest;
                int targetSquareIndex = squareIndex + GetSquareOffset(leftDiagonalDirection);
                var targetPiece = board[targetSquareIndex];
                attackSquares.Add(targetSquareIndex);
                if (targetPiece != Piece.Empty) {
                    if (targetPiece.Color != color) {
                        if (!TryAddPromotionIfNeeded(squareIndex, targetSquareIndex)) {
                            if (!capturesOnly || targetPiece.Color != color) {
                                moves.Add(new Move(squareIndex, targetSquareIndex));
                            }
                        }
                    }
                }
            }

            if (isInVertivalBounds && Board.GetFile(squareIndex) < 7) {
                var rightDiagonalDirection = color == Color.White ?
                    Direction.NorthEast : Direction.SouthEast;
                var targetSquareIndex = squareIndex + GetSquareOffset(rightDiagonalDirection);
                var targetPiece = board[targetSquareIndex];
                attackSquares.Add(targetSquareIndex);
                if (targetPiece != Piece.Empty) {
                    if (targetPiece.Color != color) {
                        if (!TryAddPromotionIfNeeded(squareIndex, targetSquareIndex)) {
                            if (!capturesOnly || targetPiece.Color != color) {
                                moves.Add(new Move(squareIndex, targetSquareIndex));
                            }
                        }
                    }
                }
            }

            var twoSquarePawnColor = twoSquarePawn != -1 ? board[twoSquarePawn].Color : Color.None;
            bool enPassant =
                twoSquarePawnColor != Color.None &&
                twoSquarePawnColor != color &&
                Mathf.Abs(Board.GetFile(squareIndex) - Board.GetFile(twoSquarePawn)) == 1 &&
                (twoSquarePawn - squareIndex) switch {
                    1 => true,
                    -1 => true,
                    _ => false
                };

            if (enPassant) {
                var offsetDirection = color == Color.White ? Direction.North : Direction.South;
                int targetSquare = twoSquarePawn + GetSquareOffset(offsetDirection);
                moves.Add(new Move(squareIndex, targetSquare));
            }

            int directionOffsetIndex = color == Color.White ? 0 : 1;
            int avaibleSquares = (color == Color.White && Board.GetRank(squareIndex) == 1
                || color == Color.Black && Board.GetRank(squareIndex) == 6) ? 2 : 1;
            for (int i = 0; i < Mathf.Min(GetMoveLimit(squareIndex, directionOffsetIndex), avaibleSquares); i++) {
                var targetSquareIndex = squareIndex + squareOffsets[directionOffsetIndex] * (i + 1);
                var targetPiece = board[targetSquareIndex];

                if (targetPiece != Piece.Empty)
                    break;

                if (!TryAddPromotionIfNeeded(squareIndex, targetSquareIndex)) {
                    moves.Add(new Move(squareIndex, targetSquareIndex));
                }
            }
        }

        bool IsMoveNeeeded(in Piece targetPiece) => targetPiece.IsEmpty && !capturesOnly || targetPiece.Color != color;
        
        void StoreMoveIfNeeded(int squareIndex, int targetSquareIndex, in Piece targetPiece) {
            attackSquares.Add(targetSquareIndex);
            if (IsMoveNeeeded(targetPiece)) {
                moves.Add(new Move(squareIndex, targetSquareIndex));
            }
        }

        private void GenerateKnightMoves(int squareIndex) {
            int file = Board.GetFile(squareIndex);
            int rank = Board.GetRank(squareIndex);

            if (file > 1 && rank > 0) {
                var targetSquareIndex = squareIndex +
                    GetSquareOffset(Direction.SouthWest) * 2 +
                    GetSquareOffset(Direction.North);

                var targetPiece = board[targetSquareIndex];

                StoreMoveIfNeeded(squareIndex, targetSquareIndex, targetPiece);
            }

            if (file > 0 && rank > 1) {
                var targetSquareIndex = squareIndex +
                    GetSquareOffset(Direction.SouthWest) * 2 +
                    GetSquareOffset(Direction.East);

                var targetPiece = board[targetSquareIndex];

                StoreMoveIfNeeded(squareIndex, targetSquareIndex, targetPiece);
            }

            if (file > 0 && rank < 6) {
                var targetSquareIndex = squareIndex +
                    GetSquareOffset(Direction.NorthWest) * 2 +
                    GetSquareOffset(Direction.East);

                var targetPiece = board[targetSquareIndex];

                StoreMoveIfNeeded(squareIndex, targetSquareIndex, targetPiece);
            }

            if (file > 1 && rank < 7) {
                var targetSquareIndex = squareIndex +
                    GetSquareOffset(Direction.NorthWest) * 2 +
                    GetSquareOffset(Direction.South);

                var targetPiece = board[targetSquareIndex];

                StoreMoveIfNeeded(squareIndex, targetSquareIndex, targetPiece);
            }

            if (file < 7 && rank < 6) {
                var targetSquareIndex = squareIndex +
                    GetSquareOffset(Direction.NorthEast) * 2 +
                    GetSquareOffset(Direction.West);

                var targetPiece = board[targetSquareIndex];

                StoreMoveIfNeeded(squareIndex, targetSquareIndex, targetPiece);
            }

            if (file < 6 && rank < 7) {
                var targetSquareIndex = squareIndex +
                    GetSquareOffset(Direction.NorthEast) * 2 +
                    GetSquareOffset(Direction.South);

                var targetPiece = board[targetSquareIndex];

                StoreMoveIfNeeded(squareIndex, targetSquareIndex, targetPiece);
            }

            if (file < 6 && rank > 0) {
                var targetSquareIndex = squareIndex +
                    GetSquareOffset(Direction.SouthEast) * 2 +
                    GetSquareOffset(Direction.North);

                var targetPiece = board[targetSquareIndex];

                StoreMoveIfNeeded(squareIndex, targetSquareIndex, targetPiece);
            }

            if (file < 7 && rank > 1) {
                var targetSquareIndex = squareIndex +
                    GetSquareOffset(Direction.SouthEast) * 2 +
                    GetSquareOffset(Direction.West);

                var targetPiece = board[targetSquareIndex];

                StoreMoveIfNeeded(squareIndex, targetSquareIndex, targetPiece);
            }
        }

        private void GenerateKingMoves(int squareIndex) {
            MyKing = squareIndex;

            // Castling
            if (color == Color.White) {
                if (isWhiteKingsideCastlingAvaible) {
                    bool isWhiteKingCastlingPossigle = true;
                    for (int i = 1; i <= 2; i++) {
                        if (!board[squareIndex + GetSquareOffset(Direction.East) * i].IsEmpty) {
                            isWhiteKingCastlingPossigle = false;
                            break;
                        }
                    }

                    if (isWhiteKingCastlingPossigle) {
                        moves.Add(new Move(squareIndex, squareIndex + GetSquareOffset(Direction.East) * 2));
                    }
                }

                if (isWhiteQueensideCastlingAvaible) {
                    bool isWhiteQueenCastlingPossigle = true;
                    for (int i = 1; i <= 3; i++) {
                        if (!board[squareIndex + GetSquareOffset(Direction.West) * i].IsEmpty) {
                            isWhiteQueenCastlingPossigle = false;
                            break;
                        }
                    }

                    if (isWhiteQueenCastlingPossigle) {
                        moves.Add(new Move(squareIndex, squareIndex + GetSquareOffset(Direction.West) * 2));
                    }
                }
            } else {
                if (isBlackKingsideCastlingAvaible) {
                    bool isBlackKingCastlingPossigle = true;
                    for (int i = 1; i <= 2; i++) {
                        if (!board[squareIndex + GetSquareOffset(Direction.East) * i].IsEmpty) {
                            isBlackKingCastlingPossigle = false;
                            break;
                        }
                    }

                    if (isBlackKingCastlingPossigle) {
                        moves.Add(new Move(squareIndex, squareIndex + GetSquareOffset(Direction.East) * 2));
                    }
                }


                if (isBlackQueensideCastlingAvaible) {
                    bool isBlackQueenCastlingPossigle = true;
                    for (int i = 1; i <= 3; i++) {
                        if (!board[squareIndex + GetSquareOffset(Direction.West) * i].IsEmpty) {
                            isBlackQueenCastlingPossigle = false;
                            break;
                        }
                    }

                    if (isBlackQueenCastlingPossigle) {
                        moves.Add(new Move(squareIndex, squareIndex + GetSquareOffset(Direction.West) * 2));
                    }
                }
            }

            for (int directionIndex = 0; directionIndex < 8; directionIndex++) {
                if (GetMoveLimit(squareIndex, directionIndex) == 0)
                    continue;

                int targetSquareIndex = squareIndex + squareOffsets[directionIndex];
                var targetPiece = board[targetSquareIndex];

                attackSquares.Add(targetSquareIndex);
                if (targetPiece.Color == color)
                    continue;

                if (!capturesOnly || targetPiece.Color != color)
                    moves.Add(new Move(squareIndex, targetSquareIndex));
            }
        }

        public void Dispose() {
            moves.Dispose();
            attackSquares.Dispose();
        }
    }
}