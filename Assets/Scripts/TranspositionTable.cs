using System;
using static System.Runtime.InteropServices.Marshal;

namespace Chess
{
    public class TranspositionTable
    {
        public const int LookupFailed = int.MinValue;

        public enum EntryKind
        {
            Exact,
            LowerBound,
            UpperBound,
        }

        public struct Entry
        {
            public static int Size => SizeOf<Entry>();

            public ulong Key;
            public int Depth;
            public int Value;
            public EntryKind Kind;
            public Move Move;
        }

        private readonly Entry[] entries;

        public TranspositionTable(int sizeInMb)
        {
            var length = sizeInMb * 1024 * 1024 / Entry.Size;
            entries = new Entry[length];
        }

        public Entry GetEntry(ulong key)
        {
            return entries[key % (ulong)entries.Length];
        }

        public int LookupEvaluation(ulong key, int depth, int plyFromRoot, int alpha, int beta)
        {
            var entry = entries[key % (ulong)entries.Length];

            if (entry.Depth >= depth)
            {
                int correctedScore = CorrectRetrievedMateScore(entry.Value, plyFromRoot);

                if (entry.Kind == EntryKind.Exact)
                {
                    return correctedScore;
                }

                if (entry.Kind == EntryKind.UpperBound && correctedScore <= alpha)
                {
                    return correctedScore;
                }

                if (entry.Kind == EntryKind.LowerBound && correctedScore >= beta)
                {
                    return correctedScore;
                }
            }

            return LookupFailed;
        }

        public void StoreEvaluation(ulong key, int depth, int numPlySearched, int eval, EntryKind evalType, Move move)
        {
            var entry = new Entry
            {
                Key = key,
                Value = CorrectMateScoreForStorage(eval, numPlySearched),
                Depth = depth,
                Kind = evalType,
                Move = move,
            };

            entries[key % (ulong)entries.Length] = entry;
        }

        int CorrectMateScoreForStorage(int score, int numPlySearched)
        {
            if (Bot.IsMateScore(score))
            {
                int sign = Math.Sign(score);
                return (score * sign + numPlySearched) * sign;
            }

            return score;
        }

        int CorrectRetrievedMateScore(int score, int numPlySearched)
        {
            if (Bot.IsMateScore(score))
            {
                int sign = Math.Sign(score);
                return (score * sign - numPlySearched) * sign;
            }

            return score;
        }

        public void Clear()
        {
            for (var i = 0; i < entries.Length; i++)
            {
                entries[i] = default;
            }
        }
    }
}
