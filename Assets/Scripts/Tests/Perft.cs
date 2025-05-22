using NUnit.Framework;
using Unity.Collections;

namespace Chess.Tests
{
    public struct Perft
    {
        private Game game;
        private readonly int depth;
        private readonly int nodes;

        public Perft(string fen, int depth, int nodes, Allocator allocator = Allocator.Persistent)
        {
            game = new Game(allocator);
            game.Load(fen);
            this.depth = depth;
            this.nodes = nodes;
        }

        public void Run()
        {
            Assert.AreEqual(nodes, Search(depth));
        }

        private int Search(int depth)
        {
            game.GenerateMoves();

            if (depth == 1)
            {
                return game.Moves.Length;
            }

            var moves = new NativeList<Move>(Allocator.Persistent);
            moves.CopyFrom(game.Moves);
            var count = 0;

            for (var i = 0; i < moves.Length; i++)
            {
                game.MakeMove(moves[i]);
                count += Search(depth - 1);
                game.UnmakeMove();
            }

            moves.Dispose();
            return count;
        }
    }
}
