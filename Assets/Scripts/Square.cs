using System;
using Unity.Mathematics;
using UnityEditor;

namespace Chess
{
    public readonly struct Square : IEquatable<Square>
    {
        public static Square Min => A1;
        public static Square Max => H8;

        public static Square A1 => new(SquareName.A1);
        public static Square B1 => new(SquareName.B1);
        public static Square C1 => new(SquareName.C1);
        public static Square D1 => new(SquareName.D1);
        public static Square E1 => new(SquareName.E1);
        public static Square F1 => new(SquareName.F1);
        public static Square G1 => new(SquareName.G1);
        public static Square H1 => new(SquareName.H1);

        public static Square A8 => new(SquareName.A8);
        public static Square B8 => new(SquareName.B8);
        public static Square C8 => new(SquareName.C8);
        public static Square D8 => new(SquareName.D8);
        public static Square E8 => new(SquareName.E8);
        public static Square F8 => new(SquareName.F8);
        public static Square G8 => new(SquareName.G8);
        public static Square H8 => new(SquareName.H8);

        public const int MinIndex = 0;
        public const int MaxIndex = Board.Area - 1;
        public const int MinComponent = 0;
        public const int MaxComponent = Board.Size - 1;

        public readonly int Index => index;

        public readonly int2 Coordinate => new(File, Rank);

        public readonly bool IsValid => index >= 0 && index < Board.Area;

        public readonly int File => index % Board.Size;

        public readonly int Rank => index / Board.Size;

        private readonly int index;

        public Square(int index)
        {
#if DEBUG
            if (index < MinIndex || index > MaxIndex)
            {
                throw new Exception("Square index is out of board range.");
            }
#endif
            this.index = index;
        }

        public Square(SquareName name)
        {
            index = (int)name;
        }

        public Square(int file, int rank)
        {
#if DEBUG
            if (file < MinComponent || file > MaxComponent)
            {
                throw new Exception("Square file is out of board range.");
            }

            if (rank < MinComponent || rank > MaxComponent)
            {
                throw new Exception("Square rank is out of board range.");
            }
#endif
            index = rank * Board.Size + file;
        }

        public Square(int2 coordinate) : this(coordinate.x, coordinate.y)
        {
        }

        public Square Translated(in Board board, in Direction direction, int distance = 1)
        {
            return board.GetTranslatedSquare(this, direction, distance);
        }

        public int GetBorderDistance(in Board board, in Direction direction)
        {
            return board.GetBorderDistance(this, direction);
        }

        public override readonly string ToString()
        {
            return $"{"abcdefgh"[File]}{Rank + 1}";
        }

        public bool Equals(Square other)
        {
            return index == other.index;
        }

        public override int GetHashCode()
        {
            return index;
        }

        public override bool Equals(object other)
        {
            if (other is not Square square)
            {
                return false;
            }

            return Equals(square);
        }

        public static explicit operator int(Square square)
        {
            return square.Index;
        }

        public static explicit operator Square(int index)
        {
            return new Square(index);
        }

        public static explicit operator SquareName(Square square)
        {
            return (SquareName)square.index;
        }

        public static explicit operator Square(SquareName name)
        {
            return new Square(name);
        }

        public static bool operator <(Square left, Square right)
        {
            return left.index < right.index;
        }

        public static bool operator <=(Square left, Square right)
        {
            return left.index <= right.index;
        }

        public static bool operator >=(Square left, Square right)
        {
            return left.index >= right.index;
        }

        public static bool operator >(Square left, Square right)
        {
            return left.index > right.index;
        }

        public static Square operator+(Square square, int shift)
        {
            return new(square.Index + shift);
        }

        public static Square operator -(Square square, int shift)
        {
            return new(square.Index - shift);
        }

        public static bool operator ==(Square left, Square right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Square left, Square right)
        {
            return !left.Equals(right);
        }
    }
}
