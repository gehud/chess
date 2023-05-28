using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using UnityEngine;

namespace Chess {
	public class Board {
		public const int SIZE = 8;
		public const int AREA = SIZE * SIZE;

		public Piece[] Pieces => pieces;

		private readonly Piece[] pieces = new Piece[AREA];

		public PieceColor MoveColor => color;

		private State state;
		private PieceColor color = PieceColor.White;

		private static readonly int[] squareOffsets = new int[] {
			8, -8, -1, 1, 7, -7, 9, -9
		};

		private static readonly int[] moveLimits = new int[AREA * 8];

		public List<Move> Moves => moves;

		private List<Move> moves;

		private struct State {
			public Move Move;
			public Piece Captured;
			public Move CastlingRook;
			public int TwoSquarePawn;
			public int EnPassantCaptured;
			public int PromotedPawn;
			public bool IsWhiteKingsideCastlingAvaible;
			public bool IsWhiteQueensideCastlingAvaible;
			public bool IsBlackKingsideCastlingAvaible;
			public bool IsBlackQueensideCastlingAvaible;
		}

		private readonly Stack<State> history = new();

		private readonly Dictionary<char, PieceType> pieceSybmolMap = new() {
			{ 'p', PieceType.Pawn },
			{ 'n', PieceType.Knight },
			{ 'b', PieceType.Bishop },
			{ 'r', PieceType.Rook },
			{ 'q', PieceType.Queen },
			{ 'k', PieceType.King },
		};

		private readonly string startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

		public Board() {
			state = new() {
				Captured = Piece.Empty,
				Move = new Move(),
				CastlingRook = new Move(),
				TwoSquarePawn = -1,
				EnPassantCaptured = -1,
				PromotedPawn = -1,
				IsWhiteKingsideCastlingAvaible = false,
				IsWhiteQueensideCastlingAvaible = false,
				IsBlackKingsideCastlingAvaible = false,
				IsBlackQueensideCastlingAvaible = false
			};
			ComputeMoveLimits();
		}

		public void Start() {
			Load(startFEN);
		}

		public void Load(string fen) {
			var state = fen.Split(' ');
			var board = state[0];
			int file = 0, rank = 7;
			foreach (var symbol in board) {
				if (symbol == '/') {
					file = 0;
					--rank;
				} else {
					if (char.IsDigit(symbol)) {
						file += (int)char.GetNumericValue(symbol);
					} else {
						var color = char.IsUpper(symbol) ? PieceColor.White : PieceColor.Black;
						var type = pieceSybmolMap[char.ToLower(symbol)];
						pieces[file + rank * SIZE] = new Piece(type, color);
						++file;
					}
				}
			}

			color = char.Parse(state[1]) == 'w' ? PieceColor.White : PieceColor.Black;

			var castlings = state[2];
			if (castlings != "-") {
				foreach (var castling in castlings) {
					if (castling == 'K')
						this.state.IsWhiteKingsideCastlingAvaible = true;
					else if (castling == 'k')
						this.state.IsBlackKingsideCastlingAvaible = true;
					else if (castling == 'Q')
						this.state.IsWhiteQueensideCastlingAvaible = true;
					else if (castling == 'q')
						this.state.IsBlackQueensideCastlingAvaible = true;
				}
			}

			var twoSquarePawnKey = state[3];
			if (twoSquarePawnKey != "-") {
				file = twoSquarePawnKey[0] - 'a';
				rank = (int)char.GetNumericValue(twoSquarePawnKey[1]) - 1;
				this.state.TwoSquarePawn = file + rank * SIZE;
			}
		}

		public bool IsCheckmate() => moves.Count == 0;

		private List<Move> GeneratePresudoMoves() {
			moves = new List<Move>();
			Parallel.For(0, AREA, (squareIndex) => {
				if (pieces[squareIndex].Color == color)
					GenerateMove(squareIndex);
			});

			return moves;
		}

		public List<Move> GenerateMoves() {
			var presudoMoves = GeneratePresudoMoves();
			var legalMoves = new List<Move>();
			foreach (var move in presudoMoves) {
				Move(move);
				int kingIndex = -1;
				Parallel.For(0, AREA, (index, controll) => {
					if (pieces[index].Type == PieceType.King && pieces[index].Color != color) {
						kingIndex = index;
						controll.Break();
					}
				});
				var opopnentResoponses = GeneratePresudoMoves();
				if (!opopnentResoponses.AsParallel().Any(move => move.To == kingIndex)) {
					legalMoves.Add(move);
				}
				Undo();
			}
			moves = legalMoves;
			return moves;
		}

		public void Move(Move move) {
			history.Push(state);
			state.CastlingRook = new Move();
			state.EnPassantCaptured = -1;
			state.PromotedPawn = -1;
			var piece = pieces[move.From];
			if (piece.Type == PieceType.King) {
				bool isWhite = piece.Color == PieceColor.White;
				bool isKingCastling = move.To == move.From + GetSquareOffset(Direction.East) * 2;
				bool isQueenCastling = move.To == move.From + GetSquareOffset(Direction.West) * 2;
				if (isKingCastling) {
					if (isWhite && state.IsWhiteKingsideCastlingAvaible || !isWhite && state.IsBlackKingsideCastlingAvaible) {
						Move rookMove = new Move(
							move.From + GetSquareOffset(Direction.East) * 3,
							move.From + GetSquareOffset(Direction.East)
						);
						pieces[rookMove.To] = pieces[rookMove.From];
						pieces[rookMove.From] = Piece.Empty;
						state.CastlingRook = rookMove;
					}
				} else if (isQueenCastling) {
					if (isWhite && state.IsWhiteQueensideCastlingAvaible || !isWhite && state.IsBlackQueensideCastlingAvaible) {
						Move rookMove = new Move(
							move.From + GetSquareOffset(Direction.West) * 4,
							move.From + GetSquareOffset(Direction.West)
						);
						pieces[rookMove.To] = pieces[rookMove.From];
						pieces[rookMove.From] = Piece.Empty;
						state.CastlingRook = rookMove;
					}
				}
				if (isWhite) {
					state.IsWhiteKingsideCastlingAvaible = false;
					state.IsWhiteQueensideCastlingAvaible = false;
				} else {
					state.IsBlackKingsideCastlingAvaible = false;
					state.IsBlackQueensideCastlingAvaible = false;
				}
			} else if (piece.Type == PieceType.Rook) {
				bool isWhite = piece.Color == PieceColor.White;
				if (isWhite) {
					if (move.From == 0)
						state.IsWhiteQueensideCastlingAvaible = false;
					else if (move.From == 7)
						state.IsWhiteKingsideCastlingAvaible = false;
				} else {
					if (move.From == 56)
						state.IsBlackQueensideCastlingAvaible = false;
					else if (move.From == 64)
						state.IsBlackKingsideCastlingAvaible = false;
				}
			} else if (piece.Type == PieceType.Pawn) {
				bool isWhite = piece.Color == PieceColor.White;
				int rank = GetRank(move.To);

				// Promotion.
				if (isWhite && rank == 7 || !isWhite && rank == 0) {
					state.PromotedPawn = move.To;
					pieces[move.From] = new Piece(PieceType.Queen, piece.Color);
				}

				int moveDifference = move.To - move.From;

				// En Passant.
				if (state.TwoSquarePawn != -1 && 
					Mathf.Abs(move.To - state.TwoSquarePawn) == 8 &&
					(Mathf.Abs(moveDifference) == 7 || Mathf.Abs(moveDifference) == 9) &&
					pieces[state.TwoSquarePawn].Color != color) {
					state.EnPassantCaptured = state.TwoSquarePawn;
					pieces[state.EnPassantCaptured] = Piece.Empty;
				}

				if (moveDifference == 16 || moveDifference == -16) {
					state.TwoSquarePawn = move.To;
				} else {
					state.TwoSquarePawn = -1;
				}
			} else {
				state.TwoSquarePawn = -1;
			}

			state.Captured = pieces[move.To];
			pieces[move.To] = pieces[move.From];
			pieces[move.From] = Piece.Empty;
			state.Move = move;
			SwapMoveColor();
		}

		public void Undo() {
			if (state.EnPassantCaptured != -1) {
				pieces[state.EnPassantCaptured] = new Piece(PieceType.Pawn, color);
			}

			if (state.CastlingRook.IsValid) {
				pieces[state.CastlingRook.From] = pieces[state.CastlingRook.To];
				pieces[state.CastlingRook.To] = Piece.Empty;
			}

			SwapMoveColor();

			if (state.PromotedPawn != -1) {
				pieces[state.PromotedPawn] = new Piece(PieceType.Pawn, color);
			}

			pieces[state.Move.From] = pieces[state.Move.To];
			pieces[state.Move.To] = state.Captured;
			state = history.Pop();
		}

		private void ComputeMoveLimits() {
			Parallel.For(0, AREA, (index) => {
				int file = index % SIZE;
				int rank = index / SIZE;

				int northSquareCount = SIZE - 1 - rank;
				int southSquareCount = rank;
				int westSquareCount = file;
				int eastSquareCount = SIZE - 1 - file;

				moveLimits[index + AREA * 0] = northSquareCount;
				moveLimits[index + AREA * 1] = southSquareCount;
				moveLimits[index + AREA * 2] = westSquareCount;
				moveLimits[index + AREA * 3] = eastSquareCount;
				moveLimits[index + AREA * 4] = Mathf.Min(northSquareCount, westSquareCount);
				moveLimits[index + AREA * 5] = Mathf.Min(southSquareCount, eastSquareCount);
				moveLimits[index + AREA * 6] = Mathf.Min(northSquareCount, eastSquareCount);
				moveLimits[index + AREA * 7] = Mathf.Min(southSquareCount, westSquareCount);
			});
		}

		private int GetFile(int squareIndex) {
			return squareIndex % 8;
		}

		private int GetRank(int squareIndex) {
			return squareIndex / 8;
		}

		private int GetMoveLimit(int squareIndex, int directionindex) {
			return moveLimits[squareIndex + directionindex * AREA];
		}

		private int GetSquareOffset(Direction direction) {
			return squareOffsets[(int)direction];
		}

		private void GeneratePawnMove(int squareIndex) {
			var color = pieces[squareIndex].Color;

			bool isInVertivalBounds = color == PieceColor.White ? GetRank(squareIndex) < 7 : GetRank(squareIndex) > 0;

			if (isInVertivalBounds && GetFile(squareIndex) > 0) {
				var leftDiagonalDirection = color == PieceColor.White ?
					Direction.NorthWest : Direction.SouthWest;
				int targetSquareIndex = squareIndex + GetSquareOffset(leftDiagonalDirection);
				var targetPiece = pieces[targetSquareIndex];
				if (targetPiece != Piece.Empty && targetPiece.Color != color) {
					lock (moves) {
						moves.Add(new Move(squareIndex, targetSquareIndex));
					}
				}
			}

			if (isInVertivalBounds && GetFile(squareIndex) < 7) {
				var rightDiagonalDirection = color == PieceColor.White ?
					Direction.NorthEast : Direction.SouthEast;
				var targetSquareIndex = squareIndex + GetSquareOffset(rightDiagonalDirection);
				var targetPiece = pieces[targetSquareIndex];
				if (targetPiece != Piece.Empty && targetPiece.Color != color) {
					lock (moves) {
						moves.Add(new Move(squareIndex, targetSquareIndex));
					}
				}
			}

			var twoSquarePawnColor = state.TwoSquarePawn != -1 ? pieces[state.TwoSquarePawn].Color : PieceColor.None;
			bool enPassant = 
				twoSquarePawnColor != PieceColor.None && 
				twoSquarePawnColor != color && 
				(squareIndex - state.TwoSquarePawn) switch {
				1 => true,
				-1 => true,
				_ => false
			};

			if (enPassant) {
				lock (moves) {
					var offsetDirection = color == PieceColor.White ? Direction.North : Direction.South;
					moves.Add(new Move(squareIndex, state.TwoSquarePawn + GetSquareOffset(offsetDirection)));
				}
			}

			int directionOffsetIndex = color == PieceColor.White ? 0 : 1;
			int avaibleSquares = (color == PieceColor.White && GetRank(squareIndex) == 1
				|| color == PieceColor.Black && GetRank(squareIndex) == 6) ? 2 : 1;
			for (int i = 0; i < Mathf.Min(GetMoveLimit(squareIndex, directionOffsetIndex), avaibleSquares); i++) {
				var targetSquareIndex = squareIndex + squareOffsets[directionOffsetIndex] * (i + 1);
				var targetPiece = pieces[targetSquareIndex];

				if (targetPiece != Piece.Empty)
					break;

				lock (moves) {
					moves.Add(new Move(squareIndex, targetSquareIndex));
				}
			}
		}

		private void GenerateKnightMove(int squareIndex) {
			int file = GetFile(squareIndex);
			int rank = GetRank(squareIndex);
			var color = pieces[squareIndex].Color;

			if (file > 1 && rank > 0) {
				var targetSquareIndex = squareIndex + GetSquareOffset(Direction.SouthWest) * 2 + GetSquareOffset(Direction.North);
				var targetPiece = pieces[targetSquareIndex];
				if (targetPiece.Color != color) {
					lock (moves) {
						moves.Add(new Move(squareIndex, targetSquareIndex));
					}
				}
			}

			if (file > 0 && rank > 1) {
				var targetSquareIndex = squareIndex + GetSquareOffset(Direction.SouthWest) * 2 + GetSquareOffset(Direction.East);
				var targetPiece = pieces[targetSquareIndex];
				if (targetPiece.Color != color) {
					lock (moves) {
						moves.Add(new Move(squareIndex, targetSquareIndex));
					}
				}
			}

			if (file > 0 && rank < 6) {
				var targetSquareIndex = squareIndex + GetSquareOffset(Direction.NorthWest) * 2 + GetSquareOffset(Direction.East);
				var targetPiece = pieces[targetSquareIndex];
				if (targetPiece.Color != color) {
					lock (moves) {
						moves.Add(new Move(squareIndex, targetSquareIndex));
					}
				}
			}

			if (file > 1 && rank < 7) {
				var targetSquareIndex = squareIndex + GetSquareOffset(Direction.NorthWest) * 2 + GetSquareOffset(Direction.South);
				var targetPiece = pieces[targetSquareIndex];
				if (targetPiece.Color != color) {
					lock (moves) {
						moves.Add(new Move(squareIndex, targetSquareIndex));
					}
				}
			}

			if (file < 7 && rank < 6) {
				var targetSquareIndex = squareIndex + GetSquareOffset(Direction.NorthEast) * 2 + GetSquareOffset(Direction.West);
				var targetPiece = pieces[targetSquareIndex];
				if (targetPiece.Color != color) {
					lock (moves) {
						moves.Add(new Move(squareIndex, targetSquareIndex));
					}
				}
			}

			if (file < 6 && rank < 7) {
				var targetSquareIndex = squareIndex + GetSquareOffset(Direction.NorthEast) * 2 + GetSquareOffset(Direction.South);
				var targetPiece = pieces[targetSquareIndex];
				if (targetPiece.Color != color) {
					lock (moves) {
						moves.Add(new Move(squareIndex, targetSquareIndex));
					}
				}
			}

			if (file < 6 && rank > 0) {
				var targetSquareIndex = squareIndex + GetSquareOffset(Direction.SouthEast) * 2 + GetSquareOffset(Direction.North);
				var targetPiece = pieces[targetSquareIndex];
				if (targetPiece.Color != color) {
					lock (moves) {
						moves.Add(new Move(squareIndex, targetSquareIndex));
					}
				}

			}

			if (file < 7 && rank > 1) {
				var targetSquareIndex = squareIndex + GetSquareOffset(Direction.SouthEast) * 2 + GetSquareOffset(Direction.West);
				var targetPiece = pieces[targetSquareIndex];
				if (targetPiece.Color != color) {
					lock (moves) {
						moves.Add(new Move(squareIndex, targetSquareIndex));
					}
				}
			}
		}

		private void GenerateKingMove(int squareIndex) {
			var color = pieces[squareIndex].Color;

			// Castling
			if (color == PieceColor.White) {
				if (state.IsWhiteKingsideCastlingAvaible) {
					bool isWhiteKingCastlingPossigle = true;
					for (int i = 1; i <= 2; i++) {
						if (!pieces[squareIndex + GetSquareOffset(Direction.East) * i].IsEmpty) {
							isWhiteKingCastlingPossigle = false;
							break;
						}
					}

					if (isWhiteKingCastlingPossigle) {
						lock (moves) {
							moves.Add(new Move(squareIndex, squareIndex + GetSquareOffset(Direction.East) * 2));
						}
					}
				}

				if (state.IsWhiteQueensideCastlingAvaible) {
					bool isWhiteQueenCastlingPossigle = true;
					for (int i = 1; i <= 3; i++) {
						if (!pieces[squareIndex + GetSquareOffset(Direction.West) * i].IsEmpty) {
							isWhiteQueenCastlingPossigle = false;
							break;
						}
					}

					if (isWhiteQueenCastlingPossigle) {
						lock (moves) {
							moves.Add(new Move(squareIndex, squareIndex + GetSquareOffset(Direction.West) * 2));
						}
					}
				}
			} else {
				if (state.IsBlackKingsideCastlingAvaible) {
					bool isBlackKingCastlingPossigle = true;
					for (int i = 1; i <= 2; i++) {
						if (!pieces[squareIndex + GetSquareOffset(Direction.East) * i].IsEmpty) {
							isBlackKingCastlingPossigle = false;
							break;
						}
					}

					if (isBlackKingCastlingPossigle) {
						lock (moves) {
							moves.Add(new Move(squareIndex, squareIndex + GetSquareOffset(Direction.East) * 2));
						}
					}
				}


				if (state.IsBlackQueensideCastlingAvaible) {
					bool isBlackQueenCastlingPossigle = true;
					for (int i = 1; i <= 3; i++) {
						if (!pieces[squareIndex + GetSquareOffset(Direction.West) * i].IsEmpty) {
							isBlackQueenCastlingPossigle = false;
							break;
						}
					}

					if (isBlackQueenCastlingPossigle) {
						lock (moves) {
							moves.Add(new Move(squareIndex, squareIndex + GetSquareOffset(Direction.West) * 2));
						}
					}
				}
			}

			for (int directionIndex = 0; directionIndex < 8; directionIndex++) {
				if (GetMoveLimit(squareIndex, directionIndex) == 0)
					continue;

				int targetSquareIndex = squareIndex + squareOffsets[directionIndex];
				var targetPiece = pieces[targetSquareIndex];

				if (targetPiece.Color == color)
					continue;

				lock (moves) {
					moves.Add(new Move(squareIndex, targetSquareIndex));
				}
			}
		}

		private void GenerateSlidingMoves(int squareIndex) {
			var piece = pieces[squareIndex];
			var type = piece.Type;

			var color = piece.Color;
			int startDirectionIndex = type == PieceType.Bishop ? 4 : 0;
			int endDirectionIndex = type == PieceType.Rook ? 4 : 8;

			for (int directionIndex = startDirectionIndex; directionIndex < endDirectionIndex; directionIndex++) {
				for (int i = 0; i < GetMoveLimit(squareIndex, directionIndex); i++) {
					int targetSquareIndex = squareIndex + squareOffsets[directionIndex] * (i + 1);
					var targetPiece = pieces[targetSquareIndex];

					// Stop on friend.
					if (targetPiece.Color == color)
						break;

					lock (moves) {
						moves.Add(new Move(squareIndex, targetSquareIndex));
					}

					// Stop on oponent.
					if (targetPiece.Color != PieceColor.None && targetPiece.Color != color)
						break;
				}
			}
		}

		private bool IsSliding(PieceType type) => type switch {
			PieceType.Bishop => true,
			PieceType.Rook => true,
			PieceType.Queen => true,
			_ => false
		};

		private void GenerateMove(int squareIndex) {
			var piece = pieces[squareIndex];

			if (piece == Piece.Empty)
				return;

			var type = piece.Type;
			if (IsSliding(type)) {
				GenerateSlidingMoves(squareIndex);
			} else if (type == PieceType.Pawn) {
				GeneratePawnMove(squareIndex);
			} else if (type == PieceType.Knight) {
				GenerateKnightMove(squareIndex);
			} else if (type == PieceType.King) {
				GenerateKingMove(squareIndex);
			}
		}

		private void SwapMoveColor() {
			color = color == PieceColor.White ? PieceColor.Black : PieceColor.White;
		}
	}
}
