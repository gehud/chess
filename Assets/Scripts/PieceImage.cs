using UnityEngine;
using UnityEngine.UI;

namespace Chess
{
    public class PieceImage : MonoBehaviour
    {
        [SerializeField]
        private Image image;
        [SerializeField]
        private PieceSprites sprites;

        public void UpdateImage(Square square)
        {
            UpdateImage(square.Piece, square.Color);
        }

        public void UpdateImage(Piece piece, Color color)
        {
            image.sprite = sprites.GetPieceSprite(piece, color);
            image.enabled = image.sprite != null;
        }
    }
}
