using System;
using Unity.Collections;

namespace Chess
{
    public struct MoveOrdering : IDisposable
    {
        private NativeArray<int> scores;

        private const int squareControlledByOpponentPawnPenalty = 350;
        private const int capturedPieceValueMultiplier = 10;

        public MoveOrdering(Allocator allocator)
        {
            scores = new(MoveList.MaxMoves, allocator);
        }

        public void OrderMoves(in Board board, TranspositionTable transpositionTable, MoveList moves, bool isQuiescenceSearch)
        {
            var hashMove = Move.Null;
            if (!isQuiescenceSearch && transpositionTable.TryGetValue(board.ZobristKey, out var entry))
            {
                hashMove = entry.Move;
            }

            for (var i = 0; i < moves.Length; i++)
            {
                var move = moves[i];
                var score = 0;
                var moveFigure = board[move.From].Figure;
                var captureFigure = board[move.To].Figure;
                var flag = move.Flag;

                if (captureFigure != Figure.None)
                {
                    score = capturedPieceValueMultiplier * Evaluation.GetFigureScore(captureFigure) - Evaluation.GetFigureScore(moveFigure);
                }

                if (moveFigure == Figure.Pawn)
                {
                    switch (flag)
                    {
                        case MoveFlag.KnightPromotion:
                            score += Evaluation.Knight;
                            break;
                        case MoveFlag.BishopPromotion:
                            score += Evaluation.Bishop;
                            break;
                        case MoveFlag.RookPromotion:
                            score += Evaluation.Rook;
                            break;
                        case MoveFlag.QueenPromotion:
                            score += Evaluation.Queen;
                            break;
                    }
                }
                else
                {
                    if (moves.AttackSquares.Contains(move.To))
                    {
                        score -= squareControlledByOpponentPawnPenalty;
                    }
                }

                if (move == hashMove)
                {
                    score += 10000;
                }

                scores[i] = score;
            }

            Sort(moves);
        }

        private void Sort(MoveList moves)
        {
            for (var i = 0; i < moves.Length - 1; i++)
            {
                for (var j = i + 1; j > 0; j--)
                {
                    var swapIndex = j - 1;
                    if (scores[swapIndex] < scores[j])
                    {
                        (moves[j], moves[swapIndex]) = (moves[swapIndex], moves[j]);
                        (scores[j], scores[swapIndex]) = (scores[swapIndex], scores[j]);
                    }
                }
            }
        }

        public void Dispose()
        {
            scores.Dispose();
        }
    }
}
