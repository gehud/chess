using NUnit.Framework;

namespace Chess.Tests
{
    public class Position1
    {
        public const string Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        [Test]
        public void Depth1()
        {
            new Perft(Fen, 1, 20).Run();
        }

        [Test]
        public void Depth2()
        {
            new Perft(Fen, 2, 400).Run();
        }

        [Test]
        public void Depth3()
        {
            new Perft(Fen, 3, 8_902).Run();
        }

        [Test]
        public void Depth4()
        {
            new Perft(Fen, 4, 197_281).Run();
        }

        [Test]
        public void Depth5()
        {
            new Perft(Fen, 5, 4_865_609).Run();
        }
    }
}
