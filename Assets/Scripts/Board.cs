﻿using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Chess
{
    public struct Board : IDisposable
    {
        public const int Size = 8;
        public const int Area = Size * Size;

        public string Fen => GetFen();

        public Piece this[Square coordinate]
        {
            get => Squares[coordinate.Index];
            set => Squares[coordinate.Index] = value;
        }

        public Piece this[int file, int rank]
        {
            get => this[new Square(file, rank)];
            set => this[new Square(file, rank)] = value;
        }

        public NativeArray<Piece> Squares;

        public NativeArray<Square> Kings;

        public NativeArray<Bitboard> PieceBitboards;

        public NativeArray<Bitboard> ColorBitboards;

        public Bitboard AllPiecesBitboard;
        public Bitboard AlliedOrthogonalSliders;
        public Bitboard AlliedDiagonalSliders;
        public Bitboard EnemyOrthogonalSliders;
        public Bitboard EnemyDiagonalSliders;

        public int TotalPieceCountWithoutPawnsAndKings;

        public NativeArray<PieceSet> Rooks;
        public NativeArray<PieceSet> Bishops;
        public NativeArray<PieceSet> Queens;
        public NativeArray<PieceSet> Knights;
        public NativeArray<PieceSet> Pawns;

        public bool IsWhiteAllied;
        public readonly Color AlliedColor => IsWhiteAllied ? Color.White : Color.Black;
        public readonly Color EnemyColor => IsWhiteAllied ? Color.Black : Color.White;
        public readonly int AlliedColorIndex => IsWhiteAllied ? (int)Color.White : (int)Color.Black;
        public readonly int EnemyColorIndex => IsWhiteAllied ? (int)Color.Black : (int)Color.White;

        public NativeList<ulong> RepetitionPositionHistory;

        public int PlyCount;
        public readonly int FiftyMoveCounter => State.FiftyMoveCounter;

        public State State;
        public readonly ulong ZobristKey => State.ZobristKey;

        public NativeList<Move> AllMoves;

        public NativeList<State> StateHistory;

        public NativeArray<PieceSet> AllPieces;

        public bool HasCachedInCheckValue;

        public NativeArray<int> MoveLimits;
        public NativeArray<int> SquareOffsets;
        public NativeArray<int2> SquareOffsets2D;
        public NativeArray<int> ManhattanDistance;
        public NativeArray<int> CenterManhattanDistance;

        public NativeArray<Bitboard> DirectionRays;
        public NativeArray<Bitboard> AlignMask;

        public NativeArray<Bitboard> KnightMoves;
        public NativeArray<Bitboard> KingMoves;
        public NativeArray<Bitboard> WhitePawnAttacks;
        public NativeArray<Bitboard> BlackPawnAttacks;
        public NativeArray<Bitboard> WhitePassedPawnMask;
        public NativeArray<Bitboard> BlackPassedPawnMask;
        public NativeArray<Bitboard> AdjacentFileMasks;

        public NativeArray<int2> RookDirections;
        public NativeArray<int2> BishopDirections;

        public Magic Magic;

        public Zobrist Zobrist;

        public Board(Allocator allocator)
        {
            Squares = new(Area, allocator);

            Kings = new(2, allocator);

            PieceBitboards = new(Piece.MaxIndex + 1, allocator);
            ColorBitboards = new(2, allocator);

            AllPiecesBitboard = default;
            AlliedOrthogonalSliders = default;
            AlliedDiagonalSliders = default;
            EnemyOrthogonalSliders = default;
            EnemyDiagonalSliders = default;

            TotalPieceCountWithoutPawnsAndKings = default;

            Pawns = new(2, allocator);
            Pawns[0] = new(8, allocator);
            Pawns[1] = new(8, allocator);

            Knights = new(2, allocator);
            Knights[0] = new(10, allocator);
            Knights[1] = new(10, allocator);

            Bishops = new(2, allocator);
            Bishops[0] = new(10, allocator);
            Bishops[1] = new(10, allocator);

            Rooks = new(2, allocator);
            Rooks[0] = new(10, allocator);
            Rooks[1] = new(10, allocator);

            Queens = new(2, allocator);
            Queens[0] = new(9, allocator);
            Queens[1] = new(9, allocator);

            IsWhiteAllied = default;

            RepetitionPositionHistory = new(allocator);

            PlyCount = default;
            State = default;
            AllMoves = new NativeList<Move>(allocator);

            StateHistory = new(allocator);

            MoveLimits = new
            (
                Area * 8,
                allocator,
                NativeArrayOptions.UninitializedMemory
            );

            for (var i = Square.Min.Index; i <= Square.Max.Index; i++)
            {
                var square = new Square(i);
                int file = square.File;
                int rank = square.Rank;

                int northSquareCount = Size - 1 - rank;
                int southSquareCount = rank;
                int eastSquareCount = Size - 1 - file;
                int westSquareCount = file;

                MoveLimits[i + Area * 0] = northSquareCount;
                MoveLimits[i + Area * 1] = southSquareCount;
                MoveLimits[i + Area * 2] = westSquareCount;
                MoveLimits[i + Area * 3] = eastSquareCount;
                MoveLimits[i + Area * 4] = math.min(northSquareCount, westSquareCount);
                MoveLimits[i + Area * 5] = math.min(southSquareCount, eastSquareCount);
                MoveLimits[i + Area * 6] = math.min(northSquareCount, eastSquareCount);
                MoveLimits[i + Area * 7] = math.min(southSquareCount, westSquareCount);
            }

            SquareOffsets = new(8, allocator);
            SquareOffsets[0] = 8;
            SquareOffsets[1] = -8;
            SquareOffsets[2] = -1;
            SquareOffsets[3] = 1;
            SquareOffsets[4] = 7;
            SquareOffsets[5] = -7;
            SquareOffsets[6] = 9;
            SquareOffsets[7] = -9;

            SquareOffsets2D = new(8, allocator);
            SquareOffsets2D[0] = new(0, 1);
            SquareOffsets2D[1] = new(0, -1);
            SquareOffsets2D[2] = new(-1, 0);
            SquareOffsets2D[3] = new(1, 0);
            SquareOffsets2D[4] = new(-1, 1);
            SquareOffsets2D[5] = new(1, -1);
            SquareOffsets2D[6] = new(1, 1);
            SquareOffsets2D[7] = new(-1, -1);

            ManhattanDistance = new(Area * Area, allocator);
            CenterManhattanDistance = new(Area, allocator);

            for (var a = Square.MinIndex; a <= Square.MaxIndex; a++)
            {
                var squareA = new Square(a);
                var coordinateA = squareA.Coordinate;
                var centerDistanceFile = math.max(3 - coordinateA.x, coordinateA.x - 4);
                var centerDistanceRank = math.max(3 - coordinateA.y, coordinateA.y - 4);
                CenterManhattanDistance[a] = centerDistanceFile + centerDistanceRank;

                for (var b = Square.MinIndex; b <= Square.MaxIndex; b++)
                {
                    var squareB = new Square(b);
                    var coordinateB = squareB.Coordinate;
                    var distanceFile = math.abs(coordinateA.x - coordinateB.x);
                    var distanceRank = math.abs(coordinateA.y - coordinateB.y);
                    ManhattanDistance[a * Area + b] = distanceFile + distanceRank;
                }
            }

            AllPieces = new(Piece.MaxIndex + 1, allocator);

            AllPieces[1] = Pawns[0];
            AllPieces[2] = Knights[0];
            AllPieces[3] = Bishops[0];
            AllPieces[4] = Rooks[0];
            AllPieces[5] = Queens[0];
            AllPieces[6] = new(1, allocator);

            AllPieces[7] = Pawns[1];
            AllPieces[8] = Knights[1];
            AllPieces[9] = Bishops[1];
            AllPieces[10] = Rooks[1];
            AllPieces[11] = Queens[1];
            AllPieces[12] = new(1, allocator);

            DirectionRays = new(Direction.Count * Area, allocator);
            for (var direction = Direction.Begin; direction <= Direction.End; direction++)
            {
                for (var square = Square.MinIndex; square <= Square.MaxIndex; square++)
                {
                    for (var i = Square.MinComponent; i <= Square.MaxComponent; i++)
                    {
                        var coordinate = new Square(square).Coordinate + SquareOffsets2D[direction] * i;

                        if (IsCoordinateValid(coordinate))
                        {
                            var targetSquare = new Square(coordinate.x, coordinate.y);
                            DirectionRays[square + Area * direction] |= targetSquare;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            AlignMask = new(Area * Area, allocator);
            for (var squareA = Square.MinIndex; squareA <= Square.MaxIndex; squareA++)
            {
                for (var squareB = Square.MinIndex; squareB <= Square.MaxIndex; squareB++)
                {
                    var cA = new Square(squareA).Coordinate;
                    var cB = new Square(squareB).Coordinate;
                    var delta = cB - cA;
                    var dir = new int2(math.sign(delta.x), math.sign(delta.y));

                    for (var i = -8; i < 8; i++)
                    {
                        var coordinate = cA + dir * i;
                        if (IsCoordinateValid(coordinate))
                        {
                            AlignMask[squareA * Area + squareB] |= new Square(coordinate);
                        }
                    }
                }
            }

            RookDirections = new NativeArray<int2>(4, allocator);
            RookDirections[0] = new(-1, 0);
            RookDirections[1] = new(0, 1);
            RookDirections[2] = new(1, 0);
            RookDirections[3] = new(0, -1);

            BishopDirections = new NativeArray<int2>(4, allocator);
            BishopDirections[0] = new(-1, -1);
            BishopDirections[1] = new(-1, 1);
            BishopDirections[2] = new(1, 1);
            BishopDirections[3] = new(1, -1);

            KnightMoves = new(Area, allocator);
            KingMoves = new(Area, allocator);
            WhitePawnAttacks = new(Area, allocator);
            BlackPawnAttacks = new(Area, allocator);

            WhitePassedPawnMask = new(Area, allocator);
            BlackPassedPawnMask = new(Area, allocator);

            for (var i = Square.MinIndex; i <= Square.MaxIndex; i++)
            {
                var square = new Square(i);
                var file = square.File;
                var rank = square.Rank;

                var adjacentFiles = Bitboard.FileA << math.max(0, file - 1) | Bitboard.FileA << math.min(7, file + 1);

                var whiteForwardMask = (Bitboard)~(ulong.MaxValue >> (64 - 8 * (rank + 1)));
                var blackForwardMask = (Bitboard)((1ul << 8 * rank) - 1);

                WhitePassedPawnMask[i] = (Bitboard.FileA << file | adjacentFiles) & whiteForwardMask;
                BlackPassedPawnMask[i] = (Bitboard.FileA << file | adjacentFiles) & blackForwardMask;
            }

            AdjacentFileMasks = new(Size, allocator);

            for (var i = 0; i < Size; i++)
            {
                var left = i > 0 ? Bitboard.FileA << (i - 1) : Bitboard.Empty;
                var right = i < 7 ? Bitboard.FileA << (i + 1) : Bitboard.Empty;
                AdjacentFileMasks[i] = left | right;
            }

            HasCachedInCheckValue = default;

            Zobrist = new(allocator);
            Magic = new(allocator);

            var knightJumps = new NativeArray<int2>(8, Allocator.Temp);
            knightJumps[0] = new(-2, -1);
            knightJumps[1] = new(-2, 1);
            knightJumps[2] = new(-1, 2);
            knightJumps[3] = new(1, 2);
            knightJumps[4] = new(2, 1);
            knightJumps[5] = new(2, -1);
            knightJumps[6] = new(1, -2);
            knightJumps[7] = new(-1, -2);

            for (var rank = Square.MinComponent; rank <= Square.MaxComponent; rank++)
            {
                for (var file = Square.MinComponent; file <= Square.MaxComponent; file++)
                {
                    InitializeSquare(knightJumps, file, rank);
                }
            }

            knightJumps.Dispose();
        }

        private void MovePiece(Piece piece, Square from, Square to)
        {
            PieceBitboards[piece.Index] ^= Bitboard.Empty.With(from).With(to);
            ColorBitboards[AlliedColorIndex] ^= Bitboard.Empty.With(from).With(to);

            AllPieces[piece.Index].Move(from, to);

            this[from] = Piece.Empty;
            this[to] = piece;
        }

        public void MakeMove(Move move, bool inSearch = false)
        {
            var from = move.From;
            var to = move.To;
            var flag = move.Flag;

            var isPromotion = move.IsPromotion;
            var isEnPassant = flag == MoveFlag.EnPassant;

            var movedPiece = this[from];
            var movedFigure = movedPiece.Figure;
            var capturedPiece = isEnPassant ? new Piece(Figure.Pawn, EnemyColor) : this[to];
            var capturedFigure = capturedPiece.Figure;

            var prevCastleState = State.CastlingRights;
            var prevEnPassantFile = State.EnPassantFile;
            var newZobristKey = State.ZobristKey;
            var newCastlingRights = State.CastlingRights;
            var newEnPassantFile = 0;

            MovePiece(movedPiece, from, to);

            if (!capturedPiece.IsEmpty)
            {
                var captureSquare = to;

                if (isEnPassant)
                {
                    captureSquare = to + (IsWhiteAllied ? -8 : 8);
                    this[captureSquare] = Piece.Empty;
                }

                if (capturedFigure != Figure.Pawn)
                {
                    TotalPieceCountWithoutPawnsAndKings--;
                }

                AllPieces[capturedPiece.Index].Remove(captureSquare);
                PieceBitboards[capturedPiece.Index] ^= captureSquare;
                ColorBitboards[EnemyColorIndex] ^= captureSquare;

                newZobristKey ^= Zobrist.Pieces[capturedPiece.Index * Area + captureSquare.Index];
            }

            if (movedFigure == Figure.King)
            {
                Kings[AlliedColorIndex] = to;
                newCastlingRights &= IsWhiteAllied ? 0b1100 : 0b0011;

                if (flag == MoveFlag.Castling)
                {
                    var rookPiece = new Piece(Figure.Rook, AlliedColor);
                    var kingside = to == Square.G1 || to == Square.G8;
                    var castlingRookFrom = kingside ? to + 1 : to - 2;
                    var castlingRookTo = kingside ? to - 1 : to + 1;

                    PieceBitboards[rookPiece.Index] ^= Bitboard.Empty.With(castlingRookFrom).With(castlingRookTo);
                    ColorBitboards[AlliedColorIndex] ^= Bitboard.Empty.With(castlingRookFrom).With(castlingRookTo);
                    AllPieces[rookPiece.Index].Move(castlingRookFrom, castlingRookTo);
                    this[castlingRookFrom] = Piece.Empty;
                    this[castlingRookTo] = new Piece(Figure.Rook, AlliedColor);

                    newZobristKey ^= Zobrist.Pieces[rookPiece.Index * Area + castlingRookFrom.Index];
                    newZobristKey ^= Zobrist.Pieces[rookPiece.Index * Area + castlingRookTo.Index];
                }
            }

            if (isPromotion)
            {
                TotalPieceCountWithoutPawnsAndKings++;

                var promotionFigure = flag switch
                {
                    MoveFlag.KnightPromotion => Figure.Knight,
                    MoveFlag.BishopPromotion => Figure.Bishop,
                    MoveFlag.RookPromotion => Figure.Rook,
                    MoveFlag.QueenPromotion => Figure.Queen,
                    _ => Figure.None,
                };

                var promotionPiece = new Piece(promotionFigure, AlliedColor);

                PieceBitboards[movedPiece.Index] ^= to;
                PieceBitboards[promotionPiece.Index] ^= to;

                AllPieces[movedPiece.Index].Remove(to);
                AllPieces[promotionPiece.Index].Add(to);

                this[to] = promotionPiece;
            }

            if (flag == MoveFlag.DoubleForwardPawn)
            {
                var file = from.File + 1;
                newEnPassantFile = file;
                newZobristKey ^= Zobrist.EnPassantFile[file];
            }

            if (prevCastleState != 0)
            {
                if (to == Square.H1 || from == Square.H1)
                {
                    newCastlingRights &= State.ClearWhiteKingsideMask;
                }
                else if (to == Square.A1 || from == Square.A1)
                {
                    newCastlingRights &= State.ClearWhiteQueensideMask;
                }
                if (to == Square.H8 || from == Square.H8)
                {
                    newCastlingRights &= State.ClearBlackKingsideMask;
                }
                else if (to == Square.A8 || from == Square.A8)
                {
                    newCastlingRights &= State.ClearBlackQueensideMask;
                }
            }

            newZobristKey ^= Zobrist.SideToMove;
            newZobristKey ^= Zobrist.Pieces[movedPiece.Index * Area + from.Index];
            newZobristKey ^= Zobrist.Pieces[this[to].Index * Area + to.Index];
            newZobristKey ^= Zobrist.EnPassantFile[prevEnPassantFile];

            if (newCastlingRights != prevCastleState)
            {
                newZobristKey ^= Zobrist.CastlingRights[prevCastleState];
                newZobristKey ^= Zobrist.CastlingRights[newCastlingRights];
            }

            IsWhiteAllied = !IsWhiteAllied;

            PlyCount++;
            var newFiftyMoveCounter = State.FiftyMoveCounter + 1;

            AllPiecesBitboard = ColorBitboards[AlliedColorIndex] | ColorBitboards[EnemyColorIndex];
            UpdateSliderBitboards();

            if (movedFigure == Figure.Pawn || capturedFigure != Figure.None)
            {
                if (!inSearch)
                {
                    RepetitionPositionHistory.Clear();
                }

                newFiftyMoveCounter = 0;
            }

            State.CapturedFigure = capturedFigure;
            State.EnPassantFile = newEnPassantFile;
            State.CastlingRights = newCastlingRights;
            State.FiftyMoveCounter = newFiftyMoveCounter;
            State.ZobristKey = newZobristKey;
            StateHistory.Add(State);
            HasCachedInCheckValue = false;

            if (!inSearch)
            {
                RepetitionPositionHistory.Add(State.ZobristKey);
                AllMoves.Add(move);
            }
        }

        void UpdateSliderBitboards()
        {
            var alliedRook = new Piece(Figure.Rook, AlliedColor);
            var alliedQueen = new Piece(Figure.Queen, AlliedColor);
            var alliedBishop = new Piece(Figure.Bishop, AlliedColor);
            AlliedOrthogonalSliders = PieceBitboards[alliedRook.Index] | PieceBitboards[alliedQueen.Index];
            AlliedDiagonalSliders = PieceBitboards[alliedBishop.Index] | PieceBitboards[alliedQueen.Index];

            var enemyRook = new Piece(Figure.Rook, EnemyColor);
            var enemyQueen = new Piece(Figure.Queen, EnemyColor);
            var enemyBishop = new Piece(Figure.Bishop, EnemyColor);
            EnemyOrthogonalSliders = PieceBitboards[enemyRook.Index] | PieceBitboards[enemyQueen.Index];
            EnemyDiagonalSliders = PieceBitboards[enemyBishop.Index] | PieceBitboards[enemyQueen.Index];
        }

        public void UnmakeMove(Move move, bool inSearch = false)
        {
            IsWhiteAllied = !IsWhiteAllied;

            var undoingWhiteMove = IsWhiteAllied;

            var from = move.From;
            var to = move.To;
            var flag = move.Flag;

            var undoingPromotion = move.IsPromotion;
            var undoingEnPassant = flag == MoveFlag.EnPassant;
            var undoingCapture = State.CapturedFigure != Figure.None;

            var movedPiece = undoingPromotion ? new Piece(Figure.Pawn, AlliedColor) : this[to];
            var movedFigure = movedPiece.Figure;
            var capturedPieceType = State.CapturedFigure;

            if (undoingPromotion)
            {
                var promotedPiece = this[to];
                var pawnPiece = new Piece(Figure.Pawn, AlliedColor);
                TotalPieceCountWithoutPawnsAndKings--;

                AllPieces[promotedPiece.Index].Remove(to);
                AllPieces[movedPiece.Index].Add(to);
                PieceBitboards[promotedPiece.Index] ^= to;
                PieceBitboards[pawnPiece.Index] ^= to;
            }

            MovePiece(movedPiece, to, from);

            if (undoingCapture)
            {
                var captureSquare = to;
                var capturedPiece = new Piece(capturedPieceType, EnemyColor);

                if (undoingEnPassant)
                {
                    captureSquare = to + (undoingWhiteMove ? -8 : 8);
                }

                if (capturedPieceType != Figure.Pawn)
                {
                    TotalPieceCountWithoutPawnsAndKings++;
                }

                PieceBitboards[capturedPiece.Index] ^= captureSquare;
                ColorBitboards[EnemyColorIndex] ^= captureSquare;
                AllPieces[capturedPiece.Index].Add(captureSquare);
                this[captureSquare] = capturedPiece;
            }

            if (movedFigure == Figure.King)
            {
                Kings[AlliedColorIndex] = from;

                if (flag == MoveFlag.Castling)
                {
                    var rookPiece = new Piece(Figure.Rook, AlliedColor);
                    var kingside = to == Square.G1 || to == Square.G8;
                    var rookSquareBeforeCastling = kingside ? to + 1 : to - 2;
                    var rookSquareAfterCastling = kingside ? to - 1 : to + 1;

                    PieceBitboards[rookPiece.Index] ^= Bitboard.Empty.With(rookSquareAfterCastling).With(rookSquareBeforeCastling);
                    ColorBitboards[AlliedColorIndex] ^= Bitboard.Empty.With(rookSquareAfterCastling).With(rookSquareBeforeCastling);

                    this[rookSquareAfterCastling] = Piece.Empty;
                    this[rookSquareBeforeCastling] = rookPiece;

                    AllPieces[rookPiece.Index].Move(rookSquareAfterCastling, rookSquareBeforeCastling);
                }
            }

            AllPiecesBitboard = ColorBitboards[AlliedColorIndex] | ColorBitboards[EnemyColorIndex];
            UpdateSliderBitboards();

            if (!inSearch && RepetitionPositionHistory.Length > 0)
            {
                RepetitionPositionHistory.RemoveAt(RepetitionPositionHistory.Length - 1);
            }

            if (!inSearch)
            {
                AllMoves.RemoveAt(AllMoves.Length - 1);
            }

            StateHistory.RemoveAt(StateHistory.Length - 1);
            State = StateHistory[^1];
            PlyCount--;
            HasCachedInCheckValue = false;
        }

        public void Load(string fen)
        {
            var parser = new Fen(fen, Allocator.Temp);
            Load(parser);
            parser.Dispose();
        }

        private void InitializeSquare(NativeArray<int2> knightJumps, int file, int rank)
        {
            var square = new Square(file, rank);

            for (var i = 0; i < 4; i++)
            {
                var orthogonalFile = file + RookDirections[i].x;
                var orthogonalRank = rank + RookDirections[i].y;
                var diagonalFile = file + BishopDirections[i].x;
                var diagonalRank = rank + BishopDirections[i].y;

                if (IsCoordinateValid(orthogonalFile, orthogonalRank))
                {
                    KingMoves[square.Index] |= new Square(orthogonalFile, orthogonalRank);
                }

                if (IsCoordinateValid(diagonalFile, diagonalRank))
                {
                    KingMoves[square.Index] |= new Square(diagonalFile, diagonalRank);
                }
            }

            for (var i = 0; i < knightJumps.Length; i++)
            {
                var jumpFile = file + knightJumps[i].x;
                var jumpRank = rank + knightJumps[i].y;

                if (IsCoordinateValid(jumpFile, jumpRank))
                {
                    KnightMoves[square.Index] |= new Square(jumpFile, jumpRank);
                }
            }

            if (IsCoordinateValid(file + 1, rank + 1))
            {
                WhitePawnAttacks[square.Index] |= new Square(file + 1, rank + 1);
            }

            if (IsCoordinateValid(file - 1, rank + 1))
            {
                WhitePawnAttacks[square.Index] |= new Square(file - 1, rank + 1);
            }

            if (IsCoordinateValid(file + 1, rank - 1))
            {
                BlackPawnAttacks[square.Index] |= new Square(file + 1, rank - 1);
            }

            if (IsCoordinateValid(file - 1, rank - 1))
            {
                BlackPawnAttacks[square.Index] |= new Square(file - 1, rank - 1);
            }
        }

        public int GetCenterManhattanDistance(Square square)
        {
            return CenterManhattanDistance[square.Index];
        }

        public int GetManhattanDistance(Square from, Square to)
        {
            return ManhattanDistance[from.Index * Area + to.Index];
        }

        public static bool IsCoordinateValid(int file, int rank)
        {
            return file >= Square.MinComponent
                && file <= Square.MaxComponent
                && rank >= Square.MinComponent
                && rank <= Square.MaxComponent;
        }

        public static bool IsCoordinateValid(int2 coordinate) => IsCoordinateValid(coordinate.x, coordinate.y);

        public Bitboard GetRay(Square square, Direction direction)
        {
            return DirectionRays[square.Index + Area * direction];
        }

        public Bitboard GetAlignMask(Square from, Square to)
        {
            return AlignMask[from.Index * Area + to.Index];
        }

        public Bitboard GetPawnAttacks(Square square, Color color)
        {
            return color switch
            {
                Color.Black => BlackPawnAttacks[square.Index],
                Color.White => WhitePawnAttacks[square.Index],
                _ => default
            };
        }

        public readonly Bitboard GetPawnAttacks(Bitboard pawns, bool isWhite)
        {
            if (isWhite)
            {
                return ((pawns << 9) & ~Bitboard.FileA) | ((pawns << 7) & ~Bitboard.FileH);
            }
            else
            {
                return ((pawns >> 7) & ~Bitboard.FileA) | ((pawns >> 9) & ~Bitboard.FileH);
            }
        }

        public readonly Bitboard GetSliderAttacks(Square square, Bitboard blockers, bool isOrthogonal)
        {
            return isOrthogonal ? GetRookAttacks(square, blockers) : GetBishopAttacks(square, blockers);
        }

        public Bitboard GetRookMask(Square square)
        {
            return Magic.GetRookMask(square);
        }

        public Bitboard GetBishopMask(Square square)
        {
            return Magic.GetBishopMask(square);
        }

        public readonly Bitboard GetRookAttacks(Square square, Bitboard blockers)
        {
            return Magic.GetRookAttacks(square, blockers);
        }

        public readonly Bitboard GetBishopAttacks(Square square, Bitboard blockers)
        {
            return Magic.GetBishopAttacks(square, blockers);
        }

        public int GetBorderDistance(Square square, Direction direction)
        {
            return MoveLimits[(int)square + Area * (int)direction];
        }

        public Square GetTranslatedSquare(Square square, Direction direction, int distance = 1)
        {
            return square + SquareOffsets[(int)direction] * distance;
        }

        public void Load(in Fen fen)
        {
            for (var i = Square.Min.Index; i <= Square.Max.Index; i++)
            {
                var square = new Square(i);
                var piece = fen.Squares[i];
                var figure = piece.Figure;
                var color = piece.Color;
                this[square] = piece;

                if (figure != Figure.None)
                {
                    PieceBitboards[piece.Index] |= square;
                    ColorBitboards[(int)color] |= square;

                    if (figure == Figure.King)
                    {
                        Kings[(int)color] = square;
                    }
                    else
                    {
                        AllPieces[piece.Index].Add(square);
                    }
                }
            }

            IsWhiteAllied = fen.IsWhiteAllied;

            AllPiecesBitboard = ColorBitboards[AlliedColorIndex] | ColorBitboards[EnemyColorIndex];
            UpdateSliderBitboards();

            var whiteCastle = (fen.WhiteCastleKingside ? 1 << 0 : 0) | (fen.WhiteCastleQueenside ? 1 << 1 : 0);
            var blackCastle = (fen.BlackCastleKingside ? 1 << 2 : 0) | (fen.BlackCastleQueenside ? 1 << 3 : 0);
            State.CastlingRights = whiteCastle | blackCastle;

            PlyCount = (fen.MoveCount - 1) * 2 + (IsWhiteAllied ? 0 : 1);

            State.FiftyMoveCounter = fen.FiftyMovePlyCount;
            State.CapturedFigure = Figure.None;
            State.EnPassantFile = fen.EnPassantFile;

            State.ZobristKey = Zobrist.CalculateKey(this);

            RepetitionPositionHistory.Add(State.ZobristKey);
            StateHistory.Add(State);
        }

        public string GetFen(bool includeEnPassant = true)
        {
            var fen = string.Empty;

            for (var rank = Size - 1; rank >= 0; rank--)
            {
                var numEmptyFiles = 0;
                for (var file = 0; file < Size; file++)
                {
                    var square = new Square(file, rank);
                    var piece = this[square];

                    if (!piece.IsEmpty)
                    {
                        if (numEmptyFiles != 0)
                        {
                            fen += numEmptyFiles;
                            numEmptyFiles = 0;
                        }

                        var isBlack = piece.Color == Color.Black;
                        var pieceFigure = piece.Figure;
                        var pieceChar = ' ';

                        switch (pieceFigure)
                        {
                            case Figure.Rook:
                                pieceChar = 'R';
                                break;
                            case Figure.Knight:
                                pieceChar = 'N';
                                break;
                            case Figure.Bishop:
                                pieceChar = 'B';
                                break;
                            case Figure.Queen:
                                pieceChar = 'Q';
                                break;
                            case Figure.King:
                                pieceChar = 'K';
                                break;
                            case Figure.Pawn:
                                pieceChar = 'P';
                                break;
                        }

                        fen += isBlack ? pieceChar.ToString().ToLower() : pieceChar.ToString();
                    }
                    else
                    {
                        numEmptyFiles++;
                    }

                }
                if (numEmptyFiles != 0)
                {
                    fen += numEmptyFiles;
                }
                if (rank != 0)
                {
                    fen += '/';
                }
            }

            fen += ' ';
            fen += IsWhiteAllied ? 'w' : 'b';

            var whiteKingside = (State.CastlingRights & 1) == 1;
            var whiteQueenside = (State.CastlingRights >> 1 & 1) == 1;
            var blackKingside = (State.CastlingRights >> 2 & 1) == 1;
            var blackQueenside = (State.CastlingRights >> 3 & 1) == 1;
            fen += ' ';
            fen += whiteKingside ? "K" : "";
            fen += whiteQueenside ? "Q" : "";
            fen += blackKingside ? "k" : "";
            fen += blackQueenside ? "q" : "";
            fen += (State.CastlingRights == 0) ? "-" : "";

            fen += ' ';
            var epFileIndex = State.EnPassantFile - 1;
            var epRankIndex = IsWhiteAllied ? 5 : 2;

            var isEnPassant = includeEnPassant && epFileIndex != -1;
            if (isEnPassant)
            {
                fen += new Square(epFileIndex, epRankIndex).ToString();
            }
            else
            {
                fen += '-';
            }

            fen += ' ';
            fen += State.FiftyMoveCounter;

            fen += ' ';
            fen += PlyCount / 2 + 1;

            return fen;
        }

        public void Dispose()
        {
            Squares.Dispose();
            Kings.Dispose();

            PieceBitboards.Dispose();
            ColorBitboards.Dispose();

            DirectionRays.Dispose();
            AlignMask.Dispose();

            Pawns[0].Dispose();
            Pawns[1].Dispose();
            Pawns.Dispose();

            Knights[0].Dispose();
            Knights[1].Dispose();
            Knights.Dispose();

            Bishops[0].Dispose();
            Bishops[1].Dispose();
            Bishops.Dispose();

            Rooks[0].Dispose();
            Rooks[1].Dispose();
            Rooks.Dispose();

            Queens[0].Dispose();
            Queens[1].Dispose();
            Queens.Dispose();

            AllPieces[6].Dispose();
            AllPieces[12].Dispose();
            AllPieces.Dispose();

            AllMoves.Dispose();

            Zobrist.Dispose();

            KnightMoves.Dispose();
            KingMoves.Dispose();
            WhitePawnAttacks.Dispose();
            BlackPawnAttacks.Dispose();
            WhitePassedPawnMask.Dispose();
            BlackPassedPawnMask.Dispose();

            AdjacentFileMasks.Dispose();

            MoveLimits.Dispose();
            SquareOffsets.Dispose();
            SquareOffsets2D.Dispose();
            ManhattanDistance.Dispose();
            CenterManhattanDistance.Dispose();

            RookDirections.Dispose();
            BishopDirections.Dispose();

            Magic.Dispose();
        }
    }
}
