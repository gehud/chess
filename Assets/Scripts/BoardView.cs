using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Chess {
	public class BoardView : MonoBehaviour {
		public event Action<PieceColor> OnCheckmate;

		private readonly Game game = new();

		[Header("Navigation")]
		[SerializeField] private GameObject cursor;
		[SerializeField] private bool startPosition = true;
		[SerializeField] private string fen;
		[SerializeField] private bool playerWithComputer = true;

		[Inject] private readonly IPieceViewFactory pieceViewFactory;

		private readonly GameObject[] views = new GameObject[Board.AREA];

		private int selected = -1;

		private readonly List<GameObject> cursors = new();

		private void Awake() {
			if (startPosition)
				game.Start();
			else
				game.Load(fen);
			Apply();
		}

		private void UpdateView(int squareIndex) {
			Piece piece = game.Board[squareIndex];

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

		//private List<GameObject> attackSquares = new();

		private void Apply() {
			foreach (var view in views) {
				if (view != null)
					Destroy(view);
			}

			for (int squareIndex = 0; squareIndex < Board.AREA; squareIndex++) {
				UpdateView(squareIndex);
			}

			game.GenerateMoves();
			GameOverCheck();

			//foreach (var item in attackSquares) {
			//	Destroy(item);
			//}

			//attackSquares.Clear();

			//foreach (var item in game.DangerousSquares) {
			//	int x = item % 8;
			//	int y = item / 8;
			//	var cursorPosition = new Vector3 {
			//		x = x - 4 + 0.5f,
			//		y = cursor.transform.position.y,
			//		z = y - 4 + 0.5f
			//	};
			//	var instance = Instantiate(cursor, cursorPosition, Quaternion.identity);
			//	instance.GetComponent<Renderer>().material.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
			//	attackSquares.Add(instance);
			//}
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
			result += game.GetPieceCount(PieceType.Pawn, color) * PAWN;
			result += game.GetPieceCount(PieceType.Knight, color) * KNIGHT;
			result += game.GetPieceCount(PieceType.Bishop, color) * BISHOP;
			result += game.GetPieceCount(PieceType.Rook, color) * ROOK;
			result += game.GetPieceCount(PieceType.Queen, color) * QUEEN;
			return result;
		}

		private int Evaluate() {
			int whiteEvaluation = CountColor(PieceColor.White);
			int blackEvaluation = CountColor(PieceColor.Black);
			int perspective = game.MoveColor == PieceColor.White ? -1 : 1;
			return (whiteEvaluation - blackEvaluation) * perspective;
		}

		private int GetPieceValue(PieceType type) => type switch {
			PieceType.Pawn => PAWN,
			PieceType.Knight => KNIGHT,
			PieceType.Bishop => BISHOP,
			PieceType.Rook => ROOK,
			PieceType.Queen => QUEEN,
			PieceType.King => POSITIVE_INFINITY,
			_ => throw new Exception("Invalid piece type.")
		};

		private List<Move> OrderMoves(List<Move> moves) {
			return moves.OrderBy(move => { 
				int quess = 0;
				var fromType = game.Board[move.From].Type;
				var toType = game.Board[move.To].Type;

				if (toType != PieceType.None) {
					quess += 10 * GetPieceValue(toType) - GetPieceValue(fromType);
				}

				if ((move.Flags & MoveFlags.Promotion) != MoveFlags.None) {
					quess += move.Flags switch {
						MoveFlags.QueenPromotion => QUEEN,
						MoveFlags.RookPromotion => ROOK,
						MoveFlags.KnightPromotion => KNIGHT,
						MoveFlags.BishopPromotion => BISHOP,
						_ => 0
					};
				}

				// Note that attack squares contains squares behind the king under sliding attack!
				// TODO: Change it in future!
				if (game.AttackSquares.Contains(move.To)) {
					quess -= GetPieceValue(fromType);
				}

				return quess;
			}).ToList();
		}

		private int SearchAllCaptures(int alpha, int beta) {
			int evaluation = Evaluate();
			if (evaluation >= beta)
				return beta;

			alpha = Mathf.Max(alpha, evaluation);

			var captures = game.GenerateMoves(true);
			captures = OrderMoves(captures);

			foreach (var move in captures) {
				game.Move(move);
				evaluation = -SearchAllCaptures(-beta, -alpha);
				game.Undo();

				if (evaluation >= beta)
					return beta;
				alpha = Mathf.Max(alpha, evaluation);
			}

			return alpha;
		}

		private int Search(int depth, int alpha = NEGATIVE_INFINITY, int beta = POSITIVE_INFINITY) {
			if (depth == 0) {
				return SearchAllCaptures(alpha, beta);
			}

			var newMoves = OrderMoves(game.GenerateMoves());
			if (newMoves.Count == 0) {
				if (game.IsCheck()) {
					return NEGATIVE_INFINITY;
				}

				return 0;
			}

			foreach (var move in newMoves) {
				game.Move(move);
				int evaluation = -Search(depth - 1, -beta, -alpha);
				game.Undo();
				if (evaluation >= beta) {
					// Move was too good.
					return beta;
				}

				alpha = Mathf.Max(alpha, evaluation);
			}

			return alpha;
		}

		private void MakeComputerMove() {
			Move? bestMove = null;
			var bestValue = NEGATIVE_INFINITY;

			var avaibleMoves = game.Moves;

			foreach (var move in avaibleMoves) {
				game.Move(move);
				var boardValue = Search(2);
				game.Undo();
				if (boardValue > bestValue) {
					bestValue = boardValue;
					bestMove = move;
				}
			}

			bestMove ??= avaibleMoves[UnityEngine.Random.Range(0, avaibleMoves.Count - 1)];

			game.Move(bestMove.Value);
			Apply();
		}

		private void GameOverCheck() {
			if (game.IsCheckmate()) {
				if (game.MoveColor == PieceColor.White)
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

			if (selected == -1 && game.Board[sqareIndex].Color == game.MoveColor) {			
				selected = sqareIndex;
				int x = sqareIndex % 8;
				int y = sqareIndex / 8;
				var cursorPosition = new Vector3 {
					x = x - 4 + 0.5f,
					y = cursor.transform.position.y,
					z = y - 4 + 0.5f
				};
				cursors.Add(Instantiate(cursor, cursorPosition, Quaternion.identity));
				foreach (var move in game.Moves.AsParallel().Where(move => move.From == selected)) {
					x = move.To % 8;
					y = move.To / 8;
					cursorPosition = new Vector3 {
						x = x - 4 + 0.5f,
						y = cursor.transform.position.y,
						z = y - 4 + 0.5f
					};
					cursors.Add(Instantiate(cursor, cursorPosition, Quaternion.identity));
				}
			} else if (game.Moves.AsParallel().Any(move => move.From == selected && move.To == sqareIndex)) {
				if (game.IsCheckmate())
					return;

				var promotion = game.Moves.AsParallel().Any(move => move.From == selected && move.To == sqareIndex && (move.Flags & MoveFlags.Promotion) != MoveFlags.None);

				game.Move(new Move(selected, sqareIndex, promotion ? MoveFlags.QueenPromotion : MoveFlags.None));
				Apply();
				foreach (var instance in cursors)
					Destroy(instance);
				cursors.Clear();
				selected = -1;

				GameOverCheck();

				if (playerWithComputer && game.MoveColor == PieceColor.Black && !game.IsCheckmate())
					MakeComputerMove();
			}
		}

		private void Update() {
			if (Input.GetKeyDown(KeyCode.X)) {
				game.Undo();
				Apply();
			}

			if (Input.GetKeyDown(KeyCode.Slash) && !game.IsCheckmate())
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