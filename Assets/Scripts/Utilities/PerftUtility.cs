using UnityEngine;

namespace Chess.Utilities {
	public class PerftUtility {
		public static int Perft(Game board, int depth, Object context = null) {
			int result = 0;

			var moves = board.GenerateMoves();

			if (depth == 1) {
				return moves.Count;
			}

			foreach (var move in moves) {
				board.Move(move);
				int count = Perft(board, depth - 1);
				result += count;
				board.Undo();
			}

			return result;
		}
	}
}
