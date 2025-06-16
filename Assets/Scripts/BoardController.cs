using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using static Codice.Client.BaseCommands.Import.Commit;

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
        private GameObject indicatorPrefab;
        [SerializeField]
        private Material moveMaterial;
        [SerializeField]
        private Material checkMaterial;

        private Board board;
        private MoveList moves;

        private GameObject[] views;
        private List<GameObject> moveIndicators;
        private GameObject checkIndicator;
        private Square? selectedSquare;

        private void ClearViews()
        {
            foreach (var view in views)
            {
                if (view != null)
                {
                    Destroy(view);
                }
            }
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
        }

        private void UpdateViews()
        {
            ClearViews();
            SpawnViews();
        }

        private void OnSquareSelected(Square square)
        {
            if (selectedSquare == square)
            {
                selectedSquare = null;

                ClearIndicators();

                return;
            }

            if (selectedSquare == null && moves.Any(move => move.From == square))
            {
                selectedSquare = square;

                var squares = moves.Where(move => move.From == square).Select(move => move.To);
                foreach (var availableSquare in squares.Append(square))
                {
                    var indicator = Instantiate(indicatorPrefab);
                    indicator.GetComponent<MeshRenderer>().material = moveMaterial;
                    indicator.transform.position = new Vector3
                    {
                        x = availableSquare.File,
                        y = indicator.transform.position.y,
                        z = availableSquare.Rank
                    };

                    moveIndicators.Add(indicator);
                }

                return;
            }

            var move = moves.FirstOrDefault(move => move.From == selectedSquare && move.To == square);

            if (!move.IsValid)
            {
                return;
            }

            selectedSquare = null;
            board.MakeMove(move);
            moves.Dispose();
            moves = new(board, true, Allocator.Persistent);
            ClearIndicators();
            UpdateViews();
        }

        private void OnSquareDeselected()
        {
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

        private void Awake()
        {
            board = new Board(Allocator.Persistent);
            board.Load(startFen);
            moves = new(board, true, Allocator.Persistent);
            views = new GameObject[Board.Area];
            moveIndicators = new();
        }

        private void OnEnable()
        {
            SquarePicker.Selected += OnSquareSelected;
            SquarePicker.Deselected += OnSquareDeselected;
        }

        private void OnDisable()
        {
            SquarePicker.Selected -= OnSquareSelected;
            SquarePicker.Deselected -= OnSquareDeselected;
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
