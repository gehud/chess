using UnityEngine;
using UnityEngine.EventSystems;

namespace Chess
{
    public class PieceSlot : MonoBehaviour, IDropHandler
    {
        public delegate void PieceDroppedHandler(Square from, Square to);
        public static event PieceDroppedHandler PieceDropped;

        public bool IsAvailable { get; set; } = false;

        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            if (!eventData.pointerDrag.TryGetComponent<DraggablePiece>(out var piece))
            {
                return;
            }

            if (!IsAvailable)
            {
                piece.Revert();
                return;
            }

            if (transform.childCount != 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            eventData.pointerDrag.transform.SetParent(transform);
            eventData.pointerDrag.transform.localPosition = Vector3.zero;

            PieceDropped?.Invoke((Square)piece.InitialSlot.GetSiblingIndex(), (Square)transform.GetSiblingIndex());
        }
    }
}
