using NUnit.Framework;
using System;
using Unity.Collections;
using Random = Unity.Mathematics.Random;

namespace Chess.Tests
{
    public class MagicTests
    {
        [Test]
        [TestCase(SquareName.A1, 0x000101010101017EUL)]
        [TestCase(SquareName.H1, 0x008080808080807EUL)]
        [TestCase(SquareName.E4, 0x001010106E101000UL)]
        public void GetRookMask_ReturnsCorrectMask(SquareName square, ulong expectedMask)
        {
            var magic = new Magic(Allocator.Temp);

            var actual = magic.GetRookMask((Square)square);
            Assert.AreEqual((Bitboard)expectedMask, actual);

            magic.Dispose();
        }

        [Test]
        [TestCase(SquareName.A1, 0x0040201008040200UL)]
        [TestCase(SquareName.H1, 0x0002040810204000UL)]
        [TestCase(SquareName.E4, 0x0002442800284400UL)]
        public void GetBishopMask_ReturnsCorrectMask(SquareName square, ulong expectedMask)
        {
            var magic = new Magic(Allocator.Temp);

            var actual = magic.GetBishopMask((Square)square);
            Assert.AreEqual((Bitboard)expectedMask, actual);

            magic.Dispose();
        }

        [Test]
        public void GetRookAttacks_WithBlockers_StopsAtBlockers()
        {
            var magic = new Magic(Allocator.Temp);

            var square = (Square)SquareName.D4;
            var blockers = Bitboard.Empty.With(SquareName.D6).With(SquareName.B4);
            var attacks = magic.GetRookAttacks(square, blockers);

            // Should attack up to but not past blockers.
            Assert.IsTrue(attacks.Contains(SquareName.D5));
            Assert.IsFalse(attacks.Contains(SquareName.D7));

            Assert.IsTrue(attacks.Contains(SquareName.C4));
            Assert.IsFalse(attacks.Contains(SquareName.A4));

            magic.Dispose();
        }

        [Test]
        public void GetBishopAttacks_WithBlockers_StopsAtBlockers()
        {
            var magic = new Magic(Allocator.Temp);

            var square = (Square)SquareName.D4;
            var blockers = Bitboard.Empty.With(SquareName.F6).With(SquareName.B2);
            var attacks = magic.GetBishopAttacks(square, blockers);

            // Should attack up to but not past blockers.
            Assert.True(attacks.Contains(SquareName.E5));
            Assert.False(attacks.Contains(SquareName.G7));

            Assert.True(attacks.Contains(SquareName.C3));
            Assert.False(attacks.Contains(SquareName.A1));

            magic.Dispose();
        }

        [Test]
        [TestCase(SquareName.A1)]
        [TestCase(SquareName.H8)]
        [TestCase(SquareName.E4)]
        public void MagicLookup_MatchesSlowVersion_ForRook(SquareName square)
        {
            var magic = new Magic(Allocator.Temp);

            // Test that magic lookup matches the slow version for random blocker configurations.
            var random = new Random((uint)DateTime.Now.Millisecond);

            for (int i = 0; i < 10; i++)
            {
                var blockers = (Bitboard)random.NextULong();
                var magicResult = magic.GetRookAttacks((Square)square, blockers);
                var slowResult = Magic.GenerateRookAttacks((Square)square, blockers);
                Assert.AreEqual(slowResult, magicResult);
            }

            magic.Dispose();
        }

        [Test]
        [TestCase(SquareName.B2)]
        [TestCase(SquareName.G7)]
        [TestCase(SquareName.D5)]
        public void MagicLookup_MatchesSlowVersion_ForBishop(SquareName square)
        {
            var magic = new Magic(Allocator.Temp);

            var random = new Random((uint)DateTime.Now.Millisecond);

            for (int i = 0; i < 10; i++)
            {
                var blockers = (Bitboard)random.NextULong();
                var magicResult = magic.GetBishopAttacks((Square)square, blockers);
                var slowResult = Magic.GenerateBishopAttacks((Square)square, blockers);
                Assert.AreEqual(slowResult, magicResult);
            }

            magic.Dispose();
        }
    }
}
