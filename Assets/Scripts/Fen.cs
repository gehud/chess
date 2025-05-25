using System;
using Unity.Collections;

namespace Chess
{
    public readonly struct Fen : IDisposable
    {
        public const string Start = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        public readonly NativeArray<Piece> Squares;
        public readonly bool WhiteCastleKingside;
        public readonly bool WhiteCastleQueenside;
        public readonly bool BlackCastleKingside;
        public readonly bool BlackCastleQueenside;
        public readonly int EnPassantFile;
        public readonly bool IsWhiteAllied;
        public readonly int FiftyMovePlyCount;
        public readonly int MoveCount;

        public Fen(string text, Allocator allocator)
        {
            Squares = new(Board.Area, allocator);

            string[] sections = text.Split(' ');

            var file = 0;
            var rank = 7;

            foreach (var symbol in sections[0])
            {
                if (symbol == '/')
                {
                    file = 0;
                    --rank;
                }
                else
                {
                    if (char.IsDigit(symbol))
                    {
                        file += (int)char.GetNumericValue(symbol);
                    }
                    else
                    {
                        var color = char.IsUpper(symbol) ? Color.White : Color.Black;
                        var figure = char.ToLower(symbol) switch
                        {
                            'k' => Figure.King,
                            'p' => Figure.Pawn,
                            'n' => Figure.Knight,
                            'b' => Figure.Bishop,
                            'r' => Figure.Rook,
                            'q' => Figure.Queen,
                            _ => Figure.None
                        };

                        Squares[new Square(file, rank)] = new Piece(figure, color);
                        ++file;
                    }
                }
            }

            IsWhiteAllied = sections[1] == "w";

            var castlingRights = sections[2];
            WhiteCastleKingside = castlingRights.Contains("K");
            WhiteCastleQueenside = castlingRights.Contains("Q");
            BlackCastleKingside = castlingRights.Contains("k");
            BlackCastleQueenside = castlingRights.Contains("q");

            EnPassantFile = 0;
            FiftyMovePlyCount = 0;
            MoveCount = 0;

            if (sections.Length > 3)
            {
                string enPassantFileName = sections[3][0].ToString();
                if ("abcdefgh".Contains(enPassantFileName))
                {
                    EnPassantFile = "abcdefgh".IndexOf(enPassantFileName) + 1;
                }
            }

            if (sections.Length > 4)
            {
                int.TryParse(sections[4], out FiftyMovePlyCount);
            }

            if (sections.Length > 5)
            {
                int.TryParse(sections[5], out MoveCount);
            }
        }

        public void Dispose()
        {
            Squares.Dispose();
        }
    }
}
