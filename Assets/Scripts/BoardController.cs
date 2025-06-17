using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
                checkIndicator = indicatorSpawner.Spawn(Indication.Check, board.Kings[board.AlliedColorIndex]);
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
                    moveIndicators.Add(indicatorSpawner.Spawn(Indication.Move, availableSquare));
                }

                yield break;
            }

            var move = moves.FirstOrDefault(move => move.From == selectedSquare && move.To == square);

            if (!move.IsValid)
            {
                yield break;
            }

            if ((move.Flags & MoveFlags.Promotion) != MoveFlags.None)
            {
                yield return StartCoroutine(promotionSelector.StartSelection(move.To));
                if (promotionSelector.Result == PromotionSelector.PromotionSelectionResult.None)
                {
                    yield break;
                }

                var flags = move.Flags;
                flags &= ~MoveFlags.Promotion;
                flags |= promotionSelector.Result switch
                {
                    PromotionSelector.PromotionSelectionResult.Queen => MoveFlags.QueenPromotion,
                    PromotionSelector.PromotionSelectionResult.Rook => MoveFlags.RookPromotion,
                    PromotionSelector.PromotionSelectionResult.Bishop => MoveFlags.BishopPromotion,
                    PromotionSelector.PromotionSelectionResult.Knight => MoveFlags.KnightPromotion,
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

            if (moves.IsInCheck && moves.Length == 0)
            {
                menuManager.ShowWinner(board.EnemyColor);
                return false;
            }

            return true;
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
