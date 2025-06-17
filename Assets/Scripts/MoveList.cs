using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;

namespace Chess
{
    public struct MoveList : IDisposable, IEnumerable<Move>
    {
        public readonly bool IsCreated => data.Value.Moves.IsCreated;

        public readonly bool IsInCheck => data.Value.IsInCheck;

        public readonly Bitboard AttackSquares => data.Value.AttackSquares;

        public NativeList<Move> Items => data.Value.Moves;

        public readonly int Length => data.Value.Moves.Length;

        public const int MaxMoves = 256;

        public Move this[int index]
        {
            get => data.Value.Moves[index];
        }

        private struct Data
        {
            public NativeList<Move> Moves;
            public bool IsInCheck;
            public Bitboard AttackSquares;
        }

        private NativeReference<Data> data;

        public MoveList(in Board board, bool quietMoves, Allocator allocator)
        {
            var moves = new NativeList<Move>(MaxMoves, allocator);

            var isInCheckRef = new NativeReference<bool>(Allocator.TempJob);
            var attackSquaresRef = new NativeReference<Bitboard>(Allocator.TempJob);

            var job = new MoveGenerationJob
            {
                Board = board,
                Moves = moves,
                QuietMoves = quietMoves,
                IsInCheck = isInCheckRef,
                AttackSquares = attackSquaresRef
            };

            job.Schedule().Complete();

            data = new(new Data
            {
                Moves = moves,
                IsInCheck = isInCheckRef.Value,
            }, allocator);

            isInCheckRef.Dispose();
            attackSquaresRef.Dispose();
        }

        public void Dispose()
        {
            data.Value.Moves.Dispose();
            data.Dispose();
        }

        public IEnumerator<Move> GetEnumerator()
        {
            return data.Value.Moves.AsReadOnly().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
