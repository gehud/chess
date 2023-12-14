using System.Collections.Generic;

namespace Chess.Utilities {
	public static class FENUtility {
		private static readonly Dictionary<char, Figure> pieceSybmolMap = new() {
			{ 'p', Figure.Pawn },
			{ 'n', Figure.Knight },
			{ 'b', Figure.Bishop },
			{ 'r', Figure.Rook },
			{ 'q', Figure.Queen },
			{ 'k', Figure.King },
		};

		public static void Load(string fen, Board board, ref State gameState, ref Color moveColor) {
			var state = fen.Split(' ');
			var squares = state[0];
			int file = 0, rank = 7;
			foreach (var symbol in squares) {
				if (symbol == '/') {
					file = 0;
					--rank;
				} else {
					if (char.IsDigit(symbol)) {
						file += (int)char.GetNumericValue(symbol);
					} else {
						var color = char.IsUpper(symbol) ? Color.White : Color.Black;
						var type = pieceSybmolMap[char.ToLower(symbol)];
						board[file + rank * Board.Size] = new Piece(type, color);
						++file;
					}
				}
			}

			moveColor = char.Parse(state[1]) == 'w' ? Color.White : Color.Black;

			var castlings = state[2];
			if (castlings != "-") {
				foreach (var castling in castlings) {
					if (castling == 'K')
						gameState.IsWhiteKingsideCastlingAvaible = true;
					else if (castling == 'k')
						gameState.IsBlackKingsideCastlingAvaible = true;
					else if (castling == 'Q')
						gameState.IsWhiteQueensideCastlingAvaible = true;
					else if (castling == 'q')
						gameState.IsBlackQueensideCastlingAvaible = true;
				}
			}

			var twoSquarePawnKey = state[3];
			if (twoSquarePawnKey != "-") {
				file = twoSquarePawnKey[0] - 'a';
				rank = (int)char.GetNumericValue(twoSquarePawnKey[1]) - 1;
				gameState.TwoSquarePawn = file + rank * Board.Size;
			}
		}
	}
}
