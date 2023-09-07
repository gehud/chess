using NUnit.Framework;

namespace Chess.Tests.EditMode.MoveGeneration {
	public class Position2 : Position {
		public Position2() : base("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - ") { }

		[Test]
		public void Depth1Positions48() {
			Assert(1, 48);
		}

		[Test]
		public void Depth2Positions2039() {
			Assert(2, 2039);
		}

		[Test]
		public void Depth3Positions97862() {
			Assert(3, 97862);
		}

		[Test]
		public void Depth4Positions4085603() {
			Assert(4, 4085603);
		}

		[Test]
		public void Depth5Positions193690690() {
			Assert(5, 193690690);
		}
	}
}
