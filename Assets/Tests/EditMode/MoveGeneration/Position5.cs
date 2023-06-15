using NUnit.Framework;

namespace Chess.Tests.EditMode.MoveGeneration {
	public class Position5 : Position {
		public Position5() : base("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8") { }

		[Test]
		public void Depth1Positions44() {
			Assert(1, 44);
		}

		[Test]
		public void Depth2Positions1486() {
			Assert(2, 1486);
		}

		[Test]
		public void Depth3Positions62379() {
			Assert(3, 62379);
		}

		[Test]
		public void Depth4Positions2103487() {
			Assert(4, 2103487);
		}

		[Test]
		public void Depth5Positions89941194() {
			Assert(5, 89941194);
		}
	}
}
