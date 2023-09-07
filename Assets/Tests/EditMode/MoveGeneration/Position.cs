using Chess.Utilities;

namespace Chess.Tests.EditMode.MoveGeneration {
	public class Position {
		private readonly string fen;

		public Position(string fen) {
			this.fen = fen;
		}

		public void Assert(int depth, int count) {
			var gameA = new Game();
			gameA.Load(fen);
			NUnit.Framework.Assert.AreEqual(count, PerftUtility.Perft(gameA, depth));
		}
	}
}
