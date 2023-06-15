using System.Collections;
using System.Collections.Generic;

namespace Chess {
	public class Board : IEnumerable<Piece> {
		public const int SIZE = 8;
		public const int AREA = SIZE * SIZE;

		private readonly Piece[] squares = new Piece[AREA];

		public Piece this[int index] {
			get => squares[index];
			set => squares[index] = value;
		}

		public static int GetFile(int squareIndex) {
			return squareIndex % SIZE;
		}

		public static int GetRank(int squareIndex) {
			return squareIndex / SIZE;
		}

		public IEnumerator<Piece> GetEnumerator() {
			return ((IEnumerable<Piece>)squares).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return squares.GetEnumerator();
		}
	}
}
