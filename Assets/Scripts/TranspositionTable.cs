using System;
using Unity.Collections;
using static System.Runtime.InteropServices.Marshal;

namespace Chess
{
    public struct TranspositionTable : IDisposable
    {
        public const int LookupFailed = int.MinValue;

        public struct Entry
        {
            public static int Size => SizeOf<Entry>();

            public ulong Key;
            public Transposition Transposition;
            public int Depth;
            public int Score;
            public Move Move;
        }

        private NativeHashMap<ulong, Entry> table;

        public TranspositionTable(int sizeInMb, Allocator allocator)
        {
            var length = sizeInMb * 1024 * 1024 / Entry.Size;
            table = new(length, allocator);
        }

        public bool TryGetValue(ulong key, out Entry entry)
        {
            return table.TryGetValue(key, out entry);
        }

        public void Add(ulong key, int depth, int score, Transposition transposition, Move move)
        {
            var entry = new Entry
            {
                Key = key,
                Depth = depth,
                Score = score,
                Transposition = transposition,
                Move = move,
            };

            table[key] = entry;
        }

        public void Clear()
        {
            table.Clear();
        }

        public void Dispose()
        {
            table.Dispose();
        }
    }
}
