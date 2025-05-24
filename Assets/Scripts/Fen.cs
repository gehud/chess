using System;
using Unity.Collections;

namespace Chess
{
    public struct Fen : IDisposable
    {
        public const string Start = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        private NativeText fen;
        private int cursor;

        public Fen(string text, Allocator allocator)
        {
            fen = new NativeText(text, allocator);
            cursor = 0;
        }

        public bool Load(ref Board board)
        {
            cursor = 0;

            if (!TryParseBoard(ref board))
            {
                return false;
            }

            if (!TryParseSeparator())
            {
                return false;
            }

            if (!TryParseMoveColor(ref board))
            {
                return false;
            }

            if (!TryParseSeparator())
            {
                return false;
            }

            var whiteCastlingKingside = false;
            var blackCastlingKingside = false;
            var whiteCastlingQueenside = false;
            var blackCastlingQueenside = false;
            if (!TryParseCastlings(ref whiteCastlingKingside, ref blackCastlingKingside, ref whiteCastlingQueenside, ref blackCastlingQueenside))
            {
                return false;
            }

            int whiteCastle = ((whiteCastlingKingside) ? 1 << 0 : 0) | ((whiteCastlingQueenside) ? 1 << 1 : 0);
            int blackCastle = ((blackCastlingKingside) ? 1 << 2 : 0) | ((blackCastlingQueenside) ? 1 << 3 : 0);
            int castlingRights = whiteCastle | blackCastle;
            board.State.castlingRights = castlingRights;

            if (!TryParseSeparator())
            {
                return false;
            }

            if (!TryParseDoubleMovePawnSquare(ref board))
            {
                return false;
            }

            var zobrist = new Zobrist(board);
            board.State.zobristKey = zobrist.Key;

            if (!TryParseSeparator())
            {
                return false;
            }

            if (!TryParseImmutableMoveCount(ref board))
            {
                return false;
            }

            if (!TryParseSeparator())
            {
                return false;
            }

            if (!TryParseNextMoveIndex(ref board))
            {
                return false;
            }

            return true;
        }

        private static bool TryParseFigureSymbol(char symbol, out Figure figure)
        {
            switch (symbol)
            {
                case 'p':
                    figure = Figure.Pawn;
                    break;
                case 'n':
                    figure = Figure.Knight;
                    break;
                case 'b':
                    figure = Figure.Bishop;
                    break;
                case 'r':
                    figure = Figure.Rook;
                    break;
                case 'q':
                    figure = Figure.Queen;
                    break;
                case 'k':
                    figure = Figure.King;
                    break;
                default:
                    figure = Figure.None;
                    return false;
            }

            return true;
        }

        private bool TryParseSeparator()
        {
            if (fen[cursor] == ' ')
            {
                ++cursor;
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool TryParseBoard(ref Board board)
        {
            var file = 0;
            var rank = Board.Size - 1;

            for (var i = cursor; i < fen.Length; i++)
            {
                cursor = i;

                if (fen.Length <= cursor)
                {
                    return false;
                }

                var symbol = (char)fen[i];

                if (symbol == ' ')
                {
                    return true;
                }
                else if (symbol == '/')
                {
                    file = 0;
                    --rank;
                }
                else
                {
                    if (char.IsDigit(symbol))
                    {
                        for (var skip = 0; skip < (int)char.GetNumericValue(symbol); skip++)
                        {
                            board[file++, rank] = Piece.Empty;
                        }
                    }
                    else
                    {
                        var color = char.IsUpper(symbol) ? Color.White : Color.Black;

                        if (!TryParseFigureSymbol(char.ToLower(symbol), out var figure))
                        {
                            return false;
                        }

                        board[file, rank] = new Piece(figure, color);

                        ++file;
                    }
                }
            }

            return false;
        }

        private bool TryParseMoveColor(ref Board board)
        {
            if (fen.Length <= cursor)
            {
                return false;
            }

            switch ((char)fen[cursor])
            {
                case 'w':
                    board.IsWhiteAllied = true;
                    break;
                case 'b':
                    board.IsWhiteAllied = false;
                    break;
                default:
                    return false;
            }

            ++cursor;

            return true;
        }

        private bool TryParseCastlings(ref bool whiteCastlingKingside, ref bool blackCastlingKingside, ref bool whiteCastlingQueenside, ref bool blackCastlingQueenside)
        {
            if (fen[cursor] == '-')
            {
                whiteCastlingKingside = false;
                blackCastlingKingside = false;
                whiteCastlingQueenside = false;
                blackCastlingQueenside = false;
                ++cursor;
                return true;
            }
            else
            {
                for (var i = cursor; i < fen.Length; i++)
                {
                    cursor = i;

                    if (fen.Length <= cursor)
                    {
                        return false;
                    }

                    var symbol = fen[i];

                    if (symbol == ' ')
                    {
                        return true;
                    }
                    else if (symbol == 'K')
                    {
                        whiteCastlingKingside = true;
                    }
                    else if (symbol == 'k')
                    {
                        blackCastlingKingside = true;
                    }
                    else if (symbol == 'Q')
                    {
                        whiteCastlingQueenside = true;
                    }
                    else if (symbol == 'q')
                    {
                        blackCastlingQueenside = true;
                    }
                }

                return false;
            }
        }

        private bool TryParseDoubleMovePawnSquare(ref Board board)
        {
            if (fen[cursor] == '-')
            {
                board.State.enPassantFile = -1;
                ++cursor;
                return true;
            }
            else
            {
                var file = fen[cursor] - 'a';
                ++cursor;

                if (fen.Length <= cursor)
                {
                    return false;
                }

                if (!char.IsNumber((char)fen[cursor]))
                {
                    return false;
                }

                board.State.enPassantFile = file;
                ++cursor;
                return true;
            }
        }

        private bool TryParseImmutableMoveCount(ref Board board)
        {
            if (fen.Length <= cursor)
            {
                return false;
            }

            if (!char.IsNumber((char)fen[cursor]))
            {
                return false;
            }

            board.State.fiftyMoveCounter = (int)char.GetNumericValue((char)fen[cursor]);
            ++cursor;
            return true;
        }

        private bool TryParseNextMoveIndex(ref Board board)
        {
            if (fen.Length <= cursor)
            {
                return false;
            }

            if (!char.IsNumber((char)fen[cursor]))
            {
                return false;
            }

            board.PlyCount = (int)char.GetNumericValue((char)fen[cursor]);
            ++cursor;
            return true;
        }

        public void Dispose()
        {
            fen.Dispose();
        }
    }
}
