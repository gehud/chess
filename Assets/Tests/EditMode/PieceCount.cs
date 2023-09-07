using NUnit.Framework;

namespace Chess.Tests.EditMode {
	public class PieceCount {
		private void GenerateMoves(Game gameA, int depth) {
			if (depth == 0)
				return;

			var moves = gameA.GenerateMoves();
			foreach (var move in moves) {
				gameA.Move(move);
				GenerateMoves(gameA, depth - 1);
				gameA.Undo();
			}
		}

		private void CompareGames(Game gameA, Game gameB) {
			Assert.AreEqual(
				gameA.GetPieceCount(PieceType.Pawn, PieceColor.White),
				gameB.GetPieceCount(PieceType.Pawn, PieceColor.White)
			);
			Assert.AreEqual(
				gameA.GetPieceCount(PieceType.Pawn, PieceColor.Black),
				gameB.GetPieceCount(PieceType.Pawn, PieceColor.Black)
			);
			Assert.AreEqual(
				gameA.GetPieceCount(PieceType.Bishop, PieceColor.White),
				gameB.GetPieceCount(PieceType.Bishop, PieceColor.White)
			);
			Assert.AreEqual(
				gameA.GetPieceCount(PieceType.Bishop, PieceColor.Black),
				gameB.GetPieceCount(PieceType.Bishop, PieceColor.Black)
			);
			Assert.AreEqual(
				gameA.GetPieceCount(PieceType.Knight, PieceColor.White),
				gameB.GetPieceCount(PieceType.Knight, PieceColor.White)
			);
			Assert.AreEqual(
				gameA.GetPieceCount(PieceType.Knight, PieceColor.Black),
				gameB.GetPieceCount(PieceType.Knight, PieceColor.Black)
			);
			Assert.AreEqual(
				gameA.GetPieceCount(PieceType.Rook, PieceColor.White),
				gameB.GetPieceCount(PieceType.Rook, PieceColor.White)
			);
			Assert.AreEqual(
				gameA.GetPieceCount(PieceType.Rook, PieceColor.Black),
				gameB.GetPieceCount(PieceType.Rook, PieceColor.Black)
			);
			Assert.AreEqual(
				gameA.GetPieceCount(PieceType.Queen, PieceColor.White),
				gameB.GetPieceCount(PieceType.Queen, PieceColor.White)
			);
			Assert.AreEqual(
				gameA.GetPieceCount(PieceType.Queen, PieceColor.Black),
				gameB.GetPieceCount(PieceType.Queen, PieceColor.Black)
			);
		}

		[Test]
		public void Depth1() {
			var gameA = new Game();
			gameA.Load("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1 ");
			var gameB = new Game();
			gameB.Load("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1 ");
			GenerateMoves(gameA, 1);
			GenerateMoves(gameB, 1);
			gameB.CountBoard();
			CompareGames(gameA, gameB);
		}

		[Test]
		public void Depth2() {
			var gameA = new Game();
			gameA.Load("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1 ");
			var gameB = new Game();
			gameB.Load("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1 ");
			GenerateMoves(gameA, 2);
			GenerateMoves(gameB, 2);
			gameB.CountBoard();
			CompareGames(gameA, gameB);
		}

		[Test]
		public void Depth3() {
			var gameA = new Game();
			gameA.Load("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1 ");
			var gameB = new Game();
			gameB.Load("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1 ");
			GenerateMoves(gameA, 3);
			GenerateMoves(gameB, 3);
			gameB.CountBoard();
			CompareGames(gameA, gameB);
		}

		[Test]
		public void Depth4() {
			var gameA = new Game();
			gameA.Load("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1 ");
			var gameB = new Game();
			gameB.Load("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1 ");
			GenerateMoves(gameA, 4);
			GenerateMoves(gameB, 4);
			gameB.CountBoard();
			CompareGames(gameA, gameB);
		}

		[Test]
		public void Depth5() {
			var gameA = new Game();
			gameA.Load("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1 ");
			var gameB = new Game();
			gameB.Load("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1 ");
			GenerateMoves(gameA, 5);
			GenerateMoves(gameB, 5);
			gameB.CountBoard();
			CompareGames(gameA, gameB);
		}
	}
}
