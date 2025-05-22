using System;
using Unity.Collections;
using Unity.Jobs;

namespace Chess
{
    public struct Game : IDisposable
    {
        public State State => state;

        public NativeList<Move> Moves;
        public Board Board;
        private State state;
        private NativeList<State> history;

        public Game(Allocator allocator)
        {
            Moves = new NativeList<Move>(allocator);
            Board = new Board(allocator);
            state = default;
            history = new NativeList<State>(allocator);
        }

        public void Start()
        {
            Load(Fen.Start);
        }

        public void Load(in Fen fen)
        {
            fen.Load(ref Board, ref state);
            UpdateHelpersState();
        }

        public void Load(string fen)
        {
            var parser = new Fen(fen, Allocator.Temp);
            Load(parser);
            parser.Dispose();
        }

        private void UpdateHelpersState()
        {
            var alliedKingSquare = new NativeReference<Square>(Allocator.TempJob);
            var enemyKingSquare = new NativeReference<Square>(Allocator.TempJob);

            var job = new HelperStateJob
            {
                Board = Board,
                MoveColor = state.MoveColor,
                AlliedKingSquare = alliedKingSquare,
                EnemyKingSquare = enemyKingSquare,
            };

            job.Schedule().Complete();

            state.AlliedKingSquare = job.AlliedKingSquare.Value;
            state.EnemyKingSquare = job.EnemyKingSquare.Value;

            alliedKingSquare.Dispose();
            enemyKingSquare.Dispose();
        }

        public void GenerateMoves()
        {
            Moves.Clear();

            new MovesJob
            {
                Moves = Moves,
                Board = Board,
                State = state,
            }.Schedule().Complete();
        }

        public void MakeMove(Move move)
        {
            history.Add(state);

            state.Move = move;
            state.CapturedPiece = Board[move.To];

            Board[move.To] = Board[move.From];
            Board[move.From] = Piece.Empty;

            if (move.From == state.AlliedKingSquare || (move.Flags & MoveFlags.Castling) != MoveFlags.None)
            {
                switch (state.MoveColor)
                {
                    case Color.Black:
                        state.BlackCastlingKingside = false;
                        state.BlackCastlingQueenside = false;
                        break;
                    case Color.White:
                        state.WhiteCastlingKingside = false;
                        state.WhiteCastlingQueenside = false;
                        break;
                }
            }

            if (move.From == state.AlliedKingSquare)
            {
                state.AlliedKingSquare = move.To;
            }

            if (move.From == new Square(0, 0) || move.To == new Square(0, 0))
            {
                state.WhiteCastlingQueenside = false;
            }
            else if (move.From == new Square(7, 0) || move.To == new Square(7, 0))
            {
                state.WhiteCastlingKingside = false;
            }
            else if (move.From == new Square(0, 7) || move.To == new Square(0, 7))
            {
                state.BlackCastlingQueenside = false;
            }
            else if (move.From == new Square(7, 7) || move.To == new Square(7, 7))
            {
                state.BlackCastlingKingside = false;
            }

            if ((move.Flags & MoveFlags.DoublePawnMove) != MoveFlags.None)
            {
                state.DoubleMovePawnSquare = move.To;
            }
            else if ((move.Flags & MoveFlags.EnPassant) != MoveFlags.None)
            {
                Board[state.DoubleMovePawnSquare] = Piece.Empty;
            }
            else if ((move.Flags & MoveFlags.CastlingKingside) != MoveFlags.None)
            {
                switch (state.MoveColor)
                {
                    case Color.Black:
                        Board[5, 7] = Board[7, 7];
                        Board[7, 7] = Piece.Empty;
                        break;
                    case Color.White:
                        Board[5, 0] = Board[7, 0];
                        Board[7, 0] = Piece.Empty;
                        break;
                }
            }
            else if ((move.Flags & MoveFlags.CastlingQueenside) != MoveFlags.None)
            {
                switch (state.MoveColor)
                {
                    case Color.Black:
                        Board[3, 7] = Board[0, 7];
                        Board[0, 7] = Piece.Empty;
                        break;
                    case Color.White:
                        Board[3, 0] = Board[0, 0];
                        Board[0, 0] = Piece.Empty;
                        break;
                }
            }
            else if ((move.Flags & MoveFlags.QueenPromotion) != MoveFlags.None)
            {
                Board[move.To] = new Piece(Figure.Queen, state.MoveColor);
            }
            else if ((move.Flags & MoveFlags.RookPromotion) != MoveFlags.None)
            {
                Board[move.To] = new Piece(Figure.Rook, state.MoveColor);
            }
            else if ((move.Flags & MoveFlags.KnightPromotion) != MoveFlags.None)
            {
                Board[move.To] = new Piece(Figure.Knight, state.MoveColor);
            }
            else if ((move.Flags & MoveFlags.BishopPromotion) != MoveFlags.None)
            {
                Board[move.To] = new Piece(Figure.Bishop, state.MoveColor);
            }

            state.DoubleMovePawnSquare = -1;

            state.MoveColor = state.MoveColor == Color.White ? Color.Black : Color.White;
            ++state.NextMoveIndex;
            (state.AlliedKingSquare, state.EnemyKingSquare) = (state.EnemyKingSquare, state.AlliedKingSquare);
        }

        public void UnmakeMove()
        {
            if (history.IsEmpty)
            {
                return;
            }

            var move = state.Move;

            Board[move.From] = Board[move.To];
            Board[move.To] = state.CapturedPiece;

            var lastMoveColor = state.MoveColor == Color.White ? Color.Black : Color.White;

            if ((move.Flags & MoveFlags.Promotion) != MoveFlags.None)
            {
                Board[move.From] = new Piece(Figure.Pawn, lastMoveColor);
            }
            else if ((move.Flags & MoveFlags.EnPassant) != MoveFlags.None)
            {
                Board[state.DoubleMovePawnSquare] = new Piece(Figure.Pawn, state.MoveColor);
            }
            else if ((move.Flags & MoveFlags.CastlingKingside) != MoveFlags.None)
            {
                switch (lastMoveColor)
                {
                    case Color.Black:
                        Board[7, 7] = Board[5, 7];
                        Board[5, 7] = Piece.Empty;
                        break;
                    case Color.White:
                        Board[7, 0] = Board[5, 0];
                        Board[5, 0] = Piece.Empty;
                        break;
                }
            }
            else if ((move.Flags & MoveFlags.CastlingQueenside) != MoveFlags.None)
            {
                switch (lastMoveColor)
                {
                    case Color.Black:
                        Board[0, 7] = Board[3, 7];
                        Board[3, 7] = Piece.Empty;
                        break;
                    case Color.White:
                        Board[0, 0] = Board[3, 0];
                        Board[3, 0] = Piece.Empty;
                        break;
                }
            }

            state = history[^1];
            history.RemoveAt(history.Length - 1);
        }

        public void Dispose()
        {
            Moves.Dispose();
            Board.Dispose();
            history.Dispose();
        }
    }
}
