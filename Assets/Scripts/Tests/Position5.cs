using NUnit.Framework;

namespace Chess.Tests
{
    public class Position5
    {
        public const string Fen = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8";

        [Test]
        public void Depth1()
        {
            new Perft(Fen, 1, 44).Run();
        }

        [Test]
        public void Depth2()
        {
            new Perft(Fen, 2, 1_486).Run();
        }

        [Test]
        public void Depth3()
        {
            new Perft(Fen, 3, 62_379).Run();
        }

        [Test]
        public void Depth4()
        {
            new Perft(Fen, 4, 2_103_487).Run();
        }

        [Test]
        public void Depth5()
        {
            new Perft(Fen, 5, 89_941_194).Run();
        }
    }
}
