using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Chess
{
    public struct Evaluation : IDisposable
    {
        public struct EvaluationFactors
        {
            public int MaterialScore;
            public int MopUpScore;
            public int PieceSquareTableScore;
            public int PawnScore;

            public readonly int Sum()
            {
                return MaterialScore + MopUpScore + PieceSquareTableScore + PawnScore;
            }
        }

        public readonly struct Material
        {
            public readonly int Score;
            public readonly int PawnCount;
            public readonly int MajorCount;
            public readonly int MinorCount;
            public readonly int BishopCount;
            public readonly int QueenCount;
            public readonly int RookCount;

            public readonly Bitboard AlliedPawns;
            public readonly Bitboard EnemyPawns;

            public readonly float Endgame;

            public Material(int pawnCount, int knightCount, int bishopCount, int queenCount, int rookCount, Bitboard alliedPawns, Bitboard enemyPawns)
            {
                PawnCount = pawnCount;
                BishopCount = bishopCount;
                QueenCount = queenCount;
                RookCount = rookCount;
                AlliedPawns = alliedPawns;
                EnemyPawns = enemyPawns;

                MajorCount = rookCount + queenCount;
                MinorCount = bishopCount + knightCount;

                Score = 0;
                Score += pawnCount * Pawn;
                Score += knightCount * Knight;
                Score += bishopCount * Bishop;
                Score += rookCount * Rook;
                Score += queenCount * Queen;

                const int queenEndgameWeight = 45;
                const int rookEndgameWeight = 20;
                const int bishopEndgameWeight = 10;
                const int knightEndgameWeight = 10;

                const int endgameStartWeight = 2 * rookEndgameWeight + 2 * bishopEndgameWeight + 2 * knightEndgameWeight + queenEndgameWeight;
                var endgameWeightSum = queenCount * queenEndgameWeight + rookCount * rookEndgameWeight + bishopCount * bishopEndgameWeight + knightCount * knightEndgameWeight;
                Endgame = 1 - math.min(1, endgameWeightSum / (float)endgameStartWeight);
            }
        }

        public const int Pawn = 100;
        public const int Knight = 300;
        public const int Bishop = 320;
        public const int Rook = 500;
        public const int Queen = 900;
        public const float Endgame = Rook * 2 + Bishop + Knight;

        private NativeArray<int> passedPawnBonuses;
        private NativeArray<int> isolatedPawnPenaltyByCount;

        public Evaluation(Allocator allocator)
        {
            passedPawnBonuses = new(new int[] { 0, 120, 80, 50, 30, 15, 15 }, allocator);
            isolatedPawnPenaltyByCount = new(new int[] { 0, -10, -25, -50, -75, -75, -75, -75, -75 }, allocator);
        }

        public int Evaluate(in Board board, in PieceSquareTables pieceSquareTables)
        {
            var whiteEvaliation = default(EvaluationFactors);
            var blackEvaluation = default(EvaluationFactors);

            var whiteMaterial = GetMaterial(board, Color.White);
            var blackMaterial = GetMaterial(board, Color.Black);

            whiteEvaliation.MaterialScore = whiteMaterial.Score;
            blackEvaluation.MaterialScore = blackMaterial.Score;

            whiteEvaliation.PieceSquareTableScore = EvaluatePieceSquareTables(board, pieceSquareTables, Color.White, blackMaterial.Endgame);
            blackEvaluation.PieceSquareTableScore = EvaluatePieceSquareTables(board, pieceSquareTables, Color.Black, whiteMaterial.Endgame);

            whiteEvaliation.MopUpScore = MopUpEval(board, Color.White, whiteMaterial, blackMaterial);
            blackEvaluation.MopUpScore = MopUpEval(board, Color.Black, blackMaterial, whiteMaterial);

            whiteEvaliation.PawnScore = EvaluatePawns(board, Color.White);
            blackEvaluation.PawnScore = EvaluatePawns(board, Color.Black);

            var perspective = board.IsWhiteAllied ? 1 : -1;
            var evaluation = whiteEvaliation.Sum() - blackEvaluation.Sum();
            return evaluation * perspective;
        }

        public static int GetFigureScore(Figure figure)
        {
            return figure switch
            {
                Figure.Pawn => Pawn,
                Figure.Knight => Knight,
                Figure.Bishop => Bishop,
                Figure.Rook => Rook,
                Figure.Queen => Queen,
                _ => 0,
            };
        }

        private int EvaluatePawns(in Board board, Color color)
        {
            var pawns = board.Pawns[(int)color];
            var friendlyPawns = board.PieceBitboards[new Piece(Figure.Pawn, color).Index];
            var opponentPawns = board.PieceBitboards[new Piece(Figure.Pawn, color.Opposite()).Index];
            var masks = color == Color.White ? board.WhitePassedPawnMask : board.BlackPassedPawnMask;
            var bonus = 0;
            var numIsolatedPawns = 0;

            for (var i = 0; i < pawns.Length; i++)
            {
                var square = pawns[i];
                var passedMask = masks[square.Index];

                if ((opponentPawns & passedMask).IsEmpty)
                {
                    var rank = square.Rank;
                    var numSquaresFromPromotion = color == Color.White ? Board.Size - rank - 1 : rank;
                    bonus += passedPawnBonuses[numSquaresFromPromotion];
                }

                if ((friendlyPawns & board.AdjacentFileMasks[square.File]).IsEmpty)
                {
                    numIsolatedPawns++;
                }
            }

            return bonus + isolatedPawnPenaltyByCount[numIsolatedPawns];
        }

        private static int MopUpEval(in Board board, Color color, Material alliedMaterial, Material enemyMaterial)
        {
            if (alliedMaterial.Score > enemyMaterial.Score + Pawn * 2 && enemyMaterial.Endgame > 0)
            {
                var mopUpScore = 0;

                var friendlyKingSquare = board.Kings[(int)color];
                var opponentKingSquare = board.Kings[(int)color.Opposite()];
                mopUpScore += (14 - board.GetManhattanDistance(friendlyKingSquare, opponentKingSquare)) * 4;
                mopUpScore += board.GetCenterManhattanDistance(opponentKingSquare) * 10;

                return (int)(mopUpScore * enemyMaterial.Endgame);
            }

            return 0;
        }

        private static int EvaluatePieceSquareTables(in Board board, in PieceSquareTables pieceSquareTables, Color color, float endgame)
        {
            var value = 0;

            value += EvaluatePieceSquareTable(pieceSquareTables.Rooks, board.Rooks[(int)color], color);
            value += EvaluatePieceSquareTable(pieceSquareTables.Knights, board.Knights[(int)color], color);
            value += EvaluatePieceSquareTable(pieceSquareTables.Bishops, board.Bishops[(int)color], color);
            value += EvaluatePieceSquareTable(pieceSquareTables.Queens, board.Queens[(int)color], color);

            var pawnEarly = EvaluatePieceSquareTable(pieceSquareTables.Pawns, board.Pawns[(int)color], color);
            var pawnLate = EvaluatePieceSquareTable(pieceSquareTables.PawnsEnd, board.Pawns[(int)color], color);
            value += (int)(pawnEarly * (1f - endgame));
            value += (int)(pawnLate * endgame);

            var kingEarlyPhase = PieceSquareTables.Read(pieceSquareTables.KingStart, board.Kings[(int)color], color);
            value += (int)(kingEarlyPhase * (1f - endgame));
            var kingLatePhase = PieceSquareTables.Read(pieceSquareTables.KingEnd, board.Kings[(int)color], color);
            value += (int)(kingLatePhase * endgame);

            return value;
        }

        private static int EvaluatePieceSquareTable(NativeArray<int>.ReadOnly table, PieceSet pieceSet, Color color)
        {
            var value = 0;

            for (var i = 0; i < pieceSet.Length; i++)
            {
                value += PieceSquareTables.Read(table, pieceSet[i], color);
            }

            return value;
        }

        private static Material GetMaterial(in Board board, Color color)
        {
            var pawnCount = board.Pawns[(int)color].Length;
            var knightCount = board.Knights[(int)color].Length;
            var bishopCount = board.Bishops[(int)color].Length;
            var rookCount = board.Rooks[(int)color].Length;
            var queenCount = board.Queens[(int)color].Length;

            var alliedPawns = board.PieceBitboards[new Piece(Figure.Pawn, color).Index];
            var enemyPawns = board.PieceBitboards[new Piece(Figure.Pawn, color.Opposite()).Index];

            return new(pawnCount, knightCount, bishopCount, queenCount, rookCount, alliedPawns, enemyPawns);
        }

        public void Dispose()
        {
            passedPawnBonuses.Dispose();
            isolatedPawnPenaltyByCount.Dispose();
        }
    }
}
