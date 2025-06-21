using System;
using Unity.Collections;
using Unity.Jobs;

namespace Chess
{
    public struct Bot : IDisposable
    {
        public bool IsSearchCompleted => isSearchCompleted || isJobStarted && searchJob.IsCompleted;

        public Move BestMove => bestMove.Value;

        private TranspositionTable transpositionTable;
        private PieceSquareTables pieceSquareTables;
        private Evaluation evaluation;
        private MoveOrdering moveOrdering;
        private readonly OpeningBook openingBook;
        private NativeReference<bool> isSearchCanceled;
        private NativeReference<Move> bestMove;
        private JobHandle searchJob;

        private const int maxBookPly = 16;
        private bool isJobStarted;
        private bool isSearchCompleted;

        public Bot(Allocator allocator)
        {
            transpositionTable = new(allocator);
            pieceSquareTables = new(allocator);
            evaluation = new(allocator);
            moveOrdering = new(allocator);
            openingBook = new();
            isSearchCanceled = new(false, allocator);
            bestMove = new(default, allocator);
            searchJob = default;
            isJobStarted = false;
            isSearchCompleted = false;
        }

        public void StartSearch(Board board)
        {
            isSearchCanceled.Value = false;
            bestMove.Value = default;

            if (TryGetOpeningBookMove(board, out var bookMove))
            {
                bestMove.Value = bookMove;
                isSearchCompleted = true;
            }
            else
            {
                var job = new MoveSearchJob
                {
                    Board = board,
                    TranspositionTable = transpositionTable,
                    PieceSquareTables = pieceSquareTables,
                    Evaluation = evaluation,
                    MoveOrdering = moveOrdering,
                    IsCanceled = isSearchCanceled,
                    BestMove = bestMove,
                };

                searchJob = job.Schedule();
                isJobStarted = true;
            }
        }

        private readonly bool TryGetOpeningBookMove(in Board board, out Move bookMove)
        {
            if (board.PlyCount <= maxBookPly && openingBook.TryGetBookMove(board.GetFen(false), out string moveString))
            {
                bookMove = new Move(moveString, board);
                return true;
            }

            bookMove = Move.Null;
            return false;
        }

        public void StopSearch()
        {
            isSearchCanceled.Value = true;
            searchJob.Complete();
            isSearchCanceled.Value = false;
            isJobStarted = false;
            isSearchCompleted = false;
        }

        public void Dispose()
        {
            transpositionTable.Dispose();
            pieceSquareTables.Dispose();
            evaluation.Dispose();
            moveOrdering.Dispose();
            isSearchCanceled.Dispose();
            bestMove.Dispose();
        }
    }
}
