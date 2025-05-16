using System;
using Unity.Collections;

namespace Chess
{
    public struct Fen : IDisposable
    {
        public static Fen Start => new("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

        private NativeText fen;
        private int cursor;

        public Fen(string text, Allocator allocator = Allocator.Persistent)
        {
            fen = new NativeText(text, allocator);
            cursor = 0;
        }

        public bool Load(ref Board board, ref State state)
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

            if (!TryParseMoveColor(ref state))
            {
                return false;
            }

            if (!TryParseSeparator())
            {
                return false;
            }

            if (!TryParseCastlings(ref state))
            {
                return false;
            }

            if (!TryParseSeparator())
            {
                return false;
            }

            if (!TryParseEnPassantTargetCoordinate(ref state))
            {
                return false;
            }

            if (!TryParseSeparator())
            {
                return false;
            }

            if (!TryParseImmutableMoveCount(ref state))
            {
                return false;
            }

            if (!TryParseSeparator())
            {
                return false;
            }

            if (!TryParseNextMoveIndex(ref state))
            {
                return false;
            }

            return true;
        }

        private static bool TryParsePieceSymbol(char symbol, out Piece piece)
        {
            switch (symbol)
            {
                case 'p':
                    piece = Piece.Pawn;
                    break;
                case 'n':
                    piece = Piece.Knight;
                    break;
                case 'b':
                    piece = Piece.Bishop;
                    break;
                case 'r':
                    piece = Piece.Rook;
                    break;
                case 'q':
                    piece = Piece.Queen;
                    break;
                case 'k':
                    piece = Piece.King;
                    break;
                default:
                    piece = Piece.None;
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
                            board[file++, rank] = Square.Empty;
                        }
                    }
                    else
                    {
                        var color = char.IsUpper(symbol) ? Color.White : Color.Black;

                        if (!TryParsePieceSymbol(char.ToLower(symbol), out var piece))
                        {
                            return false;
                        }

                        board[file, rank] = new Square(piece, color);

                        ++file;
                    }
                }
            }

            return false;
        }

        private bool TryParseMoveColor(ref State state)
        {
            if (fen.Length <= cursor)
            {
                return false;
            }

            switch ((char)fen[cursor])
            {
                case 'w':
                    state.MoveColor = Color.White;
                    break;
                case 'b':
                    state.MoveColor = Color.Black;
                    break;
                default:
                    return false;
            }

            ++cursor;

            return true;
        }

        private bool TryParseCastlings(ref State state)
        {
            if (fen[cursor] == '-')
            {
                state.WhiteCastlingKingside = false;
                state.BlackCastlingKingside = false;
                state.WhiteCastlingQueenside = false;
                state.BlackCastlingQueenside = false;
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
                        ++cursor;
                        return true;
                    }
                    else if (symbol == 'K')
                    {
                        state.WhiteCastlingKingside = true;
                    }
                    else if (symbol == 'k')
                    {
                        state.BlackCastlingKingside = true;
                    }
                    else if (symbol == 'Q')
                    {
                        state.WhiteCastlingQueenside = true;
                    }
                    else if (symbol == 'q')
                    {
                        state.BlackCastlingQueenside = true;
                    }
                }

                return false;
            }
        }

        private bool TryParseEnPassantTargetCoordinate(ref State state)
        {
            if (fen[cursor] == '-')
            {
                state.EnPassantTargetCoordinate = -1;
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

                var rank = (int)(char.GetNumericValue((char)fen[cursor]) - 1);

                state.EnPassantTargetCoordinate = new Coordinate(file, rank);
                ++cursor;
                return true;
            }
        }

        private bool TryParseImmutableMoveCount(ref State state)
        {
            if (fen.Length <= cursor)
            {
                return false;
            }

            if (!char.IsNumber((char)fen[cursor]))
            {
                return false;
            }

            state.ImmutableMoveCount = (int)char.GetNumericValue((char)fen[cursor]);
            ++cursor;
            return true;
        }

        private bool TryParseNextMoveIndex(ref State state)
        {
            if (fen.Length <= cursor)
            {
                return false;
            }

            if (!char.IsNumber((char)fen[cursor]))
            {
                return false;
            }

            state.NextMoveIndex = (int)char.GetNumericValue((char)fen[cursor]);
            ++cursor;
            return true;
        }

        public void Dispose()
        {
            fen.Dispose();
        }
    }
}
