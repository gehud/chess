using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Chess {
	public class Board : MonoBehaviour {
		public const int SIZE = 8;
		public const int AREA = SIZE * SIZE;

		public static event Action<Piece.Colors> OnCheckmate;

		public enum Directions {
			North,
			South,
			West,
			East,
			NorthWest,
			SouthEast,
			NorthEast,
			SouthWest,
		}

		[Header("Navigation")]
		[SerializeField] private GameObject cursor;

		private static readonly int[] squareOffsets = new int[] {
			8, -8, -1, 1, 7, -7, 9, -9
		};

		private static readonly int[] moveLimits = new int[AREA * 8];

		[Inject] private readonly IPieceViewFactory pieceViewFactory;

		private readonly Piece[] pieces = new Piece[64];
		private readonly GameObject[] views = new GameObject[64];

		private int selected = -1;

		private readonly List<GameObject> cursors = new();

		private List<Move> moves;

		private Piece.Colors moveColor = Piece.Colors.White;

		private struct State {
			public Piece Captured;
			public Move Move;
			public Move RookCastling;
			public int PosiibleEnPassanVictimIndex;
			public int PromotedPawnIndex;
			public bool IsWhiteKingsideCastlingAvaible;
			public bool IsWhiteQueensideCastlingAvaible;
			public bool IsBlackKingsideCastlingAvaible;
			public bool IsBlackQueensideCastlingAvaible;
		}

		private readonly Stack<State> history = new();
		private State currentState;

		private void Awake() {
			InitializeGame();
			SpawnViews();
			ComputeMoveLimits();
			GenerateMoves();
		}

		private void InitializeGame() {
			pieces[0] = new Piece(Piece.Types.Rook, Piece.Colors.White);
			pieces[1] = new Piece(Piece.Types.Knight, Piece.Colors.White);
			pieces[2] = new Piece(Piece.Types.Bishop, Piece.Colors.White);
			pieces[3] = new Piece(Piece.Types.Queen, Piece.Colors.White);
			pieces[4] = new Piece(Piece.Types.King, Piece.Colors.White);
			pieces[5] = new Piece(Piece.Types.Bishop, Piece.Colors.White);
			pieces[6] = new Piece(Piece.Types.Knight, Piece.Colors.White);
			pieces[7] = new Piece(Piece.Types.Rook, Piece.Colors.White);
			pieces[8] = new Piece(Piece.Types.Pawn, Piece.Colors.White);
			pieces[9] = new Piece(Piece.Types.Pawn, Piece.Colors.White);
			pieces[10] = new Piece(Piece.Types.Pawn, Piece.Colors.White);
			pieces[11] = new Piece(Piece.Types.Pawn, Piece.Colors.White);
			pieces[12] = new Piece(Piece.Types.Pawn, Piece.Colors.White);
			pieces[13] = new Piece(Piece.Types.Pawn, Piece.Colors.White);
			pieces[14] = new Piece(Piece.Types.Pawn, Piece.Colors.White);
			pieces[15] = new Piece(Piece.Types.Pawn, Piece.Colors.White);

			pieces[63] = new Piece(Piece.Types.Rook, Piece.Colors.Black);
			pieces[62] = new Piece(Piece.Types.Knight, Piece.Colors.Black);
			pieces[61] = new Piece(Piece.Types.Bishop, Piece.Colors.Black);
			pieces[60] = new Piece(Piece.Types.King, Piece.Colors.Black);
			pieces[59] = new Piece(Piece.Types.Queen, Piece.Colors.Black);
			pieces[58] = new Piece(Piece.Types.Bishop, Piece.Colors.Black);
			pieces[57] = new Piece(Piece.Types.Knight, Piece.Colors.Black);
			pieces[56] = new Piece(Piece.Types.Rook, Piece.Colors.Black);
			pieces[55] = new Piece(Piece.Types.Pawn, Piece.Colors.Black);
			pieces[54] = new Piece(Piece.Types.Pawn, Piece.Colors.Black);
			pieces[53] = new Piece(Piece.Types.Pawn, Piece.Colors.Black);
			pieces[52] = new Piece(Piece.Types.Pawn, Piece.Colors.Black);
			pieces[51] = new Piece(Piece.Types.Pawn, Piece.Colors.Black);
			pieces[50] = new Piece(Piece.Types.Pawn, Piece.Colors.Black);
			pieces[49] = new Piece(Piece.Types.Pawn, Piece.Colors.Black);
			pieces[48] = new Piece(Piece.Types.Pawn, Piece.Colors.Black);

			currentState = new() {
				Captured = Piece.Empty,
				Move = new Move(),
				RookCastling = new Move(),
				PosiibleEnPassanVictimIndex = -1,
				PromotedPawnIndex = -1,
				IsWhiteKingsideCastlingAvaible = true,
				IsWhiteQueensideCastlingAvaible = true,
				IsBlackKingsideCastlingAvaible = true,
				IsBlackQueensideCastlingAvaible = true
			};
		}

		private void ComputeMoveLimits() {
			Parallel.For(0, AREA, (index) => {
				int file = index % SIZE;
				int rank = index / SIZE;

				int northSquareCount = 7 - rank;
				int southSquareCount = rank;
				int westSquareCount = file;
				int eastSquareCount = 7 - file;

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

		private int GetSquareOffset(Directions direction) {
			return squareOffsets[(int)direction];
		}

		private void SpawnView(int squareIndex) {
			Piece piece = pieces[squareIndex];

			if (piece == Piece.Empty)
				return;

			int x = squareIndex % 8;
			int y = squareIndex / 8;

			var view = pieceViewFactory.Create(piece);

			view.transform.position = new Vector3 {
				x = x - 4 + 0.5f,
				y = view.transform.position.y,
				z = y - 4 + 0.5f
			};

			views[squareIndex] = view;
		}

		private void SpawnViews() {
			for (int squareIndex = 0; squareIndex < 64; squareIndex++) {
				SpawnView(squareIndex);
			}
		}

		private void GeneratePawnMove(int squareIndex) {
			var color = pieces[squareIndex].Color;

			bool isInVertivalBounds = color == Piece.Colors.White ? GetRank(squareIndex) < 7 : GetRank(squareIndex) > 0;

			if (isInVertivalBounds && GetFile(squareIndex) > 0) {
				var leftDiagonalDirection = color == Piece.Colors.White ? 
					Directions.NorthWest : Directions.SouthWest;
				int targetSquareIndex = squareIndex + GetSquareOffset(leftDiagonalDirection);
				var targetPiece = pieces[targetSquareIndex];
				if (targetPiece != Piece.Empty && targetPiece.Color != color) {
					lock (moves) {
						moves.Add(new Move(squareIndex, targetSquareIndex));
					}
				}
			}

			if (isInVertivalBounds && GetFile(squareIndex) < 7) {
				var rightDiagonalDirection = color == Piece.Colors.White ? 
					Directions.NorthEast : Directions.SouthEast;
				var targetSquareIndex = squareIndex + GetSquareOffset(rightDiagonalDirection);
				var targetPiece = pieces[targetSquareIndex];
				if (targetPiece != Piece.Empty && targetPiece.Color != color) {
					lock (moves) {
						moves.Add(new Move(squareIndex, targetSquareIndex));
					}
				}
			}

			bool enPassant = (squareIndex - currentState.PosiibleEnPassanVictimIndex) switch {
				1 => true,
				-1 => true,
				_ => false
			};

			if (enPassant) {
				lock (moves) {
					var offsetDirection = color == Piece.Colors.White ? Directions.North : Directions.South;
					moves.Add(new Move(squareIndex, currentState.PosiibleEnPassanVictimIndex + GetSquareOffset(offsetDirection)));
				}
			}

			int directionOffsetIndex = color == Piece.Colors.White ? 0 : 1;
			int avaibleSquares = (color == Piece.Colors.White && GetRank(squareIndex) == 1 
				|| color == Piece.Colors.Black && GetRank(squareIndex) == 6) ? 2 : 1;
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
				var targetSquareIndex = squareIndex + GetSquareOffset(Directions.SouthWest) * 2 + GetSquareOffset(Directions.North);
				var targetPiece = pieces[targetSquareIndex];
				if (targetPiece.Color != color) {
					lock (moves) {
						moves.Add(new Move(squareIndex, targetSquareIndex));
					}
				}
			}

			if (file > 0 && rank > 1) {
				var targetSquareIndex = squareIndex + GetSquareOffset(Directions.SouthWest) * 2 + GetSquareOffset(Directions.East);
				var targetPiece = pieces[targetSquareIndex];
				if (targetPiece.Color != color) {
					lock (moves) {
						moves.Add(new Move(squareIndex, targetSquareIndex));
					}
				}
			}

			if (file > 0 && rank < 6) {
				var targetSquareIndex = squareIndex + GetSquareOffset(Directions.NorthWest) * 2 + GetSquareOffset(Directions.East);
				var targetPiece = pieces[targetSquareIndex];
				if (targetPiece.Color != color) {
					lock (moves) {
						moves.Add(new Move(squareIndex, targetSquareIndex));
					}
				}
			}

			if (file > 1 && rank < 7) {
				var targetSquareIndex = squareIndex + GetSquareOffset(Directions.NorthWest) * 2 + GetSquareOffset(Directions.South);
				var targetPiece = pieces[targetSquareIndex];
				if (targetPiece.Color != color) {
					lock (moves) {
						moves.Add(new Move(squareIndex, targetSquareIndex));
					}
				}
			}

			if (file < 7 && rank < 6) {
				var targetSquareIndex = squareIndex + GetSquareOffset(Directions.NorthEast) * 2 + GetSquareOffset(Directions.West);
				var targetPiece = pieces[targetSquareIndex];
				if (targetPiece.Color != color) {
					lock (moves) {
						moves.Add(new Move(squareIndex, targetSquareIndex));
					}
				}
			}

			if (file < 6 && rank < 7) {
				var targetSquareIndex = squareIndex + GetSquareOffset(Directions.NorthEast) * 2 + GetSquareOffset(Directions.South);
				var targetPiece = pieces[targetSquareIndex];
				if (targetPiece.Color != color) {
					lock (moves) {
						moves.Add(new Move(squareIndex, targetSquareIndex));
					}
				}
			}

			if (file < 6 && rank > 0) {
				var targetSquareIndex = squareIndex + GetSquareOffset(Directions.SouthEast) * 2 + GetSquareOffset(Directions.North);
				var targetPiece = pieces[targetSquareIndex];
				if (targetPiece.Color != color) {
					lock (moves) {
						moves.Add(new Move(squareIndex, targetSquareIndex));
					}
				}

			}

			if (file < 7 && rank > 1) {
				var targetSquareIndex = squareIndex + GetSquareOffset(Directions.SouthEast) * 2 + GetSquareOffset(Directions.West);
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
			if (color == Piece.Colors.White) {
				if (currentState.IsWhiteKingsideCastlingAvaible) {
					bool isWhiteKingCastlingPossigle = true;
					for (int i = 1; i <= 2; i++) {
						if (!pieces[squareIndex + GetSquareOffset(Directions.East) * i].IsEmpty) {
							isWhiteKingCastlingPossigle = false;
							break;
						}
					}

					if (isWhiteKingCastlingPossigle) {
						lock (moves) {
							moves.Add(new Move(squareIndex, squareIndex + GetSquareOffset(Directions.East) * 2));
						}
					}
				}

				if (currentState.IsWhiteQueensideCastlingAvaible) {
					bool isWhiteQueenCastlingPossigle = true;
					for (int i = 1; i <= 3; i++) {
						if (!pieces[squareIndex + GetSquareOffset(Directions.West) * i].IsEmpty) {
							isWhiteQueenCastlingPossigle = false;
							break;
						}
					}

					if (isWhiteQueenCastlingPossigle) {
						lock (moves) {
							moves.Add(new Move(squareIndex, squareIndex + GetSquareOffset(Directions.West) * 2));
						}
					}
				}
			} else {
				if (currentState.IsBlackKingsideCastlingAvaible) {
					bool isBlackKingCastlingPossigle = true;
					for (int i = 1; i <= 2; i++) {
						if (!pieces[squareIndex + GetSquareOffset(Directions.East) * i].IsEmpty) {
							isBlackKingCastlingPossigle = false;
							break;
						}
					}

					if (isBlackKingCastlingPossigle) {
						lock (moves) {
							moves.Add(new Move(squareIndex, squareIndex + GetSquareOffset(Directions.East) * 2));
						}
					}
				}


				if (currentState.IsBlackQueensideCastlingAvaible) {
					bool isBlackQueenCastlingPossigle = true;
					for (int i = 1; i <= 3; i++) {
						if (!pieces[squareIndex + GetSquareOffset(Directions.West) * i].IsEmpty) {
							isBlackQueenCastlingPossigle = false;
							break;
						}
					}

					if (isBlackQueenCastlingPossigle) {
						lock (moves) {
							moves.Add(new Move(squareIndex, squareIndex + GetSquareOffset(Directions.West) * 2));
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
			int startDirectionIndex = type == Piece.Types.Bishop ? 4 : 0;
			int endDirectionIndex = type == Piece.Types.Rook ? 4 : 8;

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
					if (targetPiece.Color != Piece.Colors.None && targetPiece.Color != color)
						break;
				}
			}
		}

		private void GenerateMove(int squareIndex) {
			var piece = pieces[squareIndex];

			if (piece == Piece.Empty)
				return;

			if (piece.IsSliding)
				GenerateSlidingMoves(squareIndex);
			else {
				var type = piece.Type;
				if (type == Piece.Types.Pawn) {
					GeneratePawnMove(squareIndex);
				} else if (type == Piece.Types.Knight) {
					GenerateKnightMove(squareIndex);
				} else if (type == Piece.Types.King) {
					GenerateKingMove(squareIndex);
				}
			}
		}

		private List<Move> GenerateDirtyMoves() {
			moves = new List<Move>();
			Parallel.For(0, 64, (squareIndex) => {
				if (pieces[squareIndex].Color == moveColor)
					GenerateMove(squareIndex);
			});

			return moves;
		}

		private void SwapMoveColor() {
			moveColor = moveColor == Piece.Colors.White ? Piece.Colors.Black : Piece.Colors.White;
		}

		private List<Move> GenerateMoves() {
			var presudoMoves = GenerateDirtyMoves();
			var legalMoves = new List<Move>();
			foreach (var move in presudoMoves) {
				DoMove(move);
				int kingIndex = -1;
				Parallel.For(0, 64, (index, controll) => {
					if (pieces[index].Type == Piece.Types.King && pieces[index].Color != moveColor) {
						kingIndex = index;
						controll.Break();
					}
				});
				var opopnentResoponses = GenerateDirtyMoves();
				if (opopnentResoponses.AsParallel().Any(move => move.To == kingIndex)) {
					// Illegal move.
				} else {
					legalMoves.Add(move);
				}
				UndoMove();
			}
			moves = legalMoves;
			return moves;
		}

		private void DoMove(Move move) {
			history.Push(currentState);
			var piece = pieces[move.From];
			if (piece.Type == Piece.Types.King) {
				bool isWhite = piece.Color == Piece.Colors.White;
				bool isKingCastling = move.To == move.From + GetSquareOffset(Directions.East) * 2;
				bool isQueenCastling = move.To == move.From + GetSquareOffset(Directions.West) * 2;
				if (isKingCastling) {
					if (isWhite && currentState.IsWhiteKingsideCastlingAvaible || !isWhite && currentState.IsBlackKingsideCastlingAvaible) {
						Move rookMove = new Move(
							move.From + GetSquareOffset(Directions.East) * 3,
							move.From + GetSquareOffset(Directions.East)
						);
						pieces[rookMove.To] = pieces[rookMove.From];
						pieces[rookMove.From] = Piece.Empty;
						currentState.RookCastling = rookMove;
					}
				} else if (isQueenCastling) {
					if (isWhite && currentState.IsWhiteQueensideCastlingAvaible || !isWhite && currentState.IsBlackQueensideCastlingAvaible) {
						Move rookMove = new Move(
							move.From + GetSquareOffset(Directions.West) * 4,
							move.From + GetSquareOffset(Directions.West)
						);
						pieces[rookMove.To] = pieces[rookMove.From];
						pieces[rookMove.From] = Piece.Empty;
						currentState.RookCastling = rookMove;
					}
				}
				if (isWhite) {
					currentState.IsWhiteKingsideCastlingAvaible = false;
					currentState.IsWhiteQueensideCastlingAvaible = false;
				} else {
					currentState.IsBlackKingsideCastlingAvaible = false;
					currentState.IsBlackQueensideCastlingAvaible = false;
				}
			} else if (piece.Type == Piece.Types.Rook) {
				bool isWhite = piece.Color == Piece.Colors.White;
				if (isWhite) {
					if (move.From == 0)
						currentState.IsWhiteQueensideCastlingAvaible = false;
					else if (move.From == 7)
						currentState.IsWhiteKingsideCastlingAvaible = false;
				} else {
					if (move.From == 56)
						currentState.IsBlackQueensideCastlingAvaible = false;
					else if (move.From == 64)
						currentState.IsBlackKingsideCastlingAvaible = false;
				}
			} else if (piece.Type == Piece.Types.Pawn) {
				bool isWhite = piece.Color == Piece.Colors.White;
				int rank = GetRank(move.To);
				if (isWhite && rank == 7 || !isWhite && rank == 0) {
					currentState.PromotedPawnIndex = move.To;
					pieces[move.From] = new Piece(Piece.Types.Queen, piece.Color);
				}
			}

			currentState.Captured = pieces[move.To];
			pieces[move.To] = pieces[move.From];
			pieces[move.From] = Piece.Empty;
			currentState.Move = move;
			SwapMoveColor();
		}

		private void UndoMove() {
			SwapMoveColor();

			if (currentState.RookCastling.IsValid) {
				pieces[currentState.RookCastling.From] = pieces[currentState.RookCastling.To];
				pieces[currentState.RookCastling.To] = Piece.Empty;
			}

			if (currentState.PromotedPawnIndex != -1) {
				pieces[currentState.Move.To] = new Piece(Piece.Types.Pawn, moveColor);
			}

			pieces[currentState.Move.From] = pieces[currentState.Move.To];
			pieces[currentState.Move.To] = currentState.Captured;
			currentState = history.Pop();
		}

		private void ApplyMove() {
			history.Clear();
			if (pieces[currentState.Move.To].Type == Piece.Types.Pawn) {
				bool possibleEnPassant = false;

				if (Mathf.Abs(currentState.Move.To - currentState.Move.From) > 9) {
					currentState.PosiibleEnPassanVictimIndex = currentState.Move.To;
					possibleEnPassant = true;
				}

				bool enPassant = (currentState.Move.To - currentState.PosiibleEnPassanVictimIndex) switch { 
					8 => true,
					-8 => true,
					_ => false
				} && (currentState.Move.To - currentState.Move.From) switch { 
					7 => true,
					-7 => true,
					9 => true,
					-9 => true,
					_ => false
				};

				if (enPassant) {
					Destroy(views[currentState.PosiibleEnPassanVictimIndex]);
				}

				if (!possibleEnPassant)
					currentState.PosiibleEnPassanVictimIndex = -1;
			}

			if (currentState.PromotedPawnIndex != -1) {
				Destroy(views[currentState.Move.From]);
				views[currentState.Move.From] = pieceViewFactory.Create(new Piece(Piece.Types.Queen, pieces[currentState.Move.To].Color));
			}
			currentState.PromotedPawnIndex = -1;

			if (views[currentState.Move.To] != null) {
				Destroy(views[currentState.Move.To]);
				views[currentState.Move.To] = null;
			}
			views[currentState.Move.To] = views[currentState.Move.From];
			views[currentState.Move.From] = null;
			int x = currentState.Move.To % 8;
			int y = currentState.Move.To / 8;
			var newPosition = new Vector3 {
				x = x - 4 + 0.5f,
				y = views[currentState.Move.To].transform.position.y,
				z = y - 4 + 0.5f
			};
			views[currentState.Move.To].transform.position = newPosition;

			if (currentState.RookCastling.IsValid) {
				views[currentState.RookCastling.To] = views[currentState.RookCastling.From];
				views[currentState.RookCastling.From] = null;
				x = currentState.RookCastling.To % 8;
				y = currentState.RookCastling.To / 8;
				newPosition = new Vector3 {
					x = x - 4 + 0.5f,
					y = views[currentState.RookCastling.To].transform.position.y,
					z = y - 4 + 0.5f
				};
				views[currentState.RookCastling.To].transform.position = newPosition;
			}
			currentState.RookCastling = new Move();


			GenerateMoves();
			if (IsCheckmate()) {
				if (moveColor == Piece.Colors.White)
					OnCheckmate?.Invoke(Piece.Colors.Black);
				else
					OnCheckmate?.Invoke(Piece.Colors.White);
			}
		}

		private bool IsCheckmate() => moves.Count == 0;

		private const int PAWN = 10;
		private const int KNIGHT = 30;
		private const int BISHOP = 30;
		private const int ROOK = 50;
		private const int QUEEN = 90;

		private int CountColor(Piece.Colors color) {
			int result = 0;
			var pieces = this.pieces.AsParallel();
			result += pieces.Count(piece => piece.Type == Piece.Types.Pawn && piece.Color == color) * PAWN;
			result += pieces.Count(piece => piece.Type == Piece.Types.Knight && piece.Color == color) * KNIGHT;
			result += pieces.Count(piece => piece.Type == Piece.Types.Bishop && piece.Color == color) * BISHOP;
			result += pieces.Count(piece => piece.Type == Piece.Types.Rook && piece.Color == color) * ROOK;
			result += pieces.Count(piece => piece.Type == Piece.Types.Queen && piece.Color == color) * QUEEN;
			return result;
		}

		private int Evaluate() {
			int whiteEvaluation = CountColor(Piece.Colors.White);
			int blackEvaluation = CountColor(Piece.Colors.Black);
			int perspective = moveColor == Piece.Colors.White ? -1 : 1;
			return (whiteEvaluation - blackEvaluation) * perspective;
		}

		private int Search(int depth, int alpha = -99999, int beta = 99999) {
			if (depth == 0)
				return Evaluate();

			var newMoves = GenerateMoves();
			if (newMoves.Count == 0) {
				return -99999;
			}

			foreach (var move in newMoves) {
				DoMove(move);
				int evaluation = -Search(depth - 1, -beta, -alpha);
				UndoMove();
				if (evaluation >= beta) {
					// Move was too good.
					return beta;
				}

				alpha = Mathf.Max(alpha, evaluation);
			}

			return alpha;
		}

		private void MakeComputerMove() {
			var bestMove = new Move();
			var bestValue = -99999;

			var avaibleMoves = moves;

			foreach (var move in avaibleMoves) {
				DoMove(move);
				var boardValue = Search(2);
				UndoMove();
				if (boardValue > bestValue) {
					bestValue = boardValue;
					bestMove = move;
				}
			}

			if (!bestMove.IsValid)
				bestMove = avaibleMoves[UnityEngine.Random.Range(0, avaibleMoves.Count)];

			DoMove(bestMove);
			ApplyMove();
		}

		private void OnSquare(int sqareIndex) {
			if (selected == sqareIndex || sqareIndex == -1) {
				selected = -1;
				foreach (var instance in cursors)
					Destroy(instance);
				cursors.Clear();
				return;
			}

			if (selected == -1 && pieces[sqareIndex].Color == moveColor) {			
				selected = sqareIndex;
				int x = sqareIndex % 8;
				int y = sqareIndex / 8;
				var cursorPosition = new Vector3 {
					x = x - 4 + 0.5f,
					y = cursor.transform.position.y,
					z = y - 4 + 0.5f
				};
				cursors.Add(Instantiate(cursor, cursorPosition, Quaternion.identity));
				foreach (var move in moves.AsParallel().Where(move => move.From == selected)) {
					x = move.To % 8;
					y = move.To / 8;
					cursorPosition = new Vector3 {
						x = x - 4 + 0.5f,
						y = cursor.transform.position.y,
						z = y - 4 + 0.5f
					};
					cursors.Add(Instantiate(cursor, cursorPosition, Quaternion.identity));
				}
			} else if (moves.AsParallel().Any(move => move.From == selected && move.To == sqareIndex)) {
				if (IsCheckmate())
					return;

				DoMove(new Move(selected, sqareIndex));
				ApplyMove();
				foreach (var instance in cursors)
					Destroy(instance);
				cursors.Clear();
				selected = -1;

				if (moveColor == Piece.Colors.Black && !IsCheckmate())
					MakeComputerMove();
			}
		}

		private void Update() {
			if (!Input.GetMouseButtonDown(0))
				return;

			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (!Physics.Raycast(ray, out RaycastHit hitInfo)) {
				OnSquare(-1);
				return;
			}

			var point = hitInfo.point;

			var square = Vector3Int.FloorToInt(point);
			if (square.x < -4 || square.x > 3 || square.z < -4 || square.z > 3) {
				OnSquare(-1);
				return;
			}

			OnSquare((square.z + 4) * 8 + square.x + 4);
		}
	}
}