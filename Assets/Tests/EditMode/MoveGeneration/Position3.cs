using NUnit.Framework;

namespace Chess.Tests.EditMode.MoveGeneration {
	public class Position3 : Position {
		public Position3() : base("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -") { }

		[Test]
		public void Depth1Positions14() {
			Assert(1, 14);
		}

		[Test]
		public void Depth2Positions191() {
			Assert(2, 191);
		}

		[Test]
		public void Depth3Positions2812() {
			Assert(3, 2812);
		}

		[Test]
		public void Depth4Positions43238() {
			Assert(4, 43238);
		}

		[Test]
		public void Depth5Positions674624() {
			Assert(5, 674624);
		}
	}
}
