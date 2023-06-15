using System;

namespace Chess {
	[Flags]
	public enum MoveFlags {
		None = 0,
		QueenPromotion = 1 << 0,
		RookPromotion = 1 << 1,
		BishopPromotion = 1 << 2,
		KnightPromotion = 1 << 3,
		Promotion = QueenPromotion | RookPromotion | BishopPromotion | KnightPromotion
	}

	public readonly struct Move {
		public readonly int From;
		public readonly int To;
		public readonly MoveFlags Flags;

		public bool IsValid => From != To;

		public Move(int from, int to, MoveFlags flags = MoveFlags.None) {
			From = from;
			To = to;
			Flags = flags;
		}
	}
}