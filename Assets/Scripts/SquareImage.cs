using UnityEngine;
using UnityEngine.UI;
using UnityColor = UnityEngine.Color;

namespace Chess
{
    public class SquareImage : MonoBehaviour
    {
        [SerializeField]
        private Image image;
        [Space]
        [SerializeField]
        private UnityColor lightColor = UnityColor.white;
        [SerializeField]
        private UnityColor darkColor = UnityColor.black;
        [SerializeField]
        private UnityColor availableColor = UnityColor.green;

        public void SetDefaultColor()
        {
            var square = (Square)transform.GetSiblingIndex();
            Debug.Assert(square >= 0 || square < Board.Area, "Square is out of board range.");
            var isLight = (square.File + square.Rank) % 2 != 0;
            image.color = isLight ? lightColor : darkColor;
        }

        public void SetAvailableColor()
        {
            SetDefaultColor();
            image.color *= availableColor;
        }
    }
}
