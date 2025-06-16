using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;

namespace Chess
{
    public struct MoveList : IDisposable, IEnumerable<Move>
    {
        public readonly bool IsCreated => moves.IsCreated;

        public NativeArray<Move>.ReadOnly Items => moves.AsReadOnly();

        public readonly int Length => moves.Length;

        public const int MaxMoves = 256;

        private NativeList<Move> moves;

        public MoveList(in Board board, bool quietMoves, Allocator allocator)
        {
            moves = new(MaxMoves, allocator);

            new MoveGenerationJob
            {
                Board = board,
                Moves = moves,
                QuietMoves = quietMoves,
            }
            .Schedule()
            .Complete();
        }

        public void Dispose()
        {
            moves.Dispose();
        }

        public IEnumerator<Move> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
