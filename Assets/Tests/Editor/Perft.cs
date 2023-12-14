using Chess.Utilities;
using NUnit.Framework;
using Unity.Collections;

namespace Chess.Tests.Editor {
    public class Perft {
        [Test]
        public void InitialPositionDepth1() {
            using var game = new Game(Allocator.Persistent);
            game.Start();

            var nodes = PerftUtility.Perft(game, 1);
            Assert.AreEqual(20, nodes);
        }

        [Test]
        public void InitialPositionDepth2() {
            using var game = new Game(Allocator.Persistent);
            game.Start();

            var nodes = PerftUtility.Perft(game, 2);
            Assert.AreEqual(400, nodes);
        }

        [Test]
        public void InitialPositionDepth3() {
            using var game = new Game(Allocator.Persistent);
            game.Start();

            var nodes = PerftUtility.Perft(game, 3);
            Assert.AreEqual(8902, nodes);
        }

        [Test]
        public void InitialPositionDepth4() {
            using var game = new Game(Allocator.Persistent);
            game.Start();

            var nodes = PerftUtility.Perft(game, 4);
            Assert.AreEqual(197281, nodes);
        }

        [Test]
        public void InitialPositionDepth5() {
            using var game = new Game(Allocator.Persistent);
            game.Start();

            var nodes = PerftUtility.Perft(game, 5);
            Assert.AreEqual(4865609, nodes);
        }
    }
}