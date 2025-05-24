using System;

namespace Chess
{
    public readonly struct Direction : IEquatable<Direction>
    {
        public const int Count = 8;

        public static Direction Begin => new(0);
        public static Direction End => new(7);

        public static Direction North => new(0);
        public static Direction South => new(1);
        public static Direction West => new(2);
        public static Direction East => new(3);
        public static Direction NorthWest => new(4);
        public static Direction SouthEast => new(5);

        public static Direction NorthEast = new(6);
        public static Direction SouthWest => new(7);

        public bool IsOrthogonal => index <= 3;
        public bool IsDiagonal => index >= 4;

        public bool IsHorizontal => this == West || this == East;
        public bool IsVertical => this == North || this == South;
        public bool IsRightDiagonal => this == NorthEast || this == SouthWest;
        public bool IsLeftDiagonal => this == NorthWest || this == SouthEast;

        private readonly int index;

        private Direction(int index)
        {
            this.index = index;
        }

        public static implicit operator int(Direction direction)
        {
            return direction.index;
        }

        public static implicit operator Direction(int index)
        {
            return new Direction(index);
        }

        public bool Equals(Direction other)
        {
            return index == other.index;
        }

        public override bool Equals(object other)
        {
            if (other is not Direction direction)
            {
                return false;
            }

            return Equals(direction);
        }

        public override int GetHashCode()
        {
            return index;
        }
    }
}
