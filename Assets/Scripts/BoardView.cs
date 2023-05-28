using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Chess {
	public class BoardView : MonoBehaviour {
		public event Action<PieceColor> OnCheckmate;

		private readonly Board board = new();

		[Header("Navigation")]
		[SerializeField] private GameObject cursor;
		[SerializeField] private bool startPosition = true;
		[SerializeField] private string fen;

		[Inject] private readonly IPieceViewFactory pieceViewFactory;

		private readonly GameObject[] views = new GameObject[Board.AREA];

		private int selected = -1;

		private readonly List<GameObject> cursors = new();

		private void Awake() {
			if (startPosition)
				board.Start();
			else
				board.Load(fen);
			Apply();
		}

		private void UpdateView(int squareIndex) {
			Piece piece = board.Pieces[squareIndex];

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

		private void Apply() {
			foreach (var view in views) {
				if (view != null)
					Destroy(view);
			}

			for (int squareIndex = 0; squareIndex < Board.AREA; squareIndex++) {
				UpdateView(squareIndex);
			}

			board.GenerateMoves();
			GameOverCheck();
		}

		private const int PAWN = 10;
		private const int KNIGHT = 30;
		private const int BISHOP = 30;
		private const int ROOK = 50;
		private const int QUEEN = 90;

		private const int POSITIVE_INFINITY = +999999;
		private const int NEGATIVE_INFINITY = -999999;

		private int CountColor(PieceColor color) {
			int result = 0;
			var pieces = board.Pieces.AsParallel();
			result += pieces.Count(piece => piece.Type == PieceType.Pawn && piece.Color == color) * PAWN;
			result += pieces.Count(piece => piece.Type == PieceType.Knight && piece.Color == color) * KNIGHT;
			result += pieces.Count(piece => piece.Type == PieceType.Bishop && piece.Color == color) * BISHOP;
			result += pieces.Count(piece => piece.Type == PieceType.Rook && piece.Color == color) * ROOK;
			result += pieces.Count(piece => piece.Type == PieceType.Queen && piece.Color == color) * QUEEN;
			return result;
		}

		private int Evaluate() {
			int whiteEvaluation = CountColor(PieceColor.White);
			int blackEvaluation = CountColor(PieceColor.Black);
			int perspective = board.MoveColor == PieceColor.White ? -1 : 1;
			return (whiteEvaluation - blackEvaluation) * perspective;
		}

		private int GetPieceValue(PieceType type) => type switch {
			PieceType.Pawn => PAWN,
			PieceType.Knight => KNIGHT,
			PieceType.Bishop => BISHOP,
			PieceType.Rook => ROOK,
			PieceType.Queen => QUEEN,
			_ => throw new Exception("Invalid piece type.")
		};

		private List<Move> OrderMoves(List<Move> moves) {
			return moves.OrderBy(move => { 
				int quess = 0;
				var fromType = board.Pieces[move.From].Type;
				var toType = board.Pieces[move.To].Type;

				if (toType != PieceType.None) {
					quess += 10 * GetPieceValue(toType) - GetPieceValue(fromType);
				}

				if (fromType == PieceType.Pawn) {
					bool isWhite = board.Pieces[move.From].Color == PieceColor.White;
					if (isWhite && move.To / Board.SIZE == 7 ||
						!isWhite && move.To / Board.SIZE == 0)
						quess += QUEEN;
				}
				return quess;
			}).ToList();
		}

		private int Search(int depth, int alpha = NEGATIVE_INFINITY, int beta = POSITIVE_INFINITY) {
			if (depth == 0)
				return Evaluate();

			var newMoves = OrderMoves(board.GenerateMoves());
			if (newMoves.Count == 0) {
				return NEGATIVE_INFINITY;
			}

			foreach (var move in newMoves) {
				board.Move(move);
				int evaluation = -Search(depth - 1, -beta, -alpha);
				board.Undo();
				if (evaluation >= beta) {
					// Move was too good.
					return beta;
				}

				alpha = Mathf.Max(alpha, evaluation);
			}

			return alpha;
		}

		private void MakeComputerMove() {
			var stopwatch = Stopwatch.StartNew();
			Move? bestMove = null;
			var bestValue = NEGATIVE_INFINITY;

			var avaibleMoves = board.Moves;

			foreach (var move in avaibleMoves) {
				board.Move(move);
				var boardValue = Search(3);
				board.Undo();
				if (boardValue > bestValue) {
					bestValue = boardValue;
					bestMove = move;
				}
			}

			bestMove ??= avaibleMoves[UnityEngine.Random.Range(0, avaibleMoves.Count - 1)];

			board.Move(bestMove.Value);
			Apply();
			UnityEngine.Debug.Log(stopwatch.ElapsedMilliseconds);
		}

		private void GameOverCheck() {
			if (board.IsCheckmate()) {
				if (board.MoveColor == PieceColor.White)
					OnCheckmate?.Invoke(PieceColor.Black);
				else
					OnCheckmate?.Invoke(PieceColor.White);
			}
		}

		private void OnSquare(int sqareIndex) {
			if (selected == sqareIndex || sqareIndex == -1) {
				selected = -1;
				foreach (var instance in cursors)
					Destroy(instance);
				cursors.Clear();
				return;
			}

			if (selected == -1 && board.Pieces[sqareIndex].Color == board.MoveColor) {			
				selected = sqareIndex;
				int x = sqareIndex % 8;
				int y = sqareIndex / 8;
				var cursorPosition = new Vector3 {
					x = x - 4 + 0.5f,
					y = cursor.transform.position.y,
					z = y - 4 + 0.5f
				};
				cursors.Add(Instantiate(cursor, cursorPosition, Quaternion.identity));
				foreach (var move in board.Moves.AsParallel().Where(move => move.From == selected)) {
					x = move.To % 8;
					y = move.To / 8;
					cursorPosition = new Vector3 {
						x = x - 4 + 0.5f,
						y = cursor.transform.position.y,
						z = y - 4 + 0.5f
					};
					cursors.Add(Instantiate(cursor, cursorPosition, Quaternion.identity));
				}
			} else if (board.Moves.AsParallel().Any(move => move.From == selected && move.To == sqareIndex)) {
				if (board.IsCheckmate())
					return;

				board.Move(new Move(selected, sqareIndex));
				Apply();
				foreach (var instance in cursors)
					Destroy(instance);
				cursors.Clear();
				selected = -1;

				GameOverCheck();

				//if (color == PieceColor.Black && !IsCheckmate())
				//	MakeComputerMove();
			}
		}

		private void Update() {
			if (Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftControl)) {
				board.Undo();
				Apply();
			}

			if (Input.GetKeyDown(KeyCode.Slash) && !board.IsCheckmate())
				MakeComputerMove();

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