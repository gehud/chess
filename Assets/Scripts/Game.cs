﻿using Chess.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Chess {
	public class Game {
		public Board Board => board;

		private readonly Board board = new();

		public PieceColor MoveColor => moveColor;
		private PieceColor moveColor = PieceColor.White;

		public List<Move> Moves => moves;
		private List<Move> moves;

		private GameState state;
		public int myKing;

		private static readonly int[] squareOffsets = new int[] {
			8, -8, 1, -1, 7, -7, 9, -9
		};

		private static readonly int[] moveLimits = new int[Board.AREA * 8];

		public List<int> AttackSquares => attackSquares;
		public List<int> DangerousSquares => dangerousSquares;

		private readonly List<int> attackSquares = new();
		private readonly List<int> lockedSquares = new();
		private readonly List<int> checkSquares = new();
		private readonly List<int> dangerousSquares = new();

		private readonly Stack<GameState> history = new();

		public Game() {
			ComputeMoveLimits();
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
		}

		/// <summary>
		/// Loads initial position.
		/// </summary>
		public void Start() {
			Load("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
		}

		public void Load(string fen) {
			FENUtility.Load(fen, board, ref state, ref moveColor);
		}

		public bool IsCheckmate() => moves.Count == 0;

		private List<Move> GeneratePresudoMoves() {
			moves = new List<Move>();
			attackSquares.Clear();

			for (int i = 0; i < Board.AREA; i++) {
				if (board[i].Color == moveColor)
					SelectPresudoMove(i);
			}

			return moves;
		}

		public List<Move> GenerateMoves() {
			var presudoMoves = GeneratePresudoMoves();

			int myKing = this.myKing;

			SwapMoveColor();
			GeneratePresudoMoves();
			SwapMoveColor();

			UpdateLockedAndCheckSquares(myKing);
			moves = ValidateMoves(presudoMoves, myKing);
			return moves;
		}

		private bool IsKingComeUnderCheck(int myKing, Move move) {
			return move.From == myKing && attackSquares.Contains(move.To);
		}

		private bool IsAvoidLocking(int myKing, Move move) {
			return move.From != myKing && lockedSquares.Contains(move.From) && !lockedSquares.Contains(move.To);
		}

		public bool IsCheck() => checkSquares.Count != 0;

		private bool IsNotSavingFromCheck(int myKing, Move move) {
			return IsCheck() && move.From != myKing && !checkSquares.Contains(move.To);
		}

		private bool IsCastlingUnderAttack(int myKing, Move move) {
			int diff = Mathf.Abs(move.To - move.From);
			bool isCastling = move.From == myKing && 
				(diff == GetSquareOffset(Direction.East) * 2 || diff == GetSquareOffset(Direction.East) * 3);
			return isCastling && attackSquares.Contains(move.From);
		}

		private bool IsDangerousEnPassant(Move move) {
			var piece = board[move.From];
			var diff = Mathf.Abs(move.To - move.From);
			return piece.Color == MoveColor && piece.Type == PieceType.Pawn && 
				(diff == 7 || diff == 9) && dangerousSquares.Contains(move.From);
		}

		private bool IsMoveValid(Move move, int myKing) {
			if (IsNotSavingFromCheck(myKing, move)) {
				return false;
			}

			if (IsCastlingUnderAttack(myKing, move)) {
				return false;
			}

			if (IsAvoidLocking(myKing, move)) {
				return false;
			}

			if (IsKingComeUnderCheck(myKing, move)) {
				return false;
			}

			if (IsDangerousEnPassant(move)) {
				return false;
			}

			return true;
		}

		private List<Move> ValidateMoves(List<Move> presudoMoves, int myKing) {
			var legalMoves = new List<Move>();

			foreach (var move in presudoMoves) {
				if (IsMoveValid(move, myKing)) {
					legalMoves.Add(move);
				}
			}

			return legalMoves;
		}

		private void UpdateLockedAndCheckSquares(int myKing) {
			lockedSquares.Clear();
			checkSquares.Clear();
			dangerousSquares.Clear();
			FindSlidingValidationSquares(myKing);
			FindKnightValidationSquares(myKing);
		}

		private void FindSlidingValidationSquares(int myKing) {
			for (int directionIndex = 0; directionIndex < 8; directionIndex++) {
				bool locked = false;
				bool dangerous = false;
				for (int i = 0; i < GetMoveLimit(myKing, directionIndex); i++) {
					int targetSquareIndex = myKing + squareOffsets[directionIndex] * (i + 1);
					var targetPiece = board[targetSquareIndex];

					if (targetPiece == Piece.Empty)
						continue;

					if (targetPiece.Color == MoveColor) {
						if (locked)
							break;
						locked = true;
						continue;
					}

					var targetPieceType = targetPiece.Type;

					if (targetPieceType == PieceType.Pawn && (directionIndex == 2 || directionIndex == 3)) {
						if (dangerous)
							break;
						dangerous = true;
						continue;
					}

					if (directionIndex < 4 && targetPieceType == PieceType.Rook ||
						directionIndex >= 4 && (targetPieceType == PieceType.Bishop || i == 0 && targetPieceType == PieceType.Pawn) ||
						targetPieceType == PieceType.Queen) {
						for (int j = 0; j <= i; j++) {
							targetSquareIndex = myKing + squareOffsets[directionIndex] * (j + 1);
							if (dangerous)
								dangerousSquares.Add(targetSquareIndex);
							else if (locked)
								lockedSquares.Add(targetSquareIndex);
							else
								checkSquares.Add(targetSquareIndex);
						}
					}

					break;
				}
			}
		}

		private void FindKnightValidationSquares(int myKing) {
			int file = Board.GetFile(myKing);
			int rank = Board.GetRank(myKing);

			if (file > 1 && rank > 0) {
				var targetSquareIndex = myKing + GetSquareOffset(Direction.SouthWest) * 2 + GetSquareOffset(Direction.North);
				var targetPiece = board[targetSquareIndex];
				if (targetPiece.Type == PieceType.Knight && targetPiece.Color != MoveColor) {
					checkSquares.Add(targetSquareIndex);
				}
			}

			if (file > 0 && rank > 1) {
				var targetSquareIndex = myKing + GetSquareOffset(Direction.SouthWest) * 2 + GetSquareOffset(Direction.East);
				var targetPiece = board[targetSquareIndex];
				if (targetPiece.Type == PieceType.Knight && targetPiece.Color != MoveColor) {
					checkSquares.Add(targetSquareIndex);
				}
			}

			if (file > 0 && rank < 6) {
				var targetSquareIndex = myKing + GetSquareOffset(Direction.NorthWest) * 2 + GetSquareOffset(Direction.East);
				var targetPiece = board[targetSquareIndex];
				if (targetPiece.Type == PieceType.Knight && targetPiece.Color != MoveColor) {
					checkSquares.Add(targetSquareIndex);
				}
			}

			if (file > 1 && rank < 7) {
				var targetSquareIndex = myKing + GetSquareOffset(Direction.NorthWest) * 2 + GetSquareOffset(Direction.South);
				var targetPiece = board[targetSquareIndex];
				if (targetPiece.Type == PieceType.Knight && targetPiece.Color != MoveColor) {
					checkSquares.Add(targetSquareIndex);
				}
			}

			if (file < 7 && rank < 6) {
				var targetSquareIndex = myKing + GetSquareOffset(Direction.NorthEast) * 2 + GetSquareOffset(Direction.West);
				var targetPiece = board[targetSquareIndex];
				if (targetPiece.Type == PieceType.Knight && targetPiece.Color != MoveColor) {
					checkSquares.Add(targetSquareIndex);
				}
			}

			if (file < 6 && rank < 7) {
				var targetSquareIndex = myKing + GetSquareOffset(Direction.NorthEast) * 2 + GetSquareOffset(Direction.South);
				var targetPiece = board[targetSquareIndex];
				if (targetPiece.Type == PieceType.Knight && targetPiece.Color != MoveColor) {
					checkSquares.Add(targetSquareIndex);
				}
			}

			if (file < 6 && rank > 0) {
				var targetSquareIndex = myKing + GetSquareOffset(Direction.SouthEast) * 2 + GetSquareOffset(Direction.North);
				var targetPiece = board[targetSquareIndex];
				if (targetPiece.Type == PieceType.Knight && targetPiece.Color != MoveColor) {
					checkSquares.Add(targetSquareIndex);
				}
			}

			if (file < 7 && rank > 1) {
				var targetSquareIndex = myKing + GetSquareOffset(Direction.SouthEast) * 2 + GetSquareOffset(Direction.West);
				var targetPiece = board[targetSquareIndex];
				if (targetPiece.Type == PieceType.Knight && targetPiece.Color != MoveColor) {
					checkSquares.Add(targetSquareIndex);
				}
			}
		}

		public void Move(Move move) {
			history.Push(state);
			state.CastlingRook = new Move();
			state.EnPassantCaptured = -1;
			state.PromotedPawn = -1;
			var piece = board[move.From];
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
						board[rookMove.To] = board[rookMove.From];
						board[rookMove.From] = Piece.Empty;
						state.CastlingRook = rookMove;
					}
				} else if (isQueenCastling) {
					if (isWhite && state.IsWhiteQueensideCastlingAvaible || !isWhite && state.IsBlackQueensideCastlingAvaible) {
						Move rookMove = new Move(
							move.From + GetSquareOffset(Direction.West) * 4,
							move.From + GetSquareOffset(Direction.West)
						);
						board[rookMove.To] = board[rookMove.From];
						board[rookMove.From] = Piece.Empty;
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
					else if (move.From == 63)
						state.IsBlackKingsideCastlingAvaible = false;
				}
			} else if (piece.Type == PieceType.Pawn) {
				bool isWhite = piece.Color == PieceColor.White;
				int rank = Board.GetRank(move.To);

				// Promotion.
				if ((move.Flags & MoveFlags.Promotion) != MoveFlags.None) {
					state.PromotedPawn = move.To;
					board[move.From] = new Piece(PieceType.Queen, piece.Color);
				}

				int moveDifference = move.To - move.From;

				// En Passant.
				if (state.TwoSquarePawn != -1 && 
					Mathf.Abs(move.To - state.TwoSquarePawn) == 8 &&
					(Mathf.Abs(moveDifference) == 7 || Mathf.Abs(moveDifference) == 9) &&
					board[state.TwoSquarePawn].Color != moveColor) {
					state.EnPassantCaptured = state.TwoSquarePawn;
					board[state.EnPassantCaptured] = Piece.Empty;
				}

				if (moveDifference == 16 || moveDifference == -16) {
					state.TwoSquarePawn = move.To;
				} else {
					state.TwoSquarePawn = -1;
				}
			} else {
				state.TwoSquarePawn = -1;
			}

			// Capture the rook and test castling ability.
			if (board[move.To].Type == PieceType.Rook) {
				if (board[move.To].Color == PieceColor.White) {
					if (move.To == 0)
						state.IsWhiteQueensideCastlingAvaible = false;
					else if (move.To == 7)
						state.IsWhiteKingsideCastlingAvaible = false;
				} else {
					if (move.To == 56)
						state.IsBlackQueensideCastlingAvaible = false;
					else if (move.To == 63)
						state.IsBlackKingsideCastlingAvaible = false;
				}
			}

			state.Captured = board[move.To];
			board[move.To] = board[move.From];
			board[move.From] = Piece.Empty;
			state.Move = move;
			SwapMoveColor();
		}

		public void Undo() {
			if (state.EnPassantCaptured != -1) {
				board[state.EnPassantCaptured] = new Piece(PieceType.Pawn, moveColor);
			}

			if (state.CastlingRook.IsValid) {
				board[state.CastlingRook.From] = board[state.CastlingRook.To];
				board[state.CastlingRook.To] = Piece.Empty;
			}

			SwapMoveColor();

			if (state.PromotedPawn != -1) {
				board[state.PromotedPawn] = new Piece(PieceType.Pawn, moveColor);
			}

			board[state.Move.From] = board[state.Move.To];
			board[state.Move.To] = state.Captured;
			state = history.Pop();
		}

		private void ComputeMoveLimits() {
			Parallel.For(0, Board.AREA, (index) => {
				int file = Board.GetFile(index);
				int rank = Board.GetRank(index);

				int northSquareCount = Board.SIZE - 1 - rank;
				int southSquareCount = rank;
				int eastSquareCount = Board.SIZE - 1 - file;
				int westSquareCount = file;

				moveLimits[index + Board.AREA * 0] = northSquareCount;
				moveLimits[index + Board.AREA * 1] = southSquareCount;
				moveLimits[index + Board.AREA * 2] = eastSquareCount;
				moveLimits[index + Board.AREA * 3] = westSquareCount;
				moveLimits[index + Board.AREA * 4] = Mathf.Min(northSquareCount, westSquareCount);
				moveLimits[index + Board.AREA * 5] = Mathf.Min(southSquareCount, eastSquareCount);
				moveLimits[index + Board.AREA * 6] = Mathf.Min(northSquareCount, eastSquareCount);
				moveLimits[index + Board.AREA * 7] = Mathf.Min(southSquareCount, westSquareCount);
			});
		}

		private int GetMoveLimit(int squareIndex, int directionindex) {
			return moveLimits[squareIndex + directionindex * Board.AREA];
		}

		private int GetSquareOffset(Direction direction) {
			return squareOffsets[(int)direction];
		}

		private void GeneratePawnMoves(int squareIndex) {
			var color = board[squareIndex].Color;

			bool isInVertivalBounds = color == PieceColor.White ? Board.GetRank(squareIndex) < 7 : Board.GetRank(squareIndex) > 0;

			bool TryAddPromotionIfNeeded(int fromSquareIndex, int toSquareIndex) {
				var rank = Board.GetRank(toSquareIndex);
				if (rank == 0 || rank == 7) {
					moves.Add(new Move(fromSquareIndex, toSquareIndex, MoveFlags.QueenPromotion));
					moves.Add(new Move(fromSquareIndex, toSquareIndex, MoveFlags.RookPromotion));
					moves.Add(new Move(fromSquareIndex, toSquareIndex, MoveFlags.KnightPromotion));
					moves.Add(new Move(fromSquareIndex, toSquareIndex, MoveFlags.BishopPromotion));
					return true;
				}

				return false;
			}

			if (isInVertivalBounds && Board.GetFile(squareIndex) > 0) {
				var leftDiagonalDirection = color == PieceColor.White ?
					Direction.NorthWest : Direction.SouthWest;
				int targetSquareIndex = squareIndex + GetSquareOffset(leftDiagonalDirection);
				var targetPiece = board[targetSquareIndex];
				attackSquares.Add(targetSquareIndex);
				if (targetPiece != Piece.Empty) {
					if (targetPiece.Color != color) {
						if (!TryAddPromotionIfNeeded(squareIndex, targetSquareIndex)) {
							moves.Add(new Move(squareIndex, targetSquareIndex));
						}
					}
				}
			}

			if (isInVertivalBounds && Board.GetFile(squareIndex) < 7) {
				var rightDiagonalDirection = color == PieceColor.White ?
					Direction.NorthEast : Direction.SouthEast;
				var targetSquareIndex = squareIndex + GetSquareOffset(rightDiagonalDirection);
				var targetPiece = board[targetSquareIndex];
				attackSquares.Add(targetSquareIndex);
				if (targetPiece != Piece.Empty) {
					if (targetPiece.Color != color) {
						if (!TryAddPromotionIfNeeded(squareIndex, targetSquareIndex)) {
							moves.Add(new Move(squareIndex, targetSquareIndex));
						}
					}
				}
			}

			var twoSquarePawnColor = state.TwoSquarePawn != -1 ? board[state.TwoSquarePawn].Color : PieceColor.None;
			bool enPassant =
				twoSquarePawnColor != PieceColor.None &&
				twoSquarePawnColor != color &&
				Mathf.Abs(Board.GetFile(squareIndex) - Board.GetFile(state.TwoSquarePawn)) == 1 &&
				(state.TwoSquarePawn - squareIndex) switch {
					1 => true,
					-1 => true,
					_ => false
				};

			if (enPassant) {
				var offsetDirection = color == PieceColor.White ? Direction.North : Direction.South;
				int targetSquare = state.TwoSquarePawn + GetSquareOffset(offsetDirection);
				moves.Add(new Move(squareIndex, targetSquare));
			}

			int directionOffsetIndex = color == PieceColor.White ? 0 : 1;
			int avaibleSquares = (color == PieceColor.White && Board.GetRank(squareIndex) == 1
				|| color == PieceColor.Black && Board.GetRank(squareIndex) == 6) ? 2 : 1;
			for (int i = 0; i < Mathf.Min(GetMoveLimit(squareIndex, directionOffsetIndex), avaibleSquares); i++) {
				var targetSquareIndex = squareIndex + squareOffsets[directionOffsetIndex] * (i + 1);
				var targetPiece = board[targetSquareIndex];

				if (targetPiece != Piece.Empty)
					break;

				if (!TryAddPromotionIfNeeded(squareIndex, targetSquareIndex)) {
					moves.Add(new Move(squareIndex, targetSquareIndex));
				}
			}
		}

		private void GenerateKnightMoves(int squareIndex) {
			int file = Board.GetFile(squareIndex);
			int rank = Board.GetRank(squareIndex);
			var color = board[squareIndex].Color;

			if (file > 1 && rank > 0) {
				var targetSquareIndex = squareIndex + GetSquareOffset(Direction.SouthWest) * 2 + GetSquareOffset(Direction.North);
				var targetPiece = board[targetSquareIndex];
				attackSquares.Add(targetSquareIndex);
				if (targetPiece.Color != color) {
					moves.Add(new Move(squareIndex, targetSquareIndex));
				}
			}

			if (file > 0 && rank > 1) {
				var targetSquareIndex = squareIndex + GetSquareOffset(Direction.SouthWest) * 2 + GetSquareOffset(Direction.East);
				var targetPiece = board[targetSquareIndex];
				attackSquares.Add(targetSquareIndex);
				if (targetPiece.Color != color) {
					moves.Add(new Move(squareIndex, targetSquareIndex));
				}
			}

			if (file > 0 && rank < 6) {
				var targetSquareIndex = squareIndex + GetSquareOffset(Direction.NorthWest) * 2 + GetSquareOffset(Direction.East);
				var targetPiece = board[targetSquareIndex];
				attackSquares.Add(targetSquareIndex);
				if (targetPiece.Color != color) {
					moves.Add(new Move(squareIndex, targetSquareIndex));
				}
			}

			if (file > 1 && rank < 7) {
				var targetSquareIndex = squareIndex + GetSquareOffset(Direction.NorthWest) * 2 + GetSquareOffset(Direction.South);
				var targetPiece = board[targetSquareIndex];
				attackSquares.Add(targetSquareIndex);
				if (targetPiece.Color != color) {
					moves.Add(new Move(squareIndex, targetSquareIndex));
				}
			}

			if (file < 7 && rank < 6) {
				var targetSquareIndex = squareIndex + GetSquareOffset(Direction.NorthEast) * 2 + GetSquareOffset(Direction.West);
				var targetPiece = board[targetSquareIndex];
				attackSquares.Add(targetSquareIndex);
				if (targetPiece.Color != color) {
					moves.Add(new Move(squareIndex, targetSquareIndex));
				}
			}

			if (file < 6 && rank < 7) {
				var targetSquareIndex = squareIndex + GetSquareOffset(Direction.NorthEast) * 2 + GetSquareOffset(Direction.South);
				var targetPiece = board[targetSquareIndex];
				attackSquares.Add(targetSquareIndex);
				if (targetPiece.Color != color) {
					moves.Add(new Move(squareIndex, targetSquareIndex));
				}
			}

			if (file < 6 && rank > 0) {
				var targetSquareIndex = squareIndex + GetSquareOffset(Direction.SouthEast) * 2 + GetSquareOffset(Direction.North);
				var targetPiece = board[targetSquareIndex];
				attackSquares.Add(targetSquareIndex);
				if (targetPiece.Color != color) {
					moves.Add(new Move(squareIndex, targetSquareIndex));
				}
			}

			if (file < 7 && rank > 1) {
				var targetSquareIndex = squareIndex + GetSquareOffset(Direction.SouthEast) * 2 + GetSquareOffset(Direction.West);
				var targetPiece = board[targetSquareIndex];
				attackSquares.Add(targetSquareIndex);
				if (targetPiece.Color != color) {
					moves.Add(new Move(squareIndex, targetSquareIndex));
				}
			}
		}

		private void GenerateKingMoves(int squareIndex) {
			var color = board[squareIndex].Color;
			myKing = squareIndex;

			// Castling
			if (color == PieceColor.White) {
				if (state.IsWhiteKingsideCastlingAvaible) {
					bool isWhiteKingCastlingPossigle = true;
					for (int i = 1; i <= 2; i++) {
						if (!board[squareIndex + GetSquareOffset(Direction.East) * i].IsEmpty) {
							isWhiteKingCastlingPossigle = false;
							break;
						}
					}

					if (isWhiteKingCastlingPossigle) {
						moves.Add(new Move(squareIndex, squareIndex + GetSquareOffset(Direction.East) * 2));
					}
				}

				if (state.IsWhiteQueensideCastlingAvaible) {
					bool isWhiteQueenCastlingPossigle = true;
					for (int i = 1; i <= 3; i++) {
						if (!board[squareIndex + GetSquareOffset(Direction.West) * i].IsEmpty) {
							isWhiteQueenCastlingPossigle = false;
							break;
						}
					}

					if (isWhiteQueenCastlingPossigle) {
						moves.Add(new Move(squareIndex, squareIndex + GetSquareOffset(Direction.West) * 2));
					}
				}
			} else {
				if (state.IsBlackKingsideCastlingAvaible) {
					bool isBlackKingCastlingPossigle = true;
					for (int i = 1; i <= 2; i++) {
						if (!board[squareIndex + GetSquareOffset(Direction.East) * i].IsEmpty) {
							isBlackKingCastlingPossigle = false;
							break;
						}
					}

					if (isBlackKingCastlingPossigle) {
						moves.Add(new Move(squareIndex, squareIndex + GetSquareOffset(Direction.East) * 2));
					}
				}


				if (state.IsBlackQueensideCastlingAvaible) {
					bool isBlackQueenCastlingPossigle = true;
					for (int i = 1; i <= 3; i++) {
						if (!board[squareIndex + GetSquareOffset(Direction.West) * i].IsEmpty) {
							isBlackQueenCastlingPossigle = false;
							break;
						}
					}

					if (isBlackQueenCastlingPossigle) {
						moves.Add(new Move(squareIndex, squareIndex + GetSquareOffset(Direction.West) * 2));
					}
				}
			}

			for (int directionIndex = 0; directionIndex < 8; directionIndex++) {
				if (GetMoveLimit(squareIndex, directionIndex) == 0)
					continue;

				int targetSquareIndex = squareIndex + squareOffsets[directionIndex];
				var targetPiece = board[targetSquareIndex];

				attackSquares.Add(targetSquareIndex);
				if (targetPiece.Color == color)
					continue;

				moves.Add(new Move(squareIndex, targetSquareIndex));
			}
		}

		private void GenerateSlidingMoves(int squareIndex) {
			var piece = board[squareIndex];
			var pieceType = piece.Type;

			var pieceColor = piece.Color;
			int startDirectionIndex = pieceType == PieceType.Bishop ? 4 : 0;
			int endDirectionIndex = pieceType == PieceType.Rook ? 4 : 8;

			for (int directionIndex = startDirectionIndex; directionIndex < endDirectionIndex; directionIndex++) {
				int limit = GetMoveLimit(squareIndex, directionIndex);
				for (int i = 0; i < limit; i++) {
					int targetSquareIndex = squareIndex + squareOffsets[directionIndex] * (i + 1);
					var targetPiece = board[targetSquareIndex];

					attackSquares.Add(targetSquareIndex);
					if (targetPiece.Color == pieceColor)
						break;

					moves.Add(new Move(squareIndex, targetSquareIndex));

					if (targetPiece != Piece.Empty) {
						if (targetPiece.Type == PieceType.King && targetPiece.Color != pieceColor) {
							for (int j = i + 1; j < limit; j++) {
								targetSquareIndex = squareIndex + squareOffsets[directionIndex] * (j + 1);
								targetPiece = board[targetSquareIndex];
								attackSquares.Add(targetSquareIndex);
								if (targetPiece != Piece.Empty)
									break;
							}
						}

						break;
					}
				}
			}
		}

		private bool IsSliding(PieceType type) => type switch {
			PieceType.Bishop => true,
			PieceType.Rook => true,
			PieceType.Queen => true,
			_ => false
		};

		private void SelectPresudoMove(int squareIndex) {
			var piece = board[squareIndex];

			if (piece == Piece.Empty)
				return;

			var type = piece.Type;
			if (IsSliding(type)) {
				GenerateSlidingMoves(squareIndex);
			} else if (type == PieceType.Pawn) {
				GeneratePawnMoves(squareIndex);
			} else if (type == PieceType.Knight) {
				GenerateKnightMoves(squareIndex);
			} else if (type == PieceType.King) {
				GenerateKingMoves(squareIndex);
			}
		}

		private void SwapMoveColor() {
			moveColor = moveColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
		}
	}
}