using NUnit.Framework;

namespace Chess.Tests.EditMode.MoveGeneration {
	public class Position4 : Position {
		public Position4() : base("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1") { }

		[Test]
		public void Depth1Positions6() {
			Assert(1, 6);
		}

		[Test]
		public void Depth2Positions264() {
			Assert(2, 264);
		}

		[Test]
		public void Depth3Positions9467() {
			Assert(3, 9467);
		}

		[Test]
		public void Depth4Positions422333() {
			Assert(4, 422333);
		}

		[Test]
		public void Depth5Positions15833292() {
			Assert(5, 15833292);
		}
	}
}
