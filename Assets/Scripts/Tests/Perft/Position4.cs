using NUnit.Framework;

namespace Chess.Tests
{
    public class Position4
    {
        public const string Fen = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1";

        [Test]
        public void Depth1()
        {
            new Perft(Fen, 1, 6).Run();
        }

        [Test]
        public void Depth2()
        {
            new Perft(Fen, 2, 264).Run();
        }

        [Test]
        public void Depth3()
        {
            new Perft(Fen, 3, 9_467).Run();
        }

        [Test]
        public void Depth4()
        {
            new Perft(Fen, 4, 422_333).Run();
        }

        [Test]
        public void Depth5()
        {
            new Perft(Fen, 5, 15_833_292).Run();
        }
    }
}
