using NUnit.Framework;

namespace Chess.Tests.EditMode.MoveGeneration {
	public class Position1 : Position {
		public Position1() : base("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1 ") {}

		[Test]
		public void Depth1Positions20() {
			Assert(1, 20);
		}

		[Test]
		public void Depth2Positions400() {
			Assert(2, 400);
		}

		[Test]
		public void Depth3Positions8902() {
			Assert(3, 8902);
		}

		[Test]
		public void Depth4Positions197281() {
			Assert(4, 197281);
		}

		[Test]
		public void Depth5Positions4865609() {
			Assert(5, 4865609);
		}
	}
}
