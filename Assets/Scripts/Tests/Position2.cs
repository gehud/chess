using NUnit.Framework;

namespace Chess.Tests
{
    public class Position2
    {
        public const string Fen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - ";

        [Test]
        public void Depth1()
        {
            new Perft(Fen, 1, 48).Run();
        }

        [Test]
        public void Depth2()
        {
            new Perft(Fen, 2, 2039).Run();
        }

        [Test]
        public void Depth3()
        {
            new Perft(Fen, 3, 97_862).Run();
        }

        [Test]
        public void Depth4()
        {
            new Perft(Fen, 4, 4_085_603).Run();
        }

        [Test]
        public void Depth5()
        {
            new Perft(Fen, 5, 193_690_690).Run();
        }
    }
}
