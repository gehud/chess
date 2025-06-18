using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace Chess
{
    public class BoardController : MonoBehaviour
    {
        [SerializeField]
        private string startFen = Fen.Start;
        [Space]
        [SerializeField]
        private PieceSpawner pieceSpawner;
        [SerializeField]
        private IndicatorSpawner indicatorSpawner;
        [SerializeField]
        private PromotionSelector promotionSelector;
        [SerializeField]
        private MenuManager menuManager;
        [SerializeField]
        private BotController botController;

        private Board board;
        private MoveList moves;

        private GameObject[] views;
        private List<GameObject> moveIndicators;
        private GameObject checkIndicator;
        private Square? selectedSquare;
        private bool isPlayerMove;
        private GameObject moveFromIndicator;
        private GameObject moveToIndicator;

        private void ClearViews()
        {
            for (var i = 0; i < views.Count(); i++)
            {
                var view = views[i];
                if (view != null)
                {
                    Destroy(view);
                    views[i] = null;
                }
            }

            ClearCheckIndicator();
        }

        private void SpawnViews()
        {
            for (var i = Square.MinIndex; i <= Square.MaxIndex; i++)
            {
                var square = new Square(i);
                var piece = board[square];
                if (piece.IsEmpty)
                {
                    continue;
                }

                var view = pieceSpawner.Spawn(piece);
                view.transform.position = new Vector3(square.File, 0f, square.Rank);
                views[i] = view;
            }

            SpawnCheckIndicator();
        }

        private void SpawnCheckIndicator()
        {
            if (moves.IsInCheck)
            {
                checkIndicator = indicatorSpawner.Spawn(Indication.UnderCheck, board.Kings[board.AlliedColorIndex]);
            }
        }

        private void UpdateViews()
        {
            ClearViews();
            SpawnViews();
        }

        private void ClearCheckIndicator()
        {
            if (checkIndicator != null)
            {
                Destroy(checkIndicator);
            }
        }

        private IEnumerator SelectSquare(Square square)
        {
            if (selectedSquare == square)
            {
                selectedSquare = null;

                ClearIndicators();

                yield break;
            }

            if (selectedSquare == null && moves.Any(move => move.From == square))
            {
                selectedSquare = square;

                var squares = moves.Where(move => move.From == square).Select(move => move.To);
                foreach (var availableSquare in squares.Append(square))
                {
                    moveIndicators.Add(indicatorSpawner.Spawn(Indication.AvailableMove, availableSquare));
                }

                yield break;
            }

            var move = moves.FirstOrDefault(move => move.From == selectedSquare && move.To == square);

            if (move.IsNull)
            {
                yield break;
            }

            if (move.IsPromotion)
            {
                yield return StartCoroutine(promotionSelector.StartSelection(move.To));
                if (promotionSelector.Result == PromotionSelector.PromotionSelectionResult.None)
                {
                    yield break;
                }

                var flags = promotionSelector.Result switch
                {
                    PromotionSelector.PromotionSelectionResult.Queen => MoveFlag.QueenPromotion,
                    PromotionSelector.PromotionSelectionResult.Rook => MoveFlag.RookPromotion,
                    PromotionSelector.PromotionSelectionResult.Bishop => MoveFlag.BishopPromotion,
                    PromotionSelector.PromotionSelectionResult.Knight => MoveFlag.KnightPromotion,
                    _ => default,
                };

                move = new(move.From, move.To, flags);
            }

            selectedSquare = null;
            if (!MakeMove(move))
            {
                yield break;
            }

            if (!isPlayerMove)
            {
                botController.StartSearch(board);
            }
        }

        private void OnSquareSelected(Square square)
        {
            if (!isPlayerMove || promotionSelector.IsOpened || menuManager.IsOpened)
            {
                return;
            }

            StartCoroutine(SelectSquare(square));
        }

        private void OnSquareDeselected()
        {
            if (!isPlayerMove || promotionSelector.IsOpened || menuManager.IsOpened)
            {
                return;
            }

            selectedSquare = null;
            ClearIndicators();
        }

        private void ClearIndicators()
        {
            foreach (var indicator in moveIndicators)
            {
                Destroy(indicator);
            }

            moveIndicators.Clear();
        }

        private bool MakeMove(Move move)
        {
            board.MakeMove(move);
            moves.Dispose();
            moves = new(board, true, Allocator.Persistent);
            ClearIndicators();
            UpdateViews();
            isPlayerMove = !isPlayerMove;

            UpdateLastMoveViews(move);

            if (moves.IsInCheck && moves.Length == 0)
            {
                menuManager.ShowWinner(board.EnemyColor);
                return false;
            }

            if (board.FiftyMoveCounter >= 50)
            {
                menuManager.ShowDraw();
                return false;
            }

            return true;
        }

        private void UpdateLastMoveViews(Move move)
        {
            if (moveFromIndicator != null)
            {
                DestroyImmediate(moveFromIndicator);
            }

            if (moveToIndicator != null)
            {
                DestroyImmediate(moveToIndicator);
            }

            moveFromIndicator = indicatorSpawner.Spawn(Indication.LastMove, move.From);
            moveToIndicator = indicatorSpawner.Spawn(Indication.LastMove, move.To);
        }

        private void OnSearchCompleted(Move move)
        {
            MakeMove(move);
        }

        private void Awake()
        {
            board = new Board(Allocator.Persistent);
            board.Load(startFen);
            moves = new(board, true, Allocator.Persistent);
            views = new GameObject[Board.Area];
            moveIndicators = new();
            isPlayerMove = board.IsWhiteAllied;
        }

        private void OnEnable()
        {
            SquarePicker.Selected += OnSquareSelected;
            SquarePicker.Deselected += OnSquareDeselected;
            BotController.SearchCompleted += OnSearchCompleted;
        }

        private void OnDisable()
        {
            SquarePicker.Selected -= OnSquareSelected;
            SquarePicker.Deselected -= OnSquareDeselected;
            BotController.SearchCompleted -= OnSearchCompleted;
        }

        private void Start()
        {
            UpdateViews();
        }

        private void OnDestroy()
        {
            board.Dispose();

            if (moves.IsCreated)
            {
                moves.Dispose();
            }
        }
    }
}
