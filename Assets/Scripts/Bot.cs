using System;
using Unity.Collections;
using Unity.Jobs;

namespace Chess
{
    public struct Bot : IDisposable
    {
        public bool IsSearchCompleted => searchJob != default && searchJob.IsCompleted;

        public Move BestMove => bestMove.Value;

        private TranspositionTable transpositionTable;
        private NativeReference<bool> isSearchCanceled;
        private NativeReference<Move> bestMove;
        private JobHandle searchJob;

        public Bot(Allocator allocator)
        {
            transpositionTable = new(64, allocator);
            isSearchCanceled = new(false, allocator);
            bestMove = new(default, allocator);
            searchJob = default;
        }

        public void StartSearch(Board board)
        {
            isSearchCanceled.Value = false;
            bestMove.Value = default;

            var job = new MoveSearchJob
            {
                Board = board,
                TranspositionTable = transpositionTable,
                IsCanceled = isSearchCanceled,
                BestMove = bestMove,
            };

            searchJob = job.Schedule();
        }

        public void StopSearch()
        {
            isSearchCanceled.Value = true;

            if (searchJob != default)
            {
                searchJob.Complete();
            }

            isSearchCanceled.Value = false;
        }

        public void Dispose()
        {
            transpositionTable.Dispose();
            isSearchCanceled.Dispose();
            bestMove.Dispose();
        }
    }
}
