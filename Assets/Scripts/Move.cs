using System;

namespace Chess
{
    public readonly struct Move : IEquatable<Move>
    {
        public static Move Null => new(0);

        public readonly Square From => (Square)(data & 0b0000000000111111);
        public readonly Square To => (Square)((data & 0b0000111111000000) >> 6);
        public readonly MoveFlag Flag => (MoveFlag)(data >> 12);
        public readonly bool IsNull => this == Null;
        public readonly bool IsPromotion => Flag >= MoveFlag.KnightPromotion;

        private readonly ushort data;

        private Move(ushort data)
        {
            this.data = data;
        }

        public Move(Square from, Square to)
        {
            data = (ushort)((int)from | (int)to << 6);
        }

        public Move(Square from, Square to, MoveFlag flag)
        {
            data = (ushort)((int)from | (int)to << 6 | (int)flag << 12);
        }

        private static Square SquareIndexFromName(string name)
        {
            var fileName = name[0];
            var rankName = name[1];
            var file = "abcdefgh".IndexOf(fileName);
            var rank = "12345678".IndexOf(rankName);
            return new Square(file, rank);
        }

        public Move(string notation, in Board board)
        {
            var fromSquare = SquareIndexFromName(notation.Substring(0, 2));
            var toSquare = SquareIndexFromName(notation.Substring(2, 2));

            var movedFigure = board[fromSquare].Figure;
            var fromCoordinate = fromSquare.Coordinate;
            var toCoordindate = toSquare.Coordinate;

            var flag = MoveFlag.None;

            if (movedFigure == Figure.Pawn)
            {
                if (notation.Length > 4)
                {
                    flag = notation[^1] switch
                    {
                        'q' => MoveFlag.QueenPromotion,
                        'r' => MoveFlag.RookPromotion,
                        'n' => MoveFlag.KnightPromotion,
                        'b' => MoveFlag.BishopPromotion,
                        _ => MoveFlag.None
                    };
                }
                else if (Math.Abs(toCoordindate.y - fromCoordinate.y) == 2)
                {
                    flag = MoveFlag.DoubleForwardPawn;
                }
                else if (fromCoordinate.x != toCoordindate.x && board[toSquare] == Piece.Empty)
                {
                    flag = MoveFlag.EnPassant;
                }
            }
            else if (movedFigure == Figure.King)
            {
                if (Math.Abs(fromCoordinate.x - toCoordindate.x) > 1)
                {
                    flag = MoveFlag.Castling;
                }
            }

            this = new Move(fromSquare, toSquare, flag);
        }

        private string FormatFlag()
        {
            return Flag switch
            {
                MoveFlag.KnightPromotion => "n",
                MoveFlag.BishopPromotion => "b",
                MoveFlag.RookPromotion => "r",
                MoveFlag.QueenPromotion => "q",
                _ => string.Empty
            };
        }

        public override string ToString()
        {
            return $"{From}{To}{FormatFlag()}";
        }

        public bool Equals(Move other)
        {
            return data == other.data;
        }

        public override bool Equals(object other)
        {
            if (other is not Move move)
            {
                return false;
            }

            return Equals(move);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(data);
        }

        public static bool operator ==(Move left, Move right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Move left, Move right)
        {
            return !left.Equals(right);
        }
    }
}
