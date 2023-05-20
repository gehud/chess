using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Chess {
	public class Board : MonoBehaviour {
		[Header("Navigation")]
		[SerializeField]
		private GameObject cursor;
		[Header("Pieces")]
		[SerializeField] private GameObject whitePawn;
		[SerializeField] private GameObject whiteKnight;
		[SerializeField] private GameObject whiteBishop;
		[SerializeField] private GameObject whiteRook;
		[SerializeField] private GameObject whiteQueen;
		[SerializeField] private GameObject whiteKing;
		[SerializeField] private GameObject blackPawn;
		[SerializeField] private GameObject blackKnight;
		[SerializeField] private GameObject blackBishop;
		[SerializeField] private GameObject blackRook;
		[SerializeField] private GameObject blackQueen;
		[SerializeField] private GameObject blackKing;

		private readonly Piece[] pieces = new Piece[64];
		private readonly GameObject[] views = new GameObject[64];

		private int selected = -1;

		private readonly List<GameObject> cursors = new();
		private readonly List<Move> moves = new();

		private enum Direction {
			North,
			South,
			West,
			East,
			NorthWest,
			SouthEast,
			NorthEast,
			SouthWest,
		}

		private static readonly int[][] squaresToEdgeCount = new int[64][];

		private void Awake() {
			InitializeStartBoard();
			SpawnPieceViews();
			ComputeMoveData();
			GenerateMoves();
		}

		private void InitializeStartBoard() {
			pieces[0] = new Piece(Piece.Types.Rook, Piece.Colors.White);
			pieces[1] = new Piece(Piece.Types.Knight, Piece.Colors.White);
			pieces[2] = new Piece(Piece.Types.Bishop, Piece.Colors.White);
			pieces[3] = new Piece(Piece.Types.King, Piece.Colors.White);
			pieces[4] = new Piece(Piece.Types.Queen, Piece.Colors.White);
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

		private void SpawnPieceViews() {
			for (int i = 0; i < 64; i++) {
				int x = i % 8;
				int y = i / 8;
				var position = new Vector3 {
					x = x - 4 + 0.5f,
					y = 0.3f,
					z = y - 4 + 0.5f
				};
				Piece piece = pieces[i];
				bool isWhite = piece.Color == Piece.Colors.White;
				Piece.Types type = piece.Type;
				switch (type) {
					case Piece.Types.Pawn:
						views[i] = Instantiate(isWhite ? whitePawn : blackPawn, position, Quaternion.identity, transform);
						break;
					case Piece.Types.Knight:
						views[i] = Instantiate(isWhite ? whiteKnight : blackKnight, position, Quaternion.identity, transform);
						break;
					case Piece.Types.Bishop:
						views[i] = Instantiate(isWhite ? whiteBishop : blackBishop, position, Quaternion.identity, transform);
						break;
					case Piece.Types.Rook:
						views[i] = Instantiate(isWhite ? whiteRook : blackRook, position, Quaternion.identity, transform);
						break;
					case Piece.Types.Queen:
						views[i] = Instantiate(isWhite ? whiteQueen : blackQueen, position, Quaternion.identity, transform);
						break;
					case Piece.Types.King:
						views[i] = Instantiate(isWhite ? whiteKing : blackKing, position, Quaternion.identity, transform);
						break;
				}
			}
		}

		private void ComputeMoveData() {
			for (int file = 0; file < 8; file++) {
				for (int rank = 0; rank < 8; rank++) {
					int northSquareCount = 7 - rank;
					int southSquareCount = rank;
					int westSquareCount = file;
					int eastSquareCount = 7 - file;

					int squareIndex = rank * 8 + file;

					squaresToEdgeCount[squareIndex] = new int[] {
						northSquareCount,
						southSquareCount,
						westSquareCount,
						eastSquareCount,
						Math.Min(northSquareCount, westSquareCount),
						Math.Min(southSquareCount, eastSquareCount),
						Math.Min(northSquareCount, eastSquareCount),
						Math.Min(southSquareCount, westSquareCount),
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

		private void GenerateMoves() {
			moves.Clear();
			Parallel.For(0, 64, (squareIndex) => {
				GenerateMove(squareIndex);
			});
		}

		private static readonly int[] squareDirectionOffsets = new int[] {
			8, -8, -1, 1, 7, -7, 9, -9
		};

		private int GetSquareOffset(Direction direction) {
			return squareDirectionOffsets[(int)direction];
		}

		private int twoSquarePawn = -1;

		private void GeneratePawnMove(int squareIndex) {
			var color = pieces[squareIndex].Color;

			bool isInVertivalBounds = color == Piece.Colors.White ? GetRank(squareIndex) < 7 : GetRank(squareIndex) > 0;

			if (isInVertivalBounds && GetFile(squareIndex) > 0) {
				var leftDiagonalDirection = color == Piece.Colors.White ? 
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
				var rightDiagonalDirection = color == Piece.Colors.White ? 
					Direction.NorthEast : Direction.SouthEast;
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
					var offsetDirection = color == Piece.Colors.White ? Direction.North : Direction.South;
					moves.Add(new Move(squareIndex, twoSquarePawn + GetSquareOffset(offsetDirection)));
				}
			}

			int directionOffsetIndex = color == Piece.Colors.White ? 0 : 1;
			int avaibleSquares = (color == Piece.Colors.White && GetRank(squareIndex) == 1 
				|| color == Piece.Colors.Black && GetRank(squareIndex) == 6) ? 2 : 1;
			for (int i = 0; i < Mathf.Min(squaresToEdgeCount[squareIndex][directionOffsetIndex], avaibleSquares); i++) {
				var targetSquareIndex = squareIndex + squareDirectionOffsets[directionOffsetIndex] * (i + 1);
				var targetPiece = pieces[targetSquareIndex];

				if (targetPiece.Color == color)
					break;

				lock (moves) {
					moves.Add(new Move(squareIndex, targetSquareIndex));
				}

				if (targetPiece.Color != Piece.Colors.None && targetPiece.Color != color)
					break;
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

			for (int directionIndex = 0; directionIndex < 8; directionIndex++) {
				if (squaresToEdgeCount[squareIndex][directionIndex] == 0)
					continue;

				int targetSquareIndex = squareIndex + squareDirectionOffsets[directionIndex];
				var targetPiece = pieces[targetSquareIndex];

				if (targetPiece.Color == color)
					continue;

				lock (moves) {
					moves.Add(new Move(squareIndex, targetSquareIndex));
				}
			}
		}

		private void GenerateMove(int squareIndex) {
			var piece = pieces[squareIndex];

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

		private void GenerateSlidingMoves(int squareIndex) {
			var piece = pieces[squareIndex];
			var type = piece.Type;

			var color = piece.Color;
			int startDirectionIndex = type == Piece.Types.Bishop ? 4 : 0;
			int endDirectionIndex = type == Piece.Types.Rook ? 4 : 8;

			for (int directionIndex = startDirectionIndex; directionIndex < endDirectionIndex; directionIndex++) {
				for (int i = 0; i < squaresToEdgeCount[squareIndex][directionIndex]; i++) {
					int targetSquareIndex = squareIndex + squareDirectionOffsets[directionIndex] * (i + 1);
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

		private Piece undoPiece = Piece.Empty;
		private Move lastMove;

		private void MakeMove(Move move) {
			undoPiece = pieces[move.To];
			pieces[move.To] = pieces[move.From];
			pieces[move.From] = Piece.Empty;
			lastMove = move;
		}

		private void UndoMove(Move move) {
			pieces[move.From] = pieces[move.To];
			pieces[move.To] = undoPiece;
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
			GenerateMoves();
		}

		private void OnSquare(int sqareIndex) {
			if (selected == sqareIndex || sqareIndex == -1) {
				selected = -1;
				foreach (var instance in cursors)
					Destroy(instance);
				cursors.Clear();
				return;
			}

			if (selected == -1) {			
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
			} else {
				if (moves.AsParallel().Any(move => move.From == selected && move.To == sqareIndex)) {
					MakeMove(new Move(selected, sqareIndex));
					ApplyMove();
					foreach (var instance in cursors)
						Destroy(instance);
					cursors.Clear();
					selected = -1;
				}
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