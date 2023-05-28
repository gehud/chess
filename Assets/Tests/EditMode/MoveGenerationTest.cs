using NUnit.Framework;

namespace Chess.Tests.EditMode {
	public class MoveGenerationTest {
		private int GetPositionCount(Board board, int depth) {
			if (depth == 0) {
				return 1;
			}

			int result = 0;

			var moves = board.GenerateMoves();
			foreach (var move in moves) {
				board.Move(move);
				result += GetPositionCount(board, depth - 1);
				board.Undo();
			}

			return result;
		}

		[Test]
		public void Depth1PositionsIs20() {
			var board = new Board();
			board.Start();
			Assert.AreEqual(20, GetPositionCount(board, 1));
		}

		[Test]
		public void Depth2PositionsIs400() {
			var board = new Board();
			board.Start();
			Assert.AreEqual(400, GetPositionCount(board, 2));
		}

		[Test]
		public void Depth3PositionsIs8902() {
			var board = new Board();
			board.Start();
			Assert.AreEqual(8902, GetPositionCount(board, 3));
		}

		[Test]
		public void Position5Depth1PositionsIs44() {
			var board = new Board();
			board.Load("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8");
			Assert.AreEqual(44, GetPositionCount(board, 1));
		}

		[Test]
		public void Position5Depth2PositionsIs1486() {
			var board = new Board();
			board.Load("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8");
			Assert.AreEqual(1486, GetPositionCount(board, 2));
		}
	}
}
