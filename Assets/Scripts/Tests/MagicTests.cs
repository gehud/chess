using NUnit.Framework;
using System;
using Unity.Collections;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace Chess.Tests
{
    public class MagicTests
    {
        public static ulong GetRookAttacksSlow(Square square, ulong blockers)
        {
            ulong attacks = 0;
            int rank = square.Rank;
            int file = square.File;

            // North.
            for (int r = rank + 1; r < 8; r++)
            {
                attacks |= 1UL << r * 8 + file;
                if ((blockers & 1UL << r * 8 + file) != 0)
                    break;
            }

            // South.
            for (int r = rank - 1; r >= 0; r--)
            {
                attacks |= 1UL << r * 8 + file;
                if ((blockers & 1UL << r * 8 + file) != 0)
                    break;
            }

            // East.
            for (int f = file + 1; f < 8; f++)
            {
                attacks |= 1UL << rank * 8 + f;
                if ((blockers & 1UL << rank * 8 + f) != 0)
                    break;
            }

            // West.
            for (int f = file - 1; f >= 0; f--)
            {
                attacks |= 1UL << rank * 8 + f;
                if ((blockers & 1UL << rank * 8 + f) != 0)
                    break;
            }

            return attacks;
        }

        public static ulong GetBishopAttacksSlow(Square square, ulong blockers)
        {
            ulong attacks = 0;
            int rank = square.Rank;
            int file = square.File;

            // Northeast.
            for (int r = rank + 1, f = file + 1; r < 8 && f < 8; r++, f++)
            {
                attacks |= 1UL << r * 8 + f;
                if ((blockers & 1UL << r * 8 + f) != 0)
                    break;
            }

            // Southeast.
            for (int r = rank - 1, f = file + 1; r >= 0 && f < 8; r--, f++)
            {
                attacks |= 1UL << r * 8 + f;
                if ((blockers & 1UL << r * 8 + f) != 0)
                    break;
            }

            // Southwest.
            for (int r = rank - 1, f = file - 1; r >= 0 && f >= 0; r--, f--)
            {
                attacks |= 1UL << r * 8 + f;
                if ((blockers & 1UL << r * 8 + f) != 0)
                    break;
            }

            // Northwest.
            for (int r = rank + 1, f = file - 1; r < 8 && f >= 0; r++, f--)
            {
                attacks |= 1UL << r * 8 + f;
                if ((blockers & 1UL << r * 8 + f) != 0)
                    break;
            }

            return attacks;
        }

        [Test]
        [TestCase(SquareName.A1, 0x0040201008040200UL)]
        [TestCase(SquareName.H1, 0x0002040810204080UL)]
        [TestCase(SquareName.E4, 0x0028440280442800UL)]
        public void GetRookMask_ReturnsCorrectMask(SquareName square, ulong expectedMask)
        {
            var board = new Board(Allocator.Temp);

            var actual = board.GetRookMask((Square)square);
            Assert.AreEqual((Bitboard)expectedMask, actual);

            board.Dispose();
        }

        [Test]
        [TestCase(SquareName.A1, 0x0040201008040200UL)]
        [TestCase(SquareName.H1, 0x0002040810204080UL)]
        [TestCase(SquareName.E4, 0x0028440280442800UL)]
        public void GetBishopMask_ReturnsCorrectMask(SquareName square, ulong expectedMask)
        {
            var board = new Board(Allocator.Temp);

            var actual = board.GetBishopMask((Square)square);
            Assert.AreEqual((Bitboard)expectedMask, actual);

            board.Dispose();
        }

        [Test]
        public void GetRookAttacks_OnEmptyBoard_MatchesExpected()
        {
            var board = new Board(Allocator.Temp);

            var square = (Square)SquareName.D4;
            var attacks = board.GetRookAttacks(square, Bitboard.Empty);

            // Should attack all squares in rank and file.
            Assert.IsTrue(attacks.Contains(SquareName.D1));
            Assert.IsTrue(attacks.Contains(SquareName.D8));
            Assert.IsTrue(attacks.Contains(SquareName.A4));
            Assert.IsTrue(attacks.Contains(SquareName.H4));

            // Should not attack own square.
            Assert.IsFalse(attacks.Contains(square));

            board.Dispose();
        }

        [Test]
        public void GetRookAttacks_WithBlockers_StopsAtBlockers()
        {
            var board = new Board(Allocator.Temp);

            var square = (Square)SquareName.D4;
            var blockers = Bitboard.Empty.With(SquareName.D6).With(SquareName.B4);
            var attacks = board.GetRookAttacks(square, blockers);

            // Should attack up to but not past blockers.
            Assert.IsTrue(attacks.Contains(SquareName.D5));
            Assert.IsFalse(attacks.Contains(SquareName.D7));

            Assert.IsTrue(attacks.Contains(SquareName.C4));
            Assert.IsFalse(attacks.Contains(SquareName.A4));

            board.Dispose();
        }

        [Test]
        public void GetBishopAttacks_OnEmptyBoard_MatchesExpected()
        {
            var board = new Board(Allocator.Temp);

            var square = (Square)SquareName.D4;
            var attacks = board.GetBishopAttacks(square, Bitboard.Empty);

            // Should attack all squares on both diagonals.
            Assert.True(attacks.Contains(SquareName.A1));
            Assert.True(attacks.Contains(SquareName.G1));
            Assert.True(attacks.Contains(SquareName.H8));
            Assert.True(attacks.Contains(SquareName.A7));

            // Should not attack own square.
            Assert.False(attacks.Contains(square));

            board.Dispose();
        }

        [Test]
        public void GetBishopAttacks_WithBlockers_StopsAtBlockers()
        {
            var board = new Board(Allocator.Temp);

            var square = (Square)SquareName.D4;
            var blockers = Bitboard.Empty.With(SquareName.F6).With(SquareName.B2);
            var attacks = board.GetBishopAttacks(square, blockers);

            // Should attack up to but not past blockers.
            Assert.True(attacks.Contains(SquareName.E5));
            Assert.False(attacks.Contains(SquareName.G7));

            Assert.True(attacks.Contains(SquareName.C3));
            Assert.False(attacks.Contains(SquareName.A1));

            board.Dispose();
        }

        [Test]
        [TestCase(SquareName.A1)]
        [TestCase(SquareName.H8)]
        [TestCase(SquareName.E4)]
        public void MagicLookup_MatchesSlowVersion_ForRook(SquareName square)
        {
            var board = new Board(Allocator.Temp);

            // Test that magic lookup matches the slow version for random blocker configurations.
            var random = new Random((uint)DateTime.Now.Millisecond);

            for (int i = 0; i < 10; i++)
            {
                var blockers = random.NextULong();
                var magicResult = board.GetRookAttacks((Square)square, (Bitboard)blockers);
                var slowResult = GetRookAttacksSlow((Square)square, blockers);
                Assert.AreEqual(slowResult, magicResult);
            }

            board.Dispose();
        }

        [Test]
        [TestCase(SquareName.B2)]
        [TestCase(SquareName.G7)]
        [TestCase(SquareName.D5)]
        public void MagicLookup_MatchesSlowVersion_ForBishop(SquareName square)
        {
            var board = new Board(Allocator.Temp);

            var random = new Random((uint)DateTime.Now.Millisecond);

            for (int i = 0; i < 10; i++)
            {
                var blockers = random.NextULong();
                var magicResult = board.GetBishopAttacks((Square)square, (Bitboard)blockers);
                var slowResult = GetBishopAttacksSlow((Square)square, blockers);
                Assert.AreEqual(slowResult, magicResult);
            }

            board.Dispose();
        }

        [Test]
        public void AttackTables_AreCorrectlyInitialized()
        {
            var board = new Board(Allocator.Temp);

            // Verify no empty entries in attack tables.
            for (var i = Square.MinIndex; i <= Square.MaxIndex; i++)
            {
                var square = new Square(i);
                var rookMask = board.GetRookMask(square);
                var rookBits = math.countbits((ulong)rookMask);
                var rookTable = board.Magic.OrthogonalAttacks[i];
                Assert.AreEqual(1 << rookBits, rookTable.Length);
                
                for (var j = 0; j < rookTable.Length; j++)
                {
                    Assert.IsFalse(rookTable[j].IsEmpty);
                }

                var bishopMask = board.GetBishopMask(square);
                var bishopBits = math.countbits((ulong)bishopMask);
                var bishopTable = board.Magic.DiagonalAttacks[i];
                Assert.AreEqual(1 << bishopBits, bishopTable.Length);

                for (var j = 0; j < bishopTable.Length; j++)
                {
                    Assert.IsFalse(bishopTable[j].IsEmpty);
                }
            }

            board.Dispose();
        }
    }
}
