using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Chess
{
    public struct Magic : IDisposable
    {
        public NativeArray<ulong> RookMagics;
        public NativeArray<ulong> BishopMagics;
        public NativeArray<Bitboard> RookMasks;
        public NativeArray<Bitboard> BishopMasks;
        public NativeArray<NativeArray<Bitboard>> RookAttacks;
        public NativeArray<NativeArray<Bitboard>> BishopAttacks;

        public Magic(Allocator allocator)
        {
            RookMagics = new(new ulong[]
            {
                0x0080001020400080UL, 0x0040001000200040UL, 0x0080080010002000UL, 0x0080040008001000UL,
                0x0080020004000800UL, 0x0080010002000400UL, 0x0080008001000200UL, 0x0080000400800100UL,
                0x0000800020400080UL, 0x0000400020005000UL, 0x0000801000200040UL, 0x0000800800100020UL,
                0x0000800400080010UL, 0x0000800200040008UL, 0x0000800100020004UL, 0x0000800040008002UL,
                0x0000208000400080UL, 0x0000404000201000UL, 0x0000808010002000UL, 0x0000808008001000UL,
                0x0000808004000800UL, 0x0000808002000400UL, 0x0000808001000200UL, 0x0000808000800100UL,
                0x0000204000400080UL, 0x0000202000804000UL, 0x0000401000802000UL, 0x0000400800801000UL,
                0x0000400400800800UL, 0x0000400200800400UL, 0x0000400100800200UL, 0x0000400080800100UL,
                0x0000200040004080UL, 0x0000400020004040UL, 0x0000800010008020UL, 0x0000800008008010UL,
                0x0000800004008008UL, 0x0000800002004004UL, 0x0000800001002002UL, 0x0000800000801001UL,
                0x0000200000400040UL, 0x0000400000200080UL, 0x0000800000100080UL, 0x0000800000080080UL,
                0x0000800000040080UL, 0x0000800000020040UL, 0x0000800000010020UL, 0x0000800000008010UL,
                0x0000200000800040UL, 0x0000400000400080UL, 0x0000800000200080UL, 0x0000800000100080UL,
                0x0000800000080080UL, 0x0000800000040080UL, 0x0000800000020040UL, 0x0000800000010020UL,
                0x0000204000008000UL, 0x0000402000004000UL, 0x0000801000002000UL, 0x0000800800001000UL,
                0x0000800400000800UL, 0x0000800200000400UL, 0x0000800100000200UL, 0x0000800080000100UL,
                0x0000200040000080UL, 0x0000400020000040UL, 0x0000800010000020UL, 0x0000800008000010UL,
                0x0000800004000008UL, 0x0000800002000004UL, 0x0000800001000002UL, 0x0000800000800001UL
            }, allocator);

            BishopMagics = new(new ulong[]
            {
                0x0002020202020200UL, 0x0002020202020000UL, 0x0004010202000000UL, 0x0004040080000000UL,
                0x0001104000000000UL, 0x0000821040000000UL, 0x0000410410400000UL, 0x0000104104104000UL,
                0x0000040404040400UL, 0x0000020202020200UL, 0x0000040102020000UL, 0x0000040400800000UL,
                0x0000011040000000UL, 0x0000008210400000UL, 0x0000004104104000UL, 0x0000002082082000UL,
                0x0004000808080800UL, 0x0002000404040400UL, 0x0001000202020200UL, 0x0000800802004000UL,
                0x0000800400A00000UL, 0x0000200100884000UL, 0x0000400082082000UL, 0x0000200041041000UL,
                0x0002080010101000UL, 0x0001040008080800UL, 0x0000208004010400UL, 0x0000404004010200UL,
                0x0000840000802000UL, 0x0000404002011000UL, 0x0000808001041000UL, 0x0000404000820800UL,
                0x0001041000202000UL, 0x0000820800101000UL, 0x0000104400080800UL, 0x0000020080080080UL,
                0x0000404040040100UL, 0x0000808100020100UL, 0x0001010100020800UL, 0x0000808080010400UL,
                0x0000820820004000UL, 0x0000410410002000UL, 0x0000082088001000UL, 0x0000002011000800UL,
                0x0000080100400400UL, 0x0000010100800200UL, 0x0000040408000100UL, 0x0000040404000200UL,
                0x0000020801001000UL, 0x0004000208080800UL, 0x0002020040400400UL, 0x0001010020200200UL,
                0x0000808008009000UL, 0x0000408004011000UL, 0x0000204008021000UL, 0x0000102004010800UL,
                0x0000081002008400UL, 0x0000040801008200UL, 0x0000020400804100UL, 0x0000010200404080UL,
                0x0000008002020200UL, 0x0000040001010100UL, 0x0000020000808080UL, 0x0000010000804040UL,
                0x0000008000802020UL, 0x0000004000801010UL, 0x0000002000800808UL, 0x0000001000800404UL,
                0x0000000800800202UL, 0x0000000400800101UL, 0x0000040200010101UL, 0x0000020100008080UL
            }, allocator);

            RookMasks = new(Board.Area, allocator);
            BishopMasks = new(Board.Area, allocator);
            for (var i = Square.Min.Index; i <= Square.Max.Index; i++)
            {
                var square = new Square(i);
                RookMasks[i] = GenerateRookMask(square);
                BishopMasks[i] = GenerateBishopMask(square);
            }

            RookAttacks = new(Board.Area, allocator);
            BishopAttacks = new(Board.Area, allocator);
            for (var i = Square.Min.Index; i <= Square.Max.Index; i++)
            {
                var rookMask = RookMasks[i];
                var rookBits = math.countbits((ulong)rookMask);
                RookAttacks[i] = new(1 << rookBits, allocator);

                var bishopMask = BishopMasks[i];
                var bishopBits = math.countbits((ulong)bishopMask);
                BishopAttacks[i] = new(1 << bishopBits, allocator);

                var square = new Square(i);
                InitializeAttackTable(square, true);
                InitializeAttackTable(square, false);
            }
        }

        public Bitboard GetRookMask(Square square)
        {
            return RookMasks[square.Index];
        }

        public Bitboard GetBishopMask(Square square)
        {
            return BishopMasks[square.Index];
        }

        public readonly Bitboard GetRookAttacks(Square square, Bitboard blockers)
        {
#if USE_MAGIC
            var mask = RookMasks[square.Index];
            var magic = RookMagics[square.Index];
            var bits = math.countbits((ulong)mask);
            var key = ((blockers & mask) * (Bitboard)magic) >> (Board.Area - bits);
            return RookAttacks[square.Index][(int)(ulong)key];
#else
            return GenerateRookAttacks(square, blockers);
#endif
        }

        public readonly Bitboard GetBishopAttacks(Square square, Bitboard blockers)
        {
#if USE_MAGIC
            var mask = BishopMasks[square.Index];
            var magic = BishopMagics[square.Index];
            var bits = math.countbits((ulong)mask);
            var key = ((blockers & mask) * (Bitboard)magic) >> (Board.Area - bits);
            return BishopAttacks[square.Index][(int)(ulong)key];
#else
            return GenerateBishopAttacks(square, blockers);
#endif
        }

        public readonly Bitboard GetQueenAttacks(Square square, Bitboard blockers)
        {
            return GetRookAttacks(square, blockers) | GetBishopAttacks(square, blockers);
        }

        private void InitializeAttackTable(Square square, bool isRook)
        {
            var mask = isRook ? RookMasks[square.Index] : BishopMasks[square.Index];
            var magic = isRook ? RookMagics[square.Index] : BishopMagics[square.Index];
            var bits = math.countbits((ulong)mask);
            var table = isRook ? RookAttacks[square.Index] : BishopAttacks[square.Index];

            var blockers = Bitboard.Empty;

            do
            {
                var key = ((ulong)blockers * magic) >> (Board.Area - bits);
                var attacks = isRook ? GenerateRookAttacks(square, blockers) :
                    GenerateBishopAttacks(square, blockers);
                table[(int)key] = attacks;

                // Carry-Rippler trick to enumerate all subsets.
                blockers = (blockers - mask) & mask;
            }
            while (!blockers.IsEmpty);
        }

        public static Bitboard GenerateRookAttacks(Square square, Bitboard blockers)
        {
            var attacks = Bitboard.Empty;
            var file = square.File;
            var rank = square.Rank;

            // North.
            for (var r = rank + 1; r < 8; r++)
            {
                attacks.Include(file, r);
                if (blockers.Contains(file, r))
                    break;
            }

            // South.
            for (var r = rank - 1; r >= 0; r--)
            {
                attacks.Include(file, r);
                if (blockers.Contains(file, r))
                    break;
            }

            // East.
            for (var f = file + 1; f < 8; f++)
            {
                attacks.Include(f, rank);
                if (blockers.Contains(f, rank))
                    break;
            }

            // West.
            for (var f = file - 1; f >= 0; f--)
            {
                attacks.Include(f, rank);
                if (blockers.Contains(f, rank))
                    break;
            }

            return attacks;
        }

        public static Bitboard GenerateBishopAttacks(Square square, Bitboard blockers)
        {
            var attacks = Bitboard.Empty;
            var file = square.File;
            var rank = square.Rank;

            // NorthEast.
            for (int r = rank + 1, f = file + 1; r < 8 && f < 8; r++, f++)
            {
                attacks.Include(f, r);
                if (blockers.Contains(f, r))
                    break;
            }

            // SouthEast.
            for (int r = rank - 1, f = file + 1; r >= 0 && f < 8; r--, f++)
            {
                attacks.Include(f, r);
                if (blockers.Contains(f, r))
                    break;
            }

            // SouthWest.
            for (int r = rank - 1, f = file - 1; r >= 0 && f >= 0; r--, f--)
            {
                attacks.Include(f, r);
                if (blockers.Contains(f, r))
                    break;
            }

            // NorthWest.
            for (int r = rank + 1, f = file - 1; r < 8 && f >= 0; r++, f--)
            {
                attacks.Include(f, r);
                if (blockers.Contains(f, r))
                    break;
            }

            return attacks;
        }

        private static Bitboard GenerateRookMask(Square square)
        {
            var mask = Bitboard.Empty;
            var file = square.File;
            var rank = square.Rank;

            for (int r = rank + 1; r <= 6; r++)
                mask.Include(file, r);
            for (int r = rank - 1; r >= 1; r--)
                mask.Include(file, r);
            for (int f = file + 1; f <= 6; f++)
                mask.Include(f, rank);
            for (int f = file - 1; f >= 1; f--)
                mask.Include(f, rank);

            return mask;
        }

        private static Bitboard GenerateBishopMask(Square square)
        {
            var mask = Bitboard.Empty;
            var file = square.File;
            var rank = square.Rank;

            for (int r = rank + 1, f = file + 1; r <= 6 && f <= 6; r++, f++)
                mask.Include(f, r);
            for (int r = rank - 1, f = file + 1; r >= 1 && f <= 6; r--, f++)
                mask.Include(f, r);
            for (int r = rank + 1, f = file - 1; r <= 6 && f >= 1; r++, f--)
                mask.Include(f, r);
            for (int r = rank - 1, f = file - 1; r >= 1 && f >= 1; r--, f--)
                mask.Include(f, r);

            return mask;
        }

        public void Dispose()
        {
            RookMasks.Dispose();
            BishopMasks.Dispose();

            for (var i = 0; i < RookAttacks.Length; i++)
            {
                RookAttacks[i].Dispose();
            }

            RookAttacks.Dispose();

            for (var i = 0; i < BishopAttacks.Length; i++)
            {
                BishopAttacks[i].Dispose();
            }

            BishopAttacks.Dispose();

            RookMagics.Dispose();
            BishopMagics.Dispose();
        }
    }
}
