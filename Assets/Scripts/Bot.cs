using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Chess
{
    public class Bot
    {
        private const int immediateMateScore = 100000;
        private const int squareControlledByOpponentPawnPenalty = 350;
        private const int capturedPieceValueMultiplier = 10;

        public Move BestMove => bestMove;

        private readonly TranspositionTable transpositionTable;
        private bool hasSearchedAtLeastOneMove;
        private Move bestMoveThisIteration;
        private int bestEvalThisIteration;
        private Move bestMove;
        private int bestEval;
        private bool searchCancelled;

        public void StartSearch(Board board)
        {
            bestEvalThisIteration = bestEval = 0;
            bestMoveThisIteration = bestMove = Move.Invalid;

            RunIterativeDeepeningSearch();

            if (!bestMove.IsValid)
            {
                var moves = new MoveList(board, true, Allocator.TempJob);
                bestMove = moves[0];
                moves.Dispose();
            }

            searchCancelled = false;
        }

        public void EndSearch()
        {
            searchCancelled = true;
        }

        private void RunIterativeDeepeningSearch()
        {
            for (int searchDepth = 1; searchDepth <= 256; searchDepth++)
            {
                hasSearchedAtLeastOneMove = false;

                if (searchCancelled)
                {
                    if (hasSearchedAtLeastOneMove)
                    {
                        bestMove = bestMoveThisIteration;
                        bestEval = bestEvalThisIteration;
                    }

                    break;
                }
                else
                {
                    bestMove = bestMoveThisIteration;
                    bestEval = bestEvalThisIteration;

                    bestEvalThisIteration = int.MinValue;
                    bestMoveThisIteration = Move.Invalid;

                    if (IsMateScore(bestEval) && NumPlyToMateFromScore(bestEval) <= searchDepth)
                    {
                        break;
                    }
                }
            }
        }

        private int SearchMoves(ref Board board, int depth, int plyFromRoot, int alpha, int beta)
        {
            if (searchCancelled)
            {
                return 0;
            }

            if (plyFromRoot > 0)
            {
                if (board.RepetitionPositionHistory.Contains(board.ZobristKey))
                {
                    return 0;
                }

                alpha = Mathf.Max(alpha, -immediateMateScore + plyFromRoot);
                beta = Mathf.Min(beta, immediateMateScore - plyFromRoot);
                if (alpha >= beta)
                {
                    return alpha;
                }
            }

            int value = transpositionTable.LookupEvaluation(board.ZobristKey, depth, plyFromRoot, alpha, beta);
            if (value != TranspositionTable.LookupFailed)
            {
                if (plyFromRoot == 0)
                {
                    var entry = transpositionTable.GetEntry(board.ZobristKey);
                    bestMoveThisIteration = entry.Move;
                    bestEvalThisIteration = entry.Value;
                }

                return value;
            }

            if (depth == 0)
            {
                int evaluation = QuiescenceSearch(ref board, alpha, beta);
                return evaluation;
            }

            var moves = new MoveList(board, true, Allocator.TempJob);
            OrderMoves(board, moves, false);

            if (moves.Length == 0)
            {
                if (moves.IsInCheck)
                {
                    var mateScore = immediateMateScore - plyFromRoot;
                    moves.Dispose();
                    return -mateScore;
                }
                else
                {
                    moves.Dispose();
                    return 0;
                }
            }

            var evalType = TranspositionTable.EntryKind.UpperBound;
            var bestMoveInThisPosition = Move.Invalid;

            for (var i = 0; i < moves.Length; i++)
            {
                board.MakeMove(moves[i], inSearch: true);
                int eval = -SearchMoves(ref board, depth - 1, plyFromRoot + 1, -beta, -alpha);
                board.UnmakeMove(moves[i], inSearch: true);

                if (eval >= beta)
                {
                    transpositionTable.StoreEvaluation(board.ZobristKey, depth, plyFromRoot, beta, TranspositionTable.EntryKind.LowerBound, moves[i]);
                    moves.Dispose();
                    return beta;
                }

                if (eval > alpha)
                {
                    evalType = TranspositionTable.EntryKind.Exact;
                    bestMoveInThisPosition = moves[i];

                    alpha = eval;
                    if (plyFromRoot == 0)
                    {
                        bestMoveThisIteration = moves[i];
                        bestEvalThisIteration = eval;
                    }
                }
            }

            transpositionTable.StoreEvaluation(board.ZobristKey, depth, plyFromRoot, alpha, evalType, bestMoveInThisPosition);

            moves.Dispose();
            return alpha;

        }

        private int QuiescenceSearch(ref Board board, int alpha, int beta)
        {
            int evaluation = new Evaluation(board);

            if (evaluation >= beta)
            {
                return beta;
            }

            if (evaluation > alpha)
            {
                alpha = evaluation;
            }

            var moves = new MoveList(board, false, Allocator.TempJob);
            OrderMoves(board, moves, true);

            for (var i = 0; i < moves.Length; i++)
            {
                board.MakeMove(moves[i], true);
                evaluation = -QuiescenceSearch(ref board, -beta, -alpha);
                board.UnmakeMove(moves[i], true);

                if (evaluation >= beta)
                {
                    moves.Dispose();
                    return beta;
                }

                if (evaluation > alpha)
                {
                    alpha = evaluation;
                }
            }

            moves.Dispose();
            return alpha;
        }

        public static bool IsMateScore(int score)
        {
            if (score == int.MinValue)
            {
                return false;
            }

            const int maxMateDepth = 1000;

            return Mathf.Abs(score) > immediateMateScore - maxMateDepth;
        }

        public static int NumPlyToMateFromScore(int score)
        {
            return immediateMateScore - Mathf.Abs(score);
        }

        private struct MoreComparer : IComparer<Move>
        {
            public Move HashMove;
            public Board Board;
            public MoveList Moves;

            public int Compare(Move x, Move y)
            {
                return GetMoveScore(x) - GetMoveScore(y);
            }

            private int GetMoveScore(Move move)
            {
                var score = 0;
                var moveFigure = Board[move.From].Figure;
                var captureFigure = Board[move.To].Figure;
                var flags = move.Flags;

                if (captureFigure != Figure.None)
                {
                    score = capturedPieceValueMultiplier * new Evaluation(captureFigure) - new Evaluation(moveFigure);
                }

                if (moveFigure == Figure.Pawn)
                {

                    if ((flags & MoveFlags.QueenPromotion) != MoveFlags.None)
                    {
                        score += Evaluation.Queen;
                    }
                    else if ((flags & MoveFlags.KnightPromotion) != MoveFlags.None)
                    {
                        score += Evaluation.Knight;
                    }
                    else if ((flags & MoveFlags.RookPromotion) != MoveFlags.None)
                    {
                        score += Evaluation.Rook;
                    }
                    else if ((flags & MoveFlags.BishopPromotion) != MoveFlags.None)
                    {
                        score += Evaluation.Bishop;
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

        public void OrderMoves(Board board, MoveList moves, bool isQuietSearch)
        {
            var hashMove = Move.Invalid;

            if (!isQuietSearch)
            {
                hashMove = transpositionTable.GetEntry(board.ZobristKey).Move;
            }

            moves.Items.Sort(new MoreComparer
            {
                Board = board,
                HashMove = hashMove,
                Moves = moves,
            });
        }
    }
}
