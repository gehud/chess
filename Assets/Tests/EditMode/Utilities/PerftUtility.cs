namespace Chess.Tests.EditMode.Utilities {
	public class PerftUtility {
		public static int Perft(Game board, int depth) {
			int result = 0;

			var moves = board.GenerateMoves();

			if (depth == 1) {
				return moves.Count;
			}

			foreach (var move in moves) {
				board.Move(move);
				result += Perft(board, depth - 1);
				board.Undo();
			}

			return result;
		}
	}
}
