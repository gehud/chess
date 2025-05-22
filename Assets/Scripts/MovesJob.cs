using Unity.Collections;
using Unity.Jobs;

namespace Chess
{
    public struct MovesJob : IJob
    {
        [ReadOnly]
        public State State;
        [ReadOnly]
        public Board Board;
        [WriteOnly]
        public NativeList<Move> Moves;

        private Bitboard slidingAttackSquares;
        private Bitboard knightAttackSquares;
        private Bitboard pawnAttackSquares;
        private Bitboard kingAttackSquares;
        private Bitboard attackSquares;
        private Bitboard checkSquares;
        private Bitboard pinSquaresHorizontal;
        private Bitboard pinSquaresVertical;
        private Bitboard pinSquaresRightDiagonal;
        private Bitboard pinSquaresLeftDiagonal;
        private Bitboard pinSquares;
        private bool isInCheck;
        private bool isInDoubleCheck;

        void IJob.Execute()
        {
            FindValidationSquares();

            GenerateKingMoves();

            if (isInDoubleCheck)
            {
                return;
            }

            for (var square = Square.Zero; square < Board.Area; square++)
            {
                var piece = Board[square];

                if (piece.IsEmpty || piece.Color != State.MoveColor)
                {
                    continue;
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
                }
            }
        }

        private void FindValidationSquares()
        {
            FindPinAndCheckSquares();

            for (var square = Square.Zero; square < Board.Area; square++)
            {
                var piece = Board[square];

                if (piece.IsEmpty || piece.Color == State.MoveColor)
                {
                    continue;
                }

                switch (piece.Figure)
                {
                    case Figure.Pawn:
                        FindPawnValidationSquares(square);
                        break;
                    case Figure.Knight:
                        FindKnightValidationSquares(square);
                        break;
                    case Figure.Bishop:
                        FindSlidingValidationSquares(square, Direction.NorthWest, Direction.SouthWest);
                        break;
                    case Figure.Rook:
                        FindSlidingValidationSquares(square, Direction.North, Direction.West);
                        break;
                    case Figure.Queen:
                        FindSlidingValidationSquares(square, Direction.North, Direction.SouthWest);
                        break;
                    case Figure.King:
                        FindKingValidationSquares(square);
                        break;
                }
            }

            attackSquares.Union(slidingAttackSquares);
            attackSquares.Union(knightAttackSquares);
            attackSquares.Union(pawnAttackSquares);
            attackSquares.Union(kingAttackSquares);
        }

        private void MakeCheck()
        {
            isInDoubleCheck = isInCheck;
            isInCheck = true;
        }

        private void FindPinAndCheckSquares()
        {
            for (var direction = Direction.North; direction <= Direction.SouthWest; direction++)
            {
                var distance = State.AlliedKingSquare.GetBorderDistance(Board, direction);
                var isPinBlocked = false;
                var line = default(Bitboard);

                for (var i = 1; i <= distance; i++)
                {
                    var targetSquare = State.AlliedKingSquare.Translated(Board, direction, i);
                    line.Include(targetSquare);
                    var targetPiece = Board[targetSquare];

                    if (!targetPiece.IsEmpty)
                    {
                        if (targetPiece.Color == State.MoveColor)
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

                            var isPinned = IsPinningDirection(direction, targetFigure);

                            if (isPinned)
                            {
                                if (isPinBlocked)
                                {
                                    switch (direction)
                                    {
                                        case Direction.North:
                                        case Direction.South:
                                            pinSquaresVertical.Union(line);
                                            break;
                                        case Direction.West:
                                        case Direction.East:
                                            pinSquaresHorizontal.Union(line);
                                            break;
                                        case Direction.NorthEast:
                                        case Direction.SouthWest:
                                            pinSquaresRightDiagonal.Union(line);
                                            break;
                                        case Direction.NorthWest:
                                        case Direction.SouthEast:
                                            pinSquaresLeftDiagonal.Union(line);
                                            break;
                                    }
                                }
                                else
                                {
                                    checkSquares.Union(line);
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

            pinSquares.Union(pinSquaresHorizontal);
            pinSquares.Union(pinSquaresVertical);
            pinSquares.Union(pinSquaresRightDiagonal);
            pinSquares.Union(pinSquaresLeftDiagonal);
        }

        private readonly bool IsPinningDirection(Direction direction, Figure figure)
        {
            return direction >= Direction.NorthWest
                && figure == Figure.Bishop
                || figure == Figure.Rook
                || figure == Figure.Queen;
        }

        private void FindSlidingValidationSquares(Square square, Direction startDirection, Direction endDirection)
        {
            for (var direction = startDirection; direction <= endDirection; direction++)
            {
                var distance = square.GetBorderDistance(Board, direction);

                for (var i = 1; i <= distance; i++)
                {
                    var targetSquare = square.Translated(Board, direction, i);
                    slidingAttackSquares.Include(targetSquare);
                    if (targetSquare != State.AlliedKingSquare)
                    {
                        if (!Board[targetSquare].IsEmpty)
                        {
                            break;
                        }
                    }
                }
            }
        }

        private void FindKnightValidationSquares(Square square)
        {
            var isKnightCheck = false;

            int file = square.File;
            int rank = square.Rank;

            if (file > 1 && rank > 0)
            {
                var targetSquare = square
                    .Translated(Board, Direction.West, 2)
                    .Translated(Board, Direction.South);

                knightAttackSquares.Include(targetSquare);
            }

            if (file > 0 && rank > 1)
            {
                var targetSquare = square
                    .Translated(Board, Direction.South, 2)
                    .Translated(Board, Direction.West);

                knightAttackSquares.Include(targetSquare);
            }

            if (file > 0 && rank < 6)
            {
                var targetSquare = square
                    .Translated(Board, Direction.North, 2)
                    .Translated(Board, Direction.West);

                knightAttackSquares.Include(targetSquare);
            }

            if (file > 1 && rank < 7)
            {
                var targetSquare = square
                    .Translated(Board, Direction.West, 2)
                    .Translated(Board, Direction.North);

                knightAttackSquares.Include(targetSquare);
            }

            if (file < 7 && rank < 6)
            {
                var targetSquare = square
                    .Translated(Board, Direction.North, 2)
                    .Translated(Board, Direction.East);

                knightAttackSquares.Include(targetSquare);
            }

            if (file < 6 && rank < 7)
            {
                var targetSquare = square
                    .Translated(Board, Direction.East, 2)
                    .Translated(Board, Direction.North);

                knightAttackSquares.Include(targetSquare);
            }

            if (file < 6 && rank > 0)
            {
                var targetSquare = square
                    .Translated(Board, Direction.East, 2)
                    .Translated(Board, Direction.South);

                knightAttackSquares.Include(targetSquare);
            }

            if (file < 7 && rank > 1)
            {
                var targetSquare = square
                    .Translated(Board, Direction.South, 2)
                    .Translated(Board, Direction.East);

                knightAttackSquares.Include(targetSquare);
            }

            if (!isKnightCheck && knightAttackSquares.Contains(State.AlliedKingSquare))
            {
                isKnightCheck = true;
                MakeCheck();
                checkSquares.Include(square);
            }
        }

        private void FindPawnValidationSquares(Square square)
        {
            var isPawnCheck = false;

            var leftAttackDirection = default(Direction);
            var rightAttackDirection = default(Direction);

            switch (Board[square].Color)
            {
                case Color.Black:
                    leftAttackDirection = Direction.SouthWest;
                    rightAttackDirection = Direction.SouthEast;
                    break;
                case Color.White:
                    leftAttackDirection = Direction.NorthWest;
                    rightAttackDirection = Direction.NorthEast;
                    break;
            }

            if (square.GetBorderDistance(Board, leftAttackDirection) >= 1)
            {
                pawnAttackSquares.Include(square.Translated(Board, leftAttackDirection));
            }

            if (square.GetBorderDistance(Board, rightAttackDirection) >= 1)
            {
                pawnAttackSquares.Include(square.Translated(Board, rightAttackDirection));
            }

            if (!isPawnCheck && pawnAttackSquares.Contains(State.AlliedKingSquare))
            {
                isPawnCheck = true;
                MakeCheck();
                checkSquares.Include(square);
            }
        }

        private void FindKingValidationSquares(Square square)
        {
            for (var direction = Direction.North; direction <= Direction.SouthWest; direction++)
            {
                var distance = square.GetBorderDistance(Board, direction);

                if (distance == 0)
                {
                    continue;
                }

                var targetSquare = square.Translated(Board, direction);
                kingAttackSquares.Include(targetSquare);
            }
        }

        private void GenerateKingMoves()
        {
            for (var direction = Direction.North; direction <= Direction.SouthWest; direction++)
            {
                if (State.AlliedKingSquare.GetBorderDistance(Board, direction) == 0)
                {
                    continue;
                }

                var targetSquare = State.AlliedKingSquare.Translated(Board, direction);
                var targetPiece = Board[targetSquare];

                if (!targetPiece.IsEmpty && targetPiece.Color == State.MoveColor)
                {
                    continue;
                }

                var isCapturing = !targetPiece.IsEmpty;

                if (!isCapturing && checkSquares.Contains(targetSquare))
                {
                    continue;
                }

                if (!attackSquares.Contains(targetSquare))
                {
                    Moves.Add(new Move(State.AlliedKingSquare, targetSquare));

                    if (!isInCheck && !isCapturing)
                    {
                        if (State.MoveColor == Color.White && State.WhiteCastlingKingside && targetSquare == new Square(5, 0) ||
                            State.MoveColor == Color.Black && State.BlackCastlingKingside && targetSquare == new Square(5, 7))
                        {
                            var castlingSquare = targetSquare + 1;
                            if (Board[castlingSquare].IsEmpty && !attackSquares.Contains(castlingSquare))
                            {
                                Moves.Add(new Move(State.AlliedKingSquare, castlingSquare, MoveFlags.CastlingKingside));
                            }
                        }
                        else if (State.MoveColor == Color.White && State.WhiteCastlingQueenside && targetSquare == new Square(3, 0) ||
                                State.MoveColor == Color.Black && State.BlackCastlingQueenside && targetSquare == new Square(3, 7))
                        {
                            var castlingSquare = targetSquare - 1;
                            if (Board[castlingSquare].IsEmpty && !attackSquares.Contains(castlingSquare))
                            {
                                Moves.Add(new Move(State.AlliedKingSquare, castlingSquare, MoveFlags.CastlingQueenside));
                            }
                        }
                    }
                }
            }
        }

        private void GeneratePawnMoves(in Square square)
        {
            var forwardDirection = State.MoveColor == Color.White ? Direction.North : Direction.South;
            var isPromotionRequired = State.MoveColor == Color.White ? square.Rank == Board.Size - 2 : square.Rank == 1;
            var isFirstMove = State.MoveColor == Color.White ? square.Rank == 1 : square.Rank == 6;

            if (square.GetBorderDistance(Board, forwardDirection) == 0)
            {
                return;
            }

            var oneForwardSquare = square.Translated(Board, forwardDirection);

            if (Board[oneForwardSquare].IsEmpty)
            {
                if (!pinSquares.Contains(square) || pinSquaresVertical.Contains(oneForwardSquare))
                {
                    if (!isInCheck || checkSquares.Contains(oneForwardSquare))
                    {
                        if (isPromotionRequired)
                        {
                            AddPawnPromotion(square, oneForwardSquare);
                        }
                        else
                        {
                            Moves.Add(new Move(square, oneForwardSquare));
                        }
                    }

                    if (isFirstMove)
                    {
                        var twoForwardSquare = oneForwardSquare.Translated(Board, forwardDirection);
                        if (Board[twoForwardSquare].IsEmpty)
                        {
                            if (!isInCheck || checkSquares.Contains(twoForwardSquare))
                            {
                                Moves.Add(new Move(square, twoForwardSquare, MoveFlags.DoublePawnMove));
                            }
                        }
                    }
                }
            }

            var leftAttackDirection = default(Direction);
            var rightAttackDirection = default(Direction);

            switch (State.MoveColor)
            {
                case Color.Black:
                    leftAttackDirection = Direction.SouthWest;
                    rightAttackDirection = Direction.SouthEast;
                    break;
                case Color.White:
                    leftAttackDirection = Direction.NorthWest;
                    rightAttackDirection = Direction.NorthEast;
                    break;
            }

            if (square.GetBorderDistance(Board, leftAttackDirection) >= 1)
            {
                TryAddPawnCaptureMove(square, square.Translated(Board, leftAttackDirection), leftAttackDirection, isPromotionRequired);
            }

            if (square.GetBorderDistance(Board, rightAttackDirection) >= 1)
            {
                TryAddPawnCaptureMove(square, square.Translated(Board, rightAttackDirection), rightAttackDirection, isPromotionRequired);
            }
        }

        private void TryAddPawnCaptureMove(Square from, Square to, Direction direction, bool isPromotionRequired)
        {
            var targetPiece = Board[to];

            if (!targetPiece.IsEmpty && targetPiece.Color == State.MoveColor)
            {
                return;
            }

            var pinAxis = GetPinAxis(direction);

            if (pinSquares.Contains(from) && !pinAxis.Contains(to))
            {
                return;
            }

            var isCapturing = !targetPiece.IsEmpty;

            if (isCapturing)
            {
                if (isInCheck && !checkSquares.Contains(to))
                {
                    return;
                }

                if (isPromotionRequired)
                {
                    AddPawnPromotion(from, to);
                }
                else
                {
                    Moves.Add(new Move(from, to));
                }
            }

            var enPassantSquare = -1;
            if (State.DoubleMovePawnSquare != -1)
            {
                enPassantSquare = 8 * (State.MoveColor == Color.White ? 5 : 2) + State.DoubleMovePawnSquare.File;
            }

            if (to == enPassantSquare)
            {
                Moves.Add(new Move(from, to, MoveFlags.EnPassant));
            }
        }

        private void AddPawnPromotion(Square from, Square to)
        {
            Moves.Add(new Move(from, to, MoveFlags.QueenPromotion));
            Moves.Add(new Move(from, to, MoveFlags.RookPromotion));
            Moves.Add(new Move(from, to, MoveFlags.KnightPromotion));
            Moves.Add(new Move(from, to, MoveFlags.BishopPromotion));
        }

        private void GenerateKnightMove(Square from, Square to)
        {
            var targetPiece = Board[to];

            if (!targetPiece.IsEmpty && targetPiece.Color == State.MoveColor)
            {
                return;
            }

            if (isInCheck && !checkSquares.Contains(to))
            {
                return;
            }

            Moves.Add(new Move(from, to));
        }

        private void GenerateKnightMoves(Square square)
        {
            if (pinSquares.Contains(square))
            {
                return;
            }

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

        private readonly Bitboard GetPinAxis(Direction direction)
        {
            return direction switch
            {
                Direction.North or Direction.South => pinSquaresVertical,
                Direction.West or Direction.East => pinSquaresHorizontal,
                Direction.NorthEast or Direction.SouthWest => pinSquaresRightDiagonal,
                Direction.NorthWest or Direction.SouthEast => pinSquaresLeftDiagonal,
                _ => default
            };
        }

        private void GenerateSlidingMoves(Square square, Direction startDirection, Direction endDirection)
        {
            var isPinned = pinSquares.Contains(square);

            if (isInCheck && isPinned)
            {
                return;
            }

            for (var direction = startDirection; direction <= endDirection; direction++)
            {
                var distance = square.GetBorderDistance(Board, direction);

                var pinAxis = GetPinAxis(direction);

                for (var i = 1; i <= distance; i++)
                {
                    var targetSquare = square.Translated(Board, direction, i);
                    var targetPiece = Board[targetSquare];

                    if (isPinned && !pinAxis.Contains(targetSquare))
                    {
                        break;
                    }

                    if (!targetPiece.IsEmpty && targetPiece.Color == State.MoveColor)
                    {
                        break;
                    }

                    var isCapturing = !targetPiece.IsEmpty;

                    var isBlockingCheck = checkSquares.Contains(targetSquare);
                    if (isBlockingCheck || !isInCheck)
                    {
                        Moves.Add(new Move(square, targetSquare));
                    }

                    if (isCapturing || isBlockingCheck)
                    {
                        break;
                    }
                }
            }
        }
    }
}
