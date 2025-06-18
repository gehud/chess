using System;
using Unity.Collections;
using UnityEngine;

namespace Chess
{
    public struct PieceSet : IDisposable
    {
        public readonly int Length => length.Value;

        private NativeArray<Square> squares;
        private NativeArray<int> map;
        private NativeReference<int> length;

        public PieceSet(int maxLength, Allocator allocator)
        {
            squares = new(maxLength, allocator);
            map = new(Board.Area, allocator);
            length = new(0, allocator);
        }

        public void Add(Square square)
        {
            squares[length.Value] = square;
            map[square.Index] = length.Value++;
        }

        public void Remove(Square square)
        {
            var index = map[square.Index];
            squares[index] = squares[--length.Value];
            map[squares[index].Index] = index;
        }

        public void Move(Square from, Square to)
        {
            var index = map[from.Index];
            squares[index] = to;
            map[to.Index] = index;
        }

        public void Dispose()
        {
            squares.Dispose();
            map.Dispose();
            length.Dispose();
        }
    }
}
