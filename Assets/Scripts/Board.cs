using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Chess
{
    public struct Board : IDisposable
    {
        public const int Size = 8;
        public const int Area = Size * Size;

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

        public NativeArray<NativeHashSet<Square>> Rooks;
        public NativeArray<NativeHashSet<Square>> Bishops;
        public NativeArray<NativeHashSet<Square>> Queens;
        public NativeArray<NativeHashSet<Square>> Knights;
        public NativeArray<NativeHashSet<Square>> Pawns;

        public bool IsWhiteAllied;
        public Color AlliedColor => IsWhiteAllied ? Color.White : Color.Black;
        public Color EnemyColor => IsWhiteAllied ? Color.Black : Color.White;
        public int AlliedColorIndex => IsWhiteAllied ? (int)Color.White : (int)Color.Black;
        public int EnemyColorIndex => IsWhiteAllied ? (int)Color.Black : (int)Color.White;

        public NativeList<ulong> RepetitionPositionHistory;

        public int PlyCount;
        public int FiftyMoveCounter => State.FiftyMoveCounter;

        public State State;
        public ulong ZobristKey => State.ZobristKey;

        public NativeList<Move> Moves;
        public NativeList<Move> AllMoves;

        public NativeList<State> StateHistory;

        public NativeArray<NativeHashSet<Square>> AllPieces;

        public bool HasCachedInCheckValue;

        public NativeArray<int> MoveLimits;
        public NativeArray<int> SquareOffsets;
        public NativeArray<int2> SquareOffsets2D;

        public NativeArray<Bitboard> DirectionRays;
        public NativeArray<Bitboard> AlignMask;

        public NativeArray<Bitboard> KnightMoves;
        public NativeArray<Bitboard> KingMoves;
        public NativeArray<Bitboard> WhitePawnAttacks;
        public NativeArray<Bitboard> BlackPawnAttacks;

        public NativeArray<Bitboard> MagicRookMasks;
        public NativeArray<Bitboard> MagicBishopMasks;
        public NativeArray<NativeArray<Bitboard>> MagicRookAttacks;
        public NativeArray<NativeArray<Bitboard>> MagicBishopAttacks;
        public NativeArray<int> MagicRookShifts;
        public NativeArray<int> MagicBishopShifts;
        public NativeArray<ulong> RookMagics;
        public NativeArray<ulong> BishopMagics;

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
            Knights[0] = new(8, allocator);
            Knights[1] = new(8, allocator);

            Bishops = new(2, allocator);
            Bishops[0] = new(8, allocator);
            Bishops[1] = new(8, allocator);

            Rooks = new(2, allocator);
            Rooks[0] = new(8, allocator);
            Rooks[1] = new(8, allocator);

            Queens = new(2, allocator);
            Queens[0] = new(8, allocator);
            Queens[1] = new(8, allocator);

            IsWhiteAllied = default;

            RepetitionPositionHistory = new(allocator);

            PlyCount = default;
            State = default;
            Moves = new NativeList<Move>(allocator);
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

            AllPieces = new(Piece.MaxIndex + 1, allocator);

            AllPieces[0] = new(0, allocator);

            AllPieces[1] = Pawns[0];
            AllPieces[2] = Knights[0];
            AllPieces[3] = Bishops[0];
            AllPieces[4] = Rooks[0];
            AllPieces[5] = Queens[0];
            AllPieces[6] = new(0, allocator);

            AllPieces[7] = Pawns[1];
            AllPieces[8] = Knights[1];
            AllPieces[9] = Bishops[1];
            AllPieces[10] = Rooks[1];
            AllPieces[11] = Queens[1];
            AllPieces[12] = new(0, allocator);

            DirectionRays = new(Direction.Count * Area, allocator);
            for (var direction = Direction.Begin; direction <= Direction.End; direction++)
            {
                for (var square = Square.Min.Index; square <= Square.Max.Index; square++)
                {
                    for (var i = Square.MinComponent; i <= Square.MaxComponent; i++)
                    {
                        var coordinate = square + SquareOffsets2D[direction] * i;

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
            for (var squareA = Square.Min.Index; squareA <= Square.Max.Index; squareA++)
            {
                for (var squareB = Square.Min.Index; squareB <= Square.Max.Index; squareB++)
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

            var orthogonalDirections = new NativeArray<int2>(4, Allocator.Temp);
            orthogonalDirections[0] = new(-1, 0);
            orthogonalDirections[1] = new(0, 1);
            orthogonalDirections[2] = new(1, 0);
            orthogonalDirections[3] = new(0, -1);

            var diagonalDirections = new NativeArray<int2>(4, Allocator.Temp);
            diagonalDirections[0] = new(-1, -1);
            diagonalDirections[1] = new(-1, 1);
            diagonalDirections[2] = new(1, 1);
            diagonalDirections[3] = new(1, -1);

            KnightMoves = new(Area, allocator);
            KingMoves = new(Area, allocator);
            WhitePawnAttacks = new(Area, allocator);
            BlackPawnAttacks = new(Area, allocator);

            MagicRookMasks = new(Area, allocator);
            MagicBishopMasks = new(Area, allocator);
            MagicRookAttacks = new(Area, allocator);
            MagicBishopAttacks = new(Area, allocator);

            MagicRookShifts = new(new int[] { 52, 52, 52, 52, 52, 52, 52, 52, 53, 53, 53, 54, 53, 53, 54, 53, 53, 54, 54, 54, 53, 53, 54, 53, 53, 54, 53, 53, 54, 54, 54, 53, 52, 54, 53, 53, 53, 53, 54, 53, 52, 53, 54, 54, 53, 53, 54, 53, 53, 54, 54, 54, 53, 53, 54, 53, 52, 53, 53, 53, 53, 53, 53, 52 }, allocator);
            MagicBishopShifts = new(new int[] { 58, 60, 59, 59, 59, 59, 60, 58, 60, 59, 59, 59, 59, 59, 59, 60, 59, 59, 57, 57, 57, 57, 59, 59, 59, 59, 57, 55, 55, 57, 59, 59, 59, 59, 57, 55, 55, 57, 59, 59, 59, 59, 57, 57, 57, 57, 59, 59, 60, 60, 59, 59, 59, 59, 60, 60, 58, 60, 59, 59, 59, 59, 59, 58 }, allocator);

            RookMagics = new(new ulong[] { 468374916371625120, 18428729537625841661, 2531023729696186408, 6093370314119450896, 13830552789156493815, 16134110446239088507, 12677615322350354425, 5404321144167858432, 2111097758984580, 18428720740584907710, 17293734603602787839, 4938760079889530922, 7699325603589095390, 9078693890218258431, 578149610753690728, 9496543503900033792, 1155209038552629657, 9224076274589515780, 1835781998207181184, 509120063316431138, 16634043024132535807, 18446673631917146111, 9623686630121410312, 4648737361302392899, 738591182849868645, 1732936432546219272, 2400543327507449856, 5188164365601475096, 10414575345181196316, 1162492212166789136, 9396848738060210946, 622413200109881612, 7998357718131801918, 7719627227008073923, 16181433497662382080, 18441958655457754079, 1267153596645440, 18446726464209379263, 1214021438038606600, 4650128814733526084, 9656144899867951104, 18444421868610287615, 3695311799139303489, 10597006226145476632, 18436046904206950398, 18446726472933277663, 3458977943764860944, 39125045590687766, 9227453435446560384, 6476955465732358656, 1270314852531077632, 2882448553461416064, 11547238928203796481, 1856618300822323264, 2573991788166144, 4936544992551831040, 13690941749405253631, 15852669863439351807, 18302628748190527413, 12682135449552027479, 13830554446930287982, 18302628782487371519, 7924083509981736956, 4734295326018586370 }, allocator);
            BishopMagics = new(new ulong[] { 16509839532542417919, 14391803910955204223, 1848771770702627364, 347925068195328958, 5189277761285652493, 3750937732777063343, 18429848470517967340, 17870072066711748607, 16715520087474960373, 2459353627279607168, 7061705824611107232, 8089129053103260512, 7414579821471224013, 9520647030890121554, 17142940634164625405, 9187037984654475102, 4933695867036173873, 3035992416931960321, 15052160563071165696, 5876081268917084809, 1153484746652717320, 6365855841584713735, 2463646859659644933, 1453259901463176960, 9808859429721908488, 2829141021535244552, 576619101540319252, 5804014844877275314, 4774660099383771136, 328785038479458864, 2360590652863023124, 569550314443282, 17563974527758635567, 11698101887533589556, 5764964460729992192, 6953579832080335136, 1318441160687747328, 8090717009753444376, 16751172641200572929, 5558033503209157252, 17100156536247493656, 7899286223048400564, 4845135427956654145, 2368485888099072, 2399033289953272320, 6976678428284034058, 3134241565013966284, 8661609558376259840, 17275805361393991679, 15391050065516657151, 11529206229534274423, 9876416274250600448, 16432792402597134585, 11975705497012863580, 11457135419348969979, 9763749252098620046, 16960553411078512574, 15563877356819111679, 14994736884583272463, 9441297368950544394, 14537646123432199168, 9888547162215157388, 18140215579194907366, 18374682062228545019 }, allocator);

            HasCachedInCheckValue = default;

            Zobrist = new(allocator);

            for (var i = Square.Min.Index; i <= Square.Max.Index; i++)
            {
                var square = new Square(i);
                MagicRookMasks[i] = CreateMagicMovementMask(orthogonalDirections, diagonalDirections, square, true);
                MagicBishopMasks[i] = CreateMagicMovementMask(orthogonalDirections, diagonalDirections, square, false);
            }

            for (var i = Square.Min.Index; i <= Square.Max.Index; i++)
            {
                var square = new Square(i);
                MagicRookAttacks[i] = CreateTable(orthogonalDirections, diagonalDirections, square, true, RookMagics[i], MagicRookShifts[i], allocator);
                MagicBishopAttacks[i] = CreateTable(orthogonalDirections, diagonalDirections, square, false, BishopMagics[i], MagicBishopShifts[i], allocator);
            }

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
                    InitializeSquare(orthogonalDirections, diagonalDirections, knightJumps, file, rank);
                }
            }

            orthogonalDirections.Dispose();
            diagonalDirections.Dispose();
            knightJumps.Dispose();
        }

        private void MovePiece(Piece piece, Square from, Square to)
        {
            PieceBitboards[piece.Index] ^= Bitboard.Empty.With(from).With(to);
            ColorBitboards[AlliedColorIndex] ^= Bitboard.Empty.With(from).With(to);

            AllPieces[piece.Index].Remove(from);
            AllPieces[piece.Index].Add(to);

            this[from] = Piece.Empty;
            this[to] = piece;
        }

        public void MakeMove(Move move, bool inSearch = false)
        {
            // Get info about move
            var from = move.From;
            var to = move.To;
            var isPromotion = (move.Flags & MoveFlags.Promotion) != MoveFlags.None;
            var isEnPassant = (move.Flags & MoveFlags.EnPassant) != MoveFlags.None;

            var movedPiece = this[from];
            var movedPieceType = movedPiece.Figure;
            var capturedPiece = isEnPassant ? new Piece(Figure.Pawn, EnemyColor) : this[to];
            var capturedPieceType = capturedPiece.Figure;

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
                if (capturedPieceType != Figure.Pawn)
                {
                    TotalPieceCountWithoutPawnsAndKings--;
                }

                AllPieces[capturedPiece.Index].Remove(captureSquare);
                PieceBitboards[capturedPiece.Index] ^= captureSquare;
                ColorBitboards[EnemyColorIndex] ^= captureSquare;

                newZobristKey ^= Zobrist.Pieces[capturedPiece.Index * Area + captureSquare.Index];
            }

            if (movedPieceType == Figure.King)
            {
                Kings[AlliedColorIndex] = to;
                newCastlingRights &= IsWhiteAllied ? 0b1100 : 0b0011;

                if ((move.Flags & MoveFlags.Castling) != MoveFlags.None)
                {
                    var rookPiece = new Piece(Figure.Rook, AlliedColor);
                    var kingside = to == Square.G1 || to == Square.G8;
                    var castlingRookFrom = kingside ? to + 1 : to - 2;
                    var castlingRookTo = kingside ? to - 1 : to + 1;

                    PieceBitboards[rookPiece.Index] ^= Bitboard.Empty.With(castlingRookFrom).With(castlingRookTo);
                    ColorBitboards[AlliedColorIndex] ^= Bitboard.Empty.With(castlingRookFrom).With(castlingRookTo);
                    AllPieces[rookPiece.Index].Remove(castlingRookFrom);
                    AllPieces[rookPiece.Index].Add(castlingRookTo);
                    this[castlingRookFrom] = Piece.Empty;
                    this[castlingRookTo] = new Piece(Figure.Rook, AlliedColor);

                    newZobristKey ^= Zobrist.Pieces[rookPiece.Index * Area + castlingRookFrom.Index];
                    newZobristKey ^= Zobrist.Pieces[rookPiece.Index * Area + castlingRookTo.Index];
                }
            }

            if (isPromotion)
            {
                TotalPieceCountWithoutPawnsAndKings++;

                var promotionPieceType = Figure.None;

                if ((move.Flags & MoveFlags.QueenPromotion) != MoveFlags.None)
                {
                    promotionPieceType = Figure.Queen;
                }
                else if ((move.Flags & MoveFlags.RookPromotion) != MoveFlags.None)
                {
                    promotionPieceType = Figure.Rook;
                }
                else if ((move.Flags & MoveFlags.KnightPromotion) != MoveFlags.None)
                {
                    promotionPieceType = Figure.Knight;
                }
                else if ((move.Flags & MoveFlags.BishopPromotion) != MoveFlags.None)
                {
                    promotionPieceType = Figure.Bishop;
                }

                var promotionPiece = new Piece(promotionPieceType, AlliedColor);

                PieceBitboards[movedPiece.Index] ^= to;
                PieceBitboards[promotionPiece.Index] ^= to;

                AllPieces[movedPiece.Index].Remove(to);
                AllPieces[promotionPiece.Index].Add(to);

                this[to] = promotionPiece;
            }

            if ((move.Flags & MoveFlags.DoublePawnMove) != MoveFlags.None)
            {
                var file = from.File;
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

            if (movedPieceType == Figure.Pawn || capturedPieceType != Figure.None)
            {
                if (!inSearch)
                {
                    RepetitionPositionHistory.Clear();
                }

                newFiftyMoveCounter = 0;
            }

            State.CapturedFigure = capturedPieceType;
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

            var movedFrom = move.From;
            var movedTo = move.To;
            var moveFlag = move.Flags;

            var undoingEnPassant = (moveFlag & MoveFlags.EnPassant) != MoveFlags.None;
            var undoingPromotion = (moveFlag & MoveFlags.Promotion) != MoveFlags.None;
            var undoingCapture = State.CapturedFigure != Figure.None;

            var movedPiece = undoingPromotion ? new Piece(Figure.Pawn, AlliedColor) : this[movedTo];
            var movedPieceType = movedPiece.Figure;
            var capturedPieceType = State.CapturedFigure;

            if (undoingPromotion)
            {
                var promotedPiece = this[movedTo];
                var pawnPiece = new Piece(Figure.Pawn, AlliedColor);
                TotalPieceCountWithoutPawnsAndKings--;

                AllPieces[promotedPiece.Index].Remove(movedTo);
                AllPieces[movedPiece.Index].Add(movedTo);
                PieceBitboards[promotedPiece.Index] ^= movedTo;
                PieceBitboards[pawnPiece.Index] ^= movedTo;
            }

            MovePiece(movedPiece, movedTo, movedFrom);

            if (undoingCapture)
            {
                var captureSquare = movedTo;
                var capturedPiece = new Piece(capturedPieceType, EnemyColor);

                if (undoingEnPassant)
                {
                    captureSquare = movedTo + (undoingWhiteMove ? -8 : 8);
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

            if (movedPieceType == Figure.King)
            {
                Kings[AlliedColorIndex] = movedFrom;

                if ((moveFlag & MoveFlags.Castling) != MoveFlags.None)
                {
                    var rookPiece = new Piece(Figure.Rook, AlliedColor);
                    var kingside = movedTo == Square.G1 || movedTo == Square.G8;
                    var rookSquareBeforeCastling = kingside ? movedTo + 1 : movedTo - 2;
                    var rookSquareAfterCastling = kingside ? movedTo - 1 : movedTo + 1;

                    PieceBitboards[rookPiece.Index] ^= Bitboard.Empty.With(rookSquareAfterCastling).With(rookSquareBeforeCastling);
                    ColorBitboards[AlliedColorIndex] ^= Bitboard.Empty.With(rookSquareAfterCastling).With(rookSquareBeforeCastling);

                    this[rookSquareAfterCastling] = Piece.Empty;
                    this[rookSquareBeforeCastling] = rookPiece;

                    AllPieces[rookPiece.Index].Remove(rookSquareAfterCastling);
                    AllPieces[rookPiece.Index].Add(rookSquareBeforeCastling);
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

        public void GenerateMoves()
        {
            Moves.Clear();

            new MovesJob
            {
                Board = this,
                QuietMoves = true,
            }
            .Schedule().Complete();
        }

        private readonly NativeArray<Bitboard> CreateTable(NativeArray<int2> orthogonalDirections, NativeArray<int2> diagonalDirections, Square square, bool isOrthogonal, ulong magic, int shift, Allocator allocator)
        {
            var bitCount = Area - shift;
            var lookupSize = 1 << bitCount;
            var table = new NativeArray<Bitboard>(lookupSize, allocator);

            var mask = CreateMagicMovementMask(orthogonalDirections, diagonalDirections, square, isOrthogonal);
            var blockerPatterns = CreateAllBlockerBitboards(mask, Allocator.Temp);

            for (var i = 0; i < blockerPatterns.Length; i++)
            {
                var pattern = blockerPatterns[i];
                var index = ((ulong)pattern * magic) >> shift;
                var moves = LegalMoveBitboardFromBlockers(orthogonalDirections, diagonalDirections, square, pattern, isOrthogonal);
                table[(int)index] = moves;
            }

            blockerPatterns.Dispose();
            return table;
        }

        private readonly Bitboard LegalMoveBitboardFromBlockers(NativeArray<int2> orthogonalDirections, NativeArray<int2> diagonalDirections, Square square, Bitboard pattern, bool isOrthogonal)
        {
            var board = Bitboard.Empty;

            var directions = isOrthogonal ? orthogonalDirections : diagonalDirections;
            var coordinate = square.Coordinate;

            for (var i = 0; i < directions.Length; i++)
            {
                for (var j = 1; j < 8; j++)
                {
                    var targetCoordinate = coordinate + directions[i] * j;

                    if (IsCoordinateValid(targetCoordinate))
                    {
                        var targetSquare = new Square(targetCoordinate);
                        board.Include(targetSquare);
                        if (pattern.Contains(targetSquare))
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return board;
        }

        private readonly NativeArray<Bitboard> CreateAllBlockerBitboards(Bitboard mask, Allocator allocator)
        {
            var squares = new NativeList<Square>(Allocator.Temp);

            for (var i = Square.Min.Index; i <= Square.Max.Index; i++)
            {
                var square = new Square(i);
                if (mask.Contains(square))
                {
                    squares.Add(square);
                }
            }

            // 2 ^ n
            var patterCount = 1 << squares.Length;
            var blockerBoards = new NativeArray<Bitboard>(patterCount, allocator);

            for (var i = 0; i < patterCount; i++)
            {
                for (var j = 0; j < squares.Length; j++)
                {
                    var bit = (i >> j) & i;
                    blockerBoards[i] |= (Bitboard)(ulong)(bit << squares[j].Index);
                }
            }

            squares.Dispose();

            return blockerBoards;
        }

        private readonly Bitboard CreateMagicMovementMask(NativeArray<int2> orthogonalDirections, NativeArray<int2> diagonalDirections, Square square, bool isOrthogonal)
        {
            var mask = Bitboard.Empty;

            var directions = isOrthogonal ? orthogonalDirections : diagonalDirections;
            var coordinate = square.Coordinate;

            for (var i = 0; i < directions.Length; i++)
            {
                for (var j = 1; j < 8; j++)
                {
                    var targetCoordinate = coordinate + directions[i] * j;
                    var nextCoordinate = targetCoordinate + directions[i];

                    if (IsCoordinateValid(nextCoordinate))
                    {
                        mask.Include(new Square(targetCoordinate));
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return mask;
        }

        private void InitializeSquare(NativeArray<int2> orthogonalDirections, NativeArray<int2> diagonalDirections, NativeArray<int2> knightJumps, int file, int rank)
        {
            var square = new Square(file, rank);

            for (var i = 0; i < 4; i++)
            {
                var orthogonalFile = file + orthogonalDirections[i].x;
                var orthogonalRank = rank + orthogonalDirections[i].y;
                var diagonalFile = file + diagonalDirections[i].x;
                var diagonalRank = rank + diagonalDirections[i].y;

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
                var jumpRank = file + knightJumps[i].y;

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

        public readonly Bitboard GetPawnAttacks(Bitboard pawns, Color color)
        {
            return color switch
            {
                Color.Black => ((pawns >> 7) & ~Bitboard.FileA) | ((pawns << 7) & ~Bitboard.FileH),
                Color.White => ((pawns << 9) & ~Bitboard.FileA) | ((pawns >> 9) & ~Bitboard.FileH),
                _ => default
            };
        }

        public Bitboard GetSliderAttacks(Square square, Bitboard blockers, bool isOrthogonal)
        {
            return isOrthogonal ? GetRookAttacks(square, blockers) : GetBishopAttacks(square, blockers);
        }

        public Bitboard GetRookAttacks(Square square, Bitboard blockers)
        {
            var key = ((blockers & MagicRookMasks[square.Index]) * new Bitboard(RookMagics[square.Index])) >> MagicRookShifts[square.Index];
            return MagicRookAttacks[square.Index][(int)(ulong)key];
        }

        public Bitboard GetBishopAttacks(Square square, Bitboard blockers)
        {
            var key = ((blockers & MagicBishopMasks[square.Index]) * new Bitboard(BishopMagics[square.Index])) >> MagicBishopShifts[square.Index];
            return MagicBishopAttacks[square.Index][(int)(ulong)key];
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

            AllPieces[0].Dispose();
            AllPieces[6].Dispose();
            AllPieces[12].Dispose();
            AllPieces.Dispose();

            Moves.Dispose();
            AllMoves.Dispose();

            Zobrist.Dispose();

            KnightMoves.Dispose();
            KingMoves.Dispose();
            WhitePawnAttacks.Dispose();
            BlackPawnAttacks.Dispose();

            MoveLimits.Dispose();
            SquareOffsets.Dispose();
            SquareOffsets2D.Dispose();

            MagicRookMasks.Dispose();
            MagicBishopMasks.Dispose();

            for (var i = 0; i < MagicRookAttacks.Length; i++)
            {
                MagicRookAttacks[i].Dispose();
            }

            MagicRookAttacks.Dispose();

            for (var i = 0; i < MagicBishopAttacks.Length; i++)
            {
                MagicBishopAttacks[i].Dispose();
            }

            MagicBishopAttacks.Dispose();

            MagicRookShifts.Dispose();
            MagicBishopShifts.Dispose();
            RookMagics.Dispose();
            BishopMagics.Dispose();
        }
    }
}
