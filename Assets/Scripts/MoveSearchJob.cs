using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Chess
{
    [BurstCompile]
    public struct MoveSearchJob : IJob
    {
        [NativeDisableContainerSafetyRestriction]
        public Board Board;
        public TranspositionTable TranspositionTable;
        [ReadOnly, NativeDisableContainerSafetyRestriction]
        public NativeReference<bool> IsCanceled;
        public NativeReference<Move> BestMove;

        private Move bestMoveThisIteration;
        private bool hasSearchedAtLeastOneMove;

        private const int maxSearchDepth = int.MaxValue;
        private const int positiveInfinity = 9999999;
        private const int negativeInfinity = -positiveInfinity;
        private const int immediateMateScore = 100000;
        private const int squareControlledByOpponentPawnPenalty = 350;
        private const int capturedPieceValueMultiplier = 10;
        private const int pawnScore = 100;
        private const int knightScore = 300;
        private const int bishopScore = 330;
        private const int rookScore = 500;
        private const int queenScore = 900;
        private const float endgameScore = rookScore * 2 + bishopScore + knightScore;

        public void Execute()
        {
            bestMoveThisIteration = BestMove.Value = Move.Invalid;
            TranspositionTable.Clear();

            for (int searchDepth = 1; searchDepth <= maxSearchDepth; searchDepth++)
            {
                hasSearchedAtLeastOneMove = false;
                Search(searchDepth, 0, negativeInfinity, positiveInfinity);

                if (IsCanceled.Value)
                {
                    if (hasSearchedAtLeastOneMove)
                    {
                        BestMove.Value = bestMoveThisIteration;
                    }

                    break;
                }
                else
                {
                    BestMove.Value = bestMoveThisIteration;
                    bestMoveThisIteration = Move.Invalid;
                }
            }

            if (!BestMove.Value.IsValid)
            {
                var moves = new MoveList(Board, true, Allocator.TempJob, MoveList.Execution.Inline);
                BestMove.Value = moves[0];
                moves.Dispose();
            }
        }

        private int Search(int depth, int plyFromRoot, int alpha, int beta)
        {
            if (IsCanceled.Value)
            {
                return 0;
            }

            if (plyFromRoot > 0)
            {
                if (Board.RepetitionPositionHistory.Contains(Board.ZobristKey))
                {
                    return 0;
                }

                alpha = math.max(alpha, -immediateMateScore + plyFromRoot);
                beta = math.min(beta, immediateMateScore - plyFromRoot);
                if (alpha >= beta)
                {
                    return alpha;
                }
            }

            if (TranspositionTable.TryGetValue(Board.ZobristKey, out var entry))
            {
                if (entry.Depth >= depth)
                {
                    if (entry.Transposition == Transposition.Exact)
                    {
                        return entry.Score;
                    }

                    if (entry.Transposition == Transposition.LowerBound)
                    {
                        alpha = math.max(alpha, entry.Score);
                    }
                    else if (entry.Transposition == Transposition.UpperBound)
                    {
                        beta = math.max(beta, entry.Score);
                    }

                    if (alpha >= beta)
                    {
                        return entry.Score;
                    }
                }

                if (plyFromRoot == 0)
                {
                    bestMoveThisIteration = entry.Move;
                }

                return entry.Score;
            }

            if (depth == 0)
            {
                return QuiescenceSearch(alpha, beta);
            }

            var moves = new MoveList(Board, true, Allocator.TempJob, MoveList.Execution.Inline);

            OrderMoves(moves, false);
            if (moves.Length == 0)
            {
                moves.Dispose();

                if (moves.IsInCheck)
                {
                    var mateScore = immediateMateScore - plyFromRoot;
                    return mateScore;
                }
                else
                {
                    return 0;
                }
            }

            var transposition = Transposition.UpperBound;
            var bestMoveInThisPosition = Move.Invalid;

            for (var i = 0; i < moves.Length; i++)
            {
                Board.MakeMove(moves[i], true);
                var score = -Search(depth - 1, plyFromRoot + 1, -beta, -alpha);
                Board.UnmakeMove(moves[i], true);

                if (score >= beta)
                {
                    TranspositionTable.Add(Board.ZobristKey, depth, beta, Transposition.LowerBound, moves[i]);
                    moves.Dispose();
                    return beta;
                }

                if (score > alpha)
                {
                    transposition = Transposition.Exact;
                    bestMoveInThisPosition = moves[i];

                    alpha = score;
                    if (plyFromRoot == 0)
                    {
                        bestMoveThisIteration = moves[i];
                    }
                }
            }

            TranspositionTable.Add(Board.ZobristKey, depth, alpha, transposition, bestMoveInThisPosition);

            moves.Dispose();
            return alpha;
        }

        private int QuiescenceSearch(int alpha, int beta)
        {
            var eval = Evaluate();

            if (eval >= beta)
            {
                return beta;
            }

            if (eval > alpha)
            {
                alpha = eval;
            }

            var moves = new MoveList(Board, false, Allocator.TempJob, MoveList.Execution.Inline);
            OrderMoves(moves, true);

            for (var i = 0; i < moves.Length; i++)
            {
                Board.MakeMove(moves[i], true);
                eval = -QuiescenceSearch(-beta, -alpha);
                Board.UnmakeMove(moves[i], true);

                if (eval >= beta)
                {
                    moves.Dispose();
                    return beta;
                }
                if (eval > alpha)
                {
                    alpha = eval;
                }
            }

            moves.Dispose();
            return alpha;
        }

        private struct MoveComparer : IComparer<Move>
        {
            public Move HashMove;
            public Board Board;
            public MoveList Moves;

            public int Compare(Move x, Move y)
            {
                return GetMoveScore(y) - GetMoveScore(x);
            }

            private int GetMoveScore(Move move)
            {
                var score = 0;
                var moveFigure = Board[move.From].Figure;
                var captureFigure = Board[move.To].Figure;
                var flags = move.Flags;

                if (captureFigure != Figure.None)
                {
                    score = capturedPieceValueMultiplier * GetFigureScore(captureFigure) - GetFigureScore(moveFigure);
                }

                if (moveFigure == Figure.Pawn)
                {

                    if ((flags & MoveFlags.QueenPromotion) != MoveFlags.None)
                    {
                        score += queenScore;
                    }
                    else if ((flags & MoveFlags.KnightPromotion) != MoveFlags.None)
                    {
                        score += knightScore;
                    }
                    else if ((flags & MoveFlags.RookPromotion) != MoveFlags.None)
                    {
                        score += rookScore;
                    }
                    else if ((flags & MoveFlags.BishopPromotion) != MoveFlags.None)
                    {
                        score += bishopScore;
                    }
                }
                else
                {
                    if (Moves.AttackSquares.Contains(move.To))
                    {
                        score -= squareControlledByOpponentPawnPenalty;
                    }
                }

                if (move == HashMove)
                {
                    score += 10000;
                }

                return score;
            }
        }

        public void OrderMoves(MoveList moves, bool isQuiescenceSearch)
        {
            var hashMove = Move.Invalid;
            if (!isQuiescenceSearch && TranspositionTable.TryGetValue(Board.ZobristKey, out var entry))
            {
                hashMove = entry.Move;
            }

            moves.Items.Sort(new MoveComparer
            {
                Board = Board,
                HashMove = hashMove,
                Moves = moves,
            });
        }

        private static int GetFigureScore(Figure figure)
        {
            return figure switch
            {
                Figure.Pawn => pawnScore,
                Figure.Knight => knightScore,
                Figure.Bishop => bishopScore,
                Figure.Rook => rookScore,
                Figure.Queen => queenScore,
                _ => 0,
            };
        }

        private int Evaluate()
        {
            var whiteEvaluation = 0;
            var blackEvaluation = 0;

            var whiteColor = Count(Color.White);
            var blackColor = Count(Color.Black);

            var whiteColorWithoutPawns = whiteColor - Board.Pawns[(int)Color.White].Length * pawnScore;
            var blackColorWithoutPawns = blackColor - Board.Pawns[(int)Color.Black].Length * pawnScore;

            var whiteEndgamePhaseWeight = EndgamePhaseWeight(whiteColorWithoutPawns);
            var blackEndgamePhaseWeight = EndgamePhaseWeight(blackColorWithoutPawns);

            whiteEvaluation += whiteColor;
            blackEvaluation += blackColor;
            whiteEvaluation += MopUpEvaluation(Color.White, Color.Black, blackEndgamePhaseWeight);
            blackEvaluation += MopUpEvaluation(Color.Black, Color.White, whiteEndgamePhaseWeight);

            int evaluation = whiteEvaluation - blackEvaluation;

            var perspective = Board.IsWhiteAllied ? 1 : -1;
            return evaluation * perspective;
        }

        private readonly float EndgamePhaseWeight(int colorWithoutPawns)
        {
            const float multiplier = 1f / endgameScore;
            return 1f - math.min(1f, colorWithoutPawns * multiplier);
        }

        private int MopUpEvaluation(Color alliedColor, Color enemyColor, float endgameWeight)
        {
            var evaluation = 0;

            var alliedKing = Board.Kings[(int)alliedColor];
            var enemyKing = Board.Kings[(int)enemyColor];

            var enemyKingCenterDistanceFile = math.max(3 - enemyKing.File, enemyKing.File - 4);
            var enemyKingCenterDistanceRank = math.max(3 - enemyKing.Rank, enemyKing.Rank - 4);
            var enemyKingCenterDistance = enemyKingCenterDistanceFile + enemyKingCenterDistanceRank;
            evaluation += enemyKingCenterDistance;

            var kingDistanceFile = math.abs(alliedKing.File - enemyKing.File);
            var kingDistanceRank = math.abs(alliedKing.Rank - enemyKing.Rank);
            var kingDistance = kingDistanceFile + kingDistanceRank;
            evaluation += 14 - kingDistance;

            return (int)(evaluation * 10 + endgameWeight);
        }

        private int Count(Color color)
        {
            return
                Board.Pawns[(int)color].Length * pawnScore +
                Board.Knights[(int)color].Length * knightScore +
                Board.Bishops[(int)color].Length * bishopScore +
                Board.Rooks[(int)color].Length * rookScore +
                Board.Queens[(int)color].Length * queenScore;
        }
    }
}
