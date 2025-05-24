using System;
using Unity.Mathematics;

namespace Chess
{
    public readonly struct Square : IEquatable<Square>
    {
        public static Square Zero => new(0);

        public static Square A1 => new(0);
        public static Square B1 => new(1);
        public static Square C1 => new(2);
        public static Square D1 => new(3);
        public static Square E1 => new(4);
        public static Square F1 => new(5);
        public static Square G1 => new(6);
        public static Square H1 => new(7);
                                 
        public static Square A8 => new(56);
        public static Square B8 => new(57);
        public static Square C8 => new(58);
        public static Square D8 => new(59);
        public static Square E8 => new(60);
        public static Square F8 => new(61);
        public static Square G8 => new(62);
        public static Square H8 => new(63);

        public readonly int2 Coordinate => new(File, Rank);

        public readonly bool IsValid => index >= 0 && index < Board.Area;

        public readonly int File => index % Board.Size;

        public readonly int Rank => index / Board.Size;

        private readonly int index;

        private static readonly char[] fileNotations =
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'
        };

        public Square(int index)
        {
            this.index = index;
        }

        public Square(int file, int rank)
        {
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

        public static implicit operator int(Square square)
        {
            return square.index;
        }

        public static implicit operator Square(int index)
        {
            return new Square(index);
        }

        public override readonly string ToString()
        {
            return $"{fileNotations[File]}{Rank + 1}";
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
    }
}
