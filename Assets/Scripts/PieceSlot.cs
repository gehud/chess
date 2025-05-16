using UnityEngine;
using UnityEngine.EventSystems;

namespace Chess
{
    public class PieceSlot : MonoBehaviour, IDropHandler
    {
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag.GetComponent<DraggablePiece>() == null)
            {
                return;
            }

            if (transform.childCount != 0)
            {
                Destroy(transform.GetChild(0).gameObject);
            }

            eventData.pointerDrag.transform.SetParent(transform);
            eventData.pointerDrag.transform.localPosition = Vector3.zero;
        }
    }
}
