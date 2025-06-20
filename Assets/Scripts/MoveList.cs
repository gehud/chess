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

        public readonly bool IsInCheck => isInCheck.Value;

        public readonly Bitboard AttackSquares => attackSquares.Value;

        public readonly Bitboard PawnAttackSquares => pawnAttackSquares.Value;

        public readonly NativeList<Move> Items => moves;

        public readonly int Length => moves.Length;

        public const int MaxMoves = 256;

        public Move this[int index]
        {
            get => moves[index];
            set => moves[index] = value;
        }

        private NativeList<Move> moves;
        private NativeReference<bool> isInCheck;
        private NativeReference<Bitboard> attackSquares;
        private NativeReference<Bitboard> pawnAttackSquares;

        public enum Execution
        {
            Schedule,
            Run,
            Inline
        }

        public MoveList(in Board board, bool quietMoves, Allocator allocator, Execution execution = Execution.Schedule)
        {
            moves = new(MaxMoves, allocator);
            isInCheck = new(default, allocator);
            attackSquares = new(default, allocator);
            pawnAttackSquares = new(default, allocator);

            var job = new MoveGenerationJob
            {
                Board = board,
                Moves = moves,
                QuietMoves = quietMoves,
                IsInCheck = isInCheck,
                AttackSquares = attackSquares,
                PawnAttackSquares = pawnAttackSquares,
            };

            switch (execution)
            {
                case Execution.Schedule:
                    job.Schedule().Complete();
                    break;
                case Execution.Run:
                    job.Run();
                    break;
                case Execution.Inline:
                    job.Execute();
                    break;
            }
        }

        public void Dispose()
        {
            moves.Dispose();
            isInCheck.Dispose();
            attackSquares.Dispose();
            pawnAttackSquares.Dispose();
        }

        public IEnumerator<Move> GetEnumerator()
        {
            return moves.AsReadOnly().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
