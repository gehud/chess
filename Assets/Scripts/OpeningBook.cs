using System;
using System.Collections.Generic;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Chess
{
    public class OpeningBook
    {
        public readonly struct Move
        {
            public readonly string String;
            public readonly int PlayCount;

            public Move(string @string, int playCount)
            {
                String = @string;
                PlayCount = playCount;
            }
        }

        private Random random;
        private readonly Dictionary<string, Move[]> moves;

        public OpeningBook()
        {
            random = new Random((uint)DateTime.Now.Millisecond);
            var book = Resources.Load<TextAsset>("OpeningBook");
            var entries = book.text.Trim(new char[] { ' ', '\n' }).Split("pos").AsSpan(1);

            moves = new(entries.Length);

            for (var i = 0; i < entries.Length; i++)
            {
                var entryData = entries[i].Trim('\n').Split('\n');
                var positionFen = entryData[0].Trim();
                var allMoveData = entryData.AsSpan(1);

                var bookMoves = new Move[allMoveData.Length];

                for (var moveIndex = 0; moveIndex < bookMoves.Length; moveIndex++)
                {
                    var moveData = allMoveData[moveIndex].Split(' ');
                    bookMoves[moveIndex] = new Move(moveData[0], int.Parse(moveData[1]));
                }

                moves.Add(positionFen, bookMoves);
            }
        }

        public bool TryGetBookMove(string positionFen, out string moveString, double weightPow = 0.5)
        {
            weightPow = Math.Clamp(weightPow, 0, 1);
            if (this.moves.TryGetValue(RemoveMoveCountersFromFen(positionFen), out var moves))
            {
                var totalPlayCount = 0;
                foreach (var move in moves)
                {
                    totalPlayCount += WeightedPlayCount(move.PlayCount);
                }

                var weights = new double[moves.Length];
                var weightSum = 0.0;
                for (int i = 0; i < moves.Length; i++)
                {
                    var weight = WeightedPlayCount(moves[i].PlayCount) / (double)totalPlayCount;
                    weightSum += weight;
                    weights[i] = weight;
                }

                var probCumul = new double[moves.Length];
                for (int i = 0; i < weights.Length; i++)
                {
                    var prob = weights[i] / weightSum;
                    probCumul[i] = probCumul[Math.Max(0, i - 1)] + prob;
                }

                var random = this.random.NextDouble();
                for (int i = 0; i < moves.Length; i++)
                {

                    if (random <= probCumul[i])
                    {
                        moveString = moves[i].String;
                        return true;
                    }
                }
            }

            moveString = "Null";
            return false;

            int WeightedPlayCount(int playCount) => (int)Math.Ceiling(Math.Pow(playCount, weightPow));
        }

        public bool HasBookMove(string positionFen)
        {
            return moves.ContainsKey(RemoveMoveCountersFromFen(positionFen));
        }

        private string RemoveMoveCountersFromFen(string fen)
        {
            var fenA = fen[..fen.LastIndexOf(' ')];
            return fenA[..fenA.LastIndexOf(' ')];
        }
    }
}
