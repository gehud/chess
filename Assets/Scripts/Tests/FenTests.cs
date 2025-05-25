using NUnit.Framework;
using Unity.Collections;

namespace Chess.Tests
{
    public class FenTests
    {
        [Test]
        public void Position1()
        {
            var fen = new Fen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", Allocator.Temp);

            Assert.AreEqual(true, fen.IsWhiteAllied);
            Assert.AreEqual(true, fen.WhiteCastleKingside);
            Assert.AreEqual(true, fen.WhiteCastleQueenside);
            Assert.AreEqual(true, fen.BlackCastleKingside);
            Assert.AreEqual(true, fen.BlackCastleQueenside);
            Assert.AreEqual(0, fen.EnPassantFile);
            Assert.AreEqual(0, fen.FiftyMovePlyCount);
            Assert.AreEqual(1, fen.MoveCount);

            fen.Dispose();
        }

        [Test]
        public void Position3()
        {
            var fen = new Fen("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1", Allocator.Temp);

            Assert.AreEqual(true, fen.IsWhiteAllied);
            Assert.AreEqual(false, fen.WhiteCastleKingside);
            Assert.AreEqual(false, fen.WhiteCastleQueenside);
            Assert.AreEqual(false, fen.BlackCastleKingside);
            Assert.AreEqual(false, fen.BlackCastleQueenside);
            Assert.AreEqual(0, fen.EnPassantFile);
            Assert.AreEqual(0, fen.FiftyMovePlyCount);
            Assert.AreEqual(1, fen.MoveCount);

            fen.Dispose();
        }

        [Test]
        public void Position4()
        {
            var fen = new Fen("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1", Allocator.Temp);

            Assert.AreEqual(true, fen.IsWhiteAllied);
            Assert.AreEqual(false, fen.WhiteCastleKingside);
            Assert.AreEqual(false, fen.WhiteCastleQueenside);
            Assert.AreEqual(true, fen.BlackCastleKingside);
            Assert.AreEqual(true, fen.BlackCastleQueenside);
            Assert.AreEqual(0, fen.EnPassantFile);
            Assert.AreEqual(0, fen.FiftyMovePlyCount);
            Assert.AreEqual(1, fen.MoveCount);

            fen.Dispose();
        }

        [Test]
        public void Position5()
        {
            var fen = new Fen("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8", Allocator.Temp);

            Assert.AreEqual(true, fen.IsWhiteAllied);
            Assert.AreEqual(true, fen.WhiteCastleKingside);
            Assert.AreEqual(true, fen.WhiteCastleQueenside);
            Assert.AreEqual(false, fen.BlackCastleKingside);
            Assert.AreEqual(false, fen.BlackCastleQueenside);
            Assert.AreEqual(0, fen.EnPassantFile);
            Assert.AreEqual(1, fen.FiftyMovePlyCount);
            Assert.AreEqual(8, fen.MoveCount);

            fen.Dispose();
        }

        [Test]
        public void Position6()
        {
            var fen = new Fen("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10", Allocator.Temp);

            Assert.AreEqual(true, fen.IsWhiteAllied);
            Assert.AreEqual(false, fen.WhiteCastleKingside);
            Assert.AreEqual(false, fen.WhiteCastleQueenside);
            Assert.AreEqual(false, fen.BlackCastleKingside);
            Assert.AreEqual(false, fen.BlackCastleQueenside);
            Assert.AreEqual(0, fen.EnPassantFile);
            Assert.AreEqual(0, fen.FiftyMovePlyCount);
            Assert.AreEqual(10, fen.MoveCount);

            fen.Dispose();
        }
    }
}
