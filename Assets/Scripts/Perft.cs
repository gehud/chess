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
            board.GenerateMoves();

            if (depth == 1)
            {
                if (debug)
                {
                    for (var i = 0; i < board.Moves.Length; i++)
                    {
                        debugText.AppendLine($"{board.Moves[i]}: 1");
                    }
                }

                return (ulong)board.Moves.Length;
            }

            var moves = new NativeList<Move>(Allocator.Temp);
            moves.CopyFrom(board.Moves);

            var count = 0UL;

            for (var i = 0; i < moves.Length; i++)
            {
                board.MakeMove(moves[i]);
                var innerCount = Run(depth - 1, false);

                if (debug)
                {
                    debugText.AppendLine($"{moves[i]}: {innerCount}");
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
