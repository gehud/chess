using NUnit.Framework;
using Unity.Collections;

namespace Chess.Tests
{
    public struct Perft
    {
        private Board board;
        private readonly int depth;
        private readonly int nodes;

        public Perft(string fen, int depth, int nodes, Allocator allocator = Allocator.Persistent)
        {
            board = new Board(allocator);
            board.Load(fen);
            this.depth = depth;
            this.nodes = nodes;
        }

        public void Run()
        {
            Assert.AreEqual(nodes, Search(depth));
        }

        private int Search(int depth, bool debug = true)
        {
            board.GenerateMoves();

            if (depth == 1)
            {
#if PERFT_DEBUG_MOVES
                if (debug)
                {
                    foreach (var move in board.Moves)
                    {
                        UnityEngine.Debug.Log($"{move}: 1");
                    }
                }
#endif
                return board.Moves.Length;
            }

            var moves = new NativeList<Move>(Allocator.Persistent);
            moves.CopyFrom(board.Moves);
            var count = 0;

            for (var i = 0; i < moves.Length; i++)
            {
                board.MakeMove(moves[i]);
                var innerCount = Search(depth - 1, false);
                count += innerCount;
#if PERFT_DEBUG_MOVES
                if (debug) 
                {
                    UnityEngine.Debug.Log($"{moves[i]}: {innerCount}");
                }
#endif
                board.UnmakeMove(moves[i]);
            }

            moves.Dispose();
            return count;
        }
    }
}
