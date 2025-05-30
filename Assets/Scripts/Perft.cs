using System;
using System.Text;
using Unity.Collections;
using UnityEngine;

namespace Chess
{
    public struct Perft : IDisposable
    {
        private Board board;

        public Perft(string fen, Allocator allocator)
        {
            board = new(allocator);
            board.Load(fen);
        }

        public ulong Run(int depth, bool debug = false)
        {
            var debugText = new StringBuilder();

            var count = Search(depth, debugText, debug);

            if (debug)
            {
                Debug.Log(debugText);
            }

            return count;
        }

        private ulong Search(int depth, StringBuilder debugText, bool debug)
        {
            var moves = new MoveList(board, true, Allocator.TempJob);

            if (depth == 1)
            {
                if (debug)
                {
                    foreach (var move in moves)
                    {
                        debugText.AppendLine($"{move}: 1");
                    }
                }

                var length = (ulong)moves.Length;
                moves.Dispose();

                return length;
            }

            var count = 0UL;

            foreach (var move in moves)
            {
                board.MakeMove(move);

                var innerCount = Run(depth - 1, false);

                if (debug)
                {
                    debugText.AppendLine($"{move}: {innerCount}");
                }

                count += innerCount;

                board.UnmakeLastMove();
            }

            moves.Dispose();

            return count;
        }

        public void Dispose()
        {
            board.Dispose();
        }
    }
}
