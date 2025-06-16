using System.Collections;
using UnityEngine;

namespace Chess
{
    public partial class PromotionSelector : MonoBehaviour
    {
        public PromotionSelectionResult? Result => result;

        [SerializeField]
        private GameObject background;
        [SerializeField]
        private RectTransform origin;

        private PromotionSelectionResult? result;

        public void CancelSelection()
        {
            result = PromotionSelectionResult.None;
        }

        public IEnumerator StartSelection(Square square)
        {
            yield return null;

            background.SetActive(true);
            result = null;

            while (result == null)
            {
                var camera = Camera.main;
                var point = camera.WorldToScreenPoint(new Vector3(square.File, 0f, square.Rank));
                origin.anchoredPosition = point;
                yield return null;
            }

            background.SetActive(false);
        }

        public void SelectQueen()
        {
            result = PromotionSelectionResult.Queen;
        }

        public void SelectRook()
        {
            result = PromotionSelectionResult.Rook;
        }

        public void SelectBishop()
        {
            result = PromotionSelectionResult.Bishop;
        }

        public void SelectKnight()
        {
            result = PromotionSelectionResult.Knight;
        }
    }
}
