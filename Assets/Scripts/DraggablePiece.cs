using UnityEngine;
using UnityEngine.EventSystems;

namespace Chess
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DraggablePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public delegate void PickedHandler(GameObject pieceSlot);
        public static event PickedHandler Picked;

        public delegate void RevertedHandler();
        public static event RevertedHandler Reverted;

        public Transform InitialSlot => initialSlot;

        private Transform initialSlot;

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            initialSlot = transform.parent;
            Picked?.Invoke(transform.parent.gameObject);
            transform.SetParent(MainCanvas.Instance.transform);
            GetComponent<CanvasGroup>().blocksRaycasts = false;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)MainCanvas.Instance.transform, eventData.position, Camera.main, out var worldPoint);
            transform.position = worldPoint;
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (eventData.pointerCurrentRaycast.gameObject == null)
            {
                Revert();
            }

            GetComponent<CanvasGroup>().blocksRaycasts = true;
        }

        public void Revert()
        {
            transform.SetParent(initialSlot, false);
            transform.localPosition = Vector3.zero;
            initialSlot = null;
            Reverted?.Invoke();
        }
    }
}
