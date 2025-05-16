using System;
using Unity.Collections;

namespace Chess
{
    public struct Game : IDisposable
    {
        public Board Board;
        
        private State state;

        public Game(Allocator allocator)
        {
            Board = new Board(allocator);
            state = new State();
        }

        public void Start()
        {
            var fen = Fen.Start;
            Load(fen);
            fen.Dispose();
        }

        public void Load(in Fen fen)
        {
            fen.Load(ref Board, ref state);
        }

        public void Dispose()
        {
            Board.Dispose();
        }
    }
}
