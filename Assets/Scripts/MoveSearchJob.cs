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
        public PieceSquareTables PieceSquareTables;
        public MoveOrdering MoveOrdering;
        [ReadOnly, NativeDisableContainerSafetyRestriction]
        public NativeReference<bool> IsCanceled;
        public NativeReference<Move> BestMove;

        private Move bestMoveThisIteration;

        private const int maxSearchDepth = int.MaxValue;
        private const int positiveInfinity = 9999999;
        private const int negativeInfinity = -positiveInfinity;
        private const int immediateMateScore = 100000;
        private bool hasSearchedAtLeastOneMove;

        public void Execute()
        {
            bestMoveThisIteration = BestMove.Value = Move.Null;
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
                    bestMoveThisIteration = Move.Null;
                }
            }

            if (BestMove.Value.IsNull)
            {
                var moves = new MoveList(Board, true, Allocator.TempJob, MoveList.Execution.Inline);
                var random = new Random(404);
                BestMove.Value = moves[random.NextInt(moves.Length)];
                moves.Dispose();
            }
        }

        private bool TryLookupEvaluation(in TranspositionTable.Entry entry, int depth, int alpha, int beta)
        {
            if (entry.Depth < depth)
            {
                return false;
            }

            if (entry.Transposition == Transposition.Exact)
            {
                return true;
            }

            if (entry.Transposition == Transposition.UpperBound && entry.Score <= alpha)
            {
                return true;
            }

            if (entry.Transposition == Transposition.LowerBound && entry.Score >= alpha)
            {
                return true;
            }

            return false;
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
                if (TryLookupEvaluation(entry, depth, alpha, beta))
                {
                    if (plyFromRoot == 0)
                    {
                        bestMoveThisIteration = entry.Move;
                    }

                    return entry.Score;
                }
                ;
            }

            if (depth == 0)
            {
                return QuiescenceSearch(alpha, beta);
            }

            var moves = new MoveList(Board, true, Allocator.TempJob, MoveList.Execution.Inline);

            MoveOrdering.OrderMoves(Board, TranspositionTable, moves, false);
            if (moves.Length == 0)
            {
                moves.Dispose();

                if (moves.IsInCheck)
                {
                    var mateScore = immediateMateScore - plyFromRoot;
                    return -mateScore;
                }
                else
                {
                    return 0;
                }
            }

            var transposition = Transposition.UpperBound;
            var bestMoveInThisPosition = Move.Null;

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
                        hasSearchedAtLeastOneMove = true;
                    }
                }
            }

            TranspositionTable.Add(Board.ZobristKey, depth, alpha, transposition, bestMoveInThisPosition);

            moves.Dispose();
            return alpha;
        }

        private int QuiescenceSearch(int alpha, int beta)
        {
            var score = Evaluation.Evaluate(Board, PieceSquareTables);

            if (score >= beta)
            {
                return beta;
            }

            if (score > alpha)
            {
                alpha = score;
            }

            var moves = new MoveList(Board, false, Allocator.TempJob, MoveList.Execution.Inline);
            MoveOrdering.OrderMoves(Board, TranspositionTable, moves, true);

            for (var i = 0; i < moves.Length; i++)
            {
                Board.MakeMove(moves[i], true);
                score = -QuiescenceSearch(-beta, -alpha);
                Board.UnmakeMove(moves[i], true);

                if (score >= beta)
                {
                    moves.Dispose();
                    return beta;
                }
                if (score > alpha)
                {
                    alpha = score;
                }
            }

            moves.Dispose();
            return alpha;
        }
    }
}
