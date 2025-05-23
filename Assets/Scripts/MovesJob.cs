﻿using System;
using Unity.Collections;
using Unity.Jobs;

namespace Chess
{
    public struct MovesJob : IJob
    {
        public Board Board;

        [ReadOnly]
        public bool QuietMoves;

        private Bitboard slidingAttackSquares;
        private Bitboard knightAttackSquares;
        private Bitboard pawnAttackSquares;
        private Bitboard kingAttackSquares;
        private Bitboard attackSquares;
        private Bitboard attackSquaresNoPawns;
        private Bitboard checkRayMask;
        private Bitboard pinSquares;
        private Bitboard nonPinSquares;
        private bool isInCheck;
        private bool isInDoubleCheck;

        private int alliedIndex;
        private int enemyIndex;
        private Square alliedKingSquare;
        private Square enemyKingSquare;
        private Bitboard allPiecesBoard;
        private Bitboard alliedPieces;
        private Bitboard enemyPieces;
        private Bitboard moveTypeMask;
        private Bitboard allPieces;
        private Bitboard emptySquares;
        private Bitboard emptyOrEnemyPieces;
        private bool isWhiteAllied;

        void IJob.Execute()
        {
            Initialize();

            FindValidationSquares();

            GenerateKingMoves();

            if (isInDoubleCheck)
            {
                return;
            }

            GenerateSlidingMoves();
            GenerateKnightMoves();
            GeneratePawnMoves();
        }

        private void Initialize()
        {
            isWhiteAllied = Board.AlliedColor == Color.White;

            alliedIndex = (int)Board.AlliedColor;
            enemyIndex = (int)Board.EnemyColor;

            alliedKingSquare = Board.Kings[alliedIndex];
            enemyKingSquare = Board.Kings[enemyIndex];

            allPiecesBoard = Board.AllPiecesBitboard;
            alliedPieces = Board.ColorBitboards[alliedIndex];
            enemyPieces = Board.ColorBitboards[enemyIndex];

            allPieces = Board.AllPiecesBitboard;
            emptySquares = ~allPieces;
            emptyOrEnemyPieces = enemyPieces | enemyPieces;

            moveTypeMask = QuietMoves ? Bitboard.All : enemyPieces;
        }

        private void FindValidationSquares()
        {
            FindSlidingValidationSquares();

            FindPinAndCheckSquares();

            nonPinSquares = ~pinSquares;

            var alliedKingBoard = Board.PieceBitboards[new Piece(Figure.King, Board.AlliedColor).Index];

            var enemyKnightsBoard = Board.PieceBitboards[new Piece(Figure.Knight, Board.EnemyColor).Index];

            while (!enemyKnightsBoard.IsEmpty)
            {
                var knightSquare = enemyKnightsBoard.Pop();
                var knightMoves = Board.KnightMoves[knightSquare];
                knightAttackSquares |= knightMoves;

                if (!(knightMoves & alliedKingBoard).IsEmpty)
                {
                    MakeCheck();
                    checkRayMask.Include(knightSquare);
                }
            }

            var enemyPawnBoard = Board.PieceBitboards[new Piece(Figure.Pawn, Board.EnemyColor).Index];
            pawnAttackSquares = Board.GetPawnAttacks(enemyPawnBoard, Board.EnemyColor);
            if (pawnAttackSquares.Contains(alliedKingSquare))
            {
                MakeCheck();
                var possiblePawnAttackOrigins = isWhiteAllied ? Board.WhitePawnAttacks[alliedKingSquare] : Board.BlackPawnAttacks[alliedKingSquare];
                var pawnChecks = enemyPawnBoard & possiblePawnAttackOrigins;
                checkRayMask |= pawnChecks;
            }

            attackSquaresNoPawns = slidingAttackSquares | knightAttackSquares | Board.KingMoves[enemyKingSquare];
            attackSquares = attackSquaresNoPawns | pawnAttackSquares;

            if (!isInCheck)
            {
                checkRayMask = Bitboard.All;
            }
        }

        private void FindSlidingValidationSquares()
        {
            UpdateSlidingAttack(Board.EnemyOrthogonalSliders, true);
            UpdateSlidingAttack(Board.EnemyDiagonalSliders, false);
        }

        private void UpdateSlidingAttack(Bitboard board, bool isOrthogonal)
        {
            var blockers = (ulong)Board.AllPiecesBitboard & ~(1ul << alliedKingSquare);

            while (!board.IsEmpty)
            {
                var square = board.Pop();
                var moveBoard = Board.GetSliderAttacks(square, blockers, isOrthogonal);
                slidingAttackSquares |= moveBoard;
            }
        }

        private void MakeCheck()
        {
            isInDoubleCheck = isInCheck;
            isInCheck = true;
        }

        private void FindPinAndCheckSquares()
        {
            var startDirection = Direction.North;
            var endDirection = Direction.SouthWest;

            if (Board.Queens[enemyIndex].Count == 0)
            {
                startDirection = (Board.Rooks[enemyIndex].Count > 0) ? Direction.North : Direction.East;
                endDirection = (Board.Bishops[enemyIndex].Count > 0) ? Direction.NorthWest : Direction.SouthWest;
            }

            for (var direction = startDirection; direction <= endDirection; direction++)
            {
                var sliders = direction.IsDiagonal ? Board.EnemyDiagonalSliders : Board.EnemyOrthogonalSliders;

                if ((Board.GetRay(alliedKingSquare, direction) & sliders).IsEmpty)
                {
                    continue;
                }

                var distance = alliedKingSquare.GetBorderDistance(Board, direction);

                var isPinBlocked = false;
                var ray = default(Bitboard);

                for (var i = 1; i <= distance; i++)
                {
                    var targetSquare = alliedKingSquare.Translated(Board, direction, i);
                    ray.Include(targetSquare);
                    var targetPiece = Board[targetSquare];

                    if (!targetPiece.IsEmpty)
                    {
                        if (targetPiece.Color == Board.AlliedColor)
                        {
                            if (!isPinBlocked)
                            {
                                isPinBlocked = true;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            var targetFigure = targetPiece.Figure;

                            var isPinning = IsPinningDirection(direction, targetFigure);

                            if (isPinning)
                            {
                                if (isPinBlocked)
                                {
                                    pinSquares.Union(ray);
                                }
                                else
                                {
                                    checkRayMask.Union(ray);
                                    MakeCheck();
                                }
                            }

                            break;
                        }
                    }
                }

                if (isInDoubleCheck)
                {
                    break;
                }
            }
        }

        private readonly bool IsPinningDirection(Direction direction, Figure figure)
        {
            return direction >= Direction.NorthWest
                && figure == Figure.Bishop
                || figure == Figure.Rook
                || figure == Figure.Queen;
        }

        private void GenerateKingMoves()
        {
            var legalMask = ~(attackSquares | alliedPieces);
            var kingMoves = Board.KingMoves[alliedKingSquare] & legalMask & moveTypeMask;

            while (kingMoves != 0)
            {
                var square = kingMoves.Pop();
                Board.Moves.Add(new Move(alliedKingSquare, square));
            }

            if (!isInCheck && QuietMoves)
            {
                var castleBlockers = attackSquares | Board.AllPiecesBitboard;

                if (Board.State.HasKingsideCastleRight(isWhiteAllied))
                {
                    var castleMask = isWhiteAllied ? Bitboard.WhiteKingsideMask : Bitboard.BlackKingsideMask;
                    if ((castleBlockers & castleMask).IsEmpty)
                    {
                        var targetSquare = isWhiteAllied ? Square.G1 : Square.G8;
                        Board.Moves.Add(new Move(alliedKingSquare, targetSquare, MoveFlags.Castling));
                    }
                }

                if (Board.State.HasQueensideCastleRight(isWhiteAllied))
                {
                    ulong castleMask = isWhiteAllied ? Bitboard.WhiteQueensideMask2 : Bitboard.BlackQueensideMask2;
                    ulong castleBlockMask = isWhiteAllied ? Bitboard.WhiteQueensideMask : Bitboard.BlackQueensideMask;

                    if ((castleMask & castleBlockers).IsEmpty && (castleBlockMask & Board.AllPiecesBitboard).IsEmpty)
                    {
                        var targetSquare = isWhiteAllied ? Square.C1 : Square.C8;
                        Board.Moves.Add(new Move(alliedKingSquare, targetSquare, MoveFlags.Castling));
                    }
                }
            }
        }

        private readonly bool IsPinned(Square square)
        {
            return pinSquares.Contains(square);
        }

        private void GenerateSlidingMoves()
        {
            var moveMask = emptyOrEnemyPieces & checkRayMask & moveTypeMask;

            var orthogonalSliders = Board.AlliedOrthogonalSliders;
            var diagonalSliders = Board.AlliedDiagonalSliders;

            if (isInCheck)
            {
                orthogonalSliders &= ~pinSquares;
                diagonalSliders &= ~pinSquares;
            }

            while (!orthogonalSliders.IsEmpty)
            {
                var square = orthogonalSliders.Pop();
                var moveSquares = Board.GetRookAttacks(square, allPieces) & moveMask;

                if (IsPinned(square))
                {
                    moveSquares &= Board.GetAlignMask(square, alliedKingSquare);
                }

                while (!moveSquares.IsEmpty)
                {
                    var targetSquare = moveSquares.Pop();
                    Board.Moves.Add(new Move(square, targetSquare));
                }
            }

            while (!diagonalSliders.IsEmpty)
            {
                var square = diagonalSliders.Pop();
                var moveSquares = Board.GetBishopAttacks(square, allPieces) & moveMask;

                if (IsPinned(square))
                {
                    moveSquares &= Board.GetAlignMask(square, alliedKingSquare);
                }

                while (!moveSquares.IsEmpty)
                {
                    var targetSquare = moveSquares.Pop();
                    Board.Moves.Add(new Move(square, targetSquare));
                }
            }
        }

        private void GenerateKnightMoves()
        {
            var alliedKnightPiece = new Piece(Figure.Knight, Board.AlliedColor);
            var knights = Board.PieceBitboards[alliedKnightPiece.Index] & pinSquares;
            var moveMask = emptyOrEnemyPieces & checkRayMask & moveTypeMask;

            while (!knights.IsEmpty)
            {
                var square = knights.Pop();
                var moveSquares = Board.KnightMoves[square] & moveMask;

                while (!moveSquares.IsEmpty)
                {
                    var targetSquare = moveSquares.Pop();
                    Board.Moves.Add(new Move(square, targetSquare));
                }
            }
        }

        private void GeneratePawnMoves()
        {
            var pushDir = isWhiteAllied ? 1 : -1;
            var pushOffset = pushDir * 8;

            var friendlyPawnPiece = new Piece(Figure.Pawn, Board.AlliedColor);
            var pawns = Board.PieceBitboards[friendlyPawnPiece.Index];

            var promotionRankMask = isWhiteAllied ? Bitboard.Rank8 : Bitboard.Rank1;

            var singlePush = pawns.Shifted(pushOffset) & emptySquares;

            var pushPromotion = singlePush & promotionRankMask & checkRayMask;

            var captureEdgeFileMask = isWhiteAllied ? ~Bitboard.FileA : ~Bitboard.FileH;
            var captureEdgeFileMask2 = isWhiteAllied ? ~Bitboard.FileH : ~Bitboard.FileA;
            var captureA = (pawns & captureEdgeFileMask).Shifted(pushDir * 7) & enemyPieces;
            var captureB = (pawns & captureEdgeFileMask2).Shifted(pushDir * 9) & enemyPieces;

            var singlePushNoPromotions = singlePush & ~promotionRankMask & checkRayMask;

            var capturePromotionsA = captureA & promotionRankMask & checkRayMask;
            var capturePromotionsB = captureB & promotionRankMask & checkRayMask;

            captureA &= checkRayMask & ~promotionRankMask;
            captureB &= checkRayMask & ~promotionRankMask;

            if (QuietMoves)
            {
                while (!singlePushNoPromotions.IsEmpty)
                {
                    var targetSquare = singlePushNoPromotions.Pop();
                    var startSquare = targetSquare - pushOffset;
                    if (!IsPinned(startSquare) || Board.GetAlignMask(startSquare, alliedKingSquare) == Board.GetAlignMask(targetSquare, alliedKingSquare))
                    {
                        Board.Moves.Add(new Move(startSquare, targetSquare));
                    }
                }

                var doublePushTargetRankMask = isWhiteAllied ? Bitboard.Rank4 : Bitboard.Rank5;
                var doublePush = singlePush.Shifted(pushOffset) & emptySquares & doublePushTargetRankMask & checkRayMask;

                while (!doublePush.IsEmpty)
                {
                    var targetSquare = doublePush.Pop();
                    var startSquare = targetSquare - pushOffset * 2;
                    if (!IsPinned(startSquare) || Board.GetAlignMask(startSquare, alliedKingSquare) == Board.GetAlignMask(targetSquare, alliedKingSquare))
                    {
                        Board.Moves.Add(new Move(startSquare, targetSquare, MoveFlags.DoublePawnMove));
                    }
                }
            }

            while (!captureA.IsEmpty)
            {
                var targetSquare = captureA.Pop();
                var startSquare = targetSquare - pushDir * 7;

                if (!IsPinned(startSquare) || Board.GetAlignMask(startSquare, alliedKingSquare) == Board.GetAlignMask(targetSquare, alliedKingSquare))
                {
                    Board.Moves.Add(new Move(startSquare, targetSquare));
                }
            }

            while (!captureB.IsEmpty)
            {
                var targetSquare = captureB.Pop();
                var startSquare = targetSquare - pushDir * 9;

                if (!IsPinned(startSquare) || Board.GetAlignMask(startSquare, alliedKingSquare) == Board.GetAlignMask(targetSquare, alliedKingSquare))
                {
                    Board.Moves.Add(new Move(startSquare, targetSquare));
                }
            }

            while (!pushPromotion.IsEmpty)
            {
                var targetSquare = pushPromotion.Pop();
                var startSquare = targetSquare - pushOffset;

                if (!IsPinned(startSquare))
                {
                    GeneratePromotions(startSquare, targetSquare);
                }
            }

            while (!capturePromotionsA.IsEmpty)
            {
                var targetSquare = capturePromotionsA.Pop();
                var startSquare = targetSquare - pushDir * 7;

                if (!IsPinned(startSquare) || Board.GetAlignMask(startSquare, alliedKingSquare) == Board.GetAlignMask(targetSquare, alliedKingSquare))
                {
                    GeneratePromotions(startSquare, targetSquare);
                }
            }

            while (!capturePromotionsB.IsEmpty)
            {
                var targetSquare = capturePromotionsB.Pop();
                var startSquare = targetSquare - pushDir * 9;

                if (!IsPinned(startSquare) || Board.GetAlignMask(startSquare, alliedKingSquare) == Board.GetAlignMask(targetSquare, alliedKingSquare))
                {
                    GeneratePromotions(startSquare, targetSquare);
                }
            }

            if (Board.State.enPassantFile > 0)
            {
                var epFileIndex = Board.State.enPassantFile - 1;
                var epRankIndex = isWhiteAllied ? 5 : 2;
                var targetSquare = epRankIndex * 8 + epFileIndex;
                var capturedPawnSquare = targetSquare - pushOffset;

                if (checkRayMask.Contains(capturedPawnSquare))
                {
                    var pawnsThatCanCaptureEp = pawns & Board.GetPawnAttacks((Bitboard)(1ul << targetSquare), Board.AlliedColor);

                    while (!pawnsThatCanCaptureEp.IsEmpty)
                    {
                        var startSquare = pawnsThatCanCaptureEp.Pop();
                        if (!IsPinned(startSquare) || Board.GetAlignMask(startSquare, alliedKingSquare) == Board.GetAlignMask(targetSquare, alliedKingSquare))
                        {
                            if (!IsInCheckAfterEnPassant(startSquare, targetSquare, capturedPawnSquare))
                            {
                                Board.Moves.Add(new Move(startSquare, targetSquare, MoveFlags.EnPassant));
                            }
                        }
                    }
                }
            }
        }

        private bool IsInCheckAfterEnPassant(Square startSquare, int targetSquare, int capturedPawnSquare)
        {
            var enemyOrtho = Board.EnemyOrthogonalSliders;

            while (!enemyOrtho.IsEmpty)
            {
                var maskedBlockers = allPieces ^ (1ul << capturedPawnSquare | 1ul << startSquare | 1ul << targetSquare);
                var rookAttacks = Board.GetRookAttacks(alliedKingSquare, maskedBlockers);
                return (rookAttacks & enemyOrtho) != 0;
            }

            return false;
        }

        private void GeneratePromotions(Square from, Square to)
        {
            Board.Moves.Add(new Move(from, to, MoveFlags.QueenPromotion));
            Board.Moves.Add(new Move(from, to, MoveFlags.RookPromotion));
            Board.Moves.Add(new Move(from, to, MoveFlags.BishopPromotion));
            Board.Moves.Add(new Move(from, to, MoveFlags.KnightPromotion));
        }
    }
}
