using System;
using Unity.Collections;

namespace Chess
{
    public struct TranspositionTable : IDisposable
    {
        public struct Entry
        {
            public ulong Key;
            public Transposition Transposition;
            public int Depth;
            public int Score;
            public Move Move;
        }

        private NativeHashMap<ulong, Entry> table;

        public TranspositionTable(Allocator allocator)
        {
            table = new(0, allocator);
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
