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

        public void UpdateImage(Piece piece)
        {
            UpdateImage(piece.Figure, piece.Color);
        }

        public void UpdateImage(Figure figure, Color color)
        {
            image.sprite = sprites.GetPieceSprite(figure, color);
            image.enabled = image.sprite != null;
        }
    }
}
