using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Chess {
	public class Board : MonoBehaviour {
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

		private static readonly int[][] moveLimits = new int[64][];

		[Inject] private readonly IPieceViewFactory pieceViewFactory;

		private readonly Piece[] pieces = new Piece[64];
		private readonly GameObject[] views = new GameObject[64];

		private int selected = -1;

		private readonly List<GameObject> cursors = new();

		private List<Move> moves;

		private Piece.Colors moveColor = Piece.Colors.White;

		private int kingSquareIndex = -1;
		private int twoSquarePawn = -1;
		private bool isBlackKingCastlingAvaible = true;
		private bool isBlackQueenCastlingAvaible = true;
		private bool isWhiteKingCastlingAvaible = true;
		private bool isWhiteQueenCastlingAvaible = true;
		private bool undoIsBlackKingCastlingAvaible = true;
		private bool undoIsBlackQueenCastlingAvaible = true;
		private bool undoIsWhiteKingCastlingAvaible = true;
		private bool undoIsWhiteQueenCastlingAvaible = true;
		private bool isKingSelected = false;
		private Piece undoPiece = Piece.Empty;
		private Move castlingRookMove;
		private int undoKingSquareIndex = -1;
		private Move lastMove;
		private int promotion = -1;

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
		}

		private void ComputeMoveLimits() {
			for (int file = 0; file < 8; file++) {
				for (int rank = 0; rank < 8; rank++) {
					int northSquareCount = 7 - rank;
					int southSquareCount = rank;
					int westSquareCount = file;
					int eastSquareCount = 7 - file;

					int squareIndex = rank * 8 + file;

					moveLimits[squareIndex] = new int[] {
						northSquareCount,
						southSquareCount,
						westSquareCount,
						eastSquareCount,
						Mathf.Min(northSquareCount, westSquareCount),
						Mathf.Min(southSquareCount, eastSquareCount),
						Mathf.Min(northSquareCount, eastSquareCount),
						Mathf.Min(southSquareCount, westSquareCount),
					};
				}
			}
		}

		private int GetFile(int squareIndex) {
			return squareIndex % 8;
		}

		private int GetRank(int squareIndex) {
			return squareIndex / 8;
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

			var view = pieceViewFactory.Get(piece);

			var position = new Vector3 {
				x = x - 4 + 0.5f,
				y = view.transform.position.y,
				z = y - 4 + 0.5f
			};

			views[squareIndex] = Instantiate(view, position, Quaternion.identity);
		}

		private void SpawnViews() {
			for (int squareIndex = 0; squareIndex < 64; squareIndex++) {
				SpawnView(squareIndex);
			}
		}

		#region Move generation

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

			bool enPassant = (squareIndex - twoSquarePawn) switch {
				1 => true,
				-1 => true,
				_ => false
			};

			if (enPassant) {
				lock (moves) {
					var offsetDirection = color == Piece.Colors.White ? Directions.North : Directions.South;
					moves.Add(new Move(squareIndex, twoSquarePawn + GetSquareOffset(offsetDirection)));
				}
			}

			int directionOffsetIndex = color == Piece.Colors.White ? 0 : 1;
			int avaibleSquares = (color == Piece.Colors.White && GetRank(squareIndex) == 1 
				|| color == Piece.Colors.Black && GetRank(squareIndex) == 6) ? 2 : 1;
			for (int i = 0; i < Mathf.Min(moveLimits[squareIndex][directionOffsetIndex], avaibleSquares); i++) {
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

			if (color == moveColor && !isKingSelected) {
				kingSquareIndex = squareIndex;
				undoKingSquareIndex = kingSquareIndex;
				isKingSelected = true;
			}

			// Castling
			if (color == Piece.Colors.White) {
				if (isWhiteKingCastlingAvaible) {
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

				if (isWhiteQueenCastlingAvaible) {
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
				if (isBlackKingCastlingAvaible) {
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


				if (isBlackQueenCastlingAvaible) {
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
				if (moveLimits[squareIndex][directionIndex] == 0)
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
				for (int i = 0; i < moveLimits[squareIndex][directionIndex]; i++) {
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

		private void GenerateMoves() {
			var presudoMoves = GenerateDirtyMoves();
			var legalMoves = new List<Move>();
			foreach (var move in presudoMoves) {
				DoMove(move);
				var opopnentResoponses = GenerateDirtyMoves();
				if (opopnentResoponses.AsParallel().Any(move => move.To == kingSquareIndex)) {
					// Illegal move.
				} else {
					legalMoves.Add(move);
				}

				UndoMove(move);
			}
			moves = legalMoves;
		}

		#endregion

		private void DoMove(Move move) {
			var piece = pieces[move.From];
			if (piece.Type == Piece.Types.King) {
				kingSquareIndex = move.To;
				bool isWhite = piece.Color == Piece.Colors.White;
				bool isKingCastling = move.To == move.From + GetSquareOffset(Directions.East) * 2;
				bool isQueenCastling = move.To == move.From + GetSquareOffset(Directions.West) * 2;
				if (isKingCastling) {
					if (isWhite && isWhiteKingCastlingAvaible || !isWhite && isBlackKingCastlingAvaible) {
						Move rookMove = new Move(
							move.From + GetSquareOffset(Directions.East) * 3,
							move.From + GetSquareOffset(Directions.East)
						);
						pieces[rookMove.To] = pieces[rookMove.From];
						pieces[rookMove.From] = Piece.Empty;
						castlingRookMove = rookMove;
					}
				} else if (isQueenCastling) {
					if (isWhite && isWhiteQueenCastlingAvaible || !isWhite && isBlackQueenCastlingAvaible) {
						Move rookMove = new Move(
							move.From + GetSquareOffset(Directions.West) * 4,
							move.From + GetSquareOffset(Directions.West)
						);
						pieces[rookMove.To] = pieces[rookMove.From];
						pieces[rookMove.From] = Piece.Empty;
						castlingRookMove = rookMove;
					}
				}
				if (isWhite) {
					isWhiteKingCastlingAvaible = false;
					isWhiteQueenCastlingAvaible = false;
				} else {
					isBlackKingCastlingAvaible = false;
					isBlackQueenCastlingAvaible = false;
				}
			} else if (piece.Type == Piece.Types.Rook) {
				bool isWhite = piece.Color == Piece.Colors.White;
				if (isWhite) {
					if (move.From == 0)
						isWhiteQueenCastlingAvaible = false;
					else if (move.From == 7)
						isWhiteKingCastlingAvaible = false;
				} else {
					if (move.From == 56)
						isBlackQueenCastlingAvaible = false;
					else if (move.From == 64)
						isBlackKingCastlingAvaible = false;
				}
			} else if (piece.Type == Piece.Types.Pawn) {
				bool isWhite = piece.Color == Piece.Colors.White;
				int rank = GetRank(move.To);
				if (isWhite && rank == 7 || !isWhite && rank == 0) {
					promotion = move.To;
					pieces[move.From] = new Piece(Piece.Types.Queen, piece.Color);
				}
			}

			undoPiece = pieces[move.To];
			pieces[move.To] = pieces[move.From];
			pieces[move.From] = Piece.Empty;
			lastMove = move;
			moveColor = moveColor == Piece.Colors.White ? Piece.Colors.Black : Piece.Colors.White;
		}

		private void UndoMove(Move move) {
			if (castlingRookMove.IsValid) {
				pieces[castlingRookMove.From] = pieces[castlingRookMove.To];
				pieces[castlingRookMove.To] = Piece.Empty;
				castlingRookMove = new Move();
			}

			moveColor = moveColor == Piece.Colors.White ? Piece.Colors.Black : Piece.Colors.White;
			if (promotion != -1) {
				pieces[move.To] = new Piece(Piece.Types.Pawn, moveColor);
				promotion = -1;
			}

			pieces[move.From] = pieces[move.To];
			pieces[move.To] = undoPiece;
			kingSquareIndex = undoKingSquareIndex;
			isWhiteKingCastlingAvaible = undoIsWhiteKingCastlingAvaible;
			isWhiteQueenCastlingAvaible = undoIsWhiteQueenCastlingAvaible;
			isBlackKingCastlingAvaible = undoIsBlackKingCastlingAvaible;
			isBlackQueenCastlingAvaible = undoIsBlackQueenCastlingAvaible;
		}

		private void ApplyMove() {
			if (pieces[lastMove.To].Type == Piece.Types.Pawn) {
				bool possibleEnPassant = false;

				if (Mathf.Abs(lastMove.To - lastMove.From) > 9) {
					twoSquarePawn = lastMove.To;
					possibleEnPassant = true;
				}

				bool enPassant = (lastMove.To - twoSquarePawn) switch { 
					8 => true,
					-8 => true,
					_ => false
				} && (lastMove.To - lastMove.From) switch { 
					7 => true,
					-7 => true,
					9 => true,
					-9 => true,
					_ => false
				};

				if (enPassant) {
					Destroy(views[twoSquarePawn]);
				}

				if (!possibleEnPassant)
					twoSquarePawn = -1;
			}

			if (promotion != -1) {
				Destroy(views[lastMove.From]);
				views[lastMove.From] = Instantiate(pieceViewFactory.Get(new Piece(Piece.Types.Queen, pieces[lastMove.To].Color)));
			}
			promotion = -1;

			if (views[lastMove.To] != null) {
				Destroy(views[lastMove.To]);
				views[lastMove.To] = null;
			}
			views[lastMove.To] = views[lastMove.From];
			views[lastMove.From] = null;
			int x = lastMove.To % 8;
			int y = lastMove.To / 8;
			var newPosition = new Vector3 {
				x = x - 4 + 0.5f,
				y = views[lastMove.To].transform.position.y,
				z = y - 4 + 0.5f
			};
			views[lastMove.To].transform.position = newPosition;

			if (castlingRookMove.IsValid) {
				views[castlingRookMove.To] = views[castlingRookMove.From];
				views[castlingRookMove.From] = null;
				x = castlingRookMove.To % 8;
				y = castlingRookMove.To / 8;
				newPosition = new Vector3 {
					x = x - 4 + 0.5f,
					y = views[castlingRookMove.To].transform.position.y,
					z = y - 4 + 0.5f
				};
				views[castlingRookMove.To].transform.position = newPosition;
			}
			castlingRookMove = new Move();

			isKingSelected = false;
			undoIsWhiteKingCastlingAvaible = isWhiteKingCastlingAvaible;
			undoIsWhiteQueenCastlingAvaible = isWhiteQueenCastlingAvaible;
			undoIsBlackKingCastlingAvaible = isBlackKingCastlingAvaible;
			undoIsBlackQueenCastlingAvaible = isBlackQueenCastlingAvaible;
			GenerateMoves();
			if (IsCheckmate()) {
				if (moveColor == Piece.Colors.White)
					OnCheckmate?.Invoke(Piece.Colors.Black);
				else
					OnCheckmate?.Invoke(Piece.Colors.White);
			}
		}

		private bool IsCheckmate() => moves.Count == 0;

		private void MakeComputerMove() {
			DoMove(moves[UnityEngine.Random.Range(0, moves.Count)]);
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