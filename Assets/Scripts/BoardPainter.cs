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
        private PieceImage piecePrefab;

        public void Repaint(in Board board)
        {
            if (transform.childCount != Board.Area)
            {
                return;
            }

            for (var coordinate = Coordinate.Zero; coordinate < Board.Area; coordinate++)
            {
                var square = transform.GetChild(coordinate);

                if (board[coordinate].IsEmpty)
                {
                    if (square.childCount != 0)
                    {
                        Destroy(square.GetChild(0).gameObject);
                    }
                }
                else
                {
                    var piece = Instantiate(piecePrefab, square);
                    piece.UpdateImage(board[coordinate]);
                }
            }
        }

        public void ResetColors()
        {
            if (transform.childCount != Board.Area)
            {
                return;
            }

            for (var coordinate = Coordinate.Zero; coordinate < Board.Area; coordinate++)
            {
                var isLight = (coordinate.File + coordinate.Rank) % 2 != 0;
                transform.GetChild(coordinate).GetComponent<Image>().color = isLight ? lightColor : darkColor;
            }
        }

        private void Awake()
        {
            ResetColors();
        }

        private void OnValidate()
        {
            ResetColors();
        }
    }
}
