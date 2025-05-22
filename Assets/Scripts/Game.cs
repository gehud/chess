using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Chess
{
    public struct Game : IDisposable
    {
        public NativeList<Move> Moves;
        public Board Board;
        private State State;

        public Game(Allocator allocator)
        {
            Moves = new NativeList<Move>(allocator);
            Board = new Board(allocator);
            State = new State();
        }

        public void Start()
        {
            var fen = Fen.Start;
            Load(fen);
            fen.Dispose();
        }

        public void Load(in Fen fen)
        {
            fen.Load(ref Board, ref State);

            UpdateHelpersState();

            GenerateMoves();
        }

        private void UpdateHelpersState()
        {
            var alliedKingSquare = new NativeReference<Square>(Allocator.TempJob);

            var job = new HelperStateJob
            {
                Board = Board,
                MoveColor = State.MoveColor,
                AlliedKingSquare = alliedKingSquare,
            };

            job.Schedule().Complete();

            State.AlliedKingSquare = job.AlliedKingSquare.Value;

            alliedKingSquare.Dispose();
        }

        public void GenerateMoves()
        {
            Moves.Clear();

            new MoveJob
            {
                Moves = Moves,
                Board = Board,
                State = State,
            }.Schedule().Complete();
        }

        public void MakeMove(Move move)
        {
            Board[move.To] = Board[move.From];
            Board[move.From] = Piece.Empty;

            if (move.From == State.AlliedKingSquare || (move.Flags & MoveFlags.Castling) != MoveFlags.None)
            {
                switch (State.MoveColor)
                {
                    case Color.Black:
                        State.BlackCastlingKingside = false;
                        State.BlackCastlingQueenside = false;
                        break;
                    case Color.White:
                        State.WhiteCastlingKingside = false;
                        State.WhiteCastlingQueenside = false;
                        break;
                }
            }

            if (move.From == new Square(0, 0) || move.To == new Square(0, 0))
            {
                State.WhiteCastlingQueenside = false;
            }
            else if (move.From == new Square(7, 0) || move.To == new Square(7, 0))
            {
                State.WhiteCastlingKingside = false;
            }
            else if (move.From == new Square(0, 7) || move.To == new Square(0, 7))
            {
                State.BlackCastlingQueenside = false;
            }
            else if (move.From == new Square(7, 7) || move.To == new Square(7, 7))
            {
                State.BlackCastlingKingside = false;
            }

            if ((move.Flags & MoveFlags.DoublePawnMove) != MoveFlags.None)
            {
                State.DoubleMovePawnSquare = move.To;
            }
            else if ((move.Flags & MoveFlags.EnPassant) != MoveFlags.None)
            {
                Board[State.DoubleMovePawnSquare] = Piece.Empty;
            }
            else if ((move.Flags & MoveFlags.CastlingKingside) != MoveFlags.None)
            {
                switch (State.MoveColor)
                {
                    case Color.Black:
                        Board[5, 7] = Board[7, 7];
                        Board[7, 7] = Piece.Empty;
                        break;
                    case Color.White:
                        Board[5, 0] = Board[0, 7];
                        Board[0, 7] = Piece.Empty;
                        break;
                }
            }
            else if ((move.Flags & MoveFlags.CastlingQueenside) != MoveFlags.None)
            {
                switch (State.MoveColor)
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
            else if ((move.Flags & MoveFlags.Promotion) != MoveFlags.None)
            {
                Board[move.To] = new Piece(Figure.Queen, State.MoveColor);
            }

            State.MoveColor = State.MoveColor == Color.White ? Color.Black : Color.White;

            UpdateHelpersState();

            GenerateMoves();
        }

        public void Dispose()
        {
            Moves.Dispose();
            Board.Dispose();
        }
    }
}
