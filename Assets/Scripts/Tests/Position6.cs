using NUnit.Framework;

namespace Chess.Tests
{
    public class Position6
    {
        public const string Fen = "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10";

        [Test]
        public void Depth1()
        {
            new Perft(Fen, 1, 46).Run();
        }

        [Test]
        public void Depth2()
        {
            new Perft(Fen, 2, 2_079).Run();
        }

        [Test]
        public void Depth3()
        {
            new Perft(Fen, 3, 89_890).Run();
        }

        [Test]
        public void Depth4()
        {
            new Perft(Fen, 4, 3_894_594).Run();
        }

        [Test]
        public void Depth5()
        {
            new Perft(Fen, 5, 164_075_551).Run();
        }
    }
}
