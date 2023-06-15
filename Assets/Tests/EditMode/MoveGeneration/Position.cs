using Chess.Tests.EditMode.Utilities;

namespace Chess.Tests.EditMode.MoveGeneration {
	public class Position {
		private readonly string fen;

		public Position(string fen) {
			this.fen = fen;
		}

		public void Assert(int depth, int count) {
			var game = new Game();
			game.Load(fen);
			NUnit.Framework.Assert.AreEqual(count, PerftUtility.Perft(game, depth));
		}
	}
}
