using NUnit.Framework;

namespace Chess.Tests.EditMode.MoveGeneration {
	public class Position6 : Position {
		public Position6() : base("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10") { }

		[Test]
		public void Depth1Positions46() {
			Assert(1, 46);
		}

		[Test]
		public void Depth2Positions2079() {
			Assert(2, 2079);
		}

		[Test]
		public void Depth3Positions89890() {
			Assert(3, 89890);
		}

		[Test]
		public void Depth4Positions3894594() {
			Assert(4, 3894594);
		}

		[Test]
		public void Depth5Positions164075551() {
			Assert(5, 164075551);
		}
	}
}
