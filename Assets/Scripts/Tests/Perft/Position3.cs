using NUnit.Framework;

namespace Chess.Tests
{
    public class Position3
    {
        public const string Fen = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1";

        [Test]
        public void Depth1()
        {
            new Perft(Fen, 1, 14).Run();
        }

        [Test]
        public void Depth2()
        {
            new Perft(Fen, 2, 191).Run();
        }

        [Test]
        public void Depth3()
        {
            new Perft(Fen, 3, 2_812).Run();
        }

        [Test]
        public void Depth4()
        {
            new Perft(Fen, 4, 43_238).Run();
        }

        [Test]
        public void Depth5()
        {
            new Perft(Fen, 5, 674_624).Run();
        }
    }
}
