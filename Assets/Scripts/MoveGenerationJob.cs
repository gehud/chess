using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Chess
{
    [BurstCompile(CompileSynchronously = true)]
    public struct MoveGenerationJob : IJob
    {
        [ReadOnly, NativeDisableContainerSafetyRestriction]
        public Board Board;

        [ReadOnly]
        public bool QuietMoves;

        [WriteOnly]
        public NativeList<Move> Moves;

        private bool isWhiteAllied;
        private Square alliedKingSquare;

        private bool isInCheck;
        private bool isInDoubleCheck;

        private Bitboard checkRayMask;

        private Bitboard pinSquares;
        private Bitboard nonPinSquares;
        private Bitboard attackSquaresNoPawns;
        private Bitboard attackSquares;
        private Bitboard pawnAttackSquares;

        private Bitboard enemyPieces;
        private Bitboard alliedPieces;
        private Bitboard emptySquares;
        private Bitboard emptyOrEnemyPieces;
        private Square enemyKingSquare;
        private Bitboard moveTypeMask;
        private Bitboard allPieces;

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

        private void AddMove(Square from, Square to, MoveFlags flags = MoveFlags.None)
        {
            Moves.Add(new Move(from, to, flags));
        }

        private void Initialize()
        {
            isWhiteAllied = Board.AlliedColor == Color.White;

            alliedKingSquare = Board.Kings[(int)Board.AlliedColor];
            enemyKingSquare = Board.Kings[(int)Board.EnemyColor];

            alliedPieces = Board.ColorBitboards[(int)Board.AlliedColor];
            enemyPieces = Board.ColorBitboards[(int)Board.EnemyColor];

            allPieces = Board.AllPiecesBitboard;
            emptySquares = ~allPieces;
            emptyOrEnemyPieces = emptySquares | enemyPieces;

            moveTypeMask = QuietMoves ? Bitboard.All : enemyPieces;
        }

        private void FindValidationSquares()
        {
            var slidingAttackSquares = FindSlidingValidationSquares();

            FindPinAndCheckSquares();

            var alliedKingBoard = Board.PieceBitboards[new Piece(Figure.King, Board.AlliedColor).Index];

            var enemyKnightsBoard = Board.PieceBitboards[new Piece(Figure.Knight, Board.EnemyColor).Index];

            var knightAttackSquares = Bitboard.Empty;

            while (!enemyKnightsBoard.IsEmpty)
            {
                var knightSquare = enemyKnightsBoard.Pop();
                var knightMoves = Board.KnightMoves[knightSquare.Index];
                knightAttackSquares |= knightMoves;

                if (!(knightMoves & alliedKingBoard).IsEmpty)
                {
                    MakeCheck();
                    checkRayMask.Include(knightSquare);
                }
            }

            var enemyPawnBoard = Board.PieceBitboards[new Piece(Figure.Pawn, Board.EnemyColor).Index];
            pawnAttackSquares = Board.GetPawnAttacks(enemyPawnBoard, !isWhiteAllied);
            if (pawnAttackSquares.Contains(alliedKingSquare))
            {
                MakeCheck();
                var possiblePawnAttackOrigins = isWhiteAllied ? Board.WhitePawnAttacks[alliedKingSquare.Index] : Board.BlackPawnAttacks[alliedKingSquare.Index];
                var pawnChecks = enemyPawnBoard & possiblePawnAttackOrigins;
                checkRayMask |= pawnChecks;
            }

            attackSquaresNoPawns = slidingAttackSquares | knightAttackSquares | Board.KingMoves[enemyKingSquare.Index];
            attackSquares = attackSquaresNoPawns | pawnAttackSquares;

            if (!isInCheck)
            {
                checkRayMask = Bitboard.All;
            }
        }

        private Bitboard FindSlidingValidationSquares()
        {
            var all = Bitboard.Empty;

            all |= UpdateSlidingAttack(Board.EnemyOrthogonalSliders, true);
            all |= UpdateSlidingAttack(Board.EnemyDiagonalSliders, false);

            return all;
        }

        private Bitboard UpdateSlidingAttack(Bitboard board, bool isOrthogonal)
        {
            var result = Bitboard.Empty;

            var blockers = Board.AllPiecesBitboard.Without(alliedKingSquare);

            while (!board.IsEmpty)
            {
                var square = board.Pop();
                var moveBoard = Board.GetSliderAttacks(square, blockers, isOrthogonal);
                result |= moveBoard;
            }

            return result;
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

            if (Board.Queens[(int)Board.EnemyColor].Length == 0)
            {
                startDirection = (Board.Rooks[(int)Board.EnemyColor].Length > 0) ? Direction.North : Direction.East;
                endDirection = (Board.Bishops[(int)Board.EnemyColor].Length > 0) ? Direction.SouthWest : Direction.East;
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
                var ray = Bitboard.Empty;

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
                                    pinSquares |= ray;
                                }
                                else
                                {
                                    checkRayMask |= ray;
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

            nonPinSquares = ~pinSquares;
        }

        private readonly bool IsPinningDirection(Direction direction, Figure figure)
        {
            return figure == Figure.Queen 
                || direction >= Direction.NorthWest
                && figure == Figure.Bishop
                || direction <= Direction.East
                && figure == Figure.Rook;
        }

        private void GenerateKingMoves()
        {
            var legalMask = ~(attackSquares | alliedPieces);
            var kingMoves = Board.KingMoves[alliedKingSquare.Index] & legalMask & moveTypeMask;

            while (!kingMoves.IsEmpty)
            {
                var square = kingMoves.Pop();
                AddMove(alliedKingSquare, square);
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
                        AddMove(alliedKingSquare, targetSquare, MoveFlags.Castling);
                    }
                }

                if (Board.State.HasQueensideCastleRight(isWhiteAllied))
                {
                    var castleMask = isWhiteAllied ? Bitboard.WhiteQueensideMask2 : Bitboard.BlackQueensideMask2;
                    var castleBlockMask = isWhiteAllied ? Bitboard.WhiteQueensideMask : Bitboard.BlackQueensideMask;

                    if ((castleMask & castleBlockers).IsEmpty && (castleBlockMask & Board.AllPiecesBitboard).IsEmpty)
                    {
                        var targetSquare = isWhiteAllied ? Square.C1 : Square.C8;
                        AddMove(alliedKingSquare, targetSquare, MoveFlags.Castling);
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
                    AddMove(square, targetSquare);
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
                    AddMove(square, targetSquare);
                }
            }
        }

        private void GenerateKnightMoves()
        {
            var alliedKnightPiece = new Piece(Figure.Knight, Board.AlliedColor);
            var knights = Board.PieceBitboards[alliedKnightPiece.Index] & nonPinSquares;
            var moveMask = emptyOrEnemyPieces & checkRayMask & moveTypeMask;

            while (!knights.IsEmpty)
            {
                var square = knights.Pop();
                var moveSquares = Board.KnightMoves[square.Index] & moveMask;

                while (!moveSquares.IsEmpty)
                {
                    var targetSquare = moveSquares.Pop();
                    AddMove(square, targetSquare);
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
                        AddMove(startSquare, targetSquare);
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
                        AddMove(startSquare, targetSquare, MoveFlags.DoublePawnMove);
                    }
                }
            }

            while (!captureA.IsEmpty)
            {
                var targetSquare = captureA.Pop();
                var startSquare = targetSquare - pushDir * 7;

                if (!IsPinned(startSquare) || Board.GetAlignMask(startSquare, alliedKingSquare) == Board.GetAlignMask(targetSquare, alliedKingSquare))
                {
                    AddMove(startSquare, targetSquare);
                }
            }

            while (!captureB.IsEmpty)
            {
                var targetSquare = captureB.Pop();
                var startSquare = targetSquare - pushDir * 9;

                if (!IsPinned(startSquare) || Board.GetAlignMask(startSquare, alliedKingSquare) == Board.GetAlignMask(targetSquare, alliedKingSquare))
                {
                    AddMove(startSquare, targetSquare);
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

            if (Board.State.EnPassantFile > 0)
            {
                var epFileIndex = Board.State.EnPassantFile - 1;
                var epRankIndex = isWhiteAllied ? 5 : 2;
                var targetSquare = new Square(epFileIndex, epRankIndex);
                var capturedPawnSquare = targetSquare - pushOffset;

                if (checkRayMask.Contains(capturedPawnSquare))
                {
                    var pawnsThatCanCaptureEp = pawns & Board.GetPawnAttacks(Bitboard.Empty.With(targetSquare), !isWhiteAllied);

                    while (!pawnsThatCanCaptureEp.IsEmpty)
                    {
                        var startSquare = pawnsThatCanCaptureEp.Pop();
                        if (!IsPinned(startSquare) || Board.GetAlignMask(startSquare, alliedKingSquare) == Board.GetAlignMask(targetSquare, alliedKingSquare))
                        {
                            if (!IsInCheckAfterEnPassant(startSquare, targetSquare, capturedPawnSquare))
                            {
                                AddMove(startSquare, targetSquare, MoveFlags.EnPassant);
                            }
                        }
                    }
                }
            }
        }

        private bool IsInCheckAfterEnPassant(Square startSquare, Square targetSquare, Square capturedPawnSquare)
        {
            var enemyOrtho = Board.EnemyOrthogonalSliders;

            if (!enemyOrtho.IsEmpty)
            {
                var maskedBlockers = allPieces ^ Bitboard.Empty.With(capturedPawnSquare).With(startSquare).With(targetSquare);
                var rookAttacks = Board.GetRookAttacks(alliedKingSquare, maskedBlockers);
                return !(rookAttacks & enemyOrtho).IsEmpty;
            }

            return false;
        }

        private void GeneratePromotions(Square from, Square to)
        {
            AddMove(from, to, MoveFlags.QueenPromotion);
            AddMove(from, to, MoveFlags.RookPromotion);
            AddMove(from, to, MoveFlags.BishopPromotion);
            AddMove(from, to, MoveFlags.KnightPromotion);
        }
    }
}
