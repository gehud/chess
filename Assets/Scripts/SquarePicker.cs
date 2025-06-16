using UnityEngine;
using UnityEngine.InputSystem;

namespace Chess
{
    public class SquarePicker : MonoBehaviour
    {
        public delegate void SelectedHandler(Square square);
        public SelectedHandler Selected;

        public delegate void DeselectedHandler();
        public DeselectedHandler Deselected;

        private Controls controls;

        private void OnPress(InputAction.CallbackContext ctx)
        {
            var ray = Camera.main.ScreenPointToRay(ctx.ReadValue<Vector2>());

            if (!Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Deselected?.Invoke();
                return;
            }

            var point = hitInfo.point;

            var square = Vector3Int.FloorToInt(point);
            if (square.x < -4 || square.x > 3 || square.z < -4 || square.z > 3)
            {
                Deselected?.Invoke();
                return;
            }

            Selected?.Invoke(new((square.z + 4) * 8 + square.x + 4));
        }

        private void Awake()
        {
            controls = new();
            controls.Player.Pick.performed += OnPress;
        }

        private void OnEnable()
        {
            controls.Enable();
        }

        private void OnDisable()
        {
            controls.Disable();
        }
    }
}
