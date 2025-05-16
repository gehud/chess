using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Chess
{
    public class PickableSquare : MonoBehaviour, IPointerClickHandler
    {
        public static event Action<GameObject> Picked;

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            Picked?.Invoke(gameObject);
        }
    }
}
