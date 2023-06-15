using NUnit.Framework;

namespace Chess.Tests.EditMode {
	public class MoveGenerationTest {
		private int Perft(Game board, int depth) {
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

		[Test]
		public void Depth1Positions20() {
			var board = new Game();
			board.Start();
			Assert.AreEqual(20, Perft(board, 1));
		}

		[Test]
		public void Depth2Positions400() {
			var board = new Game();
			board.Start();
			Assert.AreEqual(400, Perft(board, 2));
		}

		[Test]
		public void Depth3Positions8902() {
			var board = new Game();
			board.Start();
			Assert.AreEqual(8902, Perft(board, 3));
		}

		[Test]
		public void Depth4Positions197281() {
			var board = new Game();
			board.Start();
			Assert.AreEqual(197281, Perft(board, 4));
		}

		[Test]
		public void Depth5Positions4865609() {
			var board = new Game();
			board.Start();
			Assert.AreEqual(4865609, Perft(board, 5));
		}

		[Test]
		public void Position3Depth1Positions14() {
			var board = new Game();
			board.Load("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -");
			Assert.AreEqual(14, Perft(board, 1));
		}

		[Test]
		public void Position3Depth2Positions191() {
			var board = new Game();
			board.Load("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -");
			Assert.AreEqual(191, Perft(board, 2));
		}

		[Test]
		public void Position3Depth3Positions2812() {
			var board = new Game();
			board.Load("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -");
			Assert.AreEqual(2812, Perft(board, 3));
		}

		[Test]
		public void Position3Depth4Positions43238() {
			var board = new Game();
			board.Load("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -");
			Assert.AreEqual(43238, Perft(board, 4));
		}

		[Test]
		public void Position3Depth5Positions674624() {
			var board = new Game();
			board.Load("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -");
			Assert.AreEqual(674624, Perft(board, 5));
		}

		[Test]
		public void Position5Depth1Positions44() {
			var board = new Game();
			board.Load("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8");
			Assert.AreEqual(44, Perft(board, 1));
		}

		[Test]
		public void Position5Depth2Positions1486() {
			var board = new Game();
			board.Load("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8");
			Assert.AreEqual(1486, Perft(board, 2));
		}

		[Test]
		public void Position5Depth3Positions62379() {
			var board = new Game();
			board.Load("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8");
			Assert.AreEqual(62379, Perft(board, 3));
		}

		[Test]
		public void Position5Depth4Positions2103487() {
			var board = new Game();
			board.Load("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8");
			Assert.AreEqual(2103487, Perft(board, 4));
		}

		[Test]
		public void Position5Depth5Positions89941194() {
			var board = new Game();
			board.Load("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8");
			Assert.AreEqual(89941194, Perft(board, 5));
		}
	}
}
