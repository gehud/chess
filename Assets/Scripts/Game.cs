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

        private NativeArray<Bitboard> pinned;

        public Game(Allocator allocator)
        {
            Moves = new NativeList<Move>(allocator);
            Board = new Board(allocator);
            State = new State();
            pinned = new NativeArray<Bitboard>((int)Direction.SouthWest, allocator, NativeArrayOptions.UninitializedMemory);
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
            var job = new HelperStateJob
            {
                Board = Board,
                MoveColor = State.MoveColor,
            };

            job.Schedule(Board.Area, default).Complete();

            State.KingSquare = job.KingSquare;
            State.DiagonalSlidingPinning = job.DiagonalSlidingPinning;
            State.StraightSlidingPinning = job.StraightSlidingPinning;
        }

        public void GenerateMoves()
        {
            Moves.Clear();

            new PinnedSquaresJob
            {
                Board = Board,
                State = State,
                Pinned = pinned,
            }.Schedule().Complete();

            new MoveJob
            {
                Moves = Moves,
                Board = Board,
                State = State,
                Pinned = pinned,
            }.Schedule(Board.Area, default).Complete();
        }

        public void MakeMove(Move move)
        {
            if (move.Flags == MoveFlags.None)
            {
                Board[move.To] = Board[move.From];
                Board[move.From] = Piece.Empty;
            }

            State.MoveColor = State.MoveColor == Color.White ? Color.Black : Color.White;

            UpdateHelpersState();

            GenerateMoves();
        }

        public void Dispose()
        {
            Moves.Dispose();
            Board.Dispose();
            pinned.Dispose();
        }
    }
}
