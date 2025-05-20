using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Chess
{
    public class BoardPainter : MonoBehaviour
    {
        [SerializeField]
        private UnityEngine.Color lightColor = UnityEngine.Color.white;
        [SerializeField]
        private UnityEngine.Color darkColor = UnityEngine.Color.black;
        [SerializeField]
        private UnityEngine.Color moveColor = UnityEngine.Color.green;
        [SerializeField]
        private PieceImage piecePrefab;

        public void Repaint(in Board board)
        {
            if (transform.childCount != Board.Area)
            {
                return;
            }

            for (var square = Square.Zero; square < Board.Area; square++)
            {
                var pieceSlot = transform.GetChild(square);

                if (board[square].IsEmpty)
                {
                    if (pieceSlot.childCount != 0)
                    {
                        Destroy(pieceSlot.GetChild(0).gameObject);
                    }
                }
                else
                {
                    var pieceImage = Instantiate(piecePrefab, pieceSlot);
                    pieceImage.UpdateImage(board[square]);
                }
            }
        }

        public void ShowMoves(GameObject pieceSlot, in NativeList<Move> moves)
        {
            var square = (Square)pieceSlot.transform.GetSiblingIndex();

            foreach (var move in moves)
            {
                if (move.From == square)
                {
                    var slot = transform.GetChild(move.To);
                    slot.GetComponent<Image>().color *= moveColor;
                    slot.GetComponent<PieceSlot>().IsAvailable = true;
                }
            }
        }

        public void ResetSquares()
        {
            if (transform.childCount != Board.Area)
            {
                return;
            }

            for (var square = Square.Zero; square < Board.Area; square++)
            {
                var isLight = (square.File + square.Rank) % 2 != 0;
                var slot = transform.GetChild(square);
                slot.GetComponent<Image>().color = isLight ? lightColor : darkColor;
                slot.GetComponent<PieceSlot>().IsAvailable = false;
            }
        }

        private void OnPieceDropped(Square from, Square to)
        {
            ResetSquares();
        }

        private void Awake()
        {
            ResetSquares();
        }

        private void OnEnable()
        {
            PieceSlot.PieceDropped += OnPieceDropped;
            DraggablePiece.Reverted += ResetSquares;
        }

        private void OnDisable()
        {
            PieceSlot.PieceDropped -= OnPieceDropped;
            DraggablePiece.Reverted -= ResetSquares;
        }

        private void OnValidate()
        {
            ResetSquares();
        }
    }
}
