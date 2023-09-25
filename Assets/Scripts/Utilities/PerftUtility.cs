using System;
using UnityEngine;

namespace Chess.Utilities {
	/// <summary>
	/// Performance test, move path enumeration
	/// </summary>
	public class PerftUtility {
		public static int Perft(Game game, int depth, bool makeLog = false) {
			if (depth < 1) {
				throw new ArgumentException("Depth must be >= 1", nameof(depth));
			}

			int result = 0;

			var moves = game.GenerateMoves();

			if (depth == 1) {
				if (makeLog) {
					foreach (var move in moves) {
						Debug.Log($"{move}: 1");
					}
				}

				return moves.Count;
			}

			foreach (var move in moves) {
				game.Move(move);

				int count = Perft(game, depth - 1);

				if (makeLog) {
					Debug.Log($"{move}: {count}");
				}

				result += count;

				game.Undo();
			}

			return result;
		}
	}
}
