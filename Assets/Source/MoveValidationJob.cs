using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Chess {
    public struct MoveValidationJob : IJob {
        public NativeList<Move> Result;

        [WriteOnly]
        private NativeList<Move> result;
        [ReadOnly]
        private NativeList<Move> moves;
        [ReadOnly]
        private NativeList<Move> oponentMoves;

        public static MoveValidationJob Create(NativeList<Move> moves, NativeList<Move> oponentMoves, Allocator allocator = Allocator.Persistent) {
            return new MoveValidationJob {
                moves = moves,
                oponentMoves = oponentMoves,
                result = new NativeList<Move>(allocator),
            };
        }

        public void Execute() {
            for (int i = 0; i < moves.Length; i++) {
                var move = moves[i];

            }
        }
    }
}