using UnityEngine;
using UnityEngine.EventSystems;

namespace Chess
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DraggablePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
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
            GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
    }
}
